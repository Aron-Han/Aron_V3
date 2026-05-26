using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

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
		private readonly object _syncRoot = new object();
		private readonly HashSet<string> _runningTaskKeys = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

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

			WriteLog("Runtime flow orchestrator started.");
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
				WriteLog("Task offline test started. Job=" + jobName + ", Task=" + taskName);

				using (TaskRunContext.Begin(options))
				{
					ApplyTestImageOverrides(options, context);

					TaskRunner runner = new TaskRunner();
					finalResult = runner.Run(task, context);

					if (options.EnableCommunicationOutput &&
						!string.IsNullOrWhiteSpace(task.CommunicationProtocol) &&
						!task.CommunicationProtocol.Equals("Not Use", StringComparison.OrdinalIgnoreCase) &&
						!task.CommunicationProtocol.Equals("None", StringComparison.OrdinalIgnoreCase))
					{
						_outputService.SendTaskOutput(task.CommunicationProtocol, task, context, finalResult);
					}
				}

				WriteLog("Task offline test finished. Job=" + jobName +
					", Task=" + taskName +
					", OK=" + finalResult.IsOK +
					", CommunicationOutput=" + options.EnableCommunicationOutput +
					", Cost=" + (DateTime.Now - startTime).TotalMilliseconds.ToString("0.0") + " ms");
			}
			catch (Exception ex)
			{
				finalResult = StepResult.NG(ex.Message);
				WriteLog("Task offline test failed. Job=" + jobName + ", Task=" + taskName + ", Error=" + ex.Message);
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

			WriteLog("Runtime flow orchestrator stopped.");
		}

		private void CommunicationRuntime_StatusChanged(object sender, CommunicationStatusChangedEventArgs e)
		{
			if (e == null)
			{
				return;
			}

			WriteLog("Communication status: " + e.CommunicationType + " / " + e.State + " / " + e.Message);
		}

		private void CommunicationRuntime_ErrorOccurred(object sender, Exception e)
		{
			if (e == null)
			{
				return;
			}

			WriteLog("Communication error: " + e.Message);
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

				WriteLog("Communication received. Protocol=" + protocolName +
					", Raw=" + e.RawText +
					", Parsed=" + RuntimeCommunicationInputParser.FormatValues(parsedValues));

				ProjectFlowConfig flowConfig = FlowConfigStore.LoadOrCreateDefault();

				if (flowConfig == null || flowConfig.Jobs == null || flowConfig.Jobs.Count <= 0)
				{
					WriteLog("No flow config was found.");
					return;
				}

				List<RuntimeTaskTarget> matchedTasks = FindMatchedTasks(flowConfig, protocolName, valueProvider);

				if (matchedTasks.Count <= 0)
				{
					WriteLog("No task matched. Protocol=" + protocolName + ", Raw=" + e.RawText + ", Parsed=" + RuntimeCommunicationInputParser.FormatValues(parsedValues));
					return;
				}

				foreach (RuntimeTaskTarget target in matchedTasks)
				{
					RuntimeTaskTarget localTarget = target;

					Task.Factory.StartNew(
						delegate
						{
							RunOneTask(localTarget, protocolName, valueProvider, e);
						},
						TaskCreationOptions.LongRunning);
				}
			}
			catch (Exception ex)
			{
				WriteLog("Process communication data failed: " + ex.Message);
			}
		}

		private List<RuntimeTaskTarget> FindMatchedTasks(
			ProjectFlowConfig flowConfig,
			string protocolName,
			RuntimeCommunicationValueProvider valueProvider)
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

					if (!IsTaskHasTrigger(task))
					{
						continue;
					}

					bool canRun = false;

					try
					{
						canRun = TriggerConditionEvaluator.CanRunTask(task, valueProvider);
					}
					catch
					{
						canRun = ManualCompareTaskCondition(task, protocolName, valueProvider);
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

		private bool IsTaskHasTrigger(TaskConfig task)
		{
			if (task == null)
			{
				return false;
			}

			if (string.IsNullOrWhiteSpace(task.TriggerName))
			{
				return false;
			}

			if (task.TriggerName.Equals("Not Use", StringComparison.OrdinalIgnoreCase) ||
				task.TriggerName.Equals("None", StringComparison.OrdinalIgnoreCase))
			{
				return false;
			}

			return true;
		}

		private bool ManualCompareTaskCondition(
			TaskConfig task,
			string protocolName,
			RuntimeCommunicationValueProvider valueProvider)
		{
			string triggerActual = valueProvider.GetInputValue(protocolName, task.TriggerName);
			bool triggerOk = TriggerConditionEvaluator.CompareValue(
				triggerActual,
				task.TriggerValue,
				task.TriggerCompare);

			if (string.IsNullOrWhiteSpace(task.PositionName) ||
				task.PositionName.Equals("Not Use", StringComparison.OrdinalIgnoreCase) ||
				task.PositionName.Equals("None", StringComparison.OrdinalIgnoreCase))
			{
				return triggerOk;
			}

			string positionActual = valueProvider.GetInputValue(protocolName, task.PositionName);
			bool positionOk = TriggerConditionEvaluator.CompareValue(
				positionActual,
				task.PositionValue,
				task.PositionCompare);

			return triggerOk && positionOk;
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

			if (!EnterTask(taskKey))
			{
				WriteLog("Task is already running, ignored duplicate trigger. " + taskKey);
				return;
			}

			DateTime startTime = DateTime.Now;
			StepResult finalResult = StepResult.NG("Task was not executed.");
			VisionRunContext context = new VisionRunContext();

			try
			{
				context.JobName = jobName;
				context.TaskName = taskName;
				context.TriggerName = target.Task.TriggerName;

				WriteLog("Task started. Job=" + jobName + ", Task=" + taskName);

				FillCommunicationInputToContext(context, protocolName, commEvent, valueProvider.Values);

				// 1. 根据 Task 的图像源先并行取像。
				AcquireTaskImages(jobName, target.Task, context);

				// 2. 按 StepFlow 的 RunOrder 执行。
				//    你已有 TaskRunner，它内部已经实现：
				//    RunOrder 相同的 Step 并行执行，全部完成后再执行下一个 RunOrder。
				TaskRunner runner = new TaskRunner();
				finalResult = runner.Run(target.Task, context);

				// 3. Task 最后一个 Step 执行完成后，把输出结果映射到通讯输出地址并反馈。
				_outputService.SendTaskOutput(protocolName, target.Task, context, finalResult);

				WriteLog("Task finished. Job=" + jobName + ", Task=" + taskName + ", OK=" + finalResult.IsOK + ", Cost=" + (DateTime.Now - startTime).TotalMilliseconds.ToString("0.0") + " ms");
			}
			catch (Exception ex)
			{
				finalResult = StepResult.NG(ex.Message);

				try
				{
					_outputService.SendTaskOutput(protocolName, target.Task, context, finalResult);
				}
				catch
				{
				}

				WriteLog("Task failed. Job=" + jobName + ", Task=" + taskName + ", Error=" + ex.Message);
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
			context.SetData("Comm.RawText", commEvent.RawText);
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

		private void AcquireTaskImages(string jobName, TaskConfig taskConfig, VisionRunContext context)
		{
			List<string> imageSourceKeys = RuntimeImageSourceParser.SplitImageSources(taskConfig.ImageSourceKey);

			if (imageSourceKeys.Count <= 0)
			{
				WriteLog("No image source configured. Task=" + taskConfig.TaskName);
				return;
			}

			List<Task<RuntimeImageAcquireResult>> acquireTasks = new List<Task<RuntimeImageAcquireResult>>();

			foreach (string sourceKey in imageSourceKeys)
			{
				string localSourceKey = sourceKey;

				Task<RuntimeImageAcquireResult> task = Task<RuntimeImageAcquireResult>.Factory.StartNew(
					delegate
					{
						return _imageAcquireService.Acquire(jobName, localSourceKey);
					});

				acquireTasks.Add(task);
			}

			Task.WaitAll(acquireTasks.ToArray());

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

				WriteLog("Image acquired. Source=" + result.SourceKey);
			}
		}

		private string GetProtocolName(CommunicationType type)
		{
			if (type == CommunicationType.TcpIp)
			{
				return "TCP/IP";
			}

			if (type == CommunicationType.Profinet)
			{
				return "Profinet";
			}

			if (type == CommunicationType.S7)
			{
				return "S7";
			}

			return type.ToString();
		}

		private void WriteLog(string message)
		{
			RuntimeFlowLogEventArgs args = new RuntimeFlowLogEventArgs(message);
			RuntimeLogStore.Append(args.Time, args.Category, args.Message);
			EventHandler<RuntimeFlowLogEventArgs> handler = LogGenerated;

			if (handler != null)
			{
				handler(this, args);
			}
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

		public RuntimeFlowLogEventArgs(string message)
		{
			Message = message ?? string.Empty;
			Time = DateTime.Now;
			Category = RuntimeLogStore.Classify(Message);
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

			if (!result.ContainsKey("Raw"))
			{
				result.Add("Raw", rawText);
			}

			if (!result.ContainsKey("RawText"))
			{
				result.Add("RawText", rawText);
			}

			CommunicationConfig config = CommunicationConfigStore.LoadOrCreateDefault();
			List<CommInputVariable> variables = GetInputVariables(protocolName, config);

			foreach (CommInputVariable item in variables)
			{
				if (item == null || string.IsNullOrWhiteSpace(item.Name))
				{
					continue;
				}

				string parsedValue = ParseInputVariable(rawText, item);

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
			CommunicationConfig config)
		{
			if (config == null)
			{
				return new List<CommInputVariable>();
			}

			if (protocolName.Equals("TCP/IP", StringComparison.OrdinalIgnoreCase) ||
				protocolName.Equals("TcpIp", StringComparison.OrdinalIgnoreCase))
			{
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
				return config.S7 == null || config.S7.InputVariables == null
					? new List<CommInputVariable>()
					: config.S7.InputVariables;
			}

			return new List<CommInputVariable>();
		}

		private static string ParseInputVariable(string rawText, CommInputVariable item)
		{
			if (rawText == null)
			{
				rawText = string.Empty;
			}

			int offset = item.ByteOffset < 0 ? 0 : item.ByteOffset;
			int length = item.Length <= 0 ? 1 : item.Length;

			if (offset >= rawText.Length)
			{
				return string.Empty;
			}

			int realLength = Math.Min(length, rawText.Length - offset);
			string value = rawText.Substring(offset, realLength);

			return value.Trim();
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

			List<string> roots = new List<string>();

			// 当前推荐目录：
			// Project\Job\<JobName>\Hardware\Camera\<CamName>\*.vpp
			if (!string.IsNullOrWhiteSpace(jobName))
			{
				roots.Add(Path.Combine(projectRoot, "Job", jobName, "Hardware", "Camera"));
				roots.Add(Path.Combine(projectRoot, "Job", jobName, "Camera"));
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
		public void SendTaskOutput(
			string protocolName,
			TaskConfig task,
			VisionRunContext context,
			StepResult finalResult)
		{
			CommunicationConfig config = CommunicationConfigStore.LoadOrCreateDefault();

			Dictionary<string, object> values = BuildOutputValues(protocolName, task, context, finalResult, config);
			SendByProtocol(protocolName, values, config);
		}

		private Dictionary<string, object> BuildOutputValues(
			string protocolName,
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
			List<CommOutputVariable> outputVariables = GetOutputVariables(protocolName, config);

			foreach (CommOutputVariable variable in outputVariables)
			{
				if (variable == null || string.IsNullOrWhiteSpace(variable.Name))
				{
					continue;
				}

				object globalValue;
				if (!string.IsNullOrWhiteSpace(variable.GlobalVariableName) &&
					GlobalVariableStore.TryGetValue(variable.GlobalVariableName, out globalValue))
				{
					result[variable.Name] = globalValue;
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
				}
				else
				{
					result[variable.Name] = string.Empty;
				}
			}

			return result;
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

		private List<CommOutputVariable> GetOutputVariables(string protocolName, CommunicationConfig config)
		{
			List<CommOutputVariable> result = new List<CommOutputVariable>();

			if (config == null)
			{
				return result;
			}

			// 优先从当前协议对象里读取 OutputVariables，例如 config.TcpIp.OutputVariables。
			object protocolObj = null;

			if (protocolName.Equals("TCP/IP", StringComparison.OrdinalIgnoreCase))
			{
				protocolObj = config.TcpIp;
			}
			else if (protocolName.Equals("Profinet", StringComparison.OrdinalIgnoreCase))
			{
				protocolObj = config.Profinet;
			}
			else if (protocolName.Equals("S7", StringComparison.OrdinalIgnoreCase))
			{
				protocolObj = config.S7;
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

		private void SendByProtocol(
			string protocolName,
			Dictionary<string, object> values,
			CommunicationConfig config)
		{
			if (protocolName.Equals("TCP/IP", StringComparison.OrdinalIgnoreCase))
			{
				TcpIpCommunicationService tcp = CommunicationRuntimeManager.Instance.TcpIp;

				if (tcp != null)
				{
					tcp.SendOutputValues(values);
				}

				return;
			}

			// Profinet / S7 下一阶段接入：
			// 这里保留统一出口，后续把 values 根据 CommOutputVariable 的 ByteOffset/Length 打包后写入对应通讯类。
			string text = BuildSimpleText(values);
			CommunicationType type = ParseCommunicationType(protocolName);
			CommunicationRuntimeManager.Instance.SendString(type, text);
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
	}
}
