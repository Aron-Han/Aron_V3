using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Aron_V3
{
	/// <summary>
	/// 主流程运行调度器：
	/// 通讯输入 -> 触发条件匹配 -> 相机并行取像 -> Task StepFlow 执行 -> 通讯输出反馈。
	/// 
	/// 适配 .NET Framework 4.7.2 / C# 7.3。
	/// </summary>
	public sealed class RuntimeFlowOrchestrator : IDisposable
	{
		private const int ImageAcquireTimeoutMs = 10000;

		private readonly object _syncRoot = new object();
		private readonly HashSet<string> _runningTaskKeys = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
		private readonly HashSet<string> _runningImageAcquireKeys = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
		private readonly Dictionary<string, string> _lastTriggerActualValuesByKey =
			new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

		private readonly RuntimeImageAcquireService _imageAcquireService;
		private readonly RuntimeCommunicationOutputService _outputService;

		private bool _started;
		private bool _disposed;

		public event EventHandler<RuntimeFlowLogEventArgs> LogGenerated;
		public event EventHandler<RuntimeTaskFinishedEventArgs> TaskFinished;

		public RuntimeFlowOrchestrator()
		{
			_imageAcquireService = new RuntimeImageAcquireService();
			_outputService = new RuntimeCommunicationOutputService();
		}

		public void Start()
		{
			if (_disposed)
			{
				throw new ObjectDisposedException("RuntimeFlowOrchestrator");
			}

			if (_started)
			{
				return;
			}

			_started = true;

			CommunicationRuntimeManager.Instance.DataReceived += CommunicationRuntime_DataReceived;
			CommunicationRuntimeManager.Instance.StatusChanged += CommunicationRuntime_StatusChanged;
			CommunicationRuntimeManager.Instance.ErrorOccurred += CommunicationRuntime_ErrorOccurred;

			WriteLog(RuntimeLogCategory.Task, "Runtime flow orchestrator started.");
		}

		public bool RunTaskTest(string jobName, string taskName, TaskRunOptions options)
		{
			if (_disposed)
			{
				throw new ObjectDisposedException("RuntimeFlowOrchestrator");
			}

			ProjectFlowConfig flowConfig = FlowConfigStore.LoadOrCreateDefault();
			JobConfig job = flowConfig == null || flowConfig.Jobs == null
				? null
				: flowConfig.Jobs.FirstOrDefault(x =>
					x != null && string.Equals(x.JobName, jobName, StringComparison.OrdinalIgnoreCase));
			TaskConfig task = job == null || job.Tasks == null
				? null
				: job.Tasks.FirstOrDefault(x =>
					x != null && string.Equals(x.TaskName, taskName, StringComparison.OrdinalIgnoreCase));

			if (task == null)
			{
				throw new Exception("Task not found: " + taskName);
			}

			if (options == null)
			{
				options = TaskRunOptions.Test(false);
			}

			DateTime startTime = DateTime.Now;
			VisionRunContext context = new VisionRunContext();
			context.JobName = jobName;
			context.TaskName = taskName;
			context.TriggerName = task.TriggerName;
			StepResult finalResult = StepResult.NG("Task was not executed.");

			try
			{
				WriteLog(RuntimeLogCategory.Task, "Task offline test started. Job=" + jobName + ", Task=" + taskName);

				using (TaskRunContext.Begin(options))
				{
					ApplyTestImageOverrides(options, context);

					TaskRunner runner = new TaskRunner();
					finalResult = runner.Run(task, context);
				}

				WriteLog(RuntimeLogCategory.Task, "Task offline test finished. Job=" + jobName +
					", Task=" + taskName +
					", CommunicationOutput=" + options.EnableCommunicationOutput +
					", Cost=" + (DateTime.Now - startTime).TotalMilliseconds.ToString("0.0") + " ms");
			}
			catch (Exception ex)
			{
				finalResult = StepResult.NG(ex.Message);
				WriteLog(RuntimeLogCategory.Task, "Task offline test failed. Job=" + jobName +
					", Task=" + taskName +
					", Error=" + ex.Message +
					", Cost=" + (DateTime.Now - startTime).TotalMilliseconds.ToString("0.0") + " ms",
					true);
				throw;
			}
			finally
			{
				OnTaskFinished(new RuntimeTaskFinishedEventArgs(
					jobName,
					taskName,
					finalResult,
					context,
					DateTime.Now - startTime));
			}

			return true;
		}

		private void ApplyTestImageOverrides(TaskRunOptions options, VisionRunContext context)
		{
			if (options == null || options.OverrideImageSources == null || context == null)
			{
				return;
			}

			foreach (KeyValuePair<string, object> pair in options.OverrideImageSources)
			{
				if (string.IsNullOrWhiteSpace(pair.Key) || pair.Value == null)
				{
					continue;
				}

				VisionImage image = new VisionImage();
				image.ImageName = pair.Key;
				image.ImageType = "OfflineTest";
				image.SourceStep = "TaskTest";
				image.RawImage = pair.Value;

				context.SetImage(pair.Key, image);
				context.SetData(pair.Key, pair.Value);
				context.SetData(pair.Key + ".RawImage", pair.Value);
			}
		}

		public void Stop()
		{
			if (!_started)
			{
				return;
			}

			_started = false;

			CommunicationRuntimeManager.Instance.DataReceived -= CommunicationRuntime_DataReceived;
			CommunicationRuntimeManager.Instance.StatusChanged -= CommunicationRuntime_StatusChanged;
			CommunicationRuntimeManager.Instance.ErrorOccurred -= CommunicationRuntime_ErrorOccurred;

			WriteLog(RuntimeLogCategory.Task, "Runtime flow orchestrator stopped.");
		}

		private void CommunicationRuntime_StatusChanged(object sender, CommunicationStatusChangedEventArgs e)
		{
			if (e == null)
			{
				return;
			}

			WriteLog(
				RuntimeLogCategory.Communication,
				"Communication status. Communication=" +
				CommunicationRuntimeNaming.FormatCommunicationName(e.CommunicationType, e.InstanceName) +
				", State=" + e.State +
				", Message=" + e.Message);
		}

		private void CommunicationRuntime_ErrorOccurred(object sender, Exception e)
		{
			if (e == null)
			{
				return;
			}

			ICommunicationRuntime runtime = sender as ICommunicationRuntime;
			string communicationName = runtime == null
				? "Communication"
				: CommunicationRuntimeNaming.FormatCommunicationName(runtime.CommunicationType, runtime.InstanceName);

			WriteLog(RuntimeLogCategory.Communication, "Communication error. Communication=" + communicationName + ", Error=" + e.Message, true);
		}

		private void CommunicationRuntime_DataReceived(object sender, CommunicationDataReceivedEventArgs e)
		{
			if (!_started || e == null)
			{
				return;
			}

			// 不要在通讯接收线程里直接执行流程，避免阻塞 TCP/PLC 接收。
			Task.Factory.StartNew(
				delegate
				{
					ProcessCommunicationData(e);
				},
				TaskCreationOptions.LongRunning);
		}

		private void ProcessCommunicationData(CommunicationDataReceivedEventArgs e)
		{
			try
			{
				string protocolName = GetProtocolName(e.CommunicationType);
				CommunicationConfig communicationConfig = CommunicationConfigStore.LoadOrCreateDefault();
				string instanceName = CommunicationRuntimeNaming.NormalizeInstanceName(protocolName, e.InstanceName, communicationConfig);
				string communicationName = CommunicationRuntimeNaming.FormatCommunicationName(protocolName, instanceName);

				// 关键修复：
				// TCP/IP 接收到 Raw="11" 这种定长字符串时，底层 e.Values 可能是空的。
				// 这里根据通讯配置里的 InputVariables 的 偏移字符(ByteOffset) + 长度(Length)
				// 把 RawText 解析成：
				// Trigger01=1
				// Pos01=1
				Dictionary<string, string> parsedValues =
					RuntimeCommunicationInputParser.BuildInputValues(protocolName, e);

				RuntimeCommunicationValueProvider valueProvider =
					new RuntimeCommunicationValueProvider(protocolName, parsedValues);

				string parsedText = FormatCommunicationLogValues(parsedValues);
				WriteLog(RuntimeLogCategory.Communication, "Communication received. Communication=" + communicationName +
					", Raw=" + e.RawText +
					", Parsed=" + parsedText);

				ProjectFlowConfig flowConfig = FlowConfigStore.LoadOrCreateDefault();
				UpdateActiveProgramByCommunication(flowConfig, protocolName, instanceName, valueProvider, communicationConfig);

				if (flowConfig == null || flowConfig.Jobs == null || flowConfig.Jobs.Count <= 0)
				{
					WriteLog(RuntimeLogCategory.Task, "No flow config was found.");
					return;
				}

				Dictionary<string, string> eventTriggerActuals =
					new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

				List<RuntimeTaskTarget> matchedTasks = FindMatchedTasks(
					flowConfig,
					protocolName,
					instanceName,
					valueProvider,
					communicationConfig,
					eventTriggerActuals);

				CommitTriggerActualValues(eventTriggerActuals);

				if (matchedTasks.Count <= 0)
				{
					WriteLog(RuntimeLogCategory.Task, "No task matched. Communication=" + communicationName + ", Raw=" + e.RawText + ", Parsed=" + parsedText);
					return;
				}

				DispatchMatchedTasks(matchedTasks, protocolName, instanceName, valueProvider, communicationConfig, e);
			}
			catch (Exception ex)
			{
				WriteLog(RuntimeLogCategory.Communication, "Process communication data failed: " + ex.Message, true);
			}
		}

		private void DispatchMatchedTasks(
			List<RuntimeTaskTarget> matchedTasks,
			string protocolName,
			string instanceName,
			RuntimeCommunicationValueProvider valueProvider,
			CommunicationConfig communicationConfig,
			CommunicationDataReceivedEventArgs commEvent)
		{
			if (matchedTasks == null || matchedTasks.Count <= 0)
			{
				return;
			}

			List<RuntimeTaskTarget> readyOnlyTargets = matchedTasks
				.Where(x => IsReadyOnlySignalTask(x, protocolName, instanceName, communicationConfig))
				.OrderBy(x => x.Task == null ? 0 : x.Task.RunOrder)
				.ToList();

			List<RuntimeTaskTarget> remainingTargets = matchedTasks
				.Where(x => !readyOnlyTargets.Contains(x))
				.OrderBy(x => x.Task == null ? 0 : x.Task.RunOrder)
				.ToList();

			Task.Factory.StartNew(
				delegate
				{
					foreach (RuntimeTaskTarget target in readyOnlyTargets)
					{
						RunOneTask(target, protocolName, valueProvider, commEvent);
					}

					foreach (RuntimeTaskTarget target in remainingTargets)
					{
						RuntimeTaskTarget localTarget = target;
						Task.Factory.StartNew(
							delegate
							{
								RunOneTask(localTarget, protocolName, valueProvider, commEvent);
							},
							TaskCreationOptions.LongRunning);
					}
				},
				TaskCreationOptions.LongRunning);
		}

		private bool IsReadyOnlySignalTask(
			RuntimeTaskTarget target,
			string protocolName,
			string instanceName,
			CommunicationConfig communicationConfig)
		{
			if (target == null || target.Task == null || target.Task.StepFlow == null)
			{
				return false;
			}

			List<StepFlowItem> enabledItems = target.Task.StepFlow
				.Where(x => x != null && x.Enabled)
				.ToList();

			if (enabledItems.Count <= 0)
			{
				return false;
			}

			foreach (StepFlowItem item in enabledItems)
			{
				if (item.IsStepBlock ||
					!string.Equals(item.BlockType, "Signal", StringComparison.OrdinalIgnoreCase))
				{
					return false;
				}
			}

			Dictionary<string, string> readyDoneValues =
				GetChannelReadyDoneValueMap(protocolName, instanceName, communicationConfig);
			bool hasReadyDoneOutput = false;

			foreach (StepFlowItem item in enabledItems)
			{
				if (item.SignalOutputs == null)
				{
					continue;
				}

				foreach (SignalOutputBinding output in item.SignalOutputs)
				{
					if (output == null || !output.Enabled || string.IsNullOrWhiteSpace(output.OutputName))
					{
						continue;
					}

					if (IsReadyDoneSignalOutput(output, readyDoneValues))
					{
						hasReadyDoneOutput = true;
						continue;
					}

					return false;
				}
			}

			return hasReadyDoneOutput;
		}

		private Dictionary<string, string> GetChannelReadyDoneValueMap(
			string protocolName,
			string instanceName,
			CommunicationConfig communicationConfig)
		{
			Dictionary<string, string> result =
				new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

			foreach (CommunicationChannelConfig channel in GetChannels(communicationConfig, protocolName, instanceName))
			{
				if (channel == null || string.IsNullOrWhiteSpace(channel.ChannelReadyOutputName))
				{
					continue;
				}

				string outputName = NormalizeConfiguredCommunicationValue(channel.ChannelReadyOutputName);
				if (string.IsNullOrWhiteSpace(outputName))
				{
					continue;
				}

				result[outputName] = string.IsNullOrWhiteSpace(channel.ChannelReadyDoneValue)
					? "1"
					: channel.ChannelReadyDoneValue.Trim();
			}

			return result;
		}

		private bool IsReadyDoneSignalOutput(
			SignalOutputBinding output,
			Dictionary<string, string> readyDoneValues)
		{
			string outputName = output.OutputName.Trim();
			string expectedValue;

			if (readyDoneValues == null ||
				!readyDoneValues.TryGetValue(outputName, out expectedValue))
			{
				if (outputName.IndexOf("Ready", StringComparison.OrdinalIgnoreCase) < 0)
				{
					return false;
				}

				expectedValue = "1";
			}

			if (!output.ForceValue)
			{
				return false;
			}

			return TriggerConditionEvaluator.CompareValue(
				output.AssignedValue,
				expectedValue,
				TriggerCompareType.Equal);
		}

		private List<RuntimeTaskTarget> FindMatchedTasks(
			ProjectFlowConfig flowConfig,
			string protocolName,
			string instanceName,
			RuntimeCommunicationValueProvider valueProvider,
			CommunicationConfig communicationConfig,
			Dictionary<string, string> eventTriggerActuals)
		{
			List<RuntimeTaskTarget> result = new List<RuntimeTaskTarget>();

			foreach (JobConfig job in flowConfig.Jobs)
			{
				if (job == null || !job.Enabled || job.Tasks == null)
				{
					continue;
				}

				foreach (TaskConfig task in job.Tasks)
				{
					if (task == null || !task.Enabled)
					{
						continue;
					}

					if (!IsTaskProtocolMatched(task, protocolName))
					{
						continue;
					}

					if (!IsTaskInstanceMatched(task, instanceName))
					{
						continue;
					}

					if (!IsJobActiveForChannel(flowConfig, job, task, protocolName))
					{
						continue;
					}

					if (!IsTaskHasTrigger(task))
					{
						continue;
					}

					bool canRun = false;

					try
					{
						canRun = CanRunTaskByChannel(task, protocolName, instanceName, valueProvider, communicationConfig, eventTriggerActuals);
					}
					catch
					{
						canRun = ManualCompareTaskCondition(task, protocolName, instanceName, valueProvider, eventTriggerActuals);
					}

					if (!canRun)
					{
						continue;
					}

					RuntimeTaskTarget target = new RuntimeTaskTarget();
					target.Job = job;
					target.Task = task;
					result.Add(target);
				}
			}

			return result
				.OrderBy(x => x.Task == null ? 0 : x.Task.RunOrder)
				.ToList();
		}

		private bool IsTaskProtocolMatched(TaskConfig task, string protocolName)
		{
			if (task == null)
			{
				return false;
			}

			if (task.CommunicationTriggerBindings != null)
			{
				foreach (TaskCommunicationTriggerBinding binding in task.CommunicationTriggerBindings)
				{
					if (binding == null)
					{
						continue;
					}

					if (IsProtocolNameMatched(binding.CommunicationProtocol, protocolName))
					{
						return true;
					}
				}
			}

			string taskProtocol = task.CommunicationProtocol == null ? string.Empty : task.CommunicationProtocol.Trim();

			if (taskProtocol.Length <= 0 ||
				taskProtocol.Equals("Not Use", StringComparison.OrdinalIgnoreCase) ||
				taskProtocol.Equals("None", StringComparison.OrdinalIgnoreCase))
			{
				return false;
			}

			return taskProtocol.Equals(protocolName, StringComparison.OrdinalIgnoreCase) ||
				   taskProtocol.Replace("/", string.Empty).Equals(protocolName.Replace("/", string.Empty), StringComparison.OrdinalIgnoreCase);
		}

		private bool IsProtocolNameMatched(string configuredProtocol, string runtimeProtocol)
		{
			if (string.IsNullOrWhiteSpace(configuredProtocol) || string.IsNullOrWhiteSpace(runtimeProtocol))
			{
				return false;
			}

			configuredProtocol = configuredProtocol.Trim();
			runtimeProtocol = runtimeProtocol.Trim();

			if (configuredProtocol.Equals("Not Use", StringComparison.OrdinalIgnoreCase) ||
				configuredProtocol.Equals("None", StringComparison.OrdinalIgnoreCase))
			{
				return false;
			}

			return configuredProtocol.Equals(runtimeProtocol, StringComparison.OrdinalIgnoreCase) ||
				configuredProtocol.Replace("/", string.Empty).Equals(runtimeProtocol.Replace("/", string.Empty), StringComparison.OrdinalIgnoreCase);
		}

		private TaskCommunicationTriggerBinding FindTriggerBinding(TaskConfig task, string protocolName, string instanceName)
		{
			if (task == null || task.CommunicationTriggerBindings == null)
			{
				return null;
			}

			foreach (TaskCommunicationTriggerBinding binding in task.CommunicationTriggerBindings)
			{
				if (binding == null || !IsProtocolNameMatched(binding.CommunicationProtocol, protocolName))
				{
					continue;
				}

				if (IsBindingInstanceMatched(binding, instanceName))
				{
					return binding;
				}
			}

			return null;
		}

		private bool IsBindingInstanceMatched(TaskCommunicationTriggerBinding binding, string instanceName)
		{
			if (binding == null)
			{
				return false;
			}

			string bindingInstance = binding.CommunicationInstanceName == null ? string.Empty : binding.CommunicationInstanceName.Trim();
			if (bindingInstance.Length <= 0 ||
				bindingInstance.Equals("Not Use", StringComparison.OrdinalIgnoreCase) ||
				bindingInstance.Equals("None", StringComparison.OrdinalIgnoreCase))
			{
				return true;
			}

			if (string.IsNullOrWhiteSpace(instanceName))
			{
				return true;
			}

			return string.Equals(bindingInstance, instanceName.Trim(), StringComparison.OrdinalIgnoreCase);
		}

		private bool IsTaskInstanceMatched(TaskConfig task, string instanceName)
		{
			if (task == null)
			{
				return false;
			}

			if (task.CommunicationTriggerBindings != null && task.CommunicationTriggerBindings.Count > 0)
			{
				foreach (TaskCommunicationTriggerBinding binding in task.CommunicationTriggerBindings)
				{
					if (binding != null && IsBindingInstanceMatched(binding, instanceName))
					{
						return true;
					}
				}

				return false;
			}

			string taskInstance = task.CommunicationInstanceName == null ? string.Empty : task.CommunicationInstanceName.Trim();
			if (taskInstance.Length <= 0 ||
				taskInstance.Equals("Not Use", StringComparison.OrdinalIgnoreCase) ||
				taskInstance.Equals("None", StringComparison.OrdinalIgnoreCase))
			{
				return true;
			}

			if (string.IsNullOrWhiteSpace(instanceName))
			{
				return true;
			}

			return string.Equals(taskInstance, instanceName.Trim(), StringComparison.OrdinalIgnoreCase);
		}

		private bool IsJobActiveForChannel(ProjectFlowConfig flowConfig, JobConfig job, TaskConfig task, string protocolName)
		{
			if (job == null)
			{
				return false;
			}

			if (task != null && !task.ProgramSwitchEnabled)
			{
				return true;
			}

			TaskCommunicationTriggerBinding binding = FindTriggerBinding(task, protocolName, string.Empty);
			string channelName = binding == null ? (task == null ? string.Empty : task.CommunicationChannel) : binding.CommunicationChannel;
			if (string.IsNullOrWhiteSpace(channelName))
			{
				channelName = job.ChannelName;
			}

			ChannelFlowConfig channel = FlowConfigStore.GetChannel(flowConfig, protocolName, channelName);
			if (channel == null)
			{
				return true;
			}

			string activeProgramNo = string.IsNullOrWhiteSpace(channel.ActiveProgramNo) ? "1" : channel.ActiveProgramNo;
			string jobProgramNo = string.IsNullOrWhiteSpace(job.ProgramNo) ? "1" : job.ProgramNo;
			return string.Equals(activeProgramNo, jobProgramNo, StringComparison.OrdinalIgnoreCase);
		}

		private void UpdateActiveProgramByCommunication(
			ProjectFlowConfig flowConfig,
			string protocolName,
			string instanceName,
			RuntimeCommunicationValueProvider valueProvider,
			CommunicationConfig communicationConfig)
		{
			if (flowConfig == null)
			{
				return;
			}

			List<CommunicationChannelConfig> channels = GetChannels(communicationConfig, protocolName, instanceName);
			string communicationName = CommunicationRuntimeNaming.FormatCommunicationName(protocolName, instanceName);

			foreach (CommunicationChannelConfig channelConfig in channels)
			{
				if (channelConfig == null || !channelConfig.Enabled)
				{
					continue;
				}

				if (string.IsNullOrWhiteSpace(NormalizeConfiguredCommunicationValue(channelConfig.ProgramSwitchEnableName)))
				{
					continue;
				}

				string switchValue = ResolveOneCommunicationDisplayValue(
					protocolName,
					valueProvider,
					channelConfig.ProgramSwitchEnableName);

				if (!IsSwitchOn(switchValue))
				{
					continue;
				}

				string programNo = ResolveOneCommunicationDisplayValue(
					protocolName,
					valueProvider,
					channelConfig.ProgramNoAddressName);

				string oldProgramNo = GetActiveProgramNo(flowConfig, protocolName, channelConfig.ChannelName);
				string finalProgramNo = oldProgramNo;
				string errorMessage;
				bool changed = false;
				bool success = false;

				if (!string.IsNullOrWhiteSpace(programNo))
				{
					SendProgramSwitchBusyOutput(protocolName, instanceName, channelConfig);
					success = TrySwitchActiveProgram(
						flowConfig,
						protocolName,
						channelConfig.ChannelName,
						programNo.Trim(),
						oldProgramNo,
						out finalProgramNo,
						out changed,
						out errorMessage);
				}
				else
				{
					errorMessage = "Program number input is empty.";
				}

				SendProgramSwitchReadyOutput(protocolName, instanceName, channelConfig, finalProgramNo);

				if (success)
				{
					WriteLog(RuntimeLogCategory.Communication,
						"Program switch completed. Communication=" + communicationName +
						", Channel=" + channelConfig.ChannelName +
						", ProgramNo=" + finalProgramNo +
						", Changed=" + changed);
				}
				else
				{
					WriteLog(RuntimeLogCategory.Communication,
						"Program switch failed. Communication=" + communicationName +
						", Channel=" + channelConfig.ChannelName +
						", RequestedProgramNo=" + (programNo ?? string.Empty) +
						", ActiveProgramNo=" + finalProgramNo +
						", Error=" + errorMessage,
						true);
				}
			}
		}

		private string GetActiveProgramNo(ProjectFlowConfig flowConfig, string protocolName, string channelName)
		{
			ChannelFlowConfig channel = FlowConfigStore.GetChannel(flowConfig, protocolName, channelName);
			if (channel == null || string.IsNullOrWhiteSpace(channel.ActiveProgramNo))
			{
				return "1";
			}

			return channel.ActiveProgramNo.Trim();
		}

		private bool TrySwitchActiveProgram(
			ProjectFlowConfig flowConfig,
			string protocolName,
			string channelName,
			string requestedProgramNo,
			string oldProgramNo,
			out string finalProgramNo,
			out bool changed,
			out string errorMessage)
		{
			finalProgramNo = string.IsNullOrWhiteSpace(oldProgramNo) ? "1" : oldProgramNo.Trim();
			changed = false;
			errorMessage = string.Empty;

			if (flowConfig == null)
			{
				errorMessage = "Flow config is empty.";
				return false;
			}

			if (string.IsNullOrWhiteSpace(requestedProgramNo))
			{
				errorMessage = "Program number input is empty.";
				return false;
			}

			lock (_syncRoot)
			{
				ChannelFlowConfig channel = FlowConfigStore.GetChannel(
					flowConfig,
					protocolName,
					channelName);

				if (channel == null)
				{
					errorMessage = "Channel flow config was not found.";
					return false;
				}

				if (channel.Jobs == null ||
					!channel.Jobs.Any(x =>
						x != null &&
						x.Enabled &&
						string.Equals(
							string.IsNullOrWhiteSpace(x.ProgramNo) ? "1" : x.ProgramNo.Trim(),
							requestedProgramNo.Trim(),
							StringComparison.OrdinalIgnoreCase)))
				{
					errorMessage = "Program number was not found in this channel.";
					return false;
				}

				string currentProgramNo = string.IsNullOrWhiteSpace(channel.ActiveProgramNo)
					? "1"
					: channel.ActiveProgramNo.Trim();

				if (string.Equals(currentProgramNo, requestedProgramNo.Trim(), StringComparison.OrdinalIgnoreCase))
				{
					finalProgramNo = currentProgramNo;
					return true;
				}

				try
				{
					channel.ActiveProgramNo = requestedProgramNo.Trim();
					FlowConfigStore.Save(flowConfig);
					finalProgramNo = requestedProgramNo.Trim();
					changed = true;
					return true;
				}
				catch (Exception ex)
				{
					channel.ActiveProgramNo = finalProgramNo;
					errorMessage = ex.Message;
					return false;
				}
			}
		}

		private void SendProgramSwitchBusyOutput(
			string protocolName,
			string instanceName,
			CommunicationChannelConfig channelConfig)
		{
			SendProgramSwitchOutputValues(
				protocolName,
				instanceName,
				channelConfig,
				string.IsNullOrWhiteSpace(channelConfig.ChannelReadyBusyValue) ? "0" : channelConfig.ChannelReadyBusyValue,
				string.Empty,
				"Program switch busy");
		}

		private void SendProgramSwitchReadyOutput(
			string protocolName,
			string instanceName,
			CommunicationChannelConfig channelConfig,
			string programNo)
		{
			SendProgramSwitchOutputValues(
				protocolName,
				instanceName,
				channelConfig,
				string.IsNullOrWhiteSpace(channelConfig.ChannelReadyDoneValue) ? "1" : channelConfig.ChannelReadyDoneValue,
				programNo,
				"Program switch ready");
		}

		private void SendProgramSwitchOutputValues(
			string protocolName,
			string instanceName,
			CommunicationChannelConfig channelConfig,
			string readyValue,
			string programNo,
			string logPrefix)
		{
			if (channelConfig == null)
			{
				return;
			}

			Dictionary<string, object> values = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
			string readyOutputName = NormalizeConfiguredCommunicationValue(channelConfig.ChannelReadyOutputName);
			string programOutputName = NormalizeConfiguredCommunicationValue(channelConfig.ProgramNoOutputName);

			if (!string.IsNullOrWhiteSpace(readyOutputName))
			{
				values[readyOutputName] = readyValue ?? string.Empty;
			}

			if (!string.IsNullOrWhiteSpace(programOutputName) && !string.IsNullOrWhiteSpace(programNo))
			{
				values[programOutputName] = programNo.Trim();
			}

			if (values.Count <= 0)
			{
				return;
			}

			bool sent = _outputService.SendConfiguredSignalOutput(protocolName, instanceName, values);
			WriteLog(
				RuntimeLogCategory.Communication,
				logPrefix + " output " + (sent ? "sent" : "failed") +
				". Communication=" + CommunicationRuntimeNaming.FormatCommunicationName(protocolName, instanceName) +
				", Channel=" + channelConfig.ChannelName +
				", Values=" + FormatObjectMap(values),
				!sent);
		}

		private bool IsSwitchOn(string value)
		{
			if (string.IsNullOrWhiteSpace(value))
			{
				return false;
			}

			return value.Equals("1", StringComparison.OrdinalIgnoreCase) ||
				value.Equals("True", StringComparison.OrdinalIgnoreCase) ||
				value.Equals("OK", StringComparison.OrdinalIgnoreCase);
		}

		private bool IsTaskHasTrigger(TaskConfig task)
		{
			if (task == null)
			{
				return false;
			}

			if (task.CommunicationTriggerBindings != null)
			{
				foreach (TaskCommunicationTriggerBinding binding in task.CommunicationTriggerBindings)
				{
					if (binding == null || string.IsNullOrWhiteSpace(binding.TriggerName))
					{
						continue;
					}

					if (!string.IsNullOrWhiteSpace(NormalizeConfiguredCommunicationValue(binding.TriggerName)))
					{
						return true;
					}
				}
			}

			if (string.IsNullOrWhiteSpace(NormalizeConfiguredCommunicationValue(task.TriggerName)))
			{
				return false;
			}

			return true;
		}

		private string NormalizeConfiguredCommunicationValue(string value)
		{
			if (string.IsNullOrWhiteSpace(value))
			{
				return string.Empty;
			}

			string text = value.Trim();
			if (text.Equals("Not Use", StringComparison.OrdinalIgnoreCase) ||
				text.Equals("None", StringComparison.OrdinalIgnoreCase) ||
				text.Equals("Select", StringComparison.OrdinalIgnoreCase) ||
				text.Equals("Select...", StringComparison.OrdinalIgnoreCase) ||
				text.Equals("选择", StringComparison.OrdinalIgnoreCase) ||
				text.Equals("选择...", StringComparison.OrdinalIgnoreCase))
			{
				return string.Empty;
			}

			return text;
		}

		private bool TryParseTriggerOption(string triggerOption, out string triggerName, out string expectedValue)
		{
			triggerName = NormalizeConfiguredCommunicationValue(triggerOption);
			expectedValue = string.Empty;

			if (string.IsNullOrWhiteSpace(triggerName))
			{
				return false;
			}

			int index = triggerName.LastIndexOf('=');
			if (index <= 0)
			{
				return false;
			}

			expectedValue = triggerName.Substring(index + 1).Trim();
			triggerName = triggerName.Substring(0, index).Trim();
			return !string.IsNullOrWhiteSpace(triggerName);
		}

		private CommunicationCustomTriggerOption FindCustomTriggerOption(
			CommunicationChannelConfig channel,
			string triggerName,
			string expectedValue)
		{
			if (channel == null ||
				channel.CustomTriggers == null ||
				string.IsNullOrWhiteSpace(triggerName))
			{
				return null;
			}

			CommunicationCustomTriggerOption nameOnlyMatch = null;
			foreach (CommunicationCustomTriggerOption option in channel.CustomTriggers)
			{
				if (option == null ||
					string.IsNullOrWhiteSpace(option.Name) ||
					!string.Equals(triggerName.Trim(), option.Name.Trim(), StringComparison.OrdinalIgnoreCase))
				{
					continue;
				}

				if (nameOnlyMatch == null)
				{
					nameOnlyMatch = option;
				}

				if (string.IsNullOrWhiteSpace(expectedValue) ||
					string.Equals((option.ExpectedValue ?? string.Empty).Trim(), expectedValue.Trim(), StringComparison.OrdinalIgnoreCase))
				{
					return option;
				}
			}

			return nameOnlyMatch;
		}

		private bool CanRunTaskByChannel(
			TaskConfig task,
			string protocolName,
			string instanceName,
			RuntimeCommunicationValueProvider valueProvider,
			CommunicationConfig communicationConfig,
			Dictionary<string, string> eventTriggerActuals)
		{
			TaskCommunicationTriggerBinding binding = FindTriggerBinding(task, protocolName, instanceName);
			string channelName = binding == null
				? (task == null ? string.Empty : task.CommunicationChannel)
				: binding.CommunicationChannel;

			CommunicationChannelConfig channel = FindChannelConfig(
				communicationConfig,
				protocolName,
				instanceName,
				channelName);

			if (channel == null)
			{
				return ManualCompareTaskCondition(task, protocolName, instanceName, valueProvider, eventTriggerActuals);
			}

			if (!channel.Enabled)
			{
				return false;
			}

			string selectedTriggerName = binding == null
				? (task == null ? string.Empty : task.TriggerName)
				: binding.TriggerName;
			string selectedTrigger;
			string parsedTriggerExpectedValue;
			if (!TryParseTriggerOption(selectedTriggerName, out selectedTrigger, out parsedTriggerExpectedValue))
			{
				selectedTrigger = NormalizeConfiguredCommunicationValue(selectedTriggerName);
				parsedTriggerExpectedValue = string.Empty;
			}
			string triggerGlobalVariableName = NormalizeConfiguredCommunicationValue(channel.TriggerGlobalVariableName);
			string triggerSourceName = NormalizeConfiguredCommunicationValue(channel.TriggerName);
			string triggerExpectedValue = channel.TriggerExpectedValue;
			string customTriggerGlobalVariableName = NormalizeConfiguredCommunicationValue(channel.CustomTriggerGlobalVariableName);
			string configuredTriggerValue = parsedTriggerExpectedValue;
			if (string.IsNullOrWhiteSpace(configuredTriggerValue))
			{
				configuredTriggerValue = binding == null
					? (task == null ? string.Empty : task.TriggerValue)
					: binding.TriggerValue;
			}

			if (string.IsNullOrWhiteSpace(selectedTrigger))
			{
				return false;
			}

			if (selectedTrigger.Equals("Trigger", StringComparison.OrdinalIgnoreCase) &&
				string.IsNullOrWhiteSpace(triggerGlobalVariableName) &&
				string.IsNullOrWhiteSpace(customTriggerGlobalVariableName) &&
				(channel.CustomTriggers == null || channel.CustomTriggers.Count <= 0))
			{
				return false;
			}

			CommunicationCustomTriggerOption customTriggerOption =
				FindCustomTriggerOption(channel, selectedTrigger, configuredTriggerValue);

			if (customTriggerOption != null)
			{
				triggerGlobalVariableName = string.Empty;
				triggerSourceName = customTriggerOption.Name;
				triggerExpectedValue = string.IsNullOrWhiteSpace(configuredTriggerValue)
					? customTriggerOption.ExpectedValue
					: configuredTriggerValue;
			}
			else if (!string.IsNullOrWhiteSpace(customTriggerGlobalVariableName) &&
				string.Equals(selectedTrigger, customTriggerGlobalVariableName, StringComparison.OrdinalIgnoreCase))
			{
				triggerGlobalVariableName = customTriggerGlobalVariableName;
				triggerSourceName = customTriggerGlobalVariableName;
				triggerExpectedValue = string.IsNullOrWhiteSpace(configuredTriggerValue)
					? channel.CustomTriggerExpectedValue
					: configuredTriggerValue;
			}
			else if (!string.IsNullOrWhiteSpace(selectedTrigger) &&
				!string.Equals(selectedTrigger, triggerGlobalVariableName, StringComparison.OrdinalIgnoreCase) &&
				!string.Equals(selectedTrigger, triggerSourceName, StringComparison.OrdinalIgnoreCase))
			{
				triggerGlobalVariableName = string.Empty;
				triggerSourceName = selectedTrigger;
				triggerExpectedValue = string.IsNullOrWhiteSpace(configuredTriggerValue) ? "1" : configuredTriggerValue;
			}

			string triggerActual = GetChannelRuntimeValue(protocolName, triggerSourceName, triggerGlobalVariableName, valueProvider);
			string triggerKey = BuildTriggerActualKey(protocolName, instanceName, triggerSourceName, triggerGlobalVariableName);
			TrackCurrentTriggerActual(eventTriggerActuals, triggerKey, triggerActual);

			bool triggerOk = TriggerConditionEvaluator.CompareValue(
				triggerActual,
				triggerExpectedValue,
				TriggerCompareType.Equal);

			if (!triggerOk)
			{
				return false;
			}

			TriggerRunMode runMode = binding == null
				? (task == null ? TriggerRunMode.OnReceive : task.TriggerRunMode)
				: binding.TriggerRunMode;

			if (!IsTriggerRunModeMatched(runMode, triggerKey, triggerActual))
			{
				return false;
			}

			return TriggerConditionEvaluator.AreExecutionConditionsMatched(
				GetExecutionConditions(task, binding));
		}

		private string BuildTriggerActualKey(
			string protocolName,
			string instanceName,
			string triggerSourceName,
			string triggerGlobalVariableName)
		{
			string sourceName = NormalizeConfiguredCommunicationValue(triggerGlobalVariableName);
			string sourceKind = "Global";

			if (string.IsNullOrWhiteSpace(sourceName))
			{
				sourceName = NormalizeConfiguredCommunicationValue(triggerSourceName);
				sourceKind = "Input";
			}

			if (string.IsNullOrWhiteSpace(sourceName))
			{
				return string.Empty;
			}

			return CommunicationRuntimeNaming.FormatCommunicationName(protocolName, instanceName) +
				"|" + sourceKind +
				"|" + sourceName;
		}

		private void TrackCurrentTriggerActual(
			Dictionary<string, string> eventTriggerActuals,
			string triggerKey,
			string triggerActual)
		{
			if (eventTriggerActuals == null || string.IsNullOrWhiteSpace(triggerKey))
			{
				return;
			}

			eventTriggerActuals[triggerKey] = triggerActual == null ? string.Empty : triggerActual.Trim();
		}

		private bool IsTriggerRunModeMatched(
			TriggerRunMode runMode,
			string triggerKey,
			string triggerActual)
		{
			if (runMode != TriggerRunMode.OnChanged)
			{
				return true;
			}

			if (string.IsNullOrWhiteSpace(triggerKey))
			{
				return true;
			}

			string currentValue = triggerActual == null ? string.Empty : triggerActual.Trim();
			lock (_syncRoot)
			{
				string previousValue;
				if (!_lastTriggerActualValuesByKey.TryGetValue(triggerKey, out previousValue))
				{
					return true;
				}

				return !string.Equals(previousValue, currentValue, StringComparison.OrdinalIgnoreCase);
			}
		}

		private void CommitTriggerActualValues(Dictionary<string, string> eventTriggerActuals)
		{
			if (eventTriggerActuals == null || eventTriggerActuals.Count <= 0)
			{
				return;
			}

			lock (_syncRoot)
			{
				foreach (KeyValuePair<string, string> pair in eventTriggerActuals)
				{
					if (string.IsNullOrWhiteSpace(pair.Key))
					{
						continue;
					}

					_lastTriggerActualValuesByKey[pair.Key] = pair.Value == null ? string.Empty : pair.Value.Trim();
				}
			}
		}

		private string GetChannelRuntimeValue(
			string protocolName,
			string sourceName,
			string globalVariableName,
			RuntimeCommunicationValueProvider valueProvider)
		{
			if (!string.IsNullOrWhiteSpace(globalVariableName))
			{
				return GlobalVariableStore.GetValueText(globalVariableName);
			}

			return valueProvider.GetInputValue(protocolName, sourceName);
		}

		private List<TaskExecutionCondition> GetExecutionConditions(
			TaskConfig task,
			TaskCommunicationTriggerBinding binding)
		{
			if (binding != null && binding.ExecutionConditions != null && binding.ExecutionConditions.Count > 0)
			{
				return binding.ExecutionConditions;
			}

			return task == null ? null : task.ExecutionConditions;
		}

		private CommunicationChannelConfig FindChannelConfig(
			CommunicationConfig config,
			string protocolName,
			string instanceName,
			string channelName)
		{
			List<CommunicationChannelConfig> channels = GetChannels(config, protocolName, instanceName);
			if (channels == null || channels.Count <= 0)
			{
				return null;
			}

			if (string.IsNullOrWhiteSpace(channelName))
			{
				channelName = "Channel01";
			}

			CommunicationChannelConfig channel = channels.FirstOrDefault(x =>
				x != null && string.Equals(x.ChannelName, channelName, StringComparison.OrdinalIgnoreCase));

			return channel ?? channels.FirstOrDefault(x => x != null && x.Enabled);
		}

		private List<CommunicationChannelConfig> GetChannels(CommunicationConfig config, string protocolName, string instanceName)
		{
			if (config == null)
			{
				return new List<CommunicationChannelConfig>();
			}

			CommunicationInstanceConfig instance =
				CommunicationRuntimeNaming.FindInstance(config, protocolName, instanceName);

			if (protocolName.Equals("TCP/IP", StringComparison.OrdinalIgnoreCase) ||
				protocolName.Equals("TcpIp", StringComparison.OrdinalIgnoreCase))
			{
				if (instance != null && instance.TcpIp != null && instance.TcpIp.Channels != null)
				{
					return instance.TcpIp.Channels;
				}

				return config.TcpIp == null ? new List<CommunicationChannelConfig>() : config.TcpIp.Channels;
			}

			if (protocolName.Equals("Profinet", StringComparison.OrdinalIgnoreCase))
			{
				return config.Profinet == null ? new List<CommunicationChannelConfig>() : config.Profinet.Channels;
			}

			if (protocolName.Equals("S7", StringComparison.OrdinalIgnoreCase))
			{
				if (instance != null && instance.S7 != null && instance.S7.Channels != null)
				{
					return instance.S7.Channels;
				}

				return config.S7 == null ? new List<CommunicationChannelConfig>() : config.S7.Channels;
			}

			return new List<CommunicationChannelConfig>();
		}

		private bool ManualCompareTaskCondition(
			TaskConfig task,
			string protocolName,
			string instanceName,
			RuntimeCommunicationValueProvider valueProvider,
			Dictionary<string, string> eventTriggerActuals)
		{
			TaskCommunicationTriggerBinding binding = FindTriggerBinding(task, protocolName, instanceName);
			string triggerName = binding == null ? task.TriggerName : binding.TriggerName;
			string triggerValue = binding == null ? task.TriggerValue : binding.TriggerValue;
			string parsedTriggerName;
			string parsedTriggerValue;
			if (TryParseTriggerOption(triggerName, out parsedTriggerName, out parsedTriggerValue))
			{
				triggerName = parsedTriggerName;
				triggerValue = parsedTriggerValue;
			}
			TriggerCompareType triggerCompare = binding == null ? task.TriggerCompare : binding.TriggerCompare;
			string triggerActual = valueProvider.GetInputValue(protocolName, triggerName);
			string triggerKey = BuildTriggerActualKey(protocolName, instanceName, triggerName, string.Empty);
			TrackCurrentTriggerActual(eventTriggerActuals, triggerKey, triggerActual);
			bool triggerOk = TriggerConditionEvaluator.CompareValue(
				triggerActual,
				triggerValue,
				triggerCompare);

			if (!triggerOk)
			{
				return false;
			}

			TriggerRunMode runMode = binding == null ? task.TriggerRunMode : binding.TriggerRunMode;
			if (!IsTriggerRunModeMatched(runMode, triggerKey, triggerActual))
			{
				return false;
			}

			return TriggerConditionEvaluator.AreExecutionConditionsMatched(
				GetExecutionConditions(task, binding));
		}

		private void RunOneTask(
			RuntimeTaskTarget target,
			string protocolName,
			RuntimeCommunicationValueProvider valueProvider,
			CommunicationDataReceivedEventArgs commEvent)
		{
			if (target == null || target.Job == null || target.Task == null)
			{
				return;
			}

			string jobName = target.Job.JobName;
			string taskName = target.Task.TaskName;
			string taskKey = jobName + "/" + taskName;
			TaskCommunicationTriggerBinding binding = FindTriggerBinding(
				target.Task,
				protocolName,
				commEvent == null ? string.Empty : commEvent.InstanceName);
			string triggerName = binding == null ? target.Task.TriggerName : binding.TriggerName;
			string channelName = binding == null ? target.Task.CommunicationChannel : binding.CommunicationChannel;
			string positionOptionName = binding == null ? target.Task.PositionOptionName : binding.PositionName;
			string communicationName = CommunicationRuntimeNaming.FormatCommunicationName(
				protocolName,
				commEvent == null ? target.Task.CommunicationInstanceName : commEvent.InstanceName);

			if (!EnterTask(taskKey))
			{
				WriteLog(RuntimeLogCategory.Task, "Task is already running, ignored duplicate trigger. " + taskKey + ", Communication=" + communicationName);
				return;
			}

			DateTime startTime = DateTime.Now;
			StepResult finalResult = StepResult.NG("Task was not executed.");
				VisionRunContext context = new VisionRunContext();

			try
			{
				context.JobName = jobName;
				context.TaskName = taskName;
				context.TriggerName = triggerName;
				context.SetData("Comm.Channel", channelName);
				context.SetData("ProgramNo", target.Job.ProgramNo);
				context.SetData("JobID", target.Job.ProgramNo);
				context.SetData("Task.PositionOptionName", positionOptionName);

				WriteLog(RuntimeLogCategory.Task, "Task started. Job=" + jobName + ", Task=" + taskName + ", Communication=" + communicationName);

				FillCommunicationInputToContext(context, protocolName, commEvent, valueProvider.Values);
				PublishTaskDisplayInfo(jobName, target.Task, protocolName, valueProvider);

				// 1. 根据 Task 的图像源先并行取像。
				AcquireTaskImages(jobName, target.Task, context);

				// 2. 按 StepFlow 的 RunOrder 执行。
				//    你已有 TaskRunner，它内部已经实现：
				//    RunOrder 相同的 Step 并行执行，全部完成后再执行下一个 RunOrder。
				TaskRunner runner = new TaskRunner();
				finalResult = runner.Run(target.Task, context);

				WriteLog(RuntimeLogCategory.Task, "Task finished. Job=" + jobName +
					", Task=" + taskName +
					", Communication=" + communicationName +
					", Cost=" + (DateTime.Now - startTime).TotalMilliseconds.ToString("0.0") + " ms");
			}
			catch (Exception ex)
			{
				finalResult = StepResult.NG(ex.Message);

				WriteLog(RuntimeLogCategory.Task, "Task failed. Job=" + jobName +
					", Task=" + taskName +
					", Communication=" + communicationName +
					", Error=" + ex.Message +
					", Cost=" + (DateTime.Now - startTime).TotalMilliseconds.ToString("0.0") + " ms",
					true);
			}
			finally
			{
				LeaveTask(taskKey);

				RuntimeTaskFinishedEventArgs args = new RuntimeTaskFinishedEventArgs(
					jobName,
					taskName,
					finalResult,
					context,
					DateTime.Now - startTime);

				OnTaskFinished(args);
			}
		}

		private bool EnterTask(string taskKey)
		{
			lock (_syncRoot)
			{
				if (_runningTaskKeys.Contains(taskKey))
				{
					return false;
				}

				_runningTaskKeys.Add(taskKey);
				return true;
			}
		}

		private void LeaveTask(string taskKey)
		{
			lock (_syncRoot)
			{
				if (_runningTaskKeys.Contains(taskKey))
				{
					_runningTaskKeys.Remove(taskKey);
				}
			}
		}

		private bool EnterImageAcquire(string acquireKey)
		{
			lock (_syncRoot)
			{
				if (_runningImageAcquireKeys.Contains(acquireKey))
				{
					return false;
				}

				_runningImageAcquireKeys.Add(acquireKey);
				return true;
			}
		}

		private void LeaveImageAcquire(string acquireKey)
		{
			lock (_syncRoot)
			{
				if (_runningImageAcquireKeys.Contains(acquireKey))
				{
					_runningImageAcquireKeys.Remove(acquireKey);
				}
			}
		}

		private void FillCommunicationInputToContext(
			VisionRunContext context,
			string protocolName,
			CommunicationDataReceivedEventArgs commEvent,
			Dictionary<string, string> parsedValues)
		{
			if (context == null || commEvent == null)
			{
				return;
			}

			context.SetData("Comm.Protocol", protocolName);
			context.SetData("Comm.InstanceName", commEvent.InstanceName);
			context.SetData("Comm.Name", CommunicationRuntimeNaming.FormatCommunicationName(protocolName, commEvent.InstanceName));
			context.SetData("Comm.RawText", commEvent.RawText);
			context.SetData("Comm.RawBytes", commEvent.RawBytes);
			context.SetData("Comm.RawHex", TcpIpPayloadCodec.ToHexString(commEvent.RawBytes));
			context.SetData("Comm.ReceiveTime", commEvent.ReceiveTime);

			if (parsedValues == null)
			{
				return;
			}

			foreach (KeyValuePair<string, string> pair in parsedValues)
			{
				context.SetData("Comm." + pair.Key, pair.Value);
				context.SetData(protocolName + "." + pair.Key, pair.Value);
				context.SetData(pair.Key, pair.Value);
			}
		}

		private void PublishTaskDisplayInfo(
			string jobName,
			TaskConfig task,
			string protocolName,
			RuntimeCommunicationValueProvider valueProvider)
		{
			if (task == null || task.StepFlow == null)
			{
				return;
			}

			List<string> displaySlots = task.StepFlow
				.Where(x => x != null &&
					!string.IsNullOrWhiteSpace(x.DisplaySlotName) &&
					!x.DisplaySlotName.Equals("Not Show", StringComparison.OrdinalIgnoreCase))
				.Select(x => x.DisplaySlotName)
				.Distinct(StringComparer.OrdinalIgnoreCase)
				.ToList();

			if (displaySlots.Count <= 0)
			{
				return;
			}

			CommunicationConfig communicationConfig = CommunicationConfigStore.LoadOrCreateDefault();
			TaskCommunicationTriggerBinding binding = FindTriggerBinding(task, protocolName, string.Empty);
			string channelNameForDisplay = binding == null ? task.CommunicationChannel : binding.CommunicationChannel;
			CommunicationChannelConfig channel = FindChannelConfig(
				communicationConfig,
				protocolName,
				task == null ? string.Empty : task.CommunicationInstanceName,
				channelNameForDisplay);

			string programNo = ResolveCommunicationDisplayValue(
				protocolName,
				valueProvider,
				channel == null ? string.Empty : channel.ProgramNoAddressName,
				"JobID",
				"JobID0",
				"JobID_0",
				"ProgramNo",
				"ProgramID");

			string positionNo = ResolveCommunicationDisplayValue(
				protocolName,
				valueProvider,
				channel == null ? string.Empty : channel.PositionGlobalVariableName,
				channel == null ? string.Empty : channel.PositionSourceName,
				task.PositionName,
				task.PositionOptionName,
				"PosID",
				"PosID0",
				"PosID_0",
				"PositionCode");

			string channelName = channelNameForDisplay;
			if (string.IsNullOrWhiteSpace(channelName) && channel != null)
			{
				channelName = channel.ChannelName;
			}

			foreach (string displaySlot in displaySlots)
			{
				DisplayRuntimeManager.UpdateInfo(
					displaySlot,
					jobName,
					programNo,
					positionNo,
					channelName);
			}
		}

		private string ResolveCommunicationDisplayValue(
			string protocolName,
			RuntimeCommunicationValueProvider valueProvider,
			params string[] candidates)
		{
			if (candidates == null)
			{
				return string.Empty;
			}

			foreach (string candidate in candidates)
			{
				string value = ResolveOneCommunicationDisplayValue(protocolName, valueProvider, candidate);
				if (!string.IsNullOrWhiteSpace(value))
				{
					return value;
				}
			}

			return string.Empty;
		}

		private string ResolveOneCommunicationDisplayValue(
			string protocolName,
			RuntimeCommunicationValueProvider valueProvider,
			string candidate)
		{
			if (string.IsNullOrWhiteSpace(candidate))
			{
				return string.Empty;
			}

			string value = GlobalVariableStore.GetValueText(candidate);
			if (!string.IsNullOrWhiteSpace(value))
			{
				return value;
			}

			if (valueProvider != null)
			{
				value = valueProvider.GetInputValue(protocolName, candidate);
				if (!string.IsNullOrWhiteSpace(value))
				{
					return value;
				}
			}

			return string.Empty;
		}

		private void AcquireTaskImages(string jobName, TaskConfig taskConfig, VisionRunContext context)
		{
			List<string> imageSourceKeys = RuntimeImageSourceParser.SplitImageSources(taskConfig.ImageSourceKey);

			if (imageSourceKeys.Count <= 0)
			{
				WriteLog(RuntimeLogCategory.Step, "No image source configured. Task=" + taskConfig.TaskName);
				return;
			}

			List<Task<RuntimeImageAcquireResult>> acquireTasks = new List<Task<RuntimeImageAcquireResult>>();
			List<string> acquireSourceKeys = new List<string>();

			foreach (string sourceKey in imageSourceKeys)
			{
				string localSourceKey = sourceKey;
				string acquireKey = jobName + "/" + localSourceKey;

				RuntimeImageAcquireResult precheck = _imageAcquireService.Precheck(jobName, localSourceKey);
				if (precheck != null && !precheck.Success)
				{
					WriteLog(RuntimeLogCategory.Step, "Image acquire skipped. Source=" + localSourceKey + ", Reason=" + precheck.Message, true);
					throw new Exception("Image acquire skipped. Source=" + localSourceKey + ", Error=" + precheck.Message);
				}

				if (!EnterImageAcquire(acquireKey))
				{
					string busyMessage = "Previous image acquire is still running. Source=" + localSourceKey;
					WriteLog(RuntimeLogCategory.Step, busyMessage, true);
					throw new TimeoutException(busyMessage);
				}

				WriteLog(RuntimeLogCategory.Step, "Image acquire started. Source=" + localSourceKey);

				Task<RuntimeImageAcquireResult> task = Task<RuntimeImageAcquireResult>.Factory.StartNew(
					delegate
					{
						try
						{
							return _imageAcquireService.Acquire(jobName, localSourceKey);
						}
						finally
						{
							LeaveImageAcquire(acquireKey);
						}
					},
					System.Threading.CancellationToken.None,
					TaskCreationOptions.LongRunning,
					TaskScheduler.Default);

				acquireTasks.Add(task);
				acquireSourceKeys.Add(localSourceKey);
			}

			bool allCompleted;
			try
			{
				allCompleted = Task.WaitAll(acquireTasks.ToArray(), ImageAcquireTimeoutMs);
			}
			catch (AggregateException ex)
			{
				Exception inner = ex.Flatten().InnerExceptions.FirstOrDefault();
				string message = inner == null ? ex.Message : inner.Message;
				WriteLog(RuntimeLogCategory.Step, "Image acquire failed: " + message, true);
				throw new Exception("Image acquire failed: " + message);
			}

			if (!allCompleted)
			{
				List<string> timeoutSources = new List<string>();

				for (int i = 0; i < acquireTasks.Count; i++)
				{
					if (!acquireTasks[i].IsCompleted)
					{
						timeoutSources.Add(acquireSourceKeys[i]);
					}
				}

				string timeoutMessage = "Image acquire timeout after " + ImageAcquireTimeoutMs + " ms. Source=" + string.Join(";", timeoutSources.ToArray());
				WriteLog(RuntimeLogCategory.Step, timeoutMessage, true);
				throw new TimeoutException(timeoutMessage);
			}

			foreach (Task<RuntimeImageAcquireResult> acquireTask in acquireTasks)
			{
				RuntimeImageAcquireResult result = acquireTask.Result;

				if (result == null)
				{
					continue;
				}

				if (!result.Success)
				{
					throw new Exception("Image acquire failed. Source=" + result.SourceKey + ", Error=" + result.Message);
				}

				if (result.Image != null)
				{
					context.SetImage(result.SourceKey, result.Image);
					context.SetData(result.SourceKey, result.Image.RawImage);
					context.SetData(result.SourceKey + ".RawImage", result.Image.RawImage);

					if (!string.IsNullOrWhiteSpace(result.Image.OutputImageKey))
					{
						context.SetImage(result.Image.OutputImageKey, result.Image);
						context.SetData(result.Image.OutputImageKey, result.Image.RawImage);
					}
				}

				WriteLog(RuntimeLogCategory.Step, "Image acquired. Source=" + result.SourceKey);
			}
		}

		private string GetProtocolName(CommunicationType type)
		{
			return CommunicationRuntimeNaming.GetProtocolName(type);
		}

		private void WriteLog(string message)
		{
			WriteLog(RuntimeLogStore.Classify(message), message);
		}

		private void WriteLog(RuntimeLogCategory category, string message)
		{
			WriteLog(category, message, RuntimeLogStore.IsErrorMessage(message));
		}

		private void WriteLog(RuntimeLogCategory category, string message, bool isError)
		{
			RuntimeFlowLogEventArgs args = new RuntimeFlowLogEventArgs(DateTime.Now, category, message, isError);
			RuntimeLogStore.Append(args.Time, args.Category, args.Message, args.IsError);
			EventHandler<RuntimeFlowLogEventArgs> handler = LogGenerated;

			if (handler != null)
			{
				handler(this, args);
			}
		}

		private string FormatObjectMap(Dictionary<string, object> values)
		{
			if (values == null || values.Count <= 0)
			{
				return "{}";
			}

			List<string> parts = new List<string>();
			foreach (KeyValuePair<string, object> pair in values)
			{
				parts.Add(pair.Key + "=" + Convert.ToString(pair.Value));
			}

			return "{" + string.Join(", ", parts.ToArray()) + "}";
		}

		private string FormatCommunicationLogValues(Dictionary<string, string> values)
		{
			if (values == null || values.Count <= 0)
			{
				return "{}";
			}

			Dictionary<string, string> filtered = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

			foreach (KeyValuePair<string, string> pair in values)
			{
				if (string.Equals(pair.Key, "Raw", StringComparison.OrdinalIgnoreCase) ||
					string.Equals(pair.Key, "RawText", StringComparison.OrdinalIgnoreCase) ||
					string.Equals(pair.Key, "RawHex", StringComparison.OrdinalIgnoreCase))
				{
					continue;
				}

				filtered[pair.Key] = pair.Value;
			}

			return RuntimeCommunicationInputParser.FormatValues(filtered);
		}

		private void OnTaskFinished(RuntimeTaskFinishedEventArgs e)
		{
			EventHandler<RuntimeTaskFinishedEventArgs> handler = TaskFinished;

			if (handler != null)
			{
				handler(this, e);
			}
		}

		public void Dispose()
		{
			if (_disposed)
			{
				return;
			}

			_disposed = true;
			Stop();
		}
	}

	public class RuntimeTaskTarget
	{
		public JobConfig Job { get; set; }
		public TaskConfig Task { get; set; }
	}

	public class RuntimeFlowLogEventArgs : EventArgs
	{
		public string Message { get; private set; }
		public DateTime Time { get; private set; }
		public RuntimeLogCategory Category { get; private set; }
		public bool IsError { get; private set; }

		public RuntimeFlowLogEventArgs(string message)
		{
			Message = message ?? string.Empty;
			Time = DateTime.Now;
			Category = RuntimeLogStore.Classify(Message);
			IsError = RuntimeLogStore.IsErrorMessage(Message);
		}

		public RuntimeFlowLogEventArgs(DateTime time, RuntimeLogCategory category, string message)
			: this(time, category, message, RuntimeLogStore.IsErrorMessage(message))
		{
		}

		public RuntimeFlowLogEventArgs(DateTime time, RuntimeLogCategory category, string message, bool isError)
		{
			Time = time;
			Category = category;
			Message = message ?? string.Empty;
			IsError = isError;
		}
	}

	public class RuntimeTaskFinishedEventArgs : EventArgs
	{
		public string JobName { get; private set; }
		public string TaskName { get; private set; }
		public StepResult FinalResult { get; private set; }
		public VisionRunContext Context { get; private set; }
		public TimeSpan Cost { get; private set; }

		public RuntimeTaskFinishedEventArgs(
			string jobName,
			string taskName,
			StepResult finalResult,
			VisionRunContext context,
			TimeSpan cost)
		{
			JobName = jobName ?? string.Empty;
			TaskName = taskName ?? string.Empty;
			FinalResult = finalResult;
			Context = context;
			Cost = cost;
		}
	}

	public static class RuntimeCommunicationInputParser
	{
		public static Dictionary<string, string> BuildInputValues(
			string protocolName,
			CommunicationDataReceivedEventArgs e)
		{
			Dictionary<string, string> result =
				new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

			if (e == null)
			{
				return result;
			}

			// 先保留底层通讯类已经解析出的值。
			if (e.Values != null)
			{
				foreach (KeyValuePair<string, string> pair in e.Values)
				{
					if (!result.ContainsKey(pair.Key))
					{
						result.Add(pair.Key, pair.Value);
					}
					else
					{
						result[pair.Key] = pair.Value;
					}
				}
			}

			string rawText = e.RawText == null ? string.Empty : e.RawText;
			string rawHex = TcpIpPayloadCodec.ToHexString(e.RawBytes);

			if (!result.ContainsKey("Raw"))
			{
				result.Add("Raw", rawText);
			}

			if (!result.ContainsKey("RawText"))
			{
				result.Add("RawText", rawText);
			}

			if (!result.ContainsKey("RawHex"))
			{
				result.Add("RawHex", rawHex);
			}

			CommunicationConfig config = CommunicationConfigStore.LoadOrCreateDefault();
			string instanceName = CommunicationRuntimeNaming.NormalizeInstanceName(protocolName, e.InstanceName, config);
			List<CommInputVariable> variables = GetInputVariables(protocolName, instanceName, config);

			foreach (CommInputVariable item in variables)
			{
				if (item == null || string.IsNullOrWhiteSpace(item.Name))
				{
					continue;
				}

				string parsedValue = ParseInputVariable(protocolName, instanceName, e, item, config);

				if (!result.ContainsKey(item.Name))
				{
					result.Add(item.Name, parsedValue);
				}
				else
				{
					result[item.Name] = parsedValue;
				}

				if (!string.IsNullOrWhiteSpace(item.GlobalVariableName))
				{
					GlobalVariableStore.SetValue(item.GlobalVariableName, parsedValue);
				}
			}

			return result;
		}

		private static List<CommInputVariable> GetInputVariables(
			string protocolName,
			string instanceName,
			CommunicationConfig config)
		{
			if (config == null)
			{
				return new List<CommInputVariable>();
			}

			CommunicationInstanceConfig instance =
				CommunicationRuntimeNaming.FindInstance(config, protocolName, instanceName);

			if (protocolName.Equals("TCP/IP", StringComparison.OrdinalIgnoreCase) ||
				protocolName.Equals("TcpIp", StringComparison.OrdinalIgnoreCase))
			{
				if (instance != null && instance.TcpIp != null && instance.TcpIp.InputVariables != null)
				{
					return instance.TcpIp.InputVariables;
				}

				return config.TcpIp == null || config.TcpIp.InputVariables == null
					? new List<CommInputVariable>()
					: config.TcpIp.InputVariables;
			}

			if (protocolName.Equals("Profinet", StringComparison.OrdinalIgnoreCase))
			{
				return config.Profinet == null || config.Profinet.InputVariables == null
					? new List<CommInputVariable>()
					: config.Profinet.InputVariables;
			}

			if (protocolName.Equals("S7", StringComparison.OrdinalIgnoreCase))
			{
				if (instance != null && instance.S7 != null && instance.S7.InputVariables != null)
				{
					return instance.S7.InputVariables;
				}

				return config.S7 == null || config.S7.InputVariables == null
					? new List<CommInputVariable>()
					: config.S7.InputVariables;
			}

			return new List<CommInputVariable>();
		}

		private static string ParseInputVariable(
			string protocolName,
			string instanceName,
			CommunicationDataReceivedEventArgs e,
			CommInputVariable item,
			CommunicationConfig config)
		{
			CommunicationInstanceConfig instance =
				CommunicationRuntimeNaming.FindInstance(config, protocolName, instanceName);
			TcpIpConfig tcpConfig = instance != null && instance.TcpIp != null
				? instance.TcpIp
				: config == null ? null : config.TcpIp;

			if (protocolName != null &&
				(protocolName.Equals("TCP/IP", StringComparison.OrdinalIgnoreCase) ||
				 protocolName.Equals("TcpIp", StringComparison.OrdinalIgnoreCase)) &&
				TcpIpPayloadCodec.IsByteMode(tcpConfig))
			{
				return TcpIpPayloadCodec.ParseInputVariableFromBytes(
					e == null ? null : e.RawBytes,
					item,
					tcpConfig.ByteOrder);
			}

			string rawText = e == null || e.RawText == null ? string.Empty : e.RawText;

			if (rawText == null)
			{
				rawText = string.Empty;
			}

			return TcpIpPayloadCodec.ParseInputVariableFromString(rawText, item);
		}

		public static string FormatValues(Dictionary<string, string> values)
		{
			if (values == null || values.Count <= 0)
			{
				return "{}";
			}

			List<string> parts = new List<string>();

			foreach (KeyValuePair<string, string> pair in values)
			{
				parts.Add(pair.Key + "=" + pair.Value);
			}

			return "{" + string.Join(", ", parts.ToArray()) + "}";
		}
	}

	public class RuntimeCommunicationValueProvider : ICommunicationRuntimeValueProvider
	{
		private readonly string _protocol;
		private readonly Dictionary<string, string> _values;

		public Dictionary<string, string> Values
		{
			get { return _values; }
		}

		public RuntimeCommunicationValueProvider(string protocol, Dictionary<string, string> values)
		{
			_protocol = protocol ?? string.Empty;
			_values = values ?? new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
		}

		public string GetInputValue(string protocol, string tagName)
		{
			if (string.IsNullOrWhiteSpace(tagName))
			{
				return string.Empty;
			}

			string value;

			if (_values.TryGetValue(tagName, out value))
			{
				return value;
			}

			string key1 = protocol + "." + tagName;
			if (_values.TryGetValue(key1, out value))
			{
				return value;
			}

			string key2 = _protocol + "." + tagName;
			if (_values.TryGetValue(key2, out value))
			{
				return value;
			}

			return string.Empty;
		}
	}

	public static class RuntimeImageSourceParser
	{
		public static List<string> SplitImageSources(string value)
		{
			List<string> result = new List<string>();

			if (string.IsNullOrWhiteSpace(value))
			{
				return result;
			}

			string[] parts = value.Split(new char[] { ';', ',', '|', '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

			foreach (string part in parts)
			{
				string item = part.Trim();

				if (item.Length <= 0)
				{
					continue;
				}

				if (item.Equals("Not Use", StringComparison.OrdinalIgnoreCase) ||
					item.Equals("None", StringComparison.OrdinalIgnoreCase))
				{
					continue;
				}

				if (!result.Contains(item, StringComparer.OrdinalIgnoreCase))
				{
					result.Add(item);
				}
			}

			return result;
		}
	}

	public class RuntimeImageAcquireResult
	{
		public string SourceKey { get; set; }
		public bool Success { get; set; }
		public string Message { get; set; }
		public RuntimeVisionImage Image { get; set; }

		public RuntimeImageAcquireResult()
		{
			SourceKey = string.Empty;
			Message = string.Empty;
		}
	}

	public class RuntimeVisionImage : VisionImage
	{
		public string OutputImageKey { get; set; }

		public RuntimeVisionImage()
		{
			OutputImageKey = string.Empty;
		}
	}

	public class RuntimeImageAcquireService
	{
		private const int MaxVisionProAcquireTimeoutMs = 10000;

		public RuntimeImageAcquireResult Precheck(string jobName, string sourceKey)
		{
			RuntimeImageAcquireResult result = new RuntimeImageAcquireResult();
			result.SourceKey = sourceKey;
			result.Success = true;

			string filePath = ResolveImageSourceFile(jobName, sourceKey);
			if (string.IsNullOrWhiteSpace(filePath) || !File.Exists(filePath))
			{
				result.Success = false;
				result.Message = "Image source file was not found.";
				return result;
			}

			string ext = Path.GetExtension(filePath);
			if (!ext.Equals(".vpp", StringComparison.OrdinalIgnoreCase) &&
				!ext.Equals(".xml", StringComparison.OrdinalIgnoreCase))
			{
				return result;
			}

			string cameraName = ResolveCameraName(sourceKey, filePath);
			CameraDeviceConfig camera = FindCameraConfig(jobName, cameraName);

			if (camera == null)
			{
				return result;
			}

			if (!camera.Enable)
			{
				result.Success = false;
				result.Message = "Camera is disabled. Camera=" + camera.CameraName;
				return result;
			}

			if (!string.Equals(camera.Status, "Connected", StringComparison.OrdinalIgnoreCase))
			{
				result.Success = false;
				result.Message = "Camera is not connected. Camera=" + camera.CameraName;
				return result;
			}

			return result;
		}

		public RuntimeImageAcquireResult Acquire(string jobName, string sourceKey)
		{
			RuntimeImageAcquireResult result = new RuntimeImageAcquireResult();
			result.SourceKey = sourceKey;

			try
			{
				string filePath = ResolveImageSourceFile(jobName, sourceKey);

				if (string.IsNullOrWhiteSpace(filePath) || !File.Exists(filePath))
				{
					result.Success = false;
					result.Message = "Image source file was not found.";
					return result;
				}

				string ext = Path.GetExtension(filePath);

				if (ext.Equals(".vpp", StringComparison.OrdinalIgnoreCase))
				{
					result.Image = AcquireByVisionPro(filePath, sourceKey);
					result.Success = true;
					return result;
				}

				if (ext.Equals(".xml", StringComparison.OrdinalIgnoreCase))
				{
					// SDK 相机第一阶段先预留。
					// 后续可以根据 XML 反序列化 SdkCameraConfig，再用 CameraSdkAdapterFactory.Create().Grab()。
					result.Image = new RuntimeVisionImage();
					result.Image.ImageName = sourceKey;
					result.Image.OutputImageKey = sourceKey;
					result.Image.ImageType = "SDK";
					result.Image.SourceStep = "Acquire";
					result.Image.RawImage = null;
					result.Success = true;
					result.Message = "SDK acquire placeholder. Please connect SDK adapter.";
					return result;
				}

				result.Success = false;
				result.Message = "Unsupported image source file type: " + ext;
				return result;
			}
			catch (Exception ex)
			{
				result.Success = false;
				result.Message = ex.Message;
				return result;
			}
		}

		private RuntimeVisionImage AcquireByVisionPro(string vppPath, string sourceKey)
		{
			RuntimeVisionImage image = null;
			Exception runException = null;
			int timeoutMs = ResolveVisionProAcquireTimeout(vppPath, sourceKey);

			Thread thread = new Thread(new ThreadStart(delegate
			{
				try
				{
					image = AcquireByVisionProCore(vppPath, sourceKey);
				}
				catch (Exception ex)
				{
					runException = ex;
				}
			}));
			thread.IsBackground = true;
			thread.Name = "VisionProAcquire-" + (sourceKey ?? string.Empty);
			thread.SetApartmentState(ApartmentState.STA);
			thread.Start();

			if (!thread.Join(timeoutMs))
			{
				throw new TimeoutException("VisionPro acquire timeout after " + timeoutMs.ToString() + " ms. Source=" + sourceKey);
			}

			if (runException != null)
			{
				throw runException;
			}

			return image;
		}

		private RuntimeVisionImage AcquireByVisionProCore(string vppPath, string sourceKey)
		{
			object tool = VisionProReflectionHelper.LoadObjectFromFile(vppPath);

			if (tool == null)
			{
				throw new Exception("VisionPro object load failed: " + vppPath);
			}

			RuntimeVisionImage image = new RuntimeVisionImage();
			image.ImageName = sourceKey;
			image.OutputImageKey = sourceKey;
			image.ImageType = "VisionPro";
			image.SourceStep = "Acquire";

			// CogAcqFifoTool / CogToolBlock 常见方式：Run()
			InvokeIfExists(tool, "Run");

			object outputImage = TryGetOutputImage(tool);

			image.RawImage = outputImage;

			return image;
		}

		private object TryGetOutputImage(object tool)
		{
			if (tool == null)
			{
				return null;
			}

			object value;

			value = GetPropertyValue(tool, "OutputImage");
			if (value != null)
			{
				return value;
			}

			value = GetPropertyValue(tool, "OutputImageKey");
			if (value != null)
			{
				return value;
			}

			object outputs = GetPropertyValue(tool, "Outputs");

			if (outputs != null)
			{
				object img = TryGetNamedTerminalValue(outputs, "OutputImage");
				if (img != null)
				{
					return img;
				}

				img = TryGetNamedTerminalValue(outputs, "Image");
				if (img != null)
				{
					return img;
				}

				img = TryGetNamedTerminalValue(outputs, "RawImage");
				if (img != null)
				{
					return img;
				}
			}

			return null;
		}

		private object TryGetNamedTerminalValue(object terminals, string name)
		{
			if (terminals == null || string.IsNullOrWhiteSpace(name))
			{
				return null;
			}

			try
			{
				PropertyInfo itemProperty = terminals.GetType().GetProperty("Item");

				if (itemProperty != null)
				{
					object terminal = itemProperty.GetValue(terminals, new object[] { name });

					if (terminal != null)
					{
						object value = GetPropertyValue(terminal, "Value");
						if (value != null)
						{
							return value;
						}
					}
				}
			}
			catch
			{
			}

			return null;
		}

		private string ResolveImageSourceFile(string jobName, string sourceKey)
		{
			if (string.IsNullOrWhiteSpace(sourceKey))
			{
				return string.Empty;
			}

			if (File.Exists(sourceKey))
			{
				return sourceKey;
			}

			string cameraName = string.Empty;
			string fileName = sourceKey;

			int dotIndex = sourceKey.IndexOf('.');
			if (dotIndex > 0)
			{
				cameraName = sourceKey.Substring(0, dotIndex);
				fileName = sourceKey.Substring(dotIndex + 1);
			}

			string projectRoot = ProjectPathStore.ProjectRoot;
			string safeJob = ProjectPathStore.MakeSafeName(jobName);

			List<string> roots = new List<string>();

			// 当前推荐目录：
			// Project\Config\Program\<JobName>\Hardware\Camera\<CamName>\*.vpp
			if (!string.IsNullOrWhiteSpace(jobName))
			{
				roots.Add(Path.Combine(projectRoot, "Config", "Program", safeJob, "Hardware", "Camera"));
				roots.Add(Path.Combine(projectRoot, "Config", "Program", safeJob, "Camera"));
				roots.Add(Path.Combine(projectRoot, "Job", safeJob, "Hardware", "Camera"));
				roots.Add(Path.Combine(projectRoot, "Job", safeJob, "Camera"));
			}

			// 兼容旧目录。
			roots.Add(Path.Combine(projectRoot, "Hardware", "Camera"));
			roots.Add(Path.Combine(projectRoot, "Config", "Hardware", "Camera"));

			foreach (string root in roots)
			{
				if (string.IsNullOrWhiteSpace(root) || !Directory.Exists(root))
				{
					continue;
				}

				if (!string.IsNullOrWhiteSpace(cameraName))
				{
					string cameraFolder = Path.Combine(root, cameraName);

					if (Directory.Exists(cameraFolder))
					{
						string direct = Path.Combine(cameraFolder, fileName);

						if (File.Exists(direct))
						{
							return direct;
						}

						string[] files = Directory.GetFiles(cameraFolder, fileName, SearchOption.AllDirectories);

						if (files.Length > 0)
						{
							return files[0];
						}
					}
				}

				string[] all = Directory.GetFiles(root, fileName, SearchOption.AllDirectories);

				if (all.Length > 0)
				{
					return all[0];
				}
			}

			return string.Empty;
		}

		private string ResolveCameraName(string sourceKey, string filePath)
		{
			if (!string.IsNullOrWhiteSpace(sourceKey))
			{
				int dotIndex = sourceKey.IndexOf('.');
				if (dotIndex > 0)
				{
					return sourceKey.Substring(0, dotIndex);
				}
			}

			if (!string.IsNullOrWhiteSpace(filePath))
			{
				DirectoryInfo directory = Directory.GetParent(filePath);
				if (directory != null && !string.IsNullOrWhiteSpace(directory.Name))
				{
					return directory.Name;
				}
			}

			return string.Empty;
		}

		private CameraDeviceConfig FindCameraConfig(string jobName, string cameraName)
		{
			if (string.IsNullOrWhiteSpace(jobName) || string.IsNullOrWhiteSpace(cameraName))
			{
				return null;
			}

			try
			{
				string safeJob = ProjectPathStore.MakeSafeName(jobName);
				List<string> configPaths = new List<string>();
				configPaths.Add(Path.Combine(ProjectPathStore.ProjectRoot, "Config", "Program", safeJob, "Hardware", "HardwareConfig.xml"));
				configPaths.Add(Path.Combine(ProjectPathStore.ProjectRoot, "Job", safeJob, "Hardware", "HardwareConfig.xml"));

				string configPath = configPaths.FirstOrDefault(File.Exists);
				if (string.IsNullOrWhiteSpace(configPath))
				{
					return null;
				}

				XmlSerializer serializer = new XmlSerializer(typeof(HardwareProjectConfig));
				using (FileStream fs = new FileStream(configPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
				{
					HardwareProjectConfig config = serializer.Deserialize(fs) as HardwareProjectConfig;
					if (config == null || config.Cameras == null)
					{
						return null;
					}

					return config.Cameras.FirstOrDefault(x =>
						x != null &&
						string.Equals(x.CameraName, cameraName, StringComparison.OrdinalIgnoreCase));
				}
			}
			catch
			{
				return null;
			}
		}

		private int ResolveVisionProAcquireTimeout(string vppPath, string sourceKey)
		{
			int timeoutMs = 3000;

			try
			{
				string jobName = ResolveJobNameFromVppPath(vppPath);
				string cameraName = ResolveCameraName(sourceKey, vppPath);
				CameraDeviceConfig camera = FindCameraConfig(jobName, cameraName);

				if (camera != null && camera.VisionPro != null && camera.VisionPro.TimeoutMs > 0)
				{
					timeoutMs = camera.VisionPro.TimeoutMs;
				}
			}
			catch
			{
			}

			if (timeoutMs < 500)
			{
				timeoutMs = 500;
			}

			if (timeoutMs > MaxVisionProAcquireTimeoutMs)
			{
				timeoutMs = MaxVisionProAcquireTimeoutMs;
			}

			return timeoutMs;
		}

		private string ResolveJobNameFromVppPath(string vppPath)
		{
			if (string.IsNullOrWhiteSpace(vppPath))
			{
				return string.Empty;
			}

			try
			{
				string fullPath = Path.GetFullPath(vppPath);
				List<string> roots = new List<string>();
				roots.Add(Path.Combine(ProjectPathStore.ProjectRoot, "Config", "Program"));
				roots.Add(Path.Combine(ProjectPathStore.ProjectRoot, "Job"));

				foreach (string root in roots)
				{
					string fullRoot = Path.GetFullPath(root).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
					if (!fullPath.StartsWith(fullRoot + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase))
					{
						continue;
					}

					string relative = fullPath.Substring(fullRoot.Length).TrimStart(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
					int separator = relative.IndexOfAny(new char[] { Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar });
					return separator > 0 ? relative.Substring(0, separator) : string.Empty;
				}

				return string.Empty;
			}
			catch
			{
				return string.Empty;
			}
		}

		private void InvokeIfExists(object obj, string methodName)
		{
			if (obj == null || string.IsNullOrWhiteSpace(methodName))
			{
				return;
			}

			MethodInfo method = obj.GetType().GetMethod(methodName, Type.EmptyTypes);

			if (method != null)
			{
				method.Invoke(obj, null);
			}
		}

		private object GetPropertyValue(object obj, string propertyName)
		{
			if (obj == null || string.IsNullOrWhiteSpace(propertyName))
			{
				return null;
			}

			PropertyInfo property = obj.GetType().GetProperty(propertyName);

			if (property == null)
			{
				return null;
			}

			try
			{
				return property.GetValue(obj, null);
			}
			catch
			{
				return null;
			}
		}
	}

	public class RuntimeCommunicationOutputService
	{
		private static readonly object OutputCacheSyncRoot = new object();
		private static readonly object OutboundSendSyncRoot = new object();
		private static readonly Dictionary<string, Dictionary<string, object>> LatestOutputValuesByProtocol =
			new Dictionary<string, Dictionary<string, object>>(StringComparer.OrdinalIgnoreCase);

		public static event EventHandler<RuntimeCommunicationOutputValuesChangedEventArgs> OutputValuesChanged;

		public static bool TryGetLatestOutputValue(string protocolName, string outputName, out object value)
		{
			value = null;

			if (string.IsNullOrWhiteSpace(outputName))
			{
				return false;
			}

			string key = NormalizeProtocolKey(protocolName);

			lock (OutputCacheSyncRoot)
			{
				Dictionary<string, object> latest;
				if (!LatestOutputValuesByProtocol.TryGetValue(key, out latest) || latest == null)
				{
					return false;
				}

				return latest.TryGetValue(outputName.Trim(), out value);
			}
		}

		public void SendTaskOutput(
			string protocolName,
			TaskConfig task,
			VisionRunContext context,
			StepResult finalResult)
		{
			CommunicationConfig config = CommunicationConfigStore.LoadOrCreateDefault();
			string instanceName = ResolveOutputInstanceName(protocolName, task, config);
			string communicationName = CommunicationRuntimeNaming.FormatCommunicationName(protocolName, instanceName);

			Dictionary<string, object> values = BuildOutputValues(protocolName, instanceName, task, context, finalResult, config);
			lock (OutboundSendSyncRoot)
			{
				bool sent = SendByProtocol(protocolName, instanceName, values, config);
				if (sent)
				{
					RememberOutputValues(protocolName, values);
				}

				RuntimeLogStore.Append(
					DateTime.Now,
					RuntimeLogCategory.Communication,
					"Task communication output " + (sent ? "sent" : "failed") +
					". Communication=" + communicationName +
					", Values=" + FormatValueMap(values),
					!sent);
			}
		}

		public bool SendPartialTaskOutput(
			string protocolName,
			TaskConfig task,
			VisionRunContext context,
			StepResult stepResult)
		{
			CommunicationConfig config = CommunicationConfigStore.LoadOrCreateDefault();
			string instanceName = ResolveOutputInstanceName(protocolName, task, config);
			return SendPartialTaskOutput(protocolName, instanceName, task, context, stepResult);
		}

		public bool SendPartialTaskOutput(
			string protocolName,
			string instanceName,
			TaskConfig task,
			VisionRunContext context,
			StepResult stepResult)
		{
			CommunicationConfig config = CommunicationConfigStore.LoadOrCreateDefault();
			instanceName = CommunicationRuntimeNaming.NormalizeInstanceName(protocolName, instanceName, config);
			string communicationName = CommunicationRuntimeNaming.FormatCommunicationName(protocolName, instanceName);
			Dictionary<string, object> changedValues = BuildChangedOutputValues(protocolName, instanceName, context, stepResult, config);

			if (changedValues.Count <= 0)
			{
				RuntimeLogStore.Append(
					DateTime.Now,
					RuntimeLogCategory.Communication,
					"Step communication output skipped. Communication=" + communicationName + ", Reason=No matched output values.");
				return true;
			}

			bool sent;
			lock (OutboundSendSyncRoot)
			{
				Dictionary<string, object> mergedValues = BuildMergedOutputValues(protocolName, changedValues);
				sent = SendByProtocol(protocolName, instanceName, mergedValues, config);
				if (sent)
				{
					RememberOutputValues(protocolName, mergedValues);
				}
			}

			RuntimeLogStore.Append(
				DateTime.Now,
				RuntimeLogCategory.Communication,
				"Step communication output " + (sent ? "sent" : "failed") +
				". Communication=" + communicationName +
				", Values=" + FormatValueMap(changedValues),
				!sent);
			return sent;
		}

		public bool SendHeartbeatOutput(string protocolName, string outputName, string heartbeatText)
		{
			return SendHeartbeatOutput(protocolName, string.Empty, outputName, heartbeatText);
		}

		public bool SendHeartbeatOutput(string protocolName, string instanceName, string outputName, string heartbeatText)
		{
			if (string.IsNullOrWhiteSpace(outputName))
			{
				return false;
			}

			CommunicationConfig config = CommunicationConfigStore.LoadOrCreateDefault();
			protocolName = CommunicationRuntimeNaming.NormalizeProtocolName(protocolName);
			instanceName = CommunicationRuntimeNaming.NormalizeInstanceName(protocolName, instanceName, config);
			Dictionary<string, object> changedValues = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
			changedValues[outputName.Trim()] = heartbeatText ?? string.Empty;

			lock (OutboundSendSyncRoot)
			{
				Dictionary<string, object> mergedValues = BuildMergedOutputValues(protocolName, changedValues);
				bool sent = SendByProtocol(protocolName, instanceName, mergedValues, config);
				if (sent)
				{
					RememberOutputValues(protocolName, mergedValues);
				}

				return sent;
			}
		}

		public bool SendConfiguredSignalOutput(
			string protocolName,
			string instanceName,
			Dictionary<string, object> changedValues)
		{
			if (changedValues == null || changedValues.Count <= 0)
			{
				return true;
			}

			CommunicationConfig config = CommunicationConfigStore.LoadOrCreateDefault();
			protocolName = CommunicationRuntimeNaming.NormalizeProtocolName(protocolName);
			instanceName = CommunicationRuntimeNaming.NormalizeInstanceName(protocolName, instanceName, config);
			string communicationName = CommunicationRuntimeNaming.FormatCommunicationName(protocolName, instanceName);

			bool sent;
			lock (OutboundSendSyncRoot)
			{
				Dictionary<string, object> mergedValues = BuildMergedOutputValues(protocolName, changedValues);
				sent = SendByProtocol(protocolName, instanceName, mergedValues, config);
				if (sent)
				{
					RememberOutputValues(protocolName, mergedValues);
				}
			}

			RuntimeLogStore.Append(
				DateTime.Now,
				RuntimeLogCategory.Communication,
				"Signal flow output " + (sent ? "sent" : "failed") +
				". Communication=" + communicationName +
				", Values=" + FormatValueMap(changedValues),
				!sent);

			return sent;
		}

		private Dictionary<string, object> BuildOutputValues(
			string protocolName,
			string instanceName,
			TaskConfig task,
			VisionRunContext context,
			StepResult finalResult,
			CommunicationConfig config)
		{
			Dictionary<string, object> result = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);

			// 系统默认输出。
			result["ResultOK"] = finalResult != null && finalResult.IsOK;
			result["Result"] = finalResult != null && finalResult.IsOK ? "OK" : "NG";
			result["Message"] = finalResult == null ? string.Empty : finalResult.Message;
			result["TaskName"] = task == null ? string.Empty : task.TaskName;

			if (finalResult != null && finalResult.Outputs != null)
			{
				foreach (KeyValuePair<string, object> pair in finalResult.Outputs)
				{
					result[pair.Key] = pair.Value;
				}
			}

			if (context != null && context.Data != null)
			{
				foreach (KeyValuePair<string, object> pair in context.Data)
				{
					if (!result.ContainsKey(pair.Key))
					{
						result.Add(pair.Key, pair.Value);
					}
				}
			}

			// 根据通讯输出变量名再做一次短名映射。
			List<CommOutputVariable> outputVariables = GetOutputVariables(protocolName, instanceName, config);

			foreach (CommOutputVariable variable in outputVariables)
			{
				if (variable == null || string.IsNullOrWhiteSpace(variable.Name))
				{
					continue;
				}

				if (result.ContainsKey(variable.Name))
				{
					continue;
				}

				object value;
				if (TryFindValueByOutputName(result, variable.Name, out value))
				{
					result[variable.Name] = value;
					continue;
				}

				object globalValue;
				if (!string.IsNullOrWhiteSpace(variable.GlobalVariableName) &&
					GlobalVariableStore.TryGetValue(variable.GlobalVariableName, out globalValue))
				{
					result[variable.Name] = globalValue;
				}
				else
				{
					result[variable.Name] = string.Empty;
				}
			}

			return result;
		}

		private Dictionary<string, object> BuildChangedOutputValues(
			string protocolName,
			string instanceName,
			VisionRunContext context,
			StepResult stepResult,
			CommunicationConfig config)
		{
			Dictionary<string, object> sourceValues = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);

			if (stepResult != null && stepResult.Outputs != null)
			{
				foreach (KeyValuePair<string, object> pair in stepResult.Outputs)
				{
					sourceValues[pair.Key] = pair.Value;
				}
			}

			Dictionary<string, object> result = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
			List<CommOutputVariable> outputVariables = GetOutputVariables(protocolName, instanceName, config);

			foreach (CommOutputVariable variable in outputVariables)
			{
				if (variable == null || string.IsNullOrWhiteSpace(variable.Name))
				{
					continue;
				}

				object value;
				if (TryFindValueByOutputName(sourceValues, variable.Name, out value))
				{
					result[variable.Name] = value;
					continue;
				}

				if (!string.IsNullOrWhiteSpace(variable.GlobalVariableName) &&
					TryFindValueByOutputName(sourceValues, variable.GlobalVariableName, out value))
				{
					result[variable.Name] = value;
				}
			}

			return result;
		}

		private void RememberOutputValues(string protocolName, Dictionary<string, object> values)
		{
			if (values == null)
			{
				return;
			}

			Dictionary<string, object> latest = MergeOutputValues(protocolName, values);
			RaiseOutputValuesChanged(protocolName, latest);
		}

		private Dictionary<string, object> BuildMergedOutputValues(string protocolName, Dictionary<string, object> changedValues)
		{
			string key = NormalizeProtocolKey(protocolName);

			lock (OutputCacheSyncRoot)
			{
				Dictionary<string, object> merged;
				Dictionary<string, object> latest;
				if (LatestOutputValuesByProtocol.TryGetValue(key, out latest) && latest != null)
				{
					merged = new Dictionary<string, object>(latest, StringComparer.OrdinalIgnoreCase);
				}
				else
				{
					merged = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
				}

				if (changedValues != null)
				{
					foreach (KeyValuePair<string, object> pair in changedValues)
					{
						merged[pair.Key] = pair.Value;
					}
				}

				return merged;
			}
		}

		private Dictionary<string, object> MergeOutputValues(string protocolName, Dictionary<string, object> changedValues)
		{
			string key = NormalizeProtocolKey(protocolName);

			lock (OutputCacheSyncRoot)
			{
				Dictionary<string, object> latest;
				if (!LatestOutputValuesByProtocol.TryGetValue(key, out latest))
				{
					latest = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
					LatestOutputValuesByProtocol[key] = latest;
				}

				if (changedValues != null)
				{
					foreach (KeyValuePair<string, object> pair in changedValues)
					{
						latest[pair.Key] = pair.Value;
					}
				}

				return new Dictionary<string, object>(latest, StringComparer.OrdinalIgnoreCase);
			}
		}

		private static string NormalizeProtocolKey(string protocolName)
		{
			if (string.IsNullOrWhiteSpace(protocolName))
			{
				return "Default";
			}

			string text = protocolName.Trim();
			if (text.Equals("TCP/IP", StringComparison.OrdinalIgnoreCase) ||
				text.Equals("TcpIp", StringComparison.OrdinalIgnoreCase) ||
				text.Replace("/", string.Empty).Equals("TcpIp", StringComparison.OrdinalIgnoreCase))
			{
				return "TCP/IP";
			}

			if (text.Equals("Profinet", StringComparison.OrdinalIgnoreCase))
			{
				return "Profinet";
			}

			if (text.Equals("S7", StringComparison.OrdinalIgnoreCase))
			{
				return "S7";
			}

			return text;
		}

		private static void RaiseOutputValuesChanged(string protocolName, Dictionary<string, object> latestValues)
		{
			EventHandler<RuntimeCommunicationOutputValuesChangedEventArgs> handler = OutputValuesChanged;
			if (handler == null)
			{
				return;
			}

			handler(null, new RuntimeCommunicationOutputValuesChangedEventArgs(
				NormalizeProtocolKey(protocolName),
				latestValues));
		}

		private bool TryFindValueByOutputName(
			Dictionary<string, object> values,
			string outputName,
			out object value)
		{
			value = null;

			if (values == null || string.IsNullOrWhiteSpace(outputName))
			{
				return false;
			}

			if (values.TryGetValue(outputName, out value))
			{
				return true;
			}

			string plcKey = "PLC." + outputName;
			if (values.TryGetValue(plcKey, out value))
			{
				return true;
			}

			foreach (KeyValuePair<string, object> pair in values)
			{
				if (pair.Key.EndsWith("." + outputName, StringComparison.OrdinalIgnoreCase))
				{
					value = pair.Value;
					return true;
				}
			}

			return false;
		}

		private List<CommOutputVariable> GetOutputVariables(
			string protocolName,
			string instanceName,
			CommunicationConfig config)
		{
			List<CommOutputVariable> result = new List<CommOutputVariable>();

			if (config == null)
			{
				return result;
			}

			// 优先从当前协议对象里读取 OutputVariables，例如 config.TcpIp.OutputVariables。
			object protocolObj = null;
			CommunicationInstanceConfig instance =
				CommunicationRuntimeNaming.FindInstance(config, protocolName, instanceName);

			if (protocolName.Equals("TCP/IP", StringComparison.OrdinalIgnoreCase))
			{
				protocolObj = instance != null && instance.TcpIp != null
					? (object)instance.TcpIp
					: config.TcpIp;
			}
			else if (protocolName.Equals("Profinet", StringComparison.OrdinalIgnoreCase))
			{
				protocolObj = config.Profinet;
			}
			else if (protocolName.Equals("S7", StringComparison.OrdinalIgnoreCase))
			{
				protocolObj = instance != null && instance.S7 != null
					? (object)instance.S7
					: config.S7;
			}

			List<CommOutputVariable> protocolOutputs = GetOutputVariablesFromObject(protocolObj);

			if (protocolOutputs.Count > 0)
			{
				return protocolOutputs;
			}

			// 兼容：如果你的 CommunicationConfig 根节点有 OutputVariables，也可以读取。
			List<CommOutputVariable> rootOutputs = GetOutputVariablesFromObject(config);

			return rootOutputs;
		}

		private List<CommOutputVariable> GetOutputVariablesFromObject(object obj)
		{
			List<CommOutputVariable> result = new List<CommOutputVariable>();

			if (obj == null)
			{
				return result;
			}

			PropertyInfo property = obj.GetType().GetProperty("OutputVariables");

			if (property == null)
			{
				return result;
			}

			object value = property.GetValue(obj, null);
			IEnumerable<CommOutputVariable> list = value as IEnumerable<CommOutputVariable>;

			if (list == null)
			{
				return result;
			}

			foreach (CommOutputVariable item in list)
			{
				result.Add(item);
			}

			return result;
		}

		private string ResolveOutputInstanceName(
			string protocolName,
			TaskConfig task,
			CommunicationConfig config)
		{
			string instanceName = task == null ? string.Empty : task.CommunicationInstanceName;
			return CommunicationRuntimeNaming.NormalizeInstanceName(protocolName, instanceName, config);
		}

		private TcpIpConfig GetTcpConfig(CommunicationConfig config, string instanceName)
		{
			CommunicationInstanceConfig instance =
				CommunicationRuntimeNaming.FindInstance(config, "TCP/IP", instanceName);

			if (instance != null && instance.TcpIp != null)
			{
				return instance.TcpIp;
			}

			return config == null ? null : config.TcpIp;
		}

		private bool SendByProtocol(
			string protocolName,
			string instanceName,
			Dictionary<string, object> values,
			CommunicationConfig config)
		{
			if (protocolName.Equals("TCP/IP", StringComparison.OrdinalIgnoreCase))
			{
				TcpIpCommunicationService tcp = null;
				ICommunicationRuntime runtime = CommunicationRuntimeManager.Instance.GetRuntime(instanceName);

				if (runtime != null)
				{
					tcp = runtime as TcpIpCommunicationService;
				}

				if (tcp == null)
				{
					tcp = CommunicationRuntimeManager.Instance.TcpIp;
				}

				if (tcp != null)
				{
					return tcp.SendOutputValues(values, GetTcpConfig(config, instanceName));
				}

				return false;
			}

			// Profinet / S7 下一阶段接入：
			// 这里保留统一出口，后续把 values 根据 CommOutputVariable 的 ByteOffset/Length 打包后写入对应通讯类。
			string text = BuildSimpleText(values);
			CommunicationType type = ParseCommunicationType(protocolName);
			return CommunicationRuntimeManager.Instance.SendString(type, text);
		}

		private CommunicationType ParseCommunicationType(string protocolName)
		{
			if (protocolName.Equals("TCP/IP", StringComparison.OrdinalIgnoreCase))
			{
				return CommunicationType.TcpIp;
			}

			if (protocolName.Equals("Profinet", StringComparison.OrdinalIgnoreCase))
			{
				return CommunicationType.Profinet;
			}

			if (protocolName.Equals("S7", StringComparison.OrdinalIgnoreCase))
			{
				return CommunicationType.S7;
			}

			return CommunicationType.TcpIp;
		}

		private string BuildSimpleText(Dictionary<string, object> values)
		{
			if (values == null || values.Count <= 0)
			{
				return string.Empty;
			}

			List<string> parts = new List<string>();

			foreach (KeyValuePair<string, object> pair in values)
			{
				parts.Add(pair.Key + "=" + Convert.ToString(pair.Value));
			}

			return string.Join(";", parts.ToArray());
		}

		private string FormatValueMap(Dictionary<string, object> values)
		{
			if (values == null || values.Count <= 0)
			{
				return "{}";
			}

			List<string> parts = new List<string>();

			foreach (KeyValuePair<string, object> pair in values)
			{
				parts.Add(pair.Key + "=" + Convert.ToString(pair.Value));
			}

			return "{" + string.Join(", ", parts.ToArray()) + "}";
		}
	}

	public class RuntimeCommunicationOutputValuesChangedEventArgs : EventArgs
	{
		public string ProtocolName { get; private set; }
		public Dictionary<string, object> Values { get; private set; }
		public DateTime Time { get; private set; }

		public RuntimeCommunicationOutputValuesChangedEventArgs(string protocolName, Dictionary<string, object> values)
		{
			ProtocolName = protocolName ?? string.Empty;
			Values = values == null
				? new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase)
				: new Dictionary<string, object>(values, StringComparer.OrdinalIgnoreCase);
			Time = DateTime.Now;
		}
	}
}
