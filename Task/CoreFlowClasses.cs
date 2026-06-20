using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Serialization;

namespace Aron_V3
{
	public enum StepType
	{
		Unknown = 0,
		Vpp = 1,
		Script = 2,
		Halcon = 3,
		VisionMaster = 4,
		OpenCv = 5,
		Composite = 6
	}

	public enum PinDataType
	{
		Bool = 0,
		Int = 1,
		Double = 2,
		Float = 3,
		String = 4,
		Image = 5,
		ByteArray = 6
	}

	public enum TriggerCompareType
	{
		Equal = 0,
		NotEqual = 1,
		Greater = 2,
		GreaterOrEqual = 3,
		Less = 4,
		LessOrEqual = 5
	}

	public enum TriggerRunMode
	{
		OnReceive = 0,
		OnChanged = 1
	}

	public enum TaskConcurrencyPolicy
	{
		IgnoreWhenRunning = 0,
		QueueWhenRunning = 1,
		AllowParallel = 2,
		CancelPrevious = 3
	}

	public interface ICommunicationRuntimeValueProvider
	{
		string GetInputValue(string protocol, string tagName);
	}

	public class ProjectPathManager
	{
		public string ProjectRoot { get; private set; }

		public string ConfigRoot { get { return Path.Combine(ProjectRoot, "Config"); } }
		public string FlowConfigRoot { get { return Path.Combine(ConfigRoot, "Flow"); } }
		public string CommunicateRoot { get { return Path.Combine(ProjectRoot, "Communicate"); } }
		public string HardwareConfigRoot { get { return Path.Combine(ConfigRoot, "Hardware"); } }
		public string CommunicationConfigRoot { get { return Path.Combine(ConfigRoot, "Communication"); } }

		public string TaskRoot { get { return Path.Combine(ProjectRoot, "Task"); } }
		public string JobRoot { get { return ProjectRoot; } }
		public string DatabaseRoot { get { return Path.Combine(ProjectRoot, "database"); } }

		// 保留 StepsRoot 属性，兼容旧代码调用。
		// 新路径统一放在 Project\Task\<TaskName>\<ProgramNo> 下。
		public string StepsRoot { get { return TaskRoot; } }

		public string ImagesRoot { get { return Path.Combine(ProjectRoot, "Images"); } }
		public string LogsRoot { get { return Path.Combine(ProjectRoot, "Logs"); } }

		public ProjectPathManager(string projectRoot)
		{
			if (string.IsNullOrWhiteSpace(projectRoot))
			{
				throw new ArgumentException("Project root is empty.");
			}

			ProjectRoot = projectRoot;
		}

		public void EnsureProjectFolders()
		{
			Directory.CreateDirectory(ProjectRoot);
			Directory.CreateDirectory(TaskRoot);
			Directory.CreateDirectory(ConfigRoot);
			Directory.CreateDirectory(FlowConfigRoot);
			Directory.CreateDirectory(CommunicationConfigRoot);
			Directory.CreateDirectory(Path.Combine(ImagesRoot, "Save"));
			Directory.CreateDirectory(Path.Combine(ImagesRoot, "Replay"));
			Directory.CreateDirectory(LogsRoot);
			Directory.CreateDirectory(DatabaseRoot);

			// 注意：
			// 不在这里创建 Config\Hardware。
			// Hardware 目录按具体配置入口需要时再创建。
		}

		public string GetJobFolder(string jobName)
		{
			return GetJobFolder("TCP/IP", "Channel01", jobName);
		}

		public string GetProtocolFolder(string protocolName)
		{
			if (string.IsNullOrWhiteSpace(protocolName))
			{
				protocolName = "TCP_IP";
			}

			return Path.Combine(CommunicateRoot, MakeSafeName(protocolName.Replace("/", "_")));
		}

		public string GetChannelFolder(string protocolName, string channelName)
		{
			if (string.IsNullOrWhiteSpace(channelName))
			{
				channelName = "Channel01";
			}

			return Path.Combine(GetProtocolFolder(protocolName), MakeSafeName(channelName));
		}

		public string GetJobFolder(string protocolName, string channelName, string jobName)
		{
			if (string.IsNullOrWhiteSpace(jobName))
			{
				jobName = "Job_001";
			}

			return Path.Combine(GetChannelFolder(protocolName, channelName), MakeSafeName(jobName));
		}

		public string GetJobHardwareFolder(string jobName)
		{
			return Path.Combine(GetJobFolder(jobName), "Hardware");
		}

		public string GetTaskRootFolder(string jobName)
		{
			return TaskRoot;
		}

		public string GetTaskRootFolder(string protocolName, string channelName, string jobName)
		{
			return TaskRoot;
		}

		public string GetTaskFolder(string jobName, string taskName)
		{
			return GetTaskFolder("TCP/IP", "Channel01", jobName, taskName);
		}

		public string GetTaskFolder(string protocolName, string channelName, string jobName, string taskName)
		{
			if (string.IsNullOrWhiteSpace(jobName))
			{
				jobName = "Job_001";
			}

			if (string.IsNullOrWhiteSpace(taskName))
			{
				taskName = "Task_New_01";
			}

			return Path.Combine(TaskRoot, MakeSafeName(taskName), MakeSafeName(jobName));
		}

		public string GetLegacyProjectRootTaskFolder(string jobName, string taskName)
		{
			if (string.IsNullOrWhiteSpace(jobName))
			{
				jobName = "Job_001";
			}

			if (string.IsNullOrWhiteSpace(taskName))
			{
				taskName = "Task_New_01";
			}

			return Path.Combine(ProjectRoot, MakeSafeName(taskName), MakeSafeName(jobName));
		}

		public string GetLegacyCommunicationTaskFolder(string protocolName, string channelName, string jobName, string taskName)
		{
			if (string.IsNullOrWhiteSpace(taskName))
			{
				taskName = "Task_New_01";
			}

			return Path.Combine(GetLegacyCommunicationJobFolder(protocolName, channelName, jobName), "Task", MakeSafeName(taskName));
		}

		public string GetLegacyCommunicationJobFolder(string protocolName, string channelName, string jobName)
		{
			if (string.IsNullOrWhiteSpace(jobName))
			{
				jobName = "Job_001";
			}

			return Path.Combine(GetChannelFolder(protocolName, channelName), MakeSafeName(jobName));
		}

		public string GetLegacyFlatTaskFolder(string jobName, string taskName)
		{
			if (string.IsNullOrWhiteSpace(jobName))
			{
				jobName = "Job_001";
			}

			if (string.IsNullOrWhiteSpace(taskName))
			{
				taskName = "Task_New_01";
			}

			return Path.Combine(ProjectRoot, "Job", MakeSafeName(jobName), MakeSafeName(taskName));
		}

		public string GetLegacyFlatTaskRootFolder(string jobName, string taskName)
		{
			if (string.IsNullOrWhiteSpace(jobName))
			{
				jobName = "Job_001";
			}

			if (string.IsNullOrWhiteSpace(taskName))
			{
				taskName = "Task_New_01";
			}

			return Path.Combine(ProjectRoot, "Job", MakeSafeName(jobName), "Task", MakeSafeName(taskName));
		}

		public List<string> GetTaskFolderCandidates(string protocolName, string channelName, string jobName, string taskName)
		{
			List<string> result = new List<string>();
			AddUniquePath(result, GetTaskFolder(protocolName, channelName, jobName, taskName));
			AddUniquePath(result, GetLegacyProjectRootTaskFolder(jobName, taskName));
			AddUniquePath(result, GetLegacyCommunicationTaskFolder(protocolName, channelName, jobName, taskName));
			AddUniquePath(result, GetLegacyFlatTaskFolder(jobName, taskName));
			AddUniquePath(result, GetLegacyFlatTaskRootFolder(jobName, taskName));
			return result;
		}

		public string ResolveExistingTaskFolder(string protocolName, string channelName, string jobName, string taskName)
		{
			foreach (string folder in GetTaskFolderCandidates(protocolName, channelName, jobName, taskName))
			{
				if (Directory.Exists(folder))
				{
					return folder;
				}
			}

			return GetTaskFolder(protocolName, channelName, jobName, taskName);
		}

		private void AddUniquePath(List<string> paths, string path)
		{
			if (paths == null || string.IsNullOrWhiteSpace(path))
			{
				return;
			}

			if (!paths.Any(x => string.Equals(x, path, StringComparison.OrdinalIgnoreCase)))
			{
				paths.Add(path);
			}
		}

		public string GetStepFolder(string jobName, string taskName, string stepName)
		{
			return GetStepFolder("TCP/IP", "Channel01", jobName, taskName, stepName);
		}

		public string GetStepFolder(string protocolName, string channelName, string jobName, string taskName, string stepName)
		{
			return GetTaskFolder(protocolName, channelName, jobName, taskName);
		}

		public void EnsureJobFolder(string jobName)
		{
			Directory.CreateDirectory(GetJobFolder(jobName));
		}

		public void EnsureTaskFolder(string jobName, string taskName)
		{
			string taskFolder = GetTaskFolder(jobName, taskName);
			Directory.CreateDirectory(taskFolder);
		}

		public void EnsureStepFolder(string jobName, string taskName, string stepName)
		{
			EnsureStepFolder("TCP/IP", "Channel01", jobName, taskName, stepName);
		}

		public void EnsureStepFolder(string protocolName, string channelName, string jobName, string taskName, string stepName)
		{
			string taskFolder = GetStepFolder(protocolName, channelName, jobName, taskName, stepName);

			Directory.CreateDirectory(taskFolder);
			Directory.CreateDirectory(Path.Combine(taskFolder, "VPP"));
			Directory.CreateDirectory(Path.Combine(taskFolder, "Script"));
			Directory.CreateDirectory(Path.Combine(taskFolder, "Hdev"));
		}

		public string MakeSafeName(string name)
		{
			if (string.IsNullOrWhiteSpace(name))
			{
				return "New";
			}

			foreach (char c in Path.GetInvalidFileNameChars())
			{
				name = name.Replace(c, '_');
			}

			return name.Trim();
		}
	}


	public class VisionImage
	{
		public string ImageName { get; set; }
		public object RawImage { get; set; }
		[XmlIgnore]
		public System.Drawing.Bitmap DisplayBitmap { get; set; }
		public string ImageType { get; set; }
		public int Width { get; set; }
		public int Height { get; set; }
		public string SourceStep { get; set; }
		public object DisplayRecord { get; set; }
		public string DisplayRecordKey { get; set; }

		public VisionImage()
		{
			ImageName = string.Empty;
			ImageType = string.Empty;
			SourceStep = string.Empty;
			DisplayRecordKey = string.Empty;
		}
	}

	public class StepResult
	{
		public bool IsOK { get; set; }
		public string Message { get; set; }
		public double CostMs { get; set; }

		public Dictionary<string, object> Inputs { get; private set; }
		public Dictionary<string, object> Outputs { get; private set; }
		public Dictionary<string, VisionImage> OutputImages { get; private set; }

		public StepResult()
		{
			IsOK = true;
			Message = string.Empty;
			Inputs = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
			Outputs = new Dictionary<string, object>();
			OutputImages = new Dictionary<string, VisionImage>(StringComparer.OrdinalIgnoreCase);
		}

		public static StepResult OK()
		{
			return new StepResult { IsOK = true };
		}

		public static StepResult OK(string message)
		{
			return new StepResult
			{
				IsOK = true,
				Message = message
			};
		}

		public static StepResult NG(string message)
		{
			return new StepResult { IsOK = false, Message = message };
		}
	}

	public class RuntimeStepResultUpdatedEventArgs : EventArgs
	{
		public string JobName { get; private set; }
		public string TaskName { get; private set; }
		public string StepName { get; private set; }
		public StepResult Result { get; private set; }

		public RuntimeStepResultUpdatedEventArgs(string jobName, string taskName, string stepName, StepResult result)
		{
			JobName = jobName ?? string.Empty;
			TaskName = taskName ?? string.Empty;
			StepName = stepName ?? string.Empty;
			Result = result;
		}
	}

	public static class RuntimeStepResultStore
	{
		private static readonly ConcurrentDictionary<string, StepResult> LatestResults =
			new ConcurrentDictionary<string, StepResult>(StringComparer.OrdinalIgnoreCase);

		public static event EventHandler<RuntimeStepResultUpdatedEventArgs> StepResultUpdated;

		public static void SetLatest(string jobName, string taskName, string stepName, StepResult result)
		{
			if (string.IsNullOrWhiteSpace(jobName) ||
				string.IsNullOrWhiteSpace(taskName) ||
				string.IsNullOrWhiteSpace(stepName) ||
				result == null)
			{
				return;
			}

			string key = BuildKey(jobName, taskName, stepName);
			LatestResults[key] = result;

			EventHandler<RuntimeStepResultUpdatedEventArgs> handler = StepResultUpdated;
			if (handler != null)
			{
				handler(null, new RuntimeStepResultUpdatedEventArgs(jobName, taskName, stepName, result));
			}
		}

		public static bool TryGetLatest(string jobName, string taskName, string stepName, out StepResult result)
		{
			result = null;
			if (string.IsNullOrWhiteSpace(jobName) ||
				string.IsNullOrWhiteSpace(taskName) ||
				string.IsNullOrWhiteSpace(stepName))
			{
				return false;
			}

			return LatestResults.TryGetValue(BuildKey(jobName, taskName, stepName), out result);
		}

		private static string BuildKey(string jobName, string taskName, string stepName)
		{
			return (jobName ?? string.Empty).Trim() + "|" +
				(taskName ?? string.Empty).Trim() + "|" +
				(stepName ?? string.Empty).Trim();
		}
	}

	public class VisionRunContext
	{
		private readonly object _syncRoot = new object();

		public string JobName { get; set; }
		public string TaskName { get; set; }
		public string TriggerName { get; set; }

		public Dictionary<string, object> Data { get; private set; }
		public Dictionary<string, VisionImage> Images { get; private set; }
		public Dictionary<string, StepResult> StepResults { get; private set; }

		public VisionRunContext()
		{
			JobName = string.Empty;
			TaskName = string.Empty;
			TriggerName = string.Empty;
			Data = new Dictionary<string, object>();
			Images = new Dictionary<string, VisionImage>();
			StepResults = new Dictionary<string, StepResult>();
		}

		public object GetData(string key)
		{
			if (string.IsNullOrWhiteSpace(key))
			{
				return null;
			}

			object value;
			if (Data.TryGetValue(key, out value))
			{
				return value;
			}

			return null;
		}


		public void SetData(string key, object value)
		{
			lock (_syncRoot)
			{
				if (Data.ContainsKey(key)) Data[key] = value;
				else Data.Add(key, value);
			}
		}

		public bool TryGetData(string key, out object value)
		{
			lock (_syncRoot)
			{
				return Data.TryGetValue(key, out value);
			}
		}

		public void SetImage(string key, VisionImage image)
		{
			lock (_syncRoot)
			{
				if (Images.ContainsKey(key)) Images[key] = image;
				else Images.Add(key, image);
			}
		}

		public bool TryGetImage(string key, out VisionImage image)
		{
			lock (_syncRoot)
			{
				return Images.TryGetValue(key, out image);
			}
		}

		public void SetStepResult(string stepName, StepResult result)
		{
			lock (_syncRoot)
			{
				if (StepResults.ContainsKey(stepName)) StepResults[stepName] = result;
				else StepResults.Add(stepName, result);
			}
		}
	}

	public class PinConfig
	{
		[XmlAttribute]
		public string PinName { get; set; }

		[XmlAttribute]
		public string SourceKey { get; set; }

		[XmlAttribute]
		public string TargetKey { get; set; }

		[XmlAttribute]
		public PinDataType DataType { get; set; }

		[XmlAttribute]
		public int Length { get; set; }

		[XmlAttribute]
		public string Description { get; set; }

		[XmlAttribute]
		public string GlobalVariableName { get; set; }

		public PinConfig()
		{
			PinName = string.Empty;
			SourceKey = string.Empty;
			TargetKey = string.Empty;
			Description = string.Empty;
			GlobalVariableName = string.Empty;
			DataType = PinDataType.String;
			Length = 0;
		}
	}

	// StepConfig 表示“可用 Step 库”中的一个 Step。
	// 点击中间 Step 区域上方 + 从本地选择 VPP / Script 文件时，只新增到这个库，不自动加入右侧执行流程。
	public class StepConfig
	{
		[XmlAttribute]
		public string StepName { get; set; }

		[XmlAttribute]
		public StepType StepType { get; set; }

		[XmlAttribute]
		public int RunOrder { get; set; }

		[XmlAttribute]
		public bool Enabled { get; set; }

		[XmlAttribute]
		public bool StopWhenNG { get; set; }

		[XmlAttribute]
		public string StepFolder { get; set; }

		[XmlAttribute]
		public string InputImageKey { get; set; }

		[XmlAttribute]
		public string OutputImageKey { get; set; }

		[XmlAttribute]
		public string Remark { get; set; }

		// 添加 Step 时的原始文件路径，例如 D:\VPP\Camera.vpp
		// 保存右侧流程时，才会把实际用到的 Step 文件复制到 Project/Steps。
		[XmlAttribute]
		public string SourceFilePath { get; set; }

		// 复制到 Project 后的相对路径，例如 VPP\Camera.vpp 或 Script\Output.csx
		[XmlAttribute]
		public string ProjectFilePath { get; set; }

		[XmlElement("VppFile")]
		public List<string> VppFiles { get; set; }

		[XmlElement("ScriptFile")]
		public List<string> ScriptFiles { get; set; }

		[XmlArray("InputPins")]
		[XmlArrayItem("Pin")]
		public List<PinConfig> InputPins { get; set; }

		[XmlArray("OutputPins")]
		[XmlArrayItem("Pin")]
		public List<PinConfig> OutputPins { get; set; }

		public string DisplayOutputKey { get; set; }
		public string DisplaySlotName { get; set; }
		public string DisplayResultKey { get; set; }
		public string DisplayMode { get; set; }

		// Script Step 可选择接收哪些前序模块作为参数对象。
		// 多个 StepName 用英文分号分隔，例如：Inspection;Measure_01。
		[XmlAttribute]
		public string ScriptInputStepKeys { get; set; }

		[XmlIgnore]
		public List<string> ScriptInputStepKeyList
		{
			get
			{
				List<string> result = new List<string>();

				if (string.IsNullOrWhiteSpace(ScriptInputStepKeys))
				{
					return result;
				}

				string[] parts = ScriptInputStepKeys.Split(new char[] { ';', ',', '|', '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
				foreach (string part in parts)
				{
					string item = (part ?? string.Empty).Trim();
					if (string.IsNullOrWhiteSpace(item))
					{
						continue;
					}

					if (!result.Any(x => string.Equals(x, item, StringComparison.OrdinalIgnoreCase)))
					{
						result.Add(item);
					}
				}

				return result;
			}
		}

		public StepConfig()
		{
			StepName = string.Empty;
			StepType = StepType.Unknown;
			RunOrder = 0;
			Enabled = true;
			StopWhenNG = false;
			StepFolder = string.Empty;
			InputImageKey = string.Empty;
			OutputImageKey = string.Empty;
			Remark = string.Empty;
			SourceFilePath = string.Empty;
			ProjectFilePath = string.Empty;

			VppFiles = new List<string>();
			ScriptFiles = new List<string>();
			InputPins = new List<PinConfig>();
			OutputPins = new List<PinConfig>();

			DisplayOutputKey = "Not Use";
			DisplaySlotName = "Not Show";
			DisplayResultKey = "Not Use";
			DisplayMode = "Fit";
			ScriptInputStepKeys = string.Empty;
		}
	}

	// StepFlowItem 表示右侧“当前 task 中实际执行的算子流程”。
	// RunOrder 允许重复：1、1、2 代表 RunOrder=1 的 Step 并行执行，全部完成后执行 RunOrder=2。
	public class SignalOutputBinding
	{
		[XmlAttribute]
		public string OutputName { get; set; }

		[XmlAttribute]
		public string AssignedValue { get; set; }

		[XmlAttribute]
		public bool ForceValue { get; set; }

		[XmlAttribute]
		public bool Enabled { get; set; }

		public SignalOutputBinding()
		{
			OutputName = string.Empty;
			AssignedValue = string.Empty;
			ForceValue = false;
			Enabled = false;
		}
	}

	public class DatabaseInputBinding
	{
		[XmlAttribute]
		public string InputName { get; set; }

		[XmlAttribute]
		public string GlobalVariableName { get; set; }

		[XmlAttribute]
		public string AssignedValue { get; set; }

		[XmlAttribute]
		public bool ForceValue { get; set; }

		[XmlAttribute]
		public bool Enabled { get; set; }

		public DatabaseInputBinding()
		{
			InputName = string.Empty;
			GlobalVariableName = string.Empty;
			AssignedValue = string.Empty;
			ForceValue = false;
			Enabled = true;
		}
	}

	public class StepFlowItem
	{
		[XmlAttribute]
		public string FlowItemId { get; set; }

		[XmlAttribute]
		public string BlockType { get; set; }

		[XmlAttribute]
		public string BlockName { get; set; }

		[XmlAttribute]
		public string BlockPath { get; set; }

		[XmlAttribute]
		public string SignalProtocol { get; set; }

		[XmlAttribute]
		public string SignalInstanceName { get; set; }

		[XmlAttribute]
		public string StepName { get; set; }

		[XmlAttribute]
		public string InputImageKey { get; set; }

		[XmlAttribute]
		public int RunOrder { get; set; }

		[XmlAttribute]
		public bool Enabled { get; set; }

		[XmlAttribute]
		public bool EnableCommunicationOutput { get; set; }

		[XmlAttribute]
		public string CommunicationOutputInstanceName { get; set; }

		[XmlAttribute]
		public string CommunicationOutputProtocol { get; set; }

		[XmlAttribute]
		public string Remark { get; set; }

		public string DisplayOutputKey { get; set; }
		public string DisplaySlotName { get; set; }
		public string DisplayResultKey { get; set; }
		public string DisplayMode { get; set; }

		[XmlArray("SignalOutputs")]
		[XmlArrayItem("Output")]
		public List<SignalOutputBinding> SignalOutputs { get; set; }

		[XmlArray("DatabaseInputs")]
		[XmlArrayItem("Input")]
		public List<DatabaseInputBinding> DatabaseInputs { get; set; }

		// Script Step 可选择接收哪些前序模块作为参数对象。
		// 多个 StepName 用英文分号分隔，例如：Inspection;Measure_01。
		[XmlAttribute]
		public string ScriptInputStepKeys { get; set; }

		[XmlIgnore]
		public bool IsStepBlock
		{
			get
			{
				return string.IsNullOrWhiteSpace(BlockType) ||
					BlockType.Equals("Step", StringComparison.OrdinalIgnoreCase);
			}
		}

		[XmlIgnore]
		public List<string> ScriptInputStepKeyList
		{
			get
			{
				List<string> result = new List<string>();

				if (string.IsNullOrWhiteSpace(ScriptInputStepKeys))
				{
					return result;
				}

				string[] parts = ScriptInputStepKeys.Split(new char[] { ';', ',', '|', '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
				foreach (string part in parts)
				{
					string item = (part ?? string.Empty).Trim();
					if (string.IsNullOrWhiteSpace(item))
					{
						continue;
					}

					if (!result.Any(x => string.Equals(x, item, StringComparison.OrdinalIgnoreCase)))
					{
						result.Add(item);
					}
				}

				return result;
			}
		}

		public StepFlowItem()
		{
			FlowItemId = Guid.NewGuid().ToString("N");
			BlockType = "Step";
			BlockName = string.Empty;
			BlockPath = string.Empty;
			SignalProtocol = string.Empty;
			SignalInstanceName = string.Empty;
			StepName = "";
			InputImageKey = "";
			RunOrder = 1;
			Enabled = true;
			EnableCommunicationOutput = false;
			CommunicationOutputInstanceName = string.Empty;
			CommunicationOutputProtocol = string.Empty;
			Remark = "";

			DisplayOutputKey = "Not Use";
			DisplaySlotName = "Not Show";
			DisplayResultKey = "Not Use";
			DisplayMode = "Fit";
			SignalOutputs = new List<SignalOutputBinding>();
			DatabaseInputs = new List<DatabaseInputBinding>();
			ScriptInputStepKeys = string.Empty;
		}
	}

	public class TaskCommunicationTriggerBinding
	{
		[XmlAttribute]
		public string CommunicationInstanceName { get; set; }

		[XmlAttribute]
		public string CommunicationProtocol { get; set; }

		[XmlAttribute]
		public string CommunicationChannel { get; set; }

		[XmlAttribute]
		public string TriggerName { get; set; }

		[XmlAttribute]
		public string TriggerValue { get; set; }

		[XmlAttribute]
		public TriggerCompareType TriggerCompare { get; set; }

		[XmlAttribute]
		public TriggerRunMode TriggerRunMode { get; set; }

		[XmlAttribute]
		public string PositionName { get; set; }

		[XmlAttribute]
		public string PositionValue { get; set; }

		[XmlAttribute]
		public TriggerCompareType PositionCompare { get; set; }

		[XmlArray("ExecutionConditions")]
		[XmlArrayItem("Condition")]
		public List<TaskExecutionCondition> ExecutionConditions { get; set; }

		public TaskCommunicationTriggerBinding()
		{
			CommunicationInstanceName = string.Empty;
			CommunicationProtocol = string.Empty;
			CommunicationChannel = "Channel01";
			TriggerName = string.Empty;
			TriggerValue = "1";
			TriggerCompare = TriggerCompareType.Equal;
			TriggerRunMode = TriggerRunMode.OnReceive;
			PositionName = "Not Use";
			PositionValue = "1";
			PositionCompare = TriggerCompareType.Equal;
			ExecutionConditions = new List<TaskExecutionCondition>();
		}
	}

	public class TaskExecutionCondition
	{
		[XmlAttribute]
		public string GlobalVariableName { get; set; }

		[XmlAttribute]
		public string ExpectedValue { get; set; }

		[XmlAttribute]
		public TriggerCompareType Compare { get; set; }

		public TaskExecutionCondition()
		{
			GlobalVariableName = string.Empty;
			ExpectedValue = string.Empty;
			Compare = TriggerCompareType.Equal;
		}
	}

	public class TaskConfig
	{
		[XmlAttribute]
		public string TaskName { get; set; }

		[XmlAttribute]
		public string CommunicationInstanceName { get; set; }

		[XmlAttribute]
		public string CommunicationProtocol { get; set; }

		[XmlAttribute]
		public string CommunicationChannel { get; set; }

		[XmlAttribute]
		public int RunOrder { get; set; }

		[XmlAttribute]
		public bool Enabled { get; set; }

		[XmlAttribute]
		public bool ProgramSwitchEnabled { get; set; }

		[XmlAttribute]
		public TaskConcurrencyPolicy ConcurrencyPolicy { get; set; }

		[XmlAttribute]
		public string TriggerName { get; set; }

		// 新字段：触发源值。
		// 只有通讯运行时读取到的 TriggerName 实际值满足 TriggerValue，才允许执行当前 Task。
		[XmlAttribute]
		public string TriggerValue { get; set; }

		[XmlAttribute]
		public TriggerCompareType TriggerCompare { get; set; }

		[XmlAttribute]
		public TriggerRunMode TriggerRunMode { get; set; }

		// 新字段：位置号。
		// 原“标志位”改为“位置号”，旧字段 FlagBit 继续保留用于兼容旧 XML。
		[XmlAttribute]
		public string PositionName { get; set; }

		[XmlAttribute]
		public string PositionOptionName { get; set; }

		// 新字段：位置号值。
		// 原“标志值”改为“位置号值”，旧字段 FlagValue 继续保留用于兼容旧 XML。
		[XmlAttribute]
		public string PositionValue { get; set; }

		[XmlAttribute]
		public TriggerCompareType PositionCompare { get; set; }

		[XmlArray("ExecutionConditions")]
		[XmlArrayItem("Condition")]
		public List<TaskExecutionCondition> ExecutionConditions { get; set; }

		// 旧字段保留，避免旧 XML 或旧代码报错。
		// 新逻辑里它可以不再代表 PLC 输入地址。
		[XmlAttribute]
		public string InputAddress { get; set; }

		// 新字段：图像源。
		// 支持单个或多个图像源，多个图像源用英文分号分隔。
		// 例如：
		// Not Use
		// Cam1.Camera.vpp
		// Cam1.Camera.vpp;Cam2.Camera.vpp
		[XmlAttribute]
		public string ImageSourceKey { get; set; }


		[XmlIgnore]
		public List<string> ImageSourceKeyList
		{
			get
			{
				List<string> result = new List<string>();

				if (string.IsNullOrWhiteSpace(ImageSourceKey))
				{
					return result;
				}

				string[] parts = ImageSourceKey.Split(new char[] { ';', ',', '|', '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

				foreach (string part in parts)
				{
					string item = part.Trim();

					if (string.IsNullOrWhiteSpace(item))
					{
						continue;
					}

					if (item.Equals("Not Use", StringComparison.OrdinalIgnoreCase))
					{
						continue;
					}

					if (!result.Any(x => string.Equals(x, item, StringComparison.OrdinalIgnoreCase)))
					{
						result.Add(item);
					}
				}

				return result;
			}
		}

		[XmlAttribute]
		public int FlagBit { get; set; }

		[XmlAttribute]
		public string FlagValue { get; set; }

		[XmlAttribute]
		public string Remark { get; set; }

		// 当前 Task 可用 Step 库，中间“所有 step”列表显示这里。
		[XmlArray("Steps")]
		[XmlArrayItem("Step")]
		public List<StepConfig> Steps { get; set; }

		// 当前 Task 实际执行流程，右侧表格显示这里。
		[XmlArray("StepFlow")]
		[XmlArrayItem("Item")]
		public List<StepFlowItem> StepFlow { get; set; }

		[XmlArray("CommunicationTriggerBindings")]
		[XmlArrayItem("Binding")]
		public List<TaskCommunicationTriggerBinding> CommunicationTriggerBindings { get; set; }

		public TaskConfig()
		{
			CommunicationInstanceName = string.Empty;
			CommunicationProtocol = string.Empty;
			CommunicationChannel = "Channel01";
			TaskName = string.Empty;
			RunOrder = 0;
			Enabled = true;
			ProgramSwitchEnabled = true;
			ConcurrencyPolicy = TaskConcurrencyPolicy.IgnoreWhenRunning;
			TriggerName = string.Empty;
			TriggerValue = "1";
			TriggerCompare = TriggerCompareType.Equal;
			TriggerRunMode = TriggerRunMode.OnReceive;
			PositionName = "Not Use";
			PositionOptionName = "Not Use";
			PositionValue = "1";
			PositionCompare = TriggerCompareType.Equal;
			ExecutionConditions = new List<TaskExecutionCondition>();
			InputAddress = string.Empty;
			ImageSourceKey = "Not Use";
			FlagBit = 0;
			FlagValue = string.Empty;
			Remark = string.Empty;
			Steps = new List<StepConfig>();
			StepFlow = new List<StepFlowItem>();
			CommunicationTriggerBindings = new List<TaskCommunicationTriggerBinding>();
		}
	}


	public class JobConfig
	{
		[XmlAttribute]
		public string ProtocolName { get; set; }

		[XmlAttribute]
		public string ChannelName { get; set; }

		[XmlAttribute]
		public string ProgramNo { get; set; }

		[XmlAttribute]
		public string JobName { get; set; }

		[XmlAttribute]
		public bool Enabled { get; set; }

		[XmlArray("Tasks")]
		[XmlArrayItem("Task")]
		public List<TaskConfig> Tasks { get; set; }

		public JobConfig()
		{
			ProtocolName = "TCP/IP";
			ChannelName = "Channel01";
			ProgramNo = "1";
			JobName = string.Empty;
			Enabled = true;
			Tasks = new List<TaskConfig>();
		}
	}

	public class ChannelFlowConfig
	{
		[XmlAttribute]
		public string ChannelName { get; set; }

		[XmlAttribute]
		public string ActiveProgramNo { get; set; }

		[XmlArray("Jobs")]
		[XmlArrayItem("Job")]
		public List<JobConfig> Jobs { get; set; }

		public ChannelFlowConfig()
		{
			ChannelName = "Channel01";
			ActiveProgramNo = "1";
			Jobs = new List<JobConfig>();
		}
	}

	public class ProtocolFlowConfig
	{
		[XmlAttribute]
		public string ProtocolName { get; set; }

		[XmlArray("Channels")]
		[XmlArrayItem("Channel")]
		public List<ChannelFlowConfig> Channels { get; set; }

		public ProtocolFlowConfig()
		{
			ProtocolName = "TCP/IP";
			Channels = new List<ChannelFlowConfig>();
		}
	}

	[XmlRoot("ProjectFlowConfig")]
	public class ProjectFlowConfig
	{
		[XmlArray("Protocols")]
		[XmlArrayItem("Protocol")]
		public List<ProtocolFlowConfig> Protocols { get; set; }

		[XmlArray("Jobs")]
		[XmlArrayItem("Job")]
		public List<JobConfig> Jobs { get; set; }

		public ProjectFlowConfig()
		{
			Protocols = new List<ProtocolFlowConfig>();
			Jobs = new List<JobConfig>();
		}
	}

	public static class XmlConfigHelper
	{
		public static void Save<T>(string filePath, T config)
		{
			string dir = Path.GetDirectoryName(filePath);
			if (!string.IsNullOrEmpty(dir)) Directory.CreateDirectory(dir);

			XmlSerializer serializer = new XmlSerializer(typeof(T));
			XmlWriterSettings settings = new XmlWriterSettings();
			settings.Encoding = Encoding.UTF8;
			settings.Indent = true;

			using (XmlWriter writer = XmlWriter.Create(filePath, settings))
			{
				serializer.Serialize(writer, config);
			}
		}

		public static T Load<T>(string filePath) where T : new()
		{
			if (!File.Exists(filePath)) return new T();

			XmlSerializer serializer = new XmlSerializer(typeof(T));

			using (FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read))
			{
				object obj = serializer.Deserialize(fs);
				if (obj == null) return new T();
				return (T)obj;
			}
		}
	}

	public static class FlowConfigStore
	{
		public static event EventHandler FlowConfigSaved;

		private static string _projectRoot = ProjectPathStore.ProjectRoot;

		public static string ProjectRoot
		{
			get { return _projectRoot; }
			set
			{
				if (!string.IsNullOrWhiteSpace(value))
				{
					_projectRoot = value;
				}
			}
		}

		public static ProjectPathManager PathManager
		{
			get { return new ProjectPathManager(ProjectRoot); }
		}

		public static string FlowConfigFile
		{
			get { return Path.Combine(PathManager.FlowConfigRoot, "ProjectFlowConfig.xml"); }
		}

		private static string LegacyFlowConfigFile
		{
			get { return Path.Combine(ProjectRoot, "Job", "ProjectFlowConfig.xml"); }
		}

		private static string LegacyCommunicationFlowConfigFile
		{
			get { return Path.Combine(PathManager.CommunicateRoot, "ProjectFlowConfig.xml"); }
		}

		public static ProjectFlowConfig LoadOrCreateDefault()
		{
			ProjectFlowConfig config = new ProjectFlowConfig();

			string filePath = FlowConfigFile;
			bool loadedFromLegacyJobFile = false;
			if (!File.Exists(filePath) && File.Exists(LegacyCommunicationFlowConfigFile))
			{
				filePath = LegacyCommunicationFlowConfigFile;
			}
			else if (!File.Exists(filePath) && File.Exists(LegacyFlowConfigFile))
			{
				filePath = LegacyFlowConfigFile;
				loadedFromLegacyJobFile = true;
			}

			if (File.Exists(filePath))
			{
				config = XmlConfigHelper.Load<ProjectFlowConfig>(filePath);

				if (config == null)
				{
					config = new ProjectFlowConfig();
				}
			}

			NormalizeConfig(config);
			try
			{
				EnsureStepFolders(config);
				MigrateLegacyProjectJobRoot(config, loadedFromLegacyJobFile);
			}
			catch
			{
			}
			return config;
		}


		public static void Save(ProjectFlowConfig config)
		{
			if (config == null)
			{
				config = new ProjectFlowConfig();
			}

			NormalizeConfig(config);
			PathManager.EnsureProjectFolders();
			SyncFlatJobs(config);
			XmlConfigHelper.Save(FlowConfigFile, config);
			EnsureStepFolders(config);
			MigrateLegacyProjectJobRoot(config, false);

			DiagnosticLogStore.Append(
				DiagnosticLogLevel.Info,
				"Config",
				"Flow config saved.",
				new Dictionary<string, string> { { "path", FlowConfigFile } });

			EventHandler handler = FlowConfigSaved;
			if (handler != null)
			{
				handler(null, EventArgs.Empty);
			}
		}

		public static JobConfig GetOrCreateJob(ProjectFlowConfig config, string jobName)
		{
			string protocolName = "TCP/IP";
			string channelName = "Channel01";
			JobConfig job = GetJobs(config, protocolName, channelName)
				.FirstOrDefault(j => string.Equals(j.JobName, jobName, StringComparison.OrdinalIgnoreCase));

			if (job == null)
			{
				job = new JobConfig();
				job.JobName = jobName;
				job.ProtocolName = protocolName;
				job.ChannelName = channelName;
				job.Enabled = true;
				GetOrCreateChannel(config, protocolName, channelName).Jobs.Add(job);
				SyncFlatJobs(config);
			}

			return job;
		}

		public static JobConfig GetOrCreateJob(ProjectFlowConfig config, string protocolName, string channelName, string jobName)
		{
			ChannelFlowConfig channel = GetOrCreateChannel(config, protocolName, channelName);
			JobConfig job = channel.Jobs.FirstOrDefault(j => string.Equals(j.JobName, jobName, StringComparison.OrdinalIgnoreCase));

			if (job == null)
			{
				job = new JobConfig();
				job.JobName = jobName;
				job.ProtocolName = NormalizeProtocolName(protocolName);
				job.ChannelName = NormalizeChannelName(channelName);
				job.Enabled = true;
				channel.Jobs.Add(job);
				SyncFlatJobs(config);
			}

			return job;
		}

		public static ChannelFlowConfig GetOrCreateChannel(ProjectFlowConfig config, string protocolName, string channelName)
		{
			if (config.Protocols == null)
			{
				config.Protocols = new List<ProtocolFlowConfig>();
			}

			protocolName = NormalizeProtocolName(protocolName);
			channelName = NormalizeChannelName(channelName);

			ProtocolFlowConfig protocol = config.Protocols.FirstOrDefault(x =>
				x != null && string.Equals(x.ProtocolName, protocolName, StringComparison.OrdinalIgnoreCase));
			if (protocol == null)
			{
				protocol = new ProtocolFlowConfig();
				protocol.ProtocolName = protocolName;
				config.Protocols.Add(protocol);
			}

			if (protocol.Channels == null)
			{
				protocol.Channels = new List<ChannelFlowConfig>();
			}

			ChannelFlowConfig channel = protocol.Channels.FirstOrDefault(x =>
				x != null && string.Equals(x.ChannelName, channelName, StringComparison.OrdinalIgnoreCase));
			if (channel == null)
			{
				channel = new ChannelFlowConfig();
				channel.ChannelName = channelName;
				protocol.Channels.Add(channel);
			}

			if (channel.Jobs == null)
			{
				channel.Jobs = new List<JobConfig>();
			}

			return channel;
		}

		public static List<string> GetProtocolNames(ProjectFlowConfig config)
		{
			NormalizeConfig(config);
			return config.Protocols
				.Where(x => x != null && !string.IsNullOrWhiteSpace(x.ProtocolName))
				.Select(x => x.ProtocolName)
				.Distinct(StringComparer.OrdinalIgnoreCase)
				.OrderBy(x => x)
				.ToList();
		}

		public static List<string> GetEnabledProtocolNames(ProjectFlowConfig config)
		{
			NormalizeConfig(config);

			try
			{
				CommunicationConfig communication = CommunicationConfigStore.LoadOrCreateDefault();
				List<string> result = new List<string>();

				if (communication.TcpIp != null && communication.TcpIp.Enabled)
				{
					result.Add("TCP/IP");
				}

				if (communication.Profinet != null && communication.Profinet.Enabled)
				{
					result.Add("Profinet");
				}

				if (communication.S7 != null && communication.S7.Enabled)
				{
					result.Add("S7");
				}

				return result
					.Where(x => !string.IsNullOrWhiteSpace(x))
					.Distinct(StringComparer.OrdinalIgnoreCase)
					.ToList();
			}
			catch
			{
				return GetProtocolNames(config);
			}
		}

		public static List<string> GetChannelNames(ProjectFlowConfig config, string protocolName)
		{
			NormalizeConfig(config);
			protocolName = NormalizeProtocolName(protocolName);
			ProtocolFlowConfig protocol = config.Protocols.FirstOrDefault(x =>
				x != null && string.Equals(x.ProtocolName, protocolName, StringComparison.OrdinalIgnoreCase));
			if (protocol == null || protocol.Channels == null)
			{
				return new List<string>();
			}

			return protocol.Channels
				.Where(x => x != null && !string.IsNullOrWhiteSpace(x.ChannelName))
				.Select(x => x.ChannelName)
				.Distinct(StringComparer.OrdinalIgnoreCase)
				.OrderBy(x => x)
				.ToList();
		}

		public static List<string> GetEnabledChannelNames(ProjectFlowConfig config, string protocolName)
		{
			NormalizeConfig(config);
			protocolName = NormalizeProtocolName(protocolName);

			try
			{
				CommunicationConfig communication = CommunicationConfigStore.LoadOrCreateDefault();
				List<CommunicationChannelConfig> channels = GetCommunicationChannels(communication, protocolName);

				return channels
					.Where(x => x != null && x.Enabled && !string.IsNullOrWhiteSpace(x.ChannelName))
					.Select(x => NormalizeChannelName(x.ChannelName))
					.Distinct(StringComparer.OrdinalIgnoreCase)
					.OrderBy(x => x)
					.ToList();
			}
			catch
			{
				return GetChannelNames(config, protocolName);
			}
		}

		private static List<CommunicationChannelConfig> GetCommunicationChannels(CommunicationConfig communication, string protocolName)
		{
			if (communication == null)
			{
				return new List<CommunicationChannelConfig>();
			}

			protocolName = NormalizeProtocolName(protocolName);

			if (protocolName.Equals("TCP/IP", StringComparison.OrdinalIgnoreCase))
			{
				return communication.TcpIp == null || communication.TcpIp.Channels == null
					? new List<CommunicationChannelConfig>()
					: communication.TcpIp.Channels;
			}

			if (protocolName.Equals("Profinet", StringComparison.OrdinalIgnoreCase))
			{
				return communication.Profinet == null || communication.Profinet.Channels == null
					? new List<CommunicationChannelConfig>()
					: communication.Profinet.Channels;
			}

			if (protocolName.Equals("S7", StringComparison.OrdinalIgnoreCase))
			{
				return communication.S7 == null || communication.S7.Channels == null
					? new List<CommunicationChannelConfig>()
					: communication.S7.Channels;
			}

			return new List<CommunicationChannelConfig>();
		}

		public static List<JobConfig> GetJobs(ProjectFlowConfig config, string protocolName, string channelName)
		{
			NormalizeConfig(config);
			protocolName = NormalizeProtocolName(protocolName);
			channelName = NormalizeChannelName(channelName);
			ChannelFlowConfig channel = GetChannel(config, protocolName, channelName);
			return channel == null || channel.Jobs == null ? new List<JobConfig>() : channel.Jobs;
		}

		public static ChannelFlowConfig GetChannel(ProjectFlowConfig config, string protocolName, string channelName)
		{
			if (config == null || config.Protocols == null)
			{
				return null;
			}

			protocolName = NormalizeProtocolName(protocolName);
			channelName = NormalizeChannelName(channelName);
			ProtocolFlowConfig protocol = config.Protocols.FirstOrDefault(x =>
				x != null && string.Equals(x.ProtocolName, protocolName, StringComparison.OrdinalIgnoreCase));
			if (protocol == null || protocol.Channels == null)
			{
				return null;
			}

			return protocol.Channels.FirstOrDefault(x =>
				x != null && string.Equals(x.ChannelName, channelName, StringComparison.OrdinalIgnoreCase));
		}

		public static TaskConfig CreateDefaultTask(string jobName, string taskName, int runOrder)
		{
			TaskConfig task = new TaskConfig();
			task.TaskName = taskName;
			task.RunOrder = runOrder;
			task.Enabled = true;
			task.ProgramSwitchEnabled = false;
			task.TriggerName = "Trigger_" + (runOrder - 1).ToString();
			task.TriggerValue = "1";
			task.TriggerCompare = TriggerCompareType.Equal;
			task.TriggerRunMode = TriggerRunMode.OnReceive;
			task.PositionName = "Not Use";
			task.PositionValue = "1";
			task.PositionCompare = TriggerCompareType.Equal;

			// 旧字段保留，避免旧逻辑报错。
			task.InputAddress = string.Empty;

			// 新字段：默认选择“无”。
			// 如果这个 Task 的取像工具直接放在 VPP Step 中，就选择“无”。
			task.ImageSourceKey = "Not Use";

			task.FlagBit = 0;
			task.FlagValue = "1";
			task.Remark = string.Empty;
			return task;
		}


		public static StepConfig CreateDefaultStep(string jobName, string taskName, string stepName, int runOrder)
		{
			return CreateDefaultStep(jobName, taskName, stepName, runOrder, StepType.Vpp);
		}

		public static StepConfig CreateDefaultStep(string jobName, string taskName, string stepName, int runOrder, StepType stepType)
		{
			StepConfig step = new StepConfig();

			step.StepName = stepName;
			step.StepType = stepType;
			step.RunOrder = runOrder;
			step.Enabled = true;
			step.StopWhenNG = true;
			step.StepFolder = Path.Combine("Task", taskName, jobName);
			step.Remark = string.Empty;

			if (stepType == StepType.Vpp)
			{
				step.InputImageKey = "Cam1.Raw";
				step.OutputImageKey = stepName + ".OutputImage";
			}
			else if (stepType == StepType.Script)
			{
				step.InputImageKey = string.Empty;
				step.OutputImageKey = string.Empty;

				step.InputPins.Add(new PinConfig
				{
					PinName = "InputValue",
					SourceKey = string.Empty,
					TargetKey = string.Empty,
					DataType = PinDataType.String,
					Length = 0,
					Description = string.Empty
				});

				step.OutputPins.Add(new PinConfig
				{
					PinName = "PLC.ResultOK",
					SourceKey = string.Empty,
					TargetKey = "PLC.ResultOK",
					DataType = PinDataType.Bool,
					Length = 1,
					Description = string.Empty
				});
			}
			else if (stepType == StepType.Halcon)
			{
				step.InputImageKey = string.Empty;
				step.OutputImageKey = string.Empty;
			}

			return step;
		}

		public static StepType GetStepTypeByFilePath(string filePath)
		{
			string ext = Path.GetExtension(filePath).ToLower();

			if (ext == ".vpp") return StepType.Vpp;
			if (ext == ".cs" || ext == ".csx" || ext == ".txt") return StepType.Script;
			if (ext == ".hdev") return StepType.Halcon;

			return StepType.Unknown;
		}

		private static ProjectFlowConfig CreateDefaultConfig()
		{
			// 不再自动创建 Job_001。
			// 删除 Project 文件夹后重新启动，流程管理页面应为空。
			// 只有用户新增 Task/程序号并保存 Step 文件时，才创建 Project\Task\<TaskName>\<ProgramNo>。
			return new ProjectFlowConfig();
		}


		private static void NormalizeConfig(ProjectFlowConfig config)
		{
			if (config == null)
			{
				return;
			}

			if (config.Jobs == null) config.Jobs = new List<JobConfig>();
			if (config.Protocols == null) config.Protocols = new List<ProtocolFlowConfig>();

			if (config.Protocols.Count <= 0 && config.Jobs.Count > 0)
			{
				foreach (JobConfig legacyJob in config.Jobs)
				{
					if (legacyJob == null)
					{
						continue;
					}

					string protocolName = ResolveJobProtocol(legacyJob);
					string channelName = ResolveJobChannel(legacyJob);
					legacyJob.ProtocolName = protocolName;
					legacyJob.ChannelName = channelName;
					GetOrCreateChannel(config, protocolName, channelName).Jobs.Add(legacyJob);
				}
			}

			foreach (ProtocolFlowConfig protocol in config.Protocols)
			{
				if (protocol == null)
				{
					continue;
				}

				protocol.ProtocolName = NormalizeProtocolName(protocol.ProtocolName);
				if (protocol.Channels == null) protocol.Channels = new List<ChannelFlowConfig>();

				foreach (ChannelFlowConfig channel in protocol.Channels)
				{
					if (channel == null)
					{
						continue;
					}

					channel.ChannelName = NormalizeChannelName(channel.ChannelName);
					if (string.IsNullOrWhiteSpace(channel.ActiveProgramNo)) channel.ActiveProgramNo = "1";
					if (channel.Jobs == null) channel.Jobs = new List<JobConfig>();

					foreach (JobConfig job in channel.Jobs)
					{
						if (job == null)
						{
							continue;
						}

						job.ProtocolName = protocol.ProtocolName;
						job.ChannelName = channel.ChannelName;
						if (string.IsNullOrWhiteSpace(job.ProgramNo)) job.ProgramNo = DeriveProgramNo(job.JobName);
						NormalizeJob(job);
					}
				}
			}

			foreach (JobConfig job in config.Jobs)
			{
				NormalizeJob(job);
			}

			RemoveEmptyJobsAndChannels(config);
			SyncFlatJobs(config);
		}

		private static void RemoveEmptyJobsAndChannels(ProjectFlowConfig config)
		{
			if (config == null || config.Protocols == null)
			{
				return;
			}

			foreach (ProtocolFlowConfig protocol in config.Protocols)
			{
				if (protocol == null || protocol.Channels == null)
				{
					continue;
				}

				foreach (ChannelFlowConfig channel in protocol.Channels)
				{
					if (channel == null || channel.Jobs == null)
					{
						continue;
					}

					channel.Jobs.RemoveAll(IsEmptyJob);
				}

				protocol.Channels.RemoveAll(channel =>
					channel == null || channel.Jobs == null || channel.Jobs.Count <= 0);
			}

			config.Protocols.RemoveAll(protocol =>
				protocol == null || protocol.Channels == null || protocol.Channels.Count <= 0);
		}

		private static bool IsEmptyJob(JobConfig job)
		{
			if (job == null)
			{
				return true;
			}

			if (job.Tasks == null)
			{
				return true;
			}

			job.Tasks.RemoveAll(task => task == null || string.IsNullOrWhiteSpace(task.TaskName));
			return job.Tasks.Count <= 0;
		}

		private static void NormalizeJob(JobConfig job)
		{
			if (job == null)
			{
				return;
			}

			if (string.IsNullOrWhiteSpace(job.ProtocolName)) job.ProtocolName = ResolveJobProtocol(job);
			if (string.IsNullOrWhiteSpace(job.ChannelName)) job.ChannelName = ResolveJobChannel(job);
			job.ProtocolName = NormalizeProtocolName(job.ProtocolName);
			job.ChannelName = NormalizeChannelName(job.ChannelName);
			if (string.IsNullOrWhiteSpace(job.ProgramNo)) job.ProgramNo = DeriveProgramNo(job.JobName);
			if (job.Tasks == null) job.Tasks = new List<TaskConfig>();
			job.Tasks.RemoveAll(task => task == null || string.IsNullOrWhiteSpace(task.TaskName));

			foreach (TaskConfig task in job.Tasks)
			{
				if (task.Steps == null) task.Steps = new List<StepConfig>();
				if (task.StepFlow == null) task.StepFlow = new List<StepFlowItem>();
				if (task.CommunicationTriggerBindings == null) task.CommunicationTriggerBindings = new List<TaskCommunicationTriggerBinding>();
				if (task.ExecutionConditions == null) task.ExecutionConditions = new List<TaskExecutionCondition>();
				NormalizeExecutionConditions(task.ExecutionConditions);

				if (string.IsNullOrEmpty(task.CommunicationProtocol)) task.CommunicationProtocol = job.ProtocolName;
				if (string.IsNullOrEmpty(task.CommunicationChannel)) task.CommunicationChannel = job.ChannelName;
				if (string.IsNullOrWhiteSpace(task.CommunicationInstanceName)) task.CommunicationInstanceName = GetDefaultCommunicationInstanceName(task.CommunicationProtocol);
				if (string.IsNullOrEmpty(task.TriggerValue)) task.TriggerValue = "1";
				if (string.IsNullOrEmpty(task.PositionName)) task.PositionName = task.FlagBit.ToString();
				if (string.IsNullOrEmpty(task.PositionOptionName)) task.PositionOptionName = task.PositionName;
				if (string.IsNullOrEmpty(task.PositionValue)) task.PositionValue = task.FlagValue;
				if (string.IsNullOrEmpty(task.PositionValue)) task.PositionValue = "1";

				task.CommunicationProtocol = job.ProtocolName;
				task.CommunicationChannel = job.ChannelName;
				if (string.IsNullOrWhiteSpace(task.CommunicationInstanceName)) task.CommunicationInstanceName = GetDefaultCommunicationInstanceName(task.CommunicationProtocol);

				EnsureLegacyTriggerBinding(task);
				NormalizeTaskTriggerBindings(task);

				int oldFlagBit;
				if (int.TryParse(task.PositionName, out oldFlagBit)) task.FlagBit = oldFlagBit;
				task.FlagValue = task.PositionValue;

				foreach (StepConfig step in task.Steps)
				{
					if (step.VppFiles == null) step.VppFiles = new List<string>();
					if (step.ScriptFiles == null) step.ScriptFiles = new List<string>();
					if (step.InputPins == null) step.InputPins = new List<PinConfig>();
					if (step.OutputPins == null) step.OutputPins = new List<PinConfig>();
					if (string.IsNullOrWhiteSpace(step.DisplayResultKey)) step.DisplayResultKey = "Not Use";
					NormalizeStepProjectRelativePaths(step);
					NormalizeStepSourcePath(job.ProtocolName, job.ChannelName, job.JobName, task.TaskName, step);
				}

				foreach (StepFlowItem flowItem in task.StepFlow)
				{
					if (flowItem == null) continue;
					if (string.IsNullOrWhiteSpace(flowItem.FlowItemId)) flowItem.FlowItemId = Guid.NewGuid().ToString("N");
					if (flowItem.SignalOutputs == null) flowItem.SignalOutputs = new List<SignalOutputBinding>();
					if (flowItem.DatabaseInputs == null) flowItem.DatabaseInputs = new List<DatabaseInputBinding>();
					foreach (SignalOutputBinding signalOutput in flowItem.SignalOutputs)
					{
						if (signalOutput == null) continue;
						if (signalOutput.OutputName == null) signalOutput.OutputName = string.Empty;
						if (signalOutput.AssignedValue == null) signalOutput.AssignedValue = string.Empty;
					}
					foreach (DatabaseInputBinding databaseInput in flowItem.DatabaseInputs)
					{
						if (databaseInput == null) continue;
						if (databaseInput.InputName == null) databaseInput.InputName = string.Empty;
						if (databaseInput.GlobalVariableName == null) databaseInput.GlobalVariableName = string.Empty;
						if (databaseInput.AssignedValue == null) databaseInput.AssignedValue = string.Empty;
					}
					if (string.IsNullOrWhiteSpace(flowItem.BlockType)) flowItem.BlockType = "Step";
					if (string.IsNullOrWhiteSpace(flowItem.BlockName)) flowItem.BlockName = flowItem.StepName;
					if (string.IsNullOrWhiteSpace(flowItem.StepName)) flowItem.StepName = flowItem.BlockName;
					if (!flowItem.IsStepBlock)
					{
						flowItem.InputImageKey = string.Empty;
					}
					if (string.IsNullOrWhiteSpace(flowItem.DisplayOutputKey)) flowItem.DisplayOutputKey = "Not Use";
					if (string.IsNullOrWhiteSpace(flowItem.DisplaySlotName)) flowItem.DisplaySlotName = "Not Show";
					if (string.IsNullOrWhiteSpace(flowItem.DisplayResultKey)) flowItem.DisplayResultKey = "Not Use";
					if (string.IsNullOrWhiteSpace(flowItem.DisplayMode)) flowItem.DisplayMode = "Fit";
					if (string.IsNullOrWhiteSpace(flowItem.CommunicationOutputProtocol)) flowItem.CommunicationOutputProtocol = task.CommunicationProtocol;
					if (string.IsNullOrWhiteSpace(flowItem.CommunicationOutputInstanceName)) flowItem.CommunicationOutputInstanceName = task.CommunicationInstanceName;
				}
			}
		}

		private static string GetDefaultCommunicationInstanceName(string protocolName)
		{
			string normalized = NormalizeProtocolName(protocolName);

			if (normalized.Equals("TCP/IP", StringComparison.OrdinalIgnoreCase) ||
				normalized.Equals("TcpIp", StringComparison.OrdinalIgnoreCase))
			{
				return "TCPIP_01";
			}

			if (normalized.Equals("Profinet", StringComparison.OrdinalIgnoreCase))
			{
				return "Profinet_01";
			}

			if (normalized.Equals("S7", StringComparison.OrdinalIgnoreCase))
			{
				return "S7_01";
			}

			return string.Empty;
		}

		private static void EnsureLegacyTriggerBinding(TaskConfig task)
		{
			if (task == null)
			{
				return;
			}

			if (task.CommunicationTriggerBindings == null)
			{
				task.CommunicationTriggerBindings = new List<TaskCommunicationTriggerBinding>();
			}

			if (task.CommunicationTriggerBindings.Count > 0)
			{
				return;
			}

			TaskCommunicationTriggerBinding binding = new TaskCommunicationTriggerBinding();
			binding.CommunicationInstanceName = task.CommunicationInstanceName;
			binding.CommunicationProtocol = task.CommunicationProtocol;
			binding.CommunicationChannel = task.CommunicationChannel;
			binding.TriggerName = task.TriggerName;
			binding.TriggerValue = task.TriggerValue;
			binding.TriggerCompare = task.TriggerCompare;
			binding.TriggerRunMode = task.TriggerRunMode;
			binding.PositionName = task.PositionName;
			binding.PositionValue = task.PositionValue;
			binding.PositionCompare = task.PositionCompare;
			binding.ExecutionConditions = CloneExecutionConditions(task.ExecutionConditions);
			task.CommunicationTriggerBindings.Add(binding);
		}

		private static void NormalizeTaskTriggerBindings(TaskConfig task)
		{
			if (task == null || task.CommunicationTriggerBindings == null)
			{
				return;
			}

			foreach (TaskCommunicationTriggerBinding binding in task.CommunicationTriggerBindings)
			{
				if (binding == null)
				{
					continue;
				}

				if (string.IsNullOrWhiteSpace(binding.CommunicationProtocol)) binding.CommunicationProtocol = task.CommunicationProtocol;
				if (string.IsNullOrWhiteSpace(binding.CommunicationChannel)) binding.CommunicationChannel = task.CommunicationChannel;
				if (string.IsNullOrWhiteSpace(binding.CommunicationInstanceName)) binding.CommunicationInstanceName = GetDefaultCommunicationInstanceName(binding.CommunicationProtocol);
				if (string.IsNullOrWhiteSpace(binding.TriggerValue)) binding.TriggerValue = "1";
				if (string.IsNullOrWhiteSpace(binding.PositionName)) binding.PositionName = "Not Use";
				if (string.IsNullOrWhiteSpace(binding.PositionValue)) binding.PositionValue = "1";
				if (binding.ExecutionConditions == null) binding.ExecutionConditions = new List<TaskExecutionCondition>();
				NormalizeExecutionConditions(binding.ExecutionConditions);
			}
		}

		private static void NormalizeExecutionConditions(List<TaskExecutionCondition> conditions)
		{
			if (conditions == null)
			{
				return;
			}

			for (int i = conditions.Count - 1; i >= 0; i--)
			{
				TaskExecutionCondition condition = conditions[i];
				if (condition == null || string.IsNullOrWhiteSpace(condition.GlobalVariableName))
				{
					conditions.RemoveAt(i);
					continue;
				}

				condition.GlobalVariableName = condition.GlobalVariableName.Trim();
				condition.ExpectedValue = condition.ExpectedValue == null ? string.Empty : condition.ExpectedValue.Trim();
			}
		}

		private static List<TaskExecutionCondition> CloneExecutionConditions(List<TaskExecutionCondition> source)
		{
			List<TaskExecutionCondition> result = new List<TaskExecutionCondition>();
			if (source == null)
			{
				return result;
			}

			foreach (TaskExecutionCondition condition in source)
			{
				if (condition == null || string.IsNullOrWhiteSpace(condition.GlobalVariableName))
				{
					continue;
				}

				result.Add(new TaskExecutionCondition
				{
					GlobalVariableName = condition.GlobalVariableName.Trim(),
					ExpectedValue = condition.ExpectedValue == null ? string.Empty : condition.ExpectedValue.Trim(),
					Compare = condition.Compare
				});
			}

			return result;
		}

		private static void SyncFlatJobs(ProjectFlowConfig config)
		{
			if (config == null)
			{
				return;
			}

			if (config.Protocols == null)
			{
				config.Protocols = new List<ProtocolFlowConfig>();
			}

			List<JobConfig> jobs = new List<JobConfig>();
			foreach (ProtocolFlowConfig protocol in config.Protocols)
			{
				if (protocol == null || protocol.Channels == null)
				{
					continue;
				}

				foreach (ChannelFlowConfig channel in protocol.Channels)
				{
					if (channel == null || channel.Jobs == null)
					{
						continue;
					}

					foreach (JobConfig job in channel.Jobs)
					{
						if (job == null)
						{
							continue;
						}

						job.ProtocolName = NormalizeProtocolName(protocol.ProtocolName);
						job.ChannelName = NormalizeChannelName(channel.ChannelName);
						jobs.Add(job);
					}
				}
			}

			config.Jobs = jobs;
		}

		public static int RenameCommunicationChannelReferences(
			string protocolName,
			string instanceName,
			IDictionary<string, string> channelRenames)
		{
			Dictionary<string, string> renames = NormalizeChannelRenameMap(channelRenames);
			if (renames.Count <= 0)
			{
				return 0;
			}

			protocolName = NormalizeProtocolName(protocolName);
			CommunicationConfig communicationConfig = null;
			try
			{
				communicationConfig = CommunicationConfigStore.LoadOrCreateDefault();
			}
			catch
			{
			}

			string targetInstanceName = CommunicationRuntimeNaming.NormalizeInstanceName(protocolName, instanceName, communicationConfig);
			ProjectFlowConfig config = LoadOrCreateDefault();
			int changed = 0;

			if (config.Protocols != null)
			{
				foreach (ProtocolFlowConfig protocol in config.Protocols)
				{
					if (protocol == null ||
						protocol.Channels == null ||
						!string.Equals(NormalizeProtocolName(protocol.ProtocolName), protocolName, StringComparison.OrdinalIgnoreCase))
					{
						continue;
					}

					for (int i = 0; i < protocol.Channels.Count; i++)
					{
						ChannelFlowConfig channel = protocol.Channels[i];
						if (channel == null || channel.Jobs == null)
						{
							continue;
						}

						string originalChannelName = NormalizeChannelName(channel.ChannelName);
						List<JobConfig> changedJobs = new List<JobConfig>();
						foreach (JobConfig job in channel.Jobs)
						{
							int jobChanges = RenameJobChannelReferences(
								job,
								protocolName,
								targetInstanceName,
								renames,
								communicationConfig);
							if (jobChanges > 0)
							{
								changedJobs.Add(job);
								changed += jobChanges;
							}
						}

						string renamedChannelName;
						if (changedJobs.Count > 0 &&
							TryGetRenamedChannelName(originalChannelName, renames, out renamedChannelName))
						{
							if (changedJobs.Count == channel.Jobs.Count)
							{
								channel.ChannelName = renamedChannelName;
							}
							else
							{
								foreach (JobConfig changedJob in changedJobs)
								{
									channel.Jobs.Remove(changedJob);
								}

								ChannelFlowConfig targetChannel = GetOrCreateProtocolChannel(
									protocol,
									renamedChannelName,
									channel.ActiveProgramNo);
								foreach (JobConfig changedJob in changedJobs)
								{
									if (!targetChannel.Jobs.Contains(changedJob))
									{
										targetChannel.Jobs.Add(changedJob);
									}
								}
							}

							changed++;
						}
					}

					MergeDuplicateChannels(protocol);
				}
			}

			if (changed > 0)
			{
				Save(config);
				RenameLegacyCommunicationChannelFolders(protocolName, renames);
			}

			return changed;
		}

		private static ChannelFlowConfig GetOrCreateProtocolChannel(
			ProtocolFlowConfig protocol,
			string channelName,
			string activeProgramNo)
		{
			if (protocol.Channels == null)
			{
				protocol.Channels = new List<ChannelFlowConfig>();
			}

			channelName = NormalizeChannelName(channelName);
			ChannelFlowConfig channel = protocol.Channels.FirstOrDefault(x =>
				x != null && string.Equals(NormalizeChannelName(x.ChannelName), channelName, StringComparison.OrdinalIgnoreCase));
			if (channel != null)
			{
				if (channel.Jobs == null) channel.Jobs = new List<JobConfig>();
				return channel;
			}

			channel = new ChannelFlowConfig();
			channel.ChannelName = channelName;
			channel.ActiveProgramNo = string.IsNullOrWhiteSpace(activeProgramNo) ? "1" : activeProgramNo;
			channel.Jobs = new List<JobConfig>();
			protocol.Channels.Add(channel);
			return channel;
		}

		private static Dictionary<string, string> NormalizeChannelRenameMap(IDictionary<string, string> channelRenames)
		{
			Dictionary<string, string> result = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
			if (channelRenames == null)
			{
				return result;
			}

			foreach (KeyValuePair<string, string> pair in channelRenames)
			{
				string oldName = NormalizeChannelName(pair.Key);
				string newName = NormalizeChannelName(pair.Value);
				if (string.IsNullOrWhiteSpace(oldName) ||
					string.IsNullOrWhiteSpace(newName) ||
					string.Equals(oldName, newName, StringComparison.OrdinalIgnoreCase))
				{
					continue;
				}

				result[oldName] = newName;
			}

			return result;
		}

		private static int RenameJobChannelReferences(
			JobConfig job,
			string protocolName,
			string targetInstanceName,
			Dictionary<string, string> channelRenames,
			CommunicationConfig communicationConfig)
		{
			if (job == null ||
				channelRenames == null ||
				channelRenames.Count <= 0 ||
				!string.Equals(NormalizeProtocolName(job.ProtocolName), protocolName, StringComparison.OrdinalIgnoreCase))
			{
				return 0;
			}

			int changed = 0;
			bool jobContainsChangedTask = false;
			if (job.Tasks != null)
			{
				foreach (TaskConfig task in job.Tasks)
				{
					if (task == null)
					{
						continue;
					}

					if (IsTargetCommunication(task.CommunicationProtocol, task.CommunicationInstanceName, protocolName, targetInstanceName, communicationConfig))
					{
						string renamedTaskChannel;
						if (TryGetRenamedChannelName(task.CommunicationChannel, channelRenames, out renamedTaskChannel))
						{
							task.CommunicationChannel = renamedTaskChannel;
							jobContainsChangedTask = true;
							changed++;
						}
					}

					if (task.CommunicationTriggerBindings == null)
					{
						continue;
					}

					foreach (TaskCommunicationTriggerBinding binding in task.CommunicationTriggerBindings)
					{
						if (binding == null)
						{
							continue;
						}

						string bindingProtocol = string.IsNullOrWhiteSpace(binding.CommunicationProtocol)
							? task.CommunicationProtocol
							: binding.CommunicationProtocol;
						string bindingInstance = string.IsNullOrWhiteSpace(binding.CommunicationInstanceName)
							? task.CommunicationInstanceName
							: binding.CommunicationInstanceName;

						if (!IsTargetCommunication(bindingProtocol, bindingInstance, protocolName, targetInstanceName, communicationConfig))
						{
							continue;
						}

						string renamedBindingChannel;
						if (TryGetRenamedChannelName(binding.CommunicationChannel, channelRenames, out renamedBindingChannel))
						{
							binding.CommunicationChannel = renamedBindingChannel;
							jobContainsChangedTask = true;
							changed++;
						}
					}
				}
			}

			string renamedJobChannel;
			if (jobContainsChangedTask &&
				TryGetRenamedChannelName(job.ChannelName, channelRenames, out renamedJobChannel))
			{
				job.ChannelName = renamedJobChannel;
				changed++;
			}

			return changed;
		}

		private static bool IsTargetCommunication(
			string actualProtocolName,
			string actualInstanceName,
			string targetProtocolName,
			string targetInstanceName,
			CommunicationConfig communicationConfig)
		{
			string protocolName = NormalizeProtocolName(actualProtocolName);
			if (string.IsNullOrWhiteSpace(protocolName))
			{
				protocolName = targetProtocolName;
			}

			if (!string.Equals(protocolName, targetProtocolName, StringComparison.OrdinalIgnoreCase))
			{
				return false;
			}

			string normalizedInstance = CommunicationRuntimeNaming.NormalizeInstanceName(protocolName, actualInstanceName, communicationConfig);
			return string.Equals(normalizedInstance, targetInstanceName, StringComparison.OrdinalIgnoreCase);
		}

		private static bool TryGetRenamedChannelName(
			string channelName,
			Dictionary<string, string> channelRenames,
			out string renamedChannelName)
		{
			renamedChannelName = string.Empty;
			if (channelRenames == null || channelRenames.Count <= 0)
			{
				return false;
			}

			string normalized = NormalizeChannelName(channelName);
			return channelRenames.TryGetValue(normalized, out renamedChannelName);
		}

		private static void MergeDuplicateChannels(ProtocolFlowConfig protocol)
		{
			if (protocol == null || protocol.Channels == null)
			{
				return;
			}

			List<ChannelFlowConfig> merged = new List<ChannelFlowConfig>();
			foreach (ChannelFlowConfig channel in protocol.Channels)
			{
				if (channel == null)
				{
					continue;
				}

				string channelName = NormalizeChannelName(channel.ChannelName);
				ChannelFlowConfig existing = merged.FirstOrDefault(x =>
					x != null && string.Equals(NormalizeChannelName(x.ChannelName), channelName, StringComparison.OrdinalIgnoreCase));
				if (existing == null)
				{
					channel.ChannelName = channelName;
					if (channel.Jobs == null) channel.Jobs = new List<JobConfig>();
					merged.Add(channel);
					continue;
				}

				if (string.IsNullOrWhiteSpace(existing.ActiveProgramNo))
				{
					existing.ActiveProgramNo = channel.ActiveProgramNo;
				}

				if (channel.Jobs == null)
				{
					continue;
				}

				foreach (JobConfig job in channel.Jobs)
				{
					if (job == null)
					{
						continue;
					}

					job.ChannelName = channelName;
					existing.Jobs.Add(job);
				}
			}

			protocol.Channels = merged;
		}

		private static void RenameLegacyCommunicationChannelFolders(
			string protocolName,
			Dictionary<string, string> channelRenames)
		{
			if (channelRenames == null || channelRenames.Count <= 0)
			{
				return;
			}

			foreach (KeyValuePair<string, string> pair in channelRenames)
			{
				try
				{
					string oldFolder = PathManager.GetChannelFolder(protocolName, pair.Key);
					string newFolder = PathManager.GetChannelFolder(protocolName, pair.Value);
					if (!Directory.Exists(oldFolder) ||
						string.Equals(oldFolder, newFolder, StringComparison.OrdinalIgnoreCase))
					{
						continue;
					}

					if (!Directory.Exists(newFolder))
					{
						Directory.Move(oldFolder, newFolder);
						continue;
					}

					MoveDirectoryContent(oldFolder, newFolder);
					if (Directory.Exists(oldFolder) &&
						Directory.GetFiles(oldFolder, "*", SearchOption.AllDirectories).Length == 0)
					{
						Directory.Delete(oldFolder, true);
					}
				}
				catch
				{
				}
			}
		}

		private static string ResolveJobProtocol(JobConfig job)
		{
			if (job == null)
			{
				return "TCP/IP";
			}

			if (!string.IsNullOrWhiteSpace(job.ProtocolName))
			{
				return NormalizeProtocolName(job.ProtocolName);
			}

			TaskConfig task = job.Tasks == null ? null : job.Tasks.FirstOrDefault(x => x != null && !string.IsNullOrWhiteSpace(x.CommunicationProtocol));
			return NormalizeProtocolName(task == null ? "TCP/IP" : task.CommunicationProtocol);
		}

		private static string ResolveJobChannel(JobConfig job)
		{
			if (job == null)
			{
				return "Channel01";
			}

			if (!string.IsNullOrWhiteSpace(job.ChannelName))
			{
				return NormalizeChannelName(job.ChannelName);
			}

			TaskConfig task = job.Tasks == null ? null : job.Tasks.FirstOrDefault(x => x != null && !string.IsNullOrWhiteSpace(x.CommunicationChannel));
			return NormalizeChannelName(task == null ? "Channel01" : task.CommunicationChannel);
		}

		public static string NormalizeProtocolName(string protocolName)
		{
			if (string.IsNullOrWhiteSpace(protocolName))
			{
				return "TCP/IP";
			}

			if (protocolName.Equals("TcpIp", StringComparison.OrdinalIgnoreCase) ||
				protocolName.Replace("/", string.Empty).Equals("TCPIP", StringComparison.OrdinalIgnoreCase))
			{
				return "TCP/IP";
			}

			return protocolName.Trim();
		}

		public static string NormalizeChannelName(string channelName)
		{
			return string.IsNullOrWhiteSpace(channelName) ? "Channel01" : channelName.Trim();
		}

		private static string DeriveProgramNo(string jobName)
		{
			if (string.IsNullOrWhiteSpace(jobName))
			{
				return "1";
			}

			string digits = new string(jobName.Where(char.IsDigit).ToArray());
			int value;
			return int.TryParse(digits, out value) && value > 0 ? value.ToString() : "1";
		}

		private static void NormalizeStepSourcePath(string jobName, string taskName, StepConfig step)
		{
			NormalizeStepSourcePath("TCP/IP", "Channel01", jobName, taskName, step);
		}

		private static void NormalizeStepSourcePath(string protocolName, string channelName, string jobName, string taskName, StepConfig step)
		{
			if (step == null || string.IsNullOrWhiteSpace(step.ProjectFilePath))
			{
				return;
			}

			try
			{
				string taskFolder = PathManager.GetTaskFolder(protocolName, channelName, jobName, taskName);
				string projectPath = Path.IsPathRooted(step.ProjectFilePath)
					? step.ProjectFilePath
					: Path.Combine(taskFolder, step.ProjectFilePath);

				if (!File.Exists(projectPath))
				{
					return;
				}

				if (string.IsNullOrWhiteSpace(step.SourceFilePath) ||
					!IsPathUnderFolder(step.SourceFilePath, taskFolder) ||
					string.Equals(Path.GetFileName(step.SourceFilePath), Path.GetFileName(projectPath), StringComparison.OrdinalIgnoreCase))
				{
					step.SourceFilePath = projectPath;
				}
			}
			catch
			{
			}
		}

		private static void NormalizeStepProjectRelativePaths(StepConfig step)
		{
			if (step == null)
			{
				return;
			}

			step.ProjectFilePath = NormalizeStepProjectRelativePath(step.ProjectFilePath);

			if (step.VppFiles != null)
			{
				for (int i = 0; i < step.VppFiles.Count; i++)
				{
					step.VppFiles[i] = NormalizeStepProjectRelativePath(step.VppFiles[i]);
				}
			}

			if (step.ScriptFiles != null)
			{
				for (int i = 0; i < step.ScriptFiles.Count; i++)
				{
					step.ScriptFiles[i] = NormalizeStepProjectRelativePath(step.ScriptFiles[i]);
				}
			}
		}

		private static string NormalizeStepProjectRelativePath(string path)
		{
			if (string.IsNullOrWhiteSpace(path) || Path.IsPathRooted(path))
			{
				return path;
			}

			string normalized = path.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);
			string legacyPrefix = "Scripts" + Path.DirectorySeparatorChar;

			if (normalized.StartsWith(legacyPrefix, StringComparison.OrdinalIgnoreCase))
			{
				return "Script" + Path.DirectorySeparatorChar + normalized.Substring(legacyPrefix.Length);
			}

			if (normalized.Equals("Scripts", StringComparison.OrdinalIgnoreCase))
			{
				return "Script";
			}

			return path;
		}

		private static bool IsPathUnderFolder(string path, string folder)
		{
			if (string.IsNullOrWhiteSpace(path) || string.IsNullOrWhiteSpace(folder))
			{
				return false;
			}

			try
			{
				string fullPath = Path.GetFullPath(path).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
				string fullFolder = Path.GetFullPath(folder).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
				return fullPath.StartsWith(fullFolder + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase) ||
					string.Equals(fullPath, fullFolder, StringComparison.OrdinalIgnoreCase);
			}
			catch
			{
				return false;
			}
		}

		private static void EnsureStepFolders(ProjectFlowConfig config)
		{
			ProjectPathManager path = PathManager;

			if (config == null || config.Jobs == null)
			{
				return;
			}

			foreach (JobConfig job in config.Jobs)
			{
				if (job == null || string.IsNullOrWhiteSpace(job.JobName))
				{
					continue;
				}

				if (job.Tasks == null)
				{
					continue;
				}

				foreach (TaskConfig task in job.Tasks)
				{
					if (task == null || string.IsNullOrWhiteSpace(task.TaskName))
					{
						continue;
					}

					// 新标准路径：
					// Project\Task\<TaskName>\<ProgramNo>
					string taskFolder = path.GetTaskFolder(job.ProtocolName, job.ChannelName, job.JobName, task.TaskName);
					Directory.CreateDirectory(taskFolder);

					// 兼容迁移旧路径：
					// Project\<TaskName>\<ProgramNo>
					// Project\Job\<JobName>\<TaskName>
					// Project\Job\<JobName>\Task\<TaskName>
					// Project\Communicate\<Protocol>\<Channel>\<JobName>\Task\<TaskName>
					foreach (string legacyTaskFolder in path.GetTaskFolderCandidates(job.ProtocolName, job.ChannelName, job.JobName, task.TaskName))
					{
						if (!Directory.Exists(legacyTaskFolder) ||
							string.Equals(legacyTaskFolder, taskFolder, StringComparison.OrdinalIgnoreCase))
						{
							continue;
						}

						try
						{
							MoveDirectoryContent(legacyTaskFolder, taskFolder);
							if (Directory.Exists(legacyTaskFolder) &&
								Directory.GetFiles(legacyTaskFolder, "*", SearchOption.AllDirectories).Length == 0)
							{
								Directory.Delete(legacyTaskFolder, true);
							}
						}
						catch
						{
							// 迁移失败不影响软件启动和保存，后续可以手动处理旧目录。
						}
					}

					NormalizeTaskStepSubFolders(taskFolder);

					if (task.Steps == null)
					{
						continue;
					}

					foreach (StepConfig step in task.Steps)
					{
						if (step == null)
						{
							continue;
						}

						if (!string.IsNullOrEmpty(step.ProjectFilePath) ||
							(step.VppFiles != null && step.VppFiles.Count > 0) ||
							(step.ScriptFiles != null && step.ScriptFiles.Count > 0))
						{
							Directory.CreateDirectory(Path.Combine(taskFolder, "VPP"));
							Directory.CreateDirectory(Path.Combine(taskFolder, "Script"));
							Directory.CreateDirectory(Path.Combine(taskFolder, "Hdev"));
						}

						NormalizeStepSourcePath(job.ProtocolName, job.ChannelName, job.JobName, task.TaskName, step);
					}
				}
			}
		}

		private static void NormalizeTaskStepSubFolders(string taskFolder)
		{
			if (string.IsNullOrWhiteSpace(taskFolder))
			{
				return;
			}

			Directory.CreateDirectory(Path.Combine(taskFolder, "VPP"));
			Directory.CreateDirectory(Path.Combine(taskFolder, "Script"));
			Directory.CreateDirectory(Path.Combine(taskFolder, "Hdev"));

			string legacyScripts = Path.Combine(taskFolder, "Scripts");
			string script = Path.Combine(taskFolder, "Script");

			if (!Directory.Exists(legacyScripts) ||
				string.Equals(legacyScripts, script, StringComparison.OrdinalIgnoreCase))
			{
				return;
			}

			try
			{
				MoveDirectoryContent(legacyScripts, script);
				if (Directory.Exists(legacyScripts) &&
					Directory.GetFiles(legacyScripts, "*", SearchOption.AllDirectories).Length == 0)
				{
					Directory.Delete(legacyScripts, true);
				}
			}
			catch
			{
			}
		}

		private static void MoveDirectoryContent(string sourceDir, string targetDir)
		{
			if (string.IsNullOrWhiteSpace(sourceDir) || string.IsNullOrWhiteSpace(targetDir))
			{
				return;
			}

			if (!Directory.Exists(sourceDir))
			{
				return;
			}

			if (!Directory.Exists(targetDir))
			{
				Directory.CreateDirectory(targetDir);
			}

			foreach (string file in Directory.GetFiles(sourceDir))
			{
				string targetFile = Path.Combine(targetDir, Path.GetFileName(file));

				if (File.Exists(targetFile))
				{
					File.Delete(targetFile);
				}

				File.Move(file, targetFile);
			}

			foreach (string dir in Directory.GetDirectories(sourceDir))
			{
				string targetSubDir = Path.Combine(targetDir, Path.GetFileName(dir));
				MoveDirectoryContent(dir, targetSubDir);

				try
				{
					if (Directory.Exists(dir) && Directory.GetFiles(dir).Length == 0 && Directory.GetDirectories(dir).Length == 0)
					{
						Directory.Delete(dir, false);
					}
				}
				catch
				{
				}
			}
		}

		private static void MigrateLegacyProjectJobRoot(ProjectFlowConfig config, bool loadedFromLegacyJobFile)
		{
			try
			{
				if (loadedFromLegacyJobFile && !File.Exists(FlowConfigFile))
				{
					XmlConfigHelper.Save(FlowConfigFile, config);
				}

				MigrateLegacyProgramHardware(config);
				ArchiveLegacyProjectJobRoot();
			}
			catch
			{
			}
		}

		private static void MigrateLegacyProgramHardware(ProjectFlowConfig config)
		{
			string legacyRoot = Path.Combine(ProjectRoot, "Job");
			if (!Directory.Exists(legacyRoot))
			{
				return;
			}

			List<string> jobNames = new List<string>();
			if (config != null && config.Jobs != null)
			{
				foreach (JobConfig job in config.Jobs)
				{
					if (job == null || string.IsNullOrWhiteSpace(job.JobName))
					{
						continue;
					}

					string safeJob = PathManager.MakeSafeName(job.JobName);
					if (!jobNames.Any(x => string.Equals(x, safeJob, StringComparison.OrdinalIgnoreCase)))
					{
						jobNames.Add(safeJob);
					}
				}
			}

			foreach (string legacyJobFolder in Directory.GetDirectories(legacyRoot))
			{
				string safeJob = Path.GetFileName(legacyJobFolder);
				if (string.IsNullOrWhiteSpace(safeJob))
				{
					continue;
				}

				if (jobNames.Count > 0 && !jobNames.Any(x => string.Equals(x, safeJob, StringComparison.OrdinalIgnoreCase)))
				{
					continue;
				}

				string currentProgramFolder = Path.Combine(ProjectRoot, "Config", "Program", safeJob);
				string legacyHardware = Path.Combine(legacyJobFolder, "Hardware");
				string legacyCamera = Path.Combine(legacyJobFolder, "Camera");

				if (Directory.Exists(legacyHardware))
				{
					MoveDirectoryContent(legacyHardware, Path.Combine(currentProgramFolder, "Hardware"));
				}

				if (Directory.Exists(legacyCamera))
				{
					MoveDirectoryContent(legacyCamera, Path.Combine(currentProgramFolder, "Hardware", "Camera"));
				}
			}
		}

		private static void ArchiveLegacyProjectJobRoot()
		{
			string legacyRoot = Path.Combine(ProjectRoot, "Job");
			if (!Directory.Exists(legacyRoot))
			{
				return;
			}

			if (!File.Exists(FlowConfigFile) && File.Exists(Path.Combine(legacyRoot, "ProjectFlowConfig.xml")))
			{
				return;
			}

			string archiveRoot = Path.Combine(ProjectRoot, "Config", "Legacy");
			Directory.CreateDirectory(archiveRoot);

			string archiveFolder = Path.Combine(archiveRoot, "Job_" + DateTime.Now.ToString("yyyyMMdd_HHmmss"));
			int suffix = 1;
			while (Directory.Exists(archiveFolder))
			{
				archiveFolder = Path.Combine(archiveRoot, "Job_" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + "_" + suffix.ToString());
				suffix++;
			}

			Directory.Move(legacyRoot, archiveFolder);
		}


	}

	public interface IVisionStep
	{
		string StepName { get; }
		StepType StepType { get; }
		StepResult Execute(VisionRunContext context);
	}

	public class VppStep : IVisionStep
	{
		private readonly StepConfig _config;

		public string StepName { get { return _config.StepName; } }
		public StepType StepType { get { return StepType.Vpp; } }

		public VppStep(StepConfig config)
		{
			_config = config;
		}

		public StepResult Execute(VisionRunContext context)
		{
			Stopwatch sw = Stopwatch.StartNew();
			StepResult result = new StepResult();

			try
			{
				string filePath = ResolveVppPath(context);
				if (string.IsNullOrWhiteSpace(filePath) || !File.Exists(filePath))
				{
					throw new Exception("VPP file was not found: " + filePath);
				}

				object toolBlock = VisionProReflectionHelper.LoadObjectFromFile(filePath);
				if (toolBlock == null)
				{
					throw new Exception("VPP load returned null: " + filePath);
				}

				List<object> inputTerminals = GetTerminals(GetPropertyValue(toolBlock, "Inputs"));
				ApplyImageInputs(inputTerminals, context);
				ApplyGlobalInputs(inputTerminals);

				MethodInfo runMethod = toolBlock.GetType().GetMethod("Run", Type.EmptyTypes);
				if (runMethod == null)
				{
					throw new Exception("VPP object does not provide Run().");
				}

				runMethod.Invoke(toolBlock, null);
				ReadOutputs(toolBlock, result);
				ReadDisplayImage(toolBlock, result);
				AlgorithmRuntimeSnapshotStore.Instance.SetRunningToolBlock(
					context.JobName, context.TaskName, _config.StepName, Path.GetFileName(filePath), toolBlock);
				result.Message = "VPP step executed.";
			}
			catch (Exception ex)
			{
				result.IsOK = false;
				TargetInvocationException targetException = ex as TargetInvocationException;
				result.Message = targetException != null && targetException.InnerException != null
					? targetException.InnerException.Message
					: ex.Message;
			}
			finally
			{
				sw.Stop();
				result.CostMs = sw.Elapsed.TotalMilliseconds;
			}

			return result;
		}

		private string ResolveVppPath(VisionRunContext context)
		{
			List<string> candidates = new List<string>();
			string taskFolder = GetRuntimeTaskFolder(context);
			AddFileCandidate(candidates, _config.ProjectFilePath, taskFolder);

			if (_config.VppFiles != null)
			{
				foreach (string file in _config.VppFiles)
				{
					AddFileCandidate(candidates, file, taskFolder);
				}
			}

			AddFileCandidate(candidates, _config.SourceFilePath, taskFolder);
			return candidates.FirstOrDefault(File.Exists) ?? (candidates.Count > 0 ? candidates[0] : string.Empty);
		}

		private string GetRuntimeTaskFolder(VisionRunContext context)
		{
			string protocolName = context == null ? string.Empty : Convert.ToString(context.GetData("Comm.Protocol"));
			string channelName = context == null ? string.Empty : Convert.ToString(context.GetData("Comm.Channel"));
			string jobName = context == null ? string.Empty : context.JobName;
			string taskName = context == null ? string.Empty : context.TaskName;

			return FlowConfigStore.PathManager.GetTaskFolder(protocolName, channelName, jobName, taskName);
		}

		private void AddFileCandidate(List<string> candidates, string file, string taskFolder)
		{
			if (string.IsNullOrWhiteSpace(file))
			{
				return;
			}

			string candidate = Path.IsPathRooted(file) ? file : Path.Combine(taskFolder, file);
			if (!candidates.Contains(candidate, StringComparer.OrdinalIgnoreCase))
			{
				candidates.Add(candidate);
			}
		}

		private void ApplyImageInputs(List<object> terminals, VisionRunContext context)
		{
			List<string> sourceKeys = RuntimeImageSourceParser.SplitImageSources(_config.InputImageKey);
			if (sourceKeys.Count <= 0)
			{
				return;
			}

			List<object> imageTerminals = terminals.Where(IsImageTerminal).ToList();
			if (sourceKeys.Count > imageTerminals.Count)
			{
				throw new Exception("VPP image input count is insufficient. Sources=" +
					sourceKeys.Count.ToString() + ", ImageInputs=" + imageTerminals.Count.ToString());
			}

			for (int i = 0; i < sourceKeys.Count; i++)
			{
				VisionImage image;
				if (!context.TryGetImage(sourceKeys[i], out image) || image == null || image.RawImage == null)
				{
					throw new Exception("VPP input image is null. Source=" + sourceKeys[i]);
				}

				SetTerminalValue(imageTerminals[i], image.RawImage);
			}
		}

		private void ApplyGlobalInputs(List<object> terminals)
		{
			if (_config.InputPins == null)
			{
				return;
			}

			foreach (PinConfig pin in _config.InputPins)
			{
				if (pin == null || string.IsNullOrWhiteSpace(pin.GlobalVariableName) || pin.DataType == PinDataType.Image)
				{
					continue;
				}

				object terminal = FindTerminal(terminals, pin.PinName);
				object value;
				if (terminal != null && GlobalVariableStore.TryGetValue(pin.GlobalVariableName, out value))
				{
					SetTerminalValue(terminal, ConvertForTerminal(value, GetPropertyValue(terminal, "Value")));
				}
			}
		}

		private void ReadOutputs(object toolBlock, StepResult result)
		{
			foreach (object terminal in GetTerminals(GetPropertyValue(toolBlock, "Outputs")))
			{
				string name = Convert.ToString(GetPropertyValue(terminal, "Name"));
				if (string.IsNullOrWhiteSpace(name))
				{
					continue;
				}

				object value = GetPropertyValue(terminal, "Value");
				result.Outputs[name] = value;

				PinConfig pin = _config.OutputPins == null ? null : _config.OutputPins.FirstOrDefault(x =>
					x != null && string.Equals(x.PinName, name, StringComparison.OrdinalIgnoreCase));
				if (pin != null && !string.IsNullOrWhiteSpace(pin.GlobalVariableName))
				{
					GlobalVariableStore.SetValue(pin.GlobalVariableName, value);
				}

				if (IsImageTerminal(terminal) && value != null)
				{
					VisionImage image = new VisionImage();
					image.ImageName = name;
					image.ImageType = "VisionPro";
					image.SourceStep = _config.StepName;
					image.RawImage = value;
					result.OutputImages[name] = image;
				}
			}
		}

		private void ReadDisplayImage(object toolBlock, StepResult result)
		{
			string outputKey = _config.DisplayOutputKey;
			if (string.IsNullOrWhiteSpace(outputKey) ||
				outputKey.Equals("Not Use", StringComparison.OrdinalIgnoreCase))
			{
				return;
			}

			VisionImage existingImage;
			if (result.OutputImages.TryGetValue(outputKey, out existingImage) && existingImage != null)
			{
				AttachLastRunDisplayRecord(toolBlock, outputKey, existingImage);
				return;
			}

			object displayRecord;
			string displayRecordKey;
			object value = TryGetLastRunImage(toolBlock, outputKey, out displayRecord, out displayRecordKey);
			if (value == null)
			{
				return;
			}

			VisionImage image = new VisionImage();
			image.ImageName = outputKey;
			image.ImageType = "VisionProRecord";
			image.SourceStep = _config.StepName;
			image.RawImage = value;
			image.DisplayRecord = displayRecord;
			image.DisplayRecordKey = displayRecordKey;
			result.OutputImages[outputKey] = image;
		}

		private void AttachLastRunDisplayRecord(object toolBlock, string outputKey, VisionImage image)
		{
			if (image == null || image.DisplayRecord != null)
			{
				return;
			}

			object displayRecord;
			string displayRecordKey;
			object value = TryGetLastRunImage(toolBlock, outputKey, out displayRecord, out displayRecordKey);
			if (displayRecord == null || string.IsNullOrWhiteSpace(displayRecordKey))
			{
				return;
			}

			image.DisplayRecord = displayRecord;
			image.DisplayRecordKey = displayRecordKey;
			if (image.RawImage == null && value != null)
			{
				image.RawImage = value;
			}
		}

		private object TryGetLastRunImage(object toolBlock, string outputKey, out object displayRecord, out string displayRecordKey)
		{
			displayRecord = null;
			displayRecordKey = string.Empty;
			if (toolBlock == null || string.IsNullOrWhiteSpace(outputKey) ||
				(outputKey.StartsWith("LastRun.", StringComparison.OrdinalIgnoreCase) &&
				 outputKey.Length <= "LastRun.".Length))
			{
				return null;
			}

			object record = CreateLastRunRecord(toolBlock);
			if (record == null)
			{
				return null;
			}

			object rootRecord = record;
			string relativeKey = outputKey.StartsWith("LastRun.", StringComparison.OrdinalIgnoreCase)
				? outputKey.Substring("LastRun.".Length)
				: outputKey.Trim();

			object imageRecord = FindRecordByKey(record, relativeKey);
			if (!IsImageRecord(imageRecord))
			{
				imageRecord = FindPreferredLastRunImageRecord(record, relativeKey);
			}

			if (imageRecord != null)
			{
				displayRecord = rootRecord;
				displayRecordKey = Convert.ToString(GetPropertyValue(imageRecord, "RecordKey"));
				object imageContent = GetPropertyValue(imageRecord, "Content");
				return imageContent ?? GetPropertyValue(imageRecord, "Image");
			}

			if (!outputKey.StartsWith("LastRun.", StringComparison.OrdinalIgnoreCase))
			{
				return null;
			}

			string[] parts = relativeKey
				.Split(new char[] { '.' }, StringSplitOptions.RemoveEmptyEntries);

			foreach (string part in parts)
			{
				record = FindRecordChild(record, part);
				if (record == null)
				{
					return null;
				}
			}

			displayRecord = rootRecord;
			displayRecordKey = Convert.ToString(GetPropertyValue(record, "RecordKey"));
			object content = GetPropertyValue(record, "Content");
			return content ?? GetPropertyValue(record, "Image");
		}

		private object CreateLastRunRecord(object toolBlock)
		{
			if (toolBlock == null)
			{
				return null;
			}

			object record = null;
			MethodInfo createRecord = toolBlock.GetType().GetMethod("CreateLastRunRecord", Type.EmptyTypes);
			if (createRecord != null)
			{
				record = createRecord.Invoke(toolBlock, null);
			}

			return record ?? GetPropertyValue(toolBlock, "LastRunRecord");
		}

		private object FindPreferredLastRunImageRecord(object record, string outputKey)
		{
			List<object> imageRecords = new List<object>();
			CollectImageRecords(record, imageRecords);
			if (imageRecords.Count <= 0)
			{
				return null;
			}

			string normalizedOutputKey = NormalizeRecordKey(outputKey);
			if (!string.IsNullOrWhiteSpace(normalizedOutputKey))
			{
				foreach (object imageRecord in imageRecords)
				{
					string recordKey = NormalizeRecordKey(Convert.ToString(GetPropertyValue(imageRecord, "RecordKey")));
					if (!string.IsNullOrWhiteSpace(recordKey) &&
						(recordKey.Contains(normalizedOutputKey) || normalizedOutputKey.Contains(recordKey)))
					{
						return imageRecord;
					}
				}
			}

			foreach (object imageRecord in imageRecords)
			{
				string recordKey = Convert.ToString(GetPropertyValue(imageRecord, "RecordKey"));
				if (!string.IsNullOrWhiteSpace(recordKey) &&
					recordKey.EndsWith(".OutputImage", StringComparison.OrdinalIgnoreCase))
				{
					return imageRecord;
				}
			}

			return imageRecords[0];
		}

		private void CollectImageRecords(object record, List<object> imageRecords)
		{
			if (record == null || imageRecords == null)
			{
				return;
			}

			if (IsImageRecord(record))
			{
				imageRecords.Add(record);
			}

			object subRecords = GetPropertyValue(record, "SubRecords");
			IEnumerable values = subRecords as IEnumerable;
			if (values == null)
			{
				return;
			}

			foreach (object child in values)
			{
				CollectImageRecords(child, imageRecords);
			}
		}

		private bool IsImageRecord(object record)
		{
			if (record == null)
			{
				return false;
			}

			object content = GetPropertyValue(record, "Content") ?? GetPropertyValue(record, "Image");
			return IsVisionImageValue(content);
		}

		private bool IsVisionImageValue(object value)
		{
			if (value == null)
			{
				return false;
			}

			Type type = value.GetType();
			string fullName = type.FullName ?? string.Empty;
			if (fullName.IndexOf("CogImage", StringComparison.OrdinalIgnoreCase) >= 0 ||
				fullName.IndexOf("Bitmap", StringComparison.OrdinalIgnoreCase) >= 0)
			{
				return true;
			}

			foreach (Type interfaceType in type.GetInterfaces())
			{
				if ((interfaceType.FullName ?? string.Empty).IndexOf("ICogImage", StringComparison.OrdinalIgnoreCase) >= 0)
				{
					return true;
				}
			}

			return false;
		}

		private string NormalizeRecordKey(string value)
		{
			if (string.IsNullOrWhiteSpace(value))
			{
				return string.Empty;
			}

			StringBuilder builder = new StringBuilder();
			foreach (char c in value)
			{
				if (char.IsLetterOrDigit(c))
				{
					builder.Append(char.ToLowerInvariant(c));
				}
			}

			string text = builder.ToString();
			if (text.StartsWith("lastrun", StringComparison.OrdinalIgnoreCase))
			{
				text = text.Substring("lastrun".Length);
			}

			return text;
		}

		private object FindRecordChild(object record, string name)
		{
			object subRecords = GetPropertyValue(record, "SubRecords");
			if (subRecords == null)
			{
				return null;
			}

			try
			{
				PropertyInfo itemProperty = subRecords.GetType().GetProperty("Item", new Type[] { typeof(string) });
				if (itemProperty != null)
				{
					object named = itemProperty.GetValue(subRecords, new object[] { name });
					if (named != null)
					{
						return named;
					}
				}
			}
			catch
			{
			}

			IEnumerable values = subRecords as IEnumerable;
			if (values != null)
			{
				foreach (object child in values)
				{
					string recordKey = Convert.ToString(GetPropertyValue(child, "RecordKey"));
					if (string.Equals(recordKey, name, StringComparison.OrdinalIgnoreCase) ||
						recordKey.EndsWith("." + name, StringComparison.OrdinalIgnoreCase))
					{
						return child;
					}
				}
			}

			return null;
		}

		private object FindRecordByKey(object record, string recordKey)
		{
			if (record == null || string.IsNullOrWhiteSpace(recordKey))
			{
				return null;
			}

			object subRecords = GetPropertyValue(record, "SubRecords");
			if (subRecords == null)
			{
				return null;
			}

			try
			{
				PropertyInfo itemProperty = subRecords.GetType().GetProperty("Item", new Type[] { typeof(string) });
				if (itemProperty != null)
				{
					object direct = itemProperty.GetValue(subRecords, new object[] { recordKey });
					if (direct != null)
					{
						return direct;
					}
				}
			}
			catch
			{
			}

			IEnumerable values = subRecords as IEnumerable;
			if (values == null)
			{
				return null;
			}

			foreach (object child in values)
			{
				string childKey = Convert.ToString(GetPropertyValue(child, "RecordKey"));
				if (string.Equals(childKey, recordKey, StringComparison.OrdinalIgnoreCase) ||
					childKey.EndsWith("." + recordKey, StringComparison.OrdinalIgnoreCase))
				{
					return child;
				}

				object nested = FindRecordByKey(child, recordKey);
				if (nested != null)
				{
					return nested;
				}
			}

			return null;
		}

		private List<object> GetTerminals(object collection)
		{
			List<object> result = new List<object>();
			IEnumerable enumerable = collection as IEnumerable;
			if (enumerable != null)
			{
				foreach (object item in enumerable)
				{
					if (item != null) result.Add(item);
				}
			}
			return result;
		}

		private object FindTerminal(List<object> terminals, string name)
		{
			return terminals.FirstOrDefault(x =>
				string.Equals(Convert.ToString(GetPropertyValue(x, "Name")), name, StringComparison.OrdinalIgnoreCase));
		}

		private bool IsImageTerminal(object terminal)
		{
			string name = Convert.ToString(GetPropertyValue(terminal, "Name"));
			object value = GetPropertyValue(terminal, "Value");
			string typeName = value == null ? Convert.ToString(GetPropertyValue(terminal, "ValueType")) : value.GetType().FullName;
			string text = (name + " " + typeName).ToLowerInvariant();
			return text.Contains("image") || text.Contains("cogimage") || text.Contains("bitmap");
		}

		private object GetPropertyValue(object obj, string propertyName)
		{
			if (obj == null)
			{
				return null;
			}

			PropertyInfo property = obj.GetType().GetProperty(propertyName);
			return property == null ? null : property.GetValue(obj, null);
		}

		private void SetTerminalValue(object terminal, object value)
		{
			PropertyInfo property = terminal == null ? null : terminal.GetType().GetProperty("Value");
			if (property == null || !property.CanWrite)
			{
				throw new Exception("VPP input terminal cannot be assigned.");
			}

			property.SetValue(terminal, value, null);
		}

		private object ConvertForTerminal(object value, object oldValue)
		{
			if (value == null || oldValue == null || oldValue.GetType().IsInstanceOfType(value))
			{
				return value;
			}

			try
			{
				return Convert.ChangeType(value, oldValue.GetType());
			}
			catch
			{
				return value;
			}
		}
	}

	public sealed class AlgorithmRuntimeSnapshotStore : IAlgorithmRuntimeSnapshotProvider
	{
		private readonly ConcurrentDictionary<string, object> _toolBlocks =
			new ConcurrentDictionary<string, object>(StringComparer.OrdinalIgnoreCase);

		public static AlgorithmRuntimeSnapshotStore Instance { get; } = new AlgorithmRuntimeSnapshotStore();

		private AlgorithmRuntimeSnapshotStore()
		{
		}

		public void SetRunningToolBlock(string jobName, string taskName, string stepName, string fileName, object toolBlock)
		{
			if (toolBlock == null)
			{
				return;
			}

			SetSnapshot(jobName, taskName, stepName, toolBlock);
			SetSnapshot(jobName, taskName, fileName, toolBlock);
		}

		public object TryGetRunningToolBlock(string jobName, string taskName, string vppName)
		{
			object value;
			return _toolBlocks.TryGetValue(GetKey(jobName, taskName, vppName), out value) ? value : null;
		}

		private void SetSnapshot(string jobName, string taskName, string name, object value)
		{
			if (!string.IsNullOrWhiteSpace(name))
			{
				_toolBlocks[GetKey(jobName, taskName, name)] = value;
			}
		}

		private string GetKey(string jobName, string taskName, string name)
		{
			return (jobName ?? string.Empty) + "|" + (taskName ?? string.Empty) + "|" + (name ?? string.Empty);
		}
	}

	public class ScriptStep : IVisionStep
	{
		private readonly StepConfig _config;

		public string StepName { get { return _config.StepName; } }
		public StepType StepType { get { return StepType.Script; } }

		public ScriptStep(StepConfig config)
		{
			_config = config;
		}

		public StepResult Execute(VisionRunContext context)
		{
			Stopwatch sw = Stopwatch.StartNew();
			StepResult result = new StepResult();

			try
			{
				// TODO:
				// 后续这里执行 Step 文件夹 Script 内部脚本。
				// 第一阶段先做输入输出引脚的数据整理。

				foreach (PinConfig pin in _config.OutputPins)
				{
					if (!string.IsNullOrEmpty(pin.SourceKey))
					{
						object value;
						if (context.TryGetData(pin.SourceKey, out value))
						{
							result.Outputs[pin.PinName] = value;
						}
					}
				}

				result.Outputs["PLC.ResultOK"] = true;
				result.Message = "Script step demo executed.";
			}
			catch (Exception ex)
			{
				result.IsOK = false;
				result.Message = ex.Message;
			}
			finally
			{
				sw.Stop();
				result.CostMs = sw.Elapsed.TotalMilliseconds;
			}

			return result;
		}
	}

	public class HalconStep : IVisionStep
	{
		private readonly StepConfig _config;
		private static readonly object _halconLoadLock = new object();
		private static readonly object _programCacheLock = new object();
		private static readonly Dictionary<string, CachedHalconProgram> _programCache = new Dictionary<string, CachedHalconProgram>(StringComparer.OrdinalIgnoreCase);
		private static bool _halconAssembliesLoaded;
		private static string _halconAssemblyLoadMessage = string.Empty;

		private class CachedHalconProgram
		{
			public object Program { get; set; }
			public long LastWriteTicks { get; set; }
		}

		[DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
		private static extern bool SetDllDirectory(string lpPathName);

		public string StepName { get { return _config.StepName; } }
		public StepType StepType { get { return StepType.Halcon; } }

		public HalconStep(StepConfig config)
		{
			_config = config;
		}

		public StepResult Execute(VisionRunContext context)
		{
			Stopwatch sw = Stopwatch.StartNew();
			StepResult result = new StepResult();

			try
			{
				string filePath = ResolveHdevPath(context);
				if (string.IsNullOrWhiteSpace(filePath) || !File.Exists(filePath))
				{
					throw new Exception("Hdev file was not found: " + filePath);
				}

				Type programType = FindHalconType("HalconDotNet.HDevProgram");
				Type callType = FindHalconType("HalconDotNet.HDevProgramCall");
				if (programType == null || callType == null)
				{
					throw new Exception("HALCON .NET runtime was not found. Please check halcondotnet.dll and hdevenginedotnet.dll. " + _halconAssemblyLoadMessage);
				}

				string engineFilePath = PrepareHdevFileForEngine(filePath, _config);
				object program = GetOrLoadProgram(engineFilePath, programType);
				object call = Activator.CreateInstance(callType, new object[] { program });

				ApplyInputPinValues(call);
				ApplyGlobalInputs(call);
				InvokeNoArg(call, "Execute");
				List<string> missingOutputs = ReadConfiguredOutputs(call, result);
				EnsureResultStatus(result);

				result.Message = BuildSuccessMessage(result, missingOutputs);
			}
			catch (Exception ex)
			{
				result.IsOK = false;
				TargetInvocationException targetException = ex as TargetInvocationException;
				result.Message = targetException != null && targetException.InnerException != null
					? targetException.InnerException.Message
					: ex.Message;
			}
			finally
			{
				sw.Stop();
				result.CostMs = sw.Elapsed.TotalMilliseconds;
			}

			return result;
		}

		private static string PrepareHdevFileForEngine(string filePath, StepConfig config)
		{
			string sourceText;
			try
			{
				sourceText = File.ReadAllText(filePath, Encoding.UTF8);
			}
			catch
			{
				return filePath;
			}

			string patchedText = sourceText;
			string injectedInterface = BuildHdevOutputInterface(config);
			int interfaceIndex;
			int interfaceLength;
			if (!string.IsNullOrWhiteSpace(injectedInterface) &&
				TryFindEmptyHdevInterface(patchedText, out interfaceIndex, out interfaceLength))
			{
				patchedText =
					patchedText.Substring(0, interfaceIndex) +
					injectedInterface +
					patchedText.Substring(interfaceIndex + interfaceLength);
			}

			patchedText = PatchHdevDisplayWindowForEngine(patchedText);
			if (string.Equals(sourceText, patchedText, StringComparison.Ordinal))
			{
				return filePath;
			}

			try
			{
				string tempName =
					SanitizeFileName(Path.GetFileNameWithoutExtension(filePath)) +
					"_" +
					File.GetLastWriteTimeUtc(filePath).Ticks.ToString() +
					"_outputs.hdev";

				foreach (string tempFolder in GetHdevEngineCacheFolders(filePath))
				{
					try
					{
						Directory.CreateDirectory(tempFolder);
						string tempPath = Path.Combine(tempFolder, tempName);

						if (!File.Exists(tempPath) || !FileEquals(tempPath, patchedText))
						{
							File.WriteAllText(tempPath, patchedText, new UTF8Encoding(false));
						}

						return tempPath;
					}
					catch
					{
					}
				}
			}
			catch
			{
			}

			return filePath;
		}

		private static string PatchHdevDisplayWindowForEngine(string text)
		{
			if (string.IsNullOrWhiteSpace(text) ||
				text.IndexOf("dev_open_window_fit_size", StringComparison.OrdinalIgnoreCase) < 0)
			{
				return text;
			}

			const string pattern =
				@"<l>\s*dev_open_window_fit_size\s*\(\s*" +
				@"(?<row>[^,]+)\s*,\s*" +
				@"(?<column>[^,]+)\s*,\s*" +
				@"(?<width>[^,]+)\s*,\s*" +
				@"(?<height>[^,]+)\s*,\s*" +
				@"(?<widthLimit>[^,]+)\s*,\s*" +
				@"(?<heightLimit>[^,]+)\s*,\s*" +
				@"(?<handle>[^)]+)\s*\)\s*</l>";

			Match firstMatch = Regex.Match(text, pattern, RegexOptions.IgnoreCase);
			string windowHandle = firstMatch.Success ? firstMatch.Groups["handle"].Value.Trim() : "WindowHandle";
			string patched = Regex.Replace(
				text,
				pattern,
				delegate(Match match)
				{
					string row = match.Groups["row"].Value.Trim();
					string column = match.Groups["column"].Value.Trim();
					string width = match.Groups["width"].Value.Trim();
					string height = match.Groups["height"].Value.Trim();
					string handle = match.Groups["handle"].Value.Trim();

					return
						"<l>open_window (" + row + ", " + column + ", " + width + ", " + height + ", 0, 'invisible', '', " + handle + ")</l>" + Environment.NewLine +
						"<l>set_part (" + handle + ", 0, 0, " + height + " - 1, " + width + " - 1)</l>" + Environment.NewLine +
						"<l>dev_set_window (" + handle + ")</l>";
				},
				RegexOptions.IgnoreCase);

			return PatchHdevDisplayOperatorsForEngine(patched, windowHandle);
		}

		private static string PatchHdevDisplayOperatorsForEngine(string text, string windowHandle)
		{
			if (string.IsNullOrWhiteSpace(text))
			{
				return text;
			}

			string handle = string.IsNullOrWhiteSpace(windowHandle) ? "WindowHandle" : windowHandle.Trim();

			text = Regex.Replace(
				text,
				@"<l>(?<indent>\s*)dev_display\s*\(\s*(?<args>.*?)\s*\)\s*</l>",
				"<l>${indent}disp_obj (${args}, " + handle + ")</l>",
				RegexOptions.IgnoreCase);

			text = Regex.Replace(
				text,
				@"<l>(?<indent>\s*)dev_set_color\s*\(\s*(?<args>.*?)\s*\)\s*</l>",
				"<l>${indent}set_color (" + handle + ", ${args})</l>",
				RegexOptions.IgnoreCase);

			text = Regex.Replace(
				text,
				@"<l>(?<indent>\s*)dev_set_line_width\s*\(\s*(?<args>.*?)\s*\)\s*</l>",
				"<l>${indent}set_line_width (" + handle + ", ${args})</l>",
				RegexOptions.IgnoreCase);

			text = Regex.Replace(
				text,
				@"<l>(?<indent>\s*)dev_set_draw\s*\(\s*(?<args>.*?)\s*\)\s*</l>",
				"<l>${indent}set_draw (" + handle + ", ${args})</l>",
				RegexOptions.IgnoreCase);

			text = Regex.Replace(
				text,
				@"<l>(?<indent>\s*)dev_disp_text\s*\(\s*(?<args>.*?)\s*\)\s*</l>",
				"<l>${indent}disp_text (" + handle + ", ${args})</l>",
				RegexOptions.IgnoreCase);

			text = Regex.Replace(
				text,
				@"<l>(?<indent>\s*)dev_clear_window\s*\(\s*\)\s*</l>",
				"<l>${indent}clear_window (" + handle + ")</l>",
				RegexOptions.IgnoreCase);

			return text;
		}

		private static string BuildHdevOutputInterface(StepConfig config)
		{
			if (config == null || config.OutputPins == null || config.OutputPins.Count == 0)
			{
				return string.Empty;
			}

			StringBuilder iconicOutputs = new StringBuilder();
			StringBuilder controlOutputs = new StringBuilder();
			foreach (PinConfig pin in config.OutputPins)
			{
				if (pin == null || string.IsNullOrWhiteSpace(pin.PinName))
				{
					continue;
				}

				string line = "<par name=\"" + EscapeXmlAttribute(pin.PinName) + "\" base_type=\"" +
					(pin.DataType == PinDataType.Image ? "iconic" : "ctrl") +
					"\" dimension=\"0\"/>";

				if (pin.DataType == PinDataType.Image)
				{
					iconicOutputs.AppendLine(line);
				}
				else
				{
					controlOutputs.AppendLine(line);
				}
			}

			if (iconicOutputs.Length == 0 && controlOutputs.Length == 0)
			{
				return string.Empty;
			}

			StringBuilder builder = new StringBuilder();
			builder.AppendLine("<interface>");
			if (iconicOutputs.Length > 0)
			{
				builder.AppendLine("<oo>");
				builder.Append(iconicOutputs);
				builder.AppendLine("</oo>");
			}

			if (controlOutputs.Length > 0)
			{
				builder.AppendLine("<oc>");
				builder.Append(controlOutputs);
				builder.AppendLine("</oc>");
			}

			builder.Append("</interface>");
			return builder.ToString();
		}

		private static IEnumerable<string> GetHdevEngineCacheFolders(string filePath)
		{
			string sourceFolder = string.Empty;
			try
			{
				sourceFolder = Path.GetDirectoryName(Path.GetFullPath(filePath));
			}
			catch
			{
			}

			if (!string.IsNullOrWhiteSpace(sourceFolder))
			{
				yield return Path.Combine(sourceFolder, ".aron_hdev_cache");
			}

			yield return Path.Combine(Path.GetTempPath(), "Aron_V3", "HdevEngine");
		}

		private static bool TryFindEmptyHdevInterface(string text, out int index, out int length)
		{
			index = -1;
			length = 0;
			if (string.IsNullOrWhiteSpace(text))
			{
				return false;
			}

			string[] selfClosing = new string[] { "<interface/>", "<interface />" };
			foreach (string marker in selfClosing)
			{
				index = text.IndexOf(marker, StringComparison.OrdinalIgnoreCase);
				if (index >= 0)
				{
					length = marker.Length;
					return true;
				}
			}

			const string openTag = "<interface>";
			const string closeTag = "</interface>";
			int openIndex = text.IndexOf(openTag, StringComparison.OrdinalIgnoreCase);
			if (openIndex < 0)
			{
				return false;
			}

			int closeIndex = text.IndexOf(closeTag, openIndex + openTag.Length, StringComparison.OrdinalIgnoreCase);
			if (closeIndex < 0)
			{
				return false;
			}

			string content = text.Substring(openIndex + openTag.Length, closeIndex - openIndex - openTag.Length);
			if (!string.IsNullOrWhiteSpace(content))
			{
				return false;
			}

			index = openIndex;
			length = closeIndex + closeTag.Length - openIndex;
			return true;
		}

		private static bool FileEquals(string path, string text)
		{
			try
			{
				return string.Equals(File.ReadAllText(path, Encoding.UTF8), text, StringComparison.Ordinal);
			}
			catch
			{
				return false;
			}
		}

		private static string SanitizeFileName(string name)
		{
			if (string.IsNullOrWhiteSpace(name))
			{
				return "hdev";
			}

			foreach (char c in Path.GetInvalidFileNameChars())
			{
				name = name.Replace(c, '_');
			}

			return name;
		}

		private static string EscapeXmlAttribute(string value)
		{
			if (value == null)
			{
				return string.Empty;
			}

			return value
				.Replace("&", "&amp;")
				.Replace("\"", "&quot;")
				.Replace("<", "&lt;")
				.Replace(">", "&gt;");
		}

		public static string PrepareProgramFileForEngine(string filePath, StepConfig config)
		{
			return PrepareHdevFileForEngine(filePath, config);
		}

		public static bool WarmUpProgram(string filePath, out string warning)
		{
			warning = string.Empty;

			try
			{
				if (string.IsNullOrWhiteSpace(filePath) || !File.Exists(filePath))
				{
					warning = "Hdev file was not found: " + filePath;
					return false;
				}

				Type programType = FindHalconType("HalconDotNet.HDevProgram");
				Type callType = FindHalconType("HalconDotNet.HDevProgramCall");
				if (programType == null || callType == null)
				{
					warning = "HALCON .NET runtime was not found. " + _halconAssemblyLoadMessage;
					return false;
				}

				GetOrLoadProgram(filePath, programType);
				return true;
			}
			catch (Exception ex)
			{
				TargetInvocationException targetException = ex as TargetInvocationException;
				warning = targetException != null && targetException.InnerException != null
					? targetException.InnerException.Message
					: ex.Message;
				return false;
			}
		}

		private static object GetOrLoadProgram(string filePath, Type programType)
		{
			if (programType == null)
			{
				throw new ArgumentNullException("programType");
			}

			string normalizedPath = Path.GetFullPath(filePath);
			long lastWriteTicks = File.GetLastWriteTimeUtc(normalizedPath).Ticks;

			lock (_programCacheLock)
			{
				CachedHalconProgram cached;
				if (_programCache.TryGetValue(normalizedPath, out cached) &&
					cached != null &&
					cached.Program != null &&
					cached.LastWriteTicks == lastWriteTicks)
				{
					return cached.Program;
				}

				object program = Activator.CreateInstance(programType, new object[] { normalizedPath });
				_programCache[normalizedPath] = new CachedHalconProgram
				{
					Program = program,
					LastWriteTicks = lastWriteTicks
				};

				return program;
			}
		}

		private string ResolveHdevPath(VisionRunContext context)
		{
			string taskFolder = GetRuntimeTaskFolder(context);
			List<string> candidates = new List<string>();
			AddFileCandidate(candidates, _config.ProjectFilePath, taskFolder);

			if (string.IsNullOrWhiteSpace(_config.ProjectFilePath) && !string.IsNullOrWhiteSpace(_config.StepName))
			{
				AddFileCandidate(candidates, Path.Combine("Hdev", _config.StepName + ".hdev"), taskFolder);
			}

			AddFileCandidate(candidates, _config.SourceFilePath, taskFolder);
			return candidates.FirstOrDefault(File.Exists) ?? (candidates.Count > 0 ? candidates[0] : string.Empty);
		}

		private string GetRuntimeTaskFolder(VisionRunContext context)
		{
			string protocolName = context == null ? string.Empty : Convert.ToString(context.GetData("Comm.Protocol"));
			string channelName = context == null ? string.Empty : Convert.ToString(context.GetData("Comm.Channel"));
			string jobName = context == null ? string.Empty : context.JobName;
			string taskName = context == null ? string.Empty : context.TaskName;

			return FlowConfigStore.PathManager.GetTaskFolder(protocolName, channelName, jobName, taskName);
		}

		private void AddFileCandidate(List<string> candidates, string file, string taskFolder)
		{
			if (string.IsNullOrWhiteSpace(file))
			{
				return;
			}

			string candidate = Path.IsPathRooted(file) ? file : Path.Combine(taskFolder, file);
			if (!candidates.Contains(candidate, StringComparer.OrdinalIgnoreCase))
			{
				candidates.Add(candidate);
			}
		}

		private static Type FindHalconType(string typeName)
		{
			Type type = FindLoadedHalconType(typeName);
			if (type != null)
			{
				return type;
			}

			LoadHalconAssemblies();
			type = FindLoadedHalconType(typeName);
			if (type != null)
			{
				return type;
			}

			string[] assemblyNames = new string[]
			{
				"HalconDotNet",
				"halcondotnet",
				"HDevEngineDotNet",
				"hdevenginedotnet"
			};

			foreach (string assemblyName in assemblyNames)
			{
				try
				{
					type = Type.GetType(typeName + ", " + assemblyName, false, true);
					if (type != null)
					{
						return type;
					}
				}
				catch
				{
				}
			}

			return null;
		}

		private static Type FindLoadedHalconType(string typeName)
		{
			foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
			{
				try
				{
					Type type = assembly.GetType(typeName, false, true);
					if (type != null)
					{
						return type;
					}
				}
				catch
				{
				}
			}

			return null;
		}

		private static void LoadHalconAssemblies()
		{
			if (_halconAssembliesLoaded)
			{
				return;
			}

			lock (_halconLoadLock)
			{
				if (_halconAssembliesLoaded)
				{
					return;
				}

				List<string> messages = new List<string>();
				ConfigureHalconNativeSearchPath(messages);
				TryLoadAssemblyByName("HalconDotNet", messages);
				TryLoadAssemblyByName("hdevenginedotnet", messages);

				foreach (string file in GetHalconAssemblyCandidates("halcondotnet.dll"))
				{
					TryLoadAssemblyFromFile(file, messages);
				}

				foreach (string file in GetHalconAssemblyCandidates("hdevenginedotnet.dll"))
				{
					TryLoadAssemblyFromFile(file, messages);
				}

				_halconAssembliesLoaded = true;
				_halconAssemblyLoadMessage = string.Join(" ", messages.ToArray());
			}
		}

		private static void ConfigureHalconNativeSearchPath(List<string> messages)
		{
			foreach (string root in GetHalconRoots())
			{
				foreach (string nativeDir in GetHalconNativeDirs(root))
				{
					if (!Directory.Exists(nativeDir))
					{
						continue;
					}

					try
					{
						if (SetDllDirectory(nativeDir))
						{
							messages.Add("NativePath=" + nativeDir);
							return;
						}
					}
					catch
					{
					}
				}
			}
		}

		private static bool TryLoadAssemblyByName(string assemblyName, List<string> messages)
		{
			try
			{
				Assembly.Load(assemblyName);
				messages.Add("Loaded=" + assemblyName);
				return true;
			}
			catch (Exception ex)
			{
				messages.Add("NameLoadFailed=" + assemblyName + "(" + ex.GetType().Name + ")");
				return false;
			}
		}

		private static bool TryLoadAssemblyFromFile(string filePath, List<string> messages)
		{
			if (string.IsNullOrWhiteSpace(filePath) || !File.Exists(filePath))
			{
				return false;
			}

			try
			{
				Assembly.LoadFrom(filePath);
				messages.Add("LoadedFrom=" + filePath);
				return true;
			}
			catch (Exception ex)
			{
				messages.Add("FileLoadFailed=" + filePath + "(" + ex.GetType().Name + ")");
				return false;
			}
		}

		private static IEnumerable<string> GetHalconAssemblyCandidates(string fileName)
		{
			HashSet<string> candidates = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
			string baseDir = AppDomain.CurrentDomain.BaseDirectory;
			AddHalconAssemblyCandidate(candidates, baseDir, fileName);

			foreach (string root in GetHalconRoots())
			{
				AddHalconAssemblyCandidate(candidates, Path.Combine(root, "bin", "dotnet35"), fileName);
				AddHalconAssemblyCandidate(candidates, Path.Combine(root, "bin", "dotnet20"), fileName);
				AddHalconAssemblyCandidate(candidates, Path.Combine(root, "bin"), fileName);
			}

			return candidates;
		}

		private static void AddHalconAssemblyCandidate(HashSet<string> candidates, string folder, string fileName)
		{
			if (string.IsNullOrWhiteSpace(folder))
			{
				return;
			}

			candidates.Add(Path.Combine(folder, fileName));
		}

		private static IEnumerable<string> GetHalconRoots()
		{
			HashSet<string> roots = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
			AddHalconRoot(roots, Environment.GetEnvironmentVariable("HALCONROOT"));
			AddHalconRoot(roots, Environment.GetEnvironmentVariable("HALCON_ROOT"));
			AddHalconRoot(roots, Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Programs", "MVTec", "HALCON-25.05-Progress"));
			return roots;
		}

		private static void AddHalconRoot(HashSet<string> roots, string root)
		{
			if (!string.IsNullOrWhiteSpace(root))
			{
				roots.Add(root);
			}
		}

		private static IEnumerable<string> GetHalconNativeDirs(string root)
		{
			string arch = Environment.GetEnvironmentVariable("HALCONARCH");
			if (!string.IsNullOrWhiteSpace(arch))
			{
				yield return Path.Combine(root, "bin", arch);
			}

			yield return Path.Combine(root, "bin", "x64-win64");
			yield return Path.Combine(root, "bin");
		}

		private void ApplyInputPinValues(object call)
		{
			if (_config.InputPins == null)
			{
				return;
			}

			foreach (PinConfig pin in _config.InputPins)
			{
				if (pin == null || pin.DataType == PinDataType.Image)
				{
					continue;
				}

				object value = null;
				if (!string.IsNullOrWhiteSpace(pin.GlobalVariableName))
				{
					GlobalVariableStore.TryGetValue(pin.GlobalVariableName, out value);
				}

				if (value == null)
				{
					value = ConvertPinDefaultValue(pin);
				}

				if (value != null)
				{
					TryInvokeByName(call, new string[] { "SetInputCtrlParamTuple", "SetCtrlVarTuple" }, pin.PinName, value);
				}
			}
		}

		private object ConvertPinDefaultValue(PinConfig pin)
		{
			if (pin == null || string.IsNullOrWhiteSpace(pin.Description))
			{
				return null;
			}

			string text = pin.Description.Trim();
			try
			{
				if (pin.DataType == PinDataType.Bool)
				{
					return text == "1" || text.Equals("true", StringComparison.OrdinalIgnoreCase);
				}

				if (pin.DataType == PinDataType.Int)
				{
					int value;
					return int.TryParse(text, out value) ? (object)value : null;
				}

				if (pin.DataType == PinDataType.Double || pin.DataType == PinDataType.Float)
				{
					double value;
					return double.TryParse(text, out value) ? (object)value : null;
				}

				return text;
			}
			catch
			{
				return null;
			}
		}

		private void ApplyGlobalInputs(object call)
		{
			if (_config.InputPins == null)
			{
				return;
			}

			foreach (PinConfig pin in _config.InputPins)
			{
				if (pin == null || string.IsNullOrWhiteSpace(pin.GlobalVariableName) || pin.DataType == PinDataType.Image)
				{
					continue;
				}

				object value;
				if (GlobalVariableStore.TryGetValue(pin.GlobalVariableName, out value))
				{
					TryInvokeByName(call, new string[] { "SetInputCtrlParamTuple", "SetCtrlVarTuple" }, pin.PinName, value);
				}
			}
		}

		private List<string> ReadConfiguredOutputs(object call, StepResult result)
		{
			List<string> missingOutputs = new List<string>();

			if (_config.OutputPins == null)
			{
				return missingOutputs;
			}

			foreach (PinConfig pin in _config.OutputPins)
			{
				if (pin == null || string.IsNullOrWhiteSpace(pin.PinName))
				{
					continue;
				}

				if (pin.DataType == PinDataType.Image)
				{
					object imageValue = TryGetHalconOutput(call, pin.PinName, true);
					if (imageValue != null)
					{
						VisionImage image = new VisionImage();
						image.ImageName = pin.PinName;
						image.ImageType = "Halcon";
						image.SourceStep = _config.StepName;
						image.RawImage = imageValue;
						image.DisplayBitmap = ImageConvertHelper.TryConvertToBitmap(imageValue);
						if (image.DisplayBitmap != null)
						{
							image.Width = image.DisplayBitmap.Width;
							image.Height = image.DisplayBitmap.Height;
						}
						else
						{
							RuntimeLogStore.Append(
								DateTime.Now,
								RuntimeLogCategory.Step,
								"Hdev output image was read but could not be converted to Bitmap. Step=" + _config.StepName +
								", Output=" + pin.PinName +
								", RawType=" + GetDebugTypeName(imageValue),
								true);
						}
						result.OutputImages[pin.PinName] = image;
					}
					else
					{
						missingOutputs.Add(pin.PinName);
					}
					continue;
				}

				object value = TryGetHalconOutput(call, pin.PinName, false);
				if (value == null)
				{
					missingOutputs.Add(pin.PinName);
					continue;
				}

				object normalized = NormalizeHalconValue(value, pin.DataType);
				result.Outputs[pin.PinName] = normalized;

				if (!string.IsNullOrWhiteSpace(pin.GlobalVariableName))
				{
					GlobalVariableStore.SetValue(pin.GlobalVariableName, normalized);
				}
			}

			return missingOutputs;
		}

		private static string GetDebugTypeName(object value)
		{
			return value == null ? "null" : (value.GetType().FullName ?? value.GetType().Name);
		}

		private string BuildSuccessMessage(StepResult result, List<string> missingOutputs)
		{
			int valueCount = result == null || result.Outputs == null ? 0 : result.Outputs.Count;
			int imageCount = result == null || result.OutputImages == null ? 0 : result.OutputImages.Count;
			string message = "Hdev step executed. Outputs=" + valueCount + ", Images=" + imageCount;

			if (missingOutputs != null && missingOutputs.Count > 0)
			{
				message += ", Missing=" + string.Join(",", missingOutputs);
			}

			return message;
		}

		private object TryGetHalconOutput(object call, string name, bool iconic)
		{
			string[] methods = iconic
				? new string[] { "GetOutputIconicParamObject", "GetIconicVarObject" }
				: new string[] { "GetOutputCtrlParamTuple", "GetCtrlVarTuple" };

			foreach (string methodName in methods)
			{
				try
				{
					MethodInfo method = call.GetType().GetMethod(methodName, new Type[] { typeof(string) });
					if (method == null)
					{
						continue;
					}

					return method.Invoke(call, new object[] { name });
				}
				catch
				{
				}
			}

			return null;
		}

		private object NormalizeHalconValue(object value, PinDataType dataType)
		{
			if (value == null)
			{
				return null;
			}

			Type type = value.GetType();
			string typeName = type.FullName ?? string.Empty;
			if (typeName.IndexOf("HTuple", StringComparison.OrdinalIgnoreCase) < 0)
			{
				return value;
			}

			return ConvertTupleText(value.ToString(), dataType);
		}

		private object ConvertTupleText(string text, PinDataType dataType)
		{
			string value = text == null ? string.Empty : text.Trim();

			if (dataType == PinDataType.Bool)
			{
				return value == "1" || value.Equals("true", StringComparison.OrdinalIgnoreCase);
			}

			if (dataType == PinDataType.Int)
			{
				int intValue;
				return int.TryParse(value, out intValue) ? (object)intValue : value;
			}

			if (dataType == PinDataType.Double || dataType == PinDataType.Float)
			{
				double doubleValue;
				return double.TryParse(value, out doubleValue) ? (object)doubleValue : value;
			}

			return value;
		}

		private void EnsureResultStatus(StepResult result)
		{
			object value;
			if (result.Outputs.TryGetValue("ResultOK", out value) && value != null)
			{
				try
				{
					result.IsOK = Convert.ToBoolean(value);
				}
				catch
				{
				}
			}
		}

		private void InvokeNoArg(object target, string methodName)
		{
			MethodInfo method = target.GetType().GetMethod(methodName, Type.EmptyTypes);
			if (method == null)
			{
				throw new Exception("HALCON call object does not provide " + methodName + "().");
			}

			method.Invoke(target, null);
		}

		private bool TryInvokeByName(object target, string[] methodNames, string name, object value)
		{
			foreach (string methodName in methodNames)
			{
				try
				{
					MethodInfo method = target.GetType().GetMethod(methodName, new Type[] { typeof(string), value == null ? typeof(object) : value.GetType() });
					if (method == null)
					{
						method = target.GetType().GetMethods().FirstOrDefault(m =>
							string.Equals(m.Name, methodName, StringComparison.OrdinalIgnoreCase) &&
							m.GetParameters().Length == 2);
					}

					if (method == null)
					{
						continue;
					}

					ParameterInfo[] parameters = method.GetParameters();
					object convertedValue = ConvertForHalconParameter(value, parameters[1].ParameterType);
					method.Invoke(target, new object[] { name, convertedValue });
					return true;
				}
				catch
				{
				}
			}

			return false;
		}

		private object ConvertForHalconParameter(object value, Type targetType)
		{
			if (targetType == null || value == null || targetType.IsInstanceOfType(value))
			{
				return value;
			}

			if ((targetType.FullName ?? string.Empty).IndexOf("HTuple", StringComparison.OrdinalIgnoreCase) >= 0)
			{
				try
				{
					return Activator.CreateInstance(targetType, new object[] { value });
				}
				catch
				{
				}
			}

			try
			{
				return Convert.ChangeType(value, targetType);
			}
			catch
			{
				return value;
			}
		}
	}

	public class HalconWarmupResult
	{
		public int TotalPrograms { get; set; }
		public int LoadedPrograms { get; set; }
		public int FailedPrograms { get; set; }
		public TimeSpan Cost { get; set; }
		public List<string> Warnings { get; private set; }

		public HalconWarmupResult()
		{
			Warnings = new List<string>();
		}
	}

	public static class HalconWarmupService
	{
		public static HalconWarmupResult WarmUp(ProjectFlowConfig flowConfig)
		{
			Stopwatch sw = Stopwatch.StartNew();
			HalconWarmupResult result = new HalconWarmupResult();
			HashSet<string> warmedPaths = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

			if (flowConfig == null || flowConfig.Jobs == null)
			{
				sw.Stop();
				result.Cost = sw.Elapsed;
				return result;
			}

			foreach (JobConfig job in flowConfig.Jobs)
			{
				if (job == null || job.Tasks == null)
				{
					continue;
				}

				foreach (TaskConfig task in job.Tasks)
				{
					WarmUpTask(job, task, result, warmedPaths);
				}
			}

			sw.Stop();
			result.Cost = sw.Elapsed;
			return result;
		}

		private static void WarmUpTask(JobConfig job, TaskConfig task, HalconWarmupResult result, HashSet<string> warmedPaths)
		{
			if (job == null || task == null || task.Steps == null || result == null)
			{
				return;
			}

			foreach (StepConfig step in GetTaskFlowHdevSteps(task))
			{
				result.TotalPrograms++;

				string warning;
				if (WarmUpHdev(job, task, step, warmedPaths, out warning))
				{
					result.LoadedPrograms++;
				}
				else
				{
					result.FailedPrograms++;
					result.Warnings.Add(warning);
				}
			}
		}

		private static IEnumerable<StepConfig> GetTaskFlowHdevSteps(TaskConfig task)
		{
			if (task == null || task.Steps == null)
			{
				yield break;
			}

			HashSet<string> yielded = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

			if (task.StepFlow != null)
			{
				foreach (StepFlowItem flowItem in task.StepFlow)
				{
					if (flowItem == null || !flowItem.Enabled || !flowItem.IsStepBlock || string.IsNullOrWhiteSpace(flowItem.StepName))
					{
						continue;
					}

					StepConfig step = task.Steps.FirstOrDefault(x =>
						x != null &&
						x.Enabled &&
						x.StepType == StepType.Halcon &&
						string.Equals(x.StepName, flowItem.StepName, StringComparison.OrdinalIgnoreCase));
					if (step == null || !yielded.Add(step.StepName))
					{
						continue;
					}

					yield return step;
				}
			}

			foreach (StepConfig step in task.Steps)
			{
				if (step == null || step.StepType != StepType.Halcon || !step.Enabled || !yielded.Add(step.StepName))
				{
					continue;
				}

				yield return step;
			}
		}

		private static bool WarmUpHdev(JobConfig job, TaskConfig task, StepConfig step, HashSet<string> warmedPaths, out string warning)
		{
			warning = string.Empty;

			try
			{
				string protocolName = ResolveProtocolName(job, task);
				string channelName = ResolveChannelName(job, task);
				string taskFolder = FlowConfigStore.PathManager.GetTaskFolder(protocolName, channelName, job.JobName, task.TaskName);
				string hdevPath = ResolveHdevPath(taskFolder, step);

				if (string.IsNullOrWhiteSpace(hdevPath) || !File.Exists(hdevPath))
				{
					warning = FormatWarning(job, task, step, "Hdev file not found.");
					return false;
				}

				string normalizedPath = Path.GetFullPath(hdevPath);
				string enginePath = HalconStep.PrepareProgramFileForEngine(normalizedPath, step);
				if (warmedPaths != null &&
					(warmedPaths.Contains(normalizedPath) || warmedPaths.Contains(enginePath)))
				{
					return true;
				}

				if (!HalconStep.WarmUpProgram(enginePath, out warning))
				{
					warning = FormatWarning(job, task, step, warning);
					return false;
				}

				if (warmedPaths != null)
				{
					warmedPaths.Add(normalizedPath);
					if (!string.Equals(normalizedPath, enginePath, StringComparison.OrdinalIgnoreCase))
					{
						warmedPaths.Add(enginePath);
					}
				}

				return true;
			}
			catch (Exception ex)
			{
				warning = FormatWarning(job, task, step, ex.Message);
				return false;
			}
		}

		private static string ResolveHdevPath(string taskFolder, StepConfig step)
		{
			if (step == null)
			{
				return string.Empty;
			}

			List<string> candidates = new List<string>();
			AddCandidate(candidates, step.ProjectFilePath);

			if (string.IsNullOrWhiteSpace(step.ProjectFilePath) && !string.IsNullOrWhiteSpace(step.StepName))
			{
				AddCandidate(candidates, Path.Combine("Hdev", step.StepName + ".hdev"));
			}

			AddCandidate(candidates, step.SourceFilePath);

			foreach (string candidate in candidates)
			{
				string resolved = ResolveCandidatePath(taskFolder, candidate);
				if (IsHdevFile(resolved) && File.Exists(resolved))
				{
					return resolved;
				}
			}

			string folder = Path.Combine(taskFolder ?? string.Empty, "Hdev");
			string safeStepName = MakeSafeName(step.StepName);
			string direct = Path.Combine(folder, safeStepName + ".hdev");
			if (File.Exists(direct))
			{
				return direct;
			}

			if (Directory.Exists(folder))
			{
				string[] files = Directory.GetFiles(folder, "*.hdev", SearchOption.TopDirectoryOnly);
				if (files.Length > 0)
				{
					return files[0];
				}
			}

			return string.Empty;
		}

		private static void AddCandidate(List<string> candidates, string value)
		{
			if (candidates == null || string.IsNullOrWhiteSpace(value))
			{
				return;
			}

			if (!candidates.Any(x => string.Equals(x, value, StringComparison.OrdinalIgnoreCase)))
			{
				candidates.Add(value);
			}
		}

		private static string ResolveCandidatePath(string taskFolder, string path)
		{
			if (string.IsNullOrWhiteSpace(path))
			{
				return string.Empty;
			}

			if (Path.IsPathRooted(path))
			{
				return path;
			}

			string candidate = Path.Combine(taskFolder ?? string.Empty, path);
			if (File.Exists(candidate))
			{
				return candidate;
			}

			candidate = Path.Combine(taskFolder ?? string.Empty, "Hdev", path);
			if (File.Exists(candidate))
			{
				return candidate;
			}

			return Path.Combine(ProjectPathStore.ProjectRoot, path);
		}

		private static bool IsHdevFile(string path)
		{
			if (string.IsNullOrWhiteSpace(path))
			{
				return false;
			}

			return Path.GetExtension(path).Equals(".hdev", StringComparison.OrdinalIgnoreCase);
		}

		private static string ResolveProtocolName(JobConfig job, TaskConfig task)
		{
			if (job != null && !string.IsNullOrWhiteSpace(job.ProtocolName))
			{
				return FlowConfigStore.NormalizeProtocolName(job.ProtocolName);
			}

			return FlowConfigStore.NormalizeProtocolName(task == null ? "TCP/IP" : task.CommunicationProtocol);
		}

		private static string ResolveChannelName(JobConfig job, TaskConfig task)
		{
			if (job != null && !string.IsNullOrWhiteSpace(job.ChannelName))
			{
				return FlowConfigStore.NormalizeChannelName(job.ChannelName);
			}

			return FlowConfigStore.NormalizeChannelName(task == null ? "Channel01" : task.CommunicationChannel);
		}

		private static string MakeSafeName(string name)
		{
			if (string.IsNullOrWhiteSpace(name))
			{
				return "New";
			}

			foreach (char c in Path.GetInvalidFileNameChars())
			{
				name = name.Replace(c, '_');
			}

			return name.Trim();
		}

		private static string FormatWarning(JobConfig job, TaskConfig task, StepConfig step, string detail)
		{
			return "Job=" + (job == null ? string.Empty : job.JobName) +
				", Task=" + (task == null ? string.Empty : task.TaskName) +
				", Step=" + (step == null ? string.Empty : step.StepName) +
				", " + (detail ?? string.Empty);
		}
	}

	public static class StepFactory
	{
		public static IVisionStep Create(StepConfig config)
		{
			if (config == null) throw new ArgumentNullException("config");

			switch (config.StepType)
			{
				case StepType.Vpp:
					return new VppStep(config);

				case StepType.Script:
					return new CSharpScriptRuntimeStepRunner(config);

				case StepType.Halcon:
					return new HalconStep(config);

				default:
					throw new NotSupportedException("Unsupported step type: " + config.StepType);
			}
		}
	}

	public static class TriggerConditionEvaluator
	{
		public static bool CanRunTask(TaskConfig taskConfig, ICommunicationRuntimeValueProvider valueProvider)
		{
			if (taskConfig == null)
			{
				return false;
			}

			// 没有配置通讯协议或触发源时，认为不需要外部触发条件。
			if (string.IsNullOrWhiteSpace(taskConfig.CommunicationProtocol) ||
				taskConfig.CommunicationProtocol.Equals("Not Use", StringComparison.OrdinalIgnoreCase) ||
				string.IsNullOrWhiteSpace(taskConfig.TriggerName) ||
				taskConfig.TriggerName.Equals("Not Use", StringComparison.OrdinalIgnoreCase))
			{
				return true;
			}

			if (valueProvider == null)
			{
				return false;
			}

			string triggerName = taskConfig.TriggerName;
			string triggerValue = taskConfig.TriggerValue;
			string parsedTriggerName;
			string parsedTriggerValue;
			if (TryParseTriggerOption(triggerName, out parsedTriggerName, out parsedTriggerValue))
			{
				triggerName = parsedTriggerName;
				triggerValue = parsedTriggerValue;
			}

			string triggerActualValue = valueProvider.GetInputValue(taskConfig.CommunicationProtocol, triggerName);
			bool triggerOk = CompareValue(triggerActualValue, triggerValue, taskConfig.TriggerCompare);

			return triggerOk && AreExecutionConditionsMatched(taskConfig.ExecutionConditions);
		}

		public static bool AreExecutionConditionsMatched(List<TaskExecutionCondition> conditions)
		{
			if (conditions == null || conditions.Count <= 0)
			{
				return true;
			}

			foreach (TaskExecutionCondition condition in conditions)
			{
				if (condition == null || string.IsNullOrWhiteSpace(condition.GlobalVariableName))
				{
					return false;
				}

				string actualValue = GlobalVariableStore.GetValueText(condition.GlobalVariableName);
				if (!CompareValue(actualValue, condition.ExpectedValue, condition.Compare))
				{
					return false;
				}
			}

			return true;
		}

		private static bool TryParseTriggerOption(string triggerOption, out string triggerName, out string expectedValue)
		{
			triggerName = triggerOption == null ? string.Empty : triggerOption.Trim();
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

		public static bool CompareValue(string actual, string expected, TriggerCompareType compare)
		{
			actual = actual == null ? string.Empty : actual.Trim();
			expected = expected == null ? string.Empty : expected.Trim();

			double actualNumber;
			double expectedNumber;

			bool actualIsNumber = double.TryParse(actual, out actualNumber);
			bool expectedIsNumber = double.TryParse(expected, out expectedNumber);

			if (actualIsNumber && expectedIsNumber)
			{
				if (compare == TriggerCompareType.Equal) return actualNumber == expectedNumber;
				if (compare == TriggerCompareType.NotEqual) return actualNumber != expectedNumber;
				if (compare == TriggerCompareType.Greater) return actualNumber > expectedNumber;
				if (compare == TriggerCompareType.GreaterOrEqual) return actualNumber >= expectedNumber;
				if (compare == TriggerCompareType.Less) return actualNumber < expectedNumber;
				if (compare == TriggerCompareType.LessOrEqual) return actualNumber <= expectedNumber;
			}

			if (compare == TriggerCompareType.Equal)
			{
				return string.Equals(actual, expected, StringComparison.OrdinalIgnoreCase);
			}

			if (compare == TriggerCompareType.NotEqual)
			{
				return !string.Equals(actual, expected, StringComparison.OrdinalIgnoreCase);
			}

			return false;
		}
	}

	public class TaskRunner
	{

		public StepResult Run(TaskConfig taskConfig, VisionRunContext context)
		{
			if (taskConfig == null) throw new ArgumentNullException("taskConfig");
			if (context == null) throw new ArgumentNullException("context");

			context.TaskName = taskConfig.TaskName;

			StepResult finalResult = StepResult.OK();
			bool previousStepsOK = true;
			bool taskHadNG = false;

			List<IGrouping<int, StepFlowItem>> groups = taskConfig.StepFlow
				.Where(x => x.Enabled)
				.OrderBy(x => x.RunOrder)
				.GroupBy(x => x.RunOrder)
				.ToList();

			foreach (IGrouping<int, StepFlowItem> group in groups)
			{
				context.SetData(CSharpScriptStepStore.PreviousStepsOkInputName, previousStepsOK);
				context.SetData("Task." + CSharpScriptStepStore.PreviousStepsOkInputName, previousStepsOK);

				List<System.Threading.Tasks.Task<StepExecuteResult>> runningTasks =
					new List<System.Threading.Tasks.Task<StepExecuteResult>>();

				foreach (StepFlowItem flowItem in group)
				{
					StepFlowItem localFlowItem = flowItem;

					System.Threading.Tasks.Task<StepExecuteResult> runTask =
						System.Threading.Tasks.Task.Run(() =>
						{
							return ExecuteOneStep(taskConfig, localFlowItem, context);
						});

					runningTasks.Add(runTask);
				}

				System.Threading.Tasks.Task.WaitAll(runningTasks.ToArray());

				bool groupOK = true;

				foreach (System.Threading.Tasks.Task<StepExecuteResult> runTask in runningTasks)
				{
					StepExecuteResult executeResult = runTask.Result;

					if (executeResult == null || executeResult.StepConfig == null || executeResult.StepResult == null)
					{
						continue;
					}

					StepConfig stepConfig = executeResult.StepConfig;
					StepResult stepResult = executeResult.StepResult;

					context.SetStepResult(stepConfig.StepName, stepResult);

					foreach (KeyValuePair<string, object> output in stepResult.Outputs)
					{
						context.SetData(stepConfig.StepName + "." + output.Key, output.Value);
					}

					foreach (KeyValuePair<string, VisionImage> image in stepResult.OutputImages)
					{
						context.SetImage(stepConfig.StepName + "." + image.Key, image.Value);
					}

					RuntimeStepResultStore.SetLatest(
						context.JobName,
						context.TaskName,
						stepConfig.StepName,
						stepResult);

					StepDisplayBindingRunner.TryPublishStepImage(
						context.JobName,
						context.TaskName,
						stepConfig,
						stepResult,
						context);

					finalResult = stepResult;

					if (!stepResult.IsOK)
					{
						groupOK = false;
						taskHadNG = true;
					}
				}

				previousStepsOK = previousStepsOK && groupOK;
			}

			if (taskHadNG && (finalResult == null || finalResult.IsOK))
			{
				finalResult = StepResult.NG("One or more steps failed. Flow continued.");
			}

			return finalResult;
		}

		private class StepExecuteResult
		{
			public StepFlowItem FlowItem { get; set; }
			public StepConfig StepConfig { get; set; }
			public StepResult StepResult { get; set; }
		}

		private StepExecuteResult ExecuteOneStep(TaskConfig taskConfig, StepFlowItem flowItem, VisionRunContext context)
		{
			StepExecuteResult executeResult = new StepExecuteResult();
			executeResult.FlowItem = flowItem;

			if (flowItem != null && !flowItem.IsStepBlock)
			{
				string blockName = string.IsNullOrWhiteSpace(flowItem.BlockName) ? flowItem.StepName : flowItem.BlockName;
				if (string.IsNullOrWhiteSpace(blockName))
				{
					blockName = flowItem.BlockType;
				}

				executeResult.StepConfig = new StepConfig
				{
					StepName = blockName,
					StepType = StepType.Unknown,
					DisplayOutputKey = "Not Use",
					DisplaySlotName = "Not Show",
					DisplayResultKey = "Not Use",
					DisplayMode = "Fit"
				};
				bool blockOk = true;
				string blockMessage = flowItem.BlockType + " flow block completed.";
				Dictionary<string, object> signalOutputValues = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
				Dictionary<string, VisionImage> hardwareOutputImages = new Dictionary<string, VisionImage>(StringComparer.OrdinalIgnoreCase);
				Stopwatch blockWatch = Stopwatch.StartNew();

				if (IsHardwareFlowBlock(flowItem))
				{
					blockOk = AcquireHardwareFlowBlockImage(flowItem, context, hardwareOutputImages, out blockMessage);
				}
				else if (IsSignalFlowBlock(flowItem))
				{
					blockOk = SendSignalFlowBlockOutput(taskConfig, flowItem, context, signalOutputValues);
					blockMessage = blockOk ? "Signal output completed." : "Signal output failed.";
				}
				else if (IsDatabaseFlowBlock(flowItem))
				{
					blockOk = EnqueueDatabaseFlowBlockWrite(taskConfig, flowItem, context, out blockMessage);
				}

				blockWatch.Stop();

				executeResult.StepResult = blockOk
					? StepResult.OK(blockMessage)
					: StepResult.NG(blockMessage);
				executeResult.StepResult.CostMs = blockWatch.Elapsed.TotalMilliseconds;

				foreach (KeyValuePair<string, object> output in signalOutputValues)
				{
					executeResult.StepResult.Outputs[output.Key] = output.Value;
				}

				foreach (KeyValuePair<string, VisionImage> image in hardwareOutputImages)
				{
					executeResult.StepResult.OutputImages[image.Key] = image.Value;
				}

				AppendStepLog(
					RuntimeLogCategory.Step,
					"Flow block completed. Job=" + context.JobName +
					", Task=" + context.TaskName +
					", BlockType=" + flowItem.BlockType +
					", Block=" + blockName +
					", RunOrder=" + flowItem.RunOrder +
					", Cost=" + blockWatch.Elapsed.TotalMilliseconds.ToString("0.0") + " ms");

				return executeResult;
			}

			StepConfig sourceStepConfig = taskConfig.Steps.FirstOrDefault(s =>
				string.Equals(s.StepName, flowItem.StepName, StringComparison.OrdinalIgnoreCase));

			if (sourceStepConfig == null)
			{
				executeResult.StepConfig = new StepConfig { StepName = flowItem.StepName };
				executeResult.StepResult = StepResult.NG("Step config not found: " + flowItem.StepName);
				return executeResult;
			}

			// 复制一份 StepConfig，避免并行执行时多个线程修改同一个对象。
			StepConfig runStepConfig = CloneStepConfig(sourceStepConfig);

			if (!string.IsNullOrEmpty(flowItem.InputImageKey))
			{
				runStepConfig.InputImageKey = flowItem.InputImageKey;
			}

			if (runStepConfig.StepType != StepType.Script && !string.IsNullOrEmpty(flowItem.ScriptInputStepKeys))
			{
				runStepConfig.ScriptInputStepKeys = flowItem.ScriptInputStepKeys;
			}

			if (runStepConfig.StepType == StepType.Script)
			{
				runStepConfig.ScriptInputStepKeys = string.Empty;
			}

			runStepConfig.DisplayOutputKey = flowItem.DisplayOutputKey;
			runStepConfig.DisplaySlotName = flowItem.DisplaySlotName;
			runStepConfig.DisplayResultKey = flowItem.DisplayResultKey;
			runStepConfig.DisplayMode = flowItem.DisplayMode;

			executeResult.StepConfig = runStepConfig;

			try
			{
				AppendStepLog(
					RuntimeLogCategory.Step,
					"Step started. Job=" + context.JobName +
					", Task=" + context.TaskName +
					", Step=" + runStepConfig.StepName +
					", RunOrder=" + flowItem.RunOrder);

				DateTime stepStartTime = DateTime.Now;
				bool previousStepsOK = GetPreviousStepsOK(context);

				string skipMessage;
				if (ShouldSkipForMissingInputImage(runStepConfig, context, out skipMessage))
				{
					executeResult.StepResult = StepResult.NG(skipMessage);
					FillDefaultOutputs(runStepConfig, executeResult.StepResult);
				}
				else
				{
					IVisionStep step = StepFactory.Create(runStepConfig);
					executeResult.StepResult = step.Execute(context);
					if (executeResult.StepResult != null && !executeResult.StepResult.IsOK)
					{
						FillDefaultOutputs(runStepConfig, executeResult.StepResult);
					}
				}

				FillStepInputSnapshot(runStepConfig, executeResult.StepResult, previousStepsOK, context);

				if (runStepConfig.StepType == StepType.Script && !previousStepsOK)
				{
					if (executeResult.StepResult != null &&
						executeResult.StepResult.Outputs != null &&
						executeResult.StepResult.Outputs.Count > 0)
					{
						AppendStepLog(
							RuntimeLogCategory.Step,
							"Previous step status was abnormal. Script produced release outputs. Step=" + runStepConfig.StepName);
					}
					else
					{
						AppendStepLog(
							RuntimeLogCategory.Step,
							"Previous step status was abnormal, but script produced no release outputs. Check " +
							CSharpScriptStepStore.PreviousStepsOkInputName +
							" handling. Step=" + runStepConfig.StepName);
					}
				}

				AppendStepLog(
					RuntimeLogCategory.Step,
					"Step finished. Job=" + context.JobName +
					", Task=" + context.TaskName +
					", Step=" + runStepConfig.StepName +
					", Cost=" + (DateTime.Now - stepStartTime).TotalMilliseconds.ToString("0.0") + " ms");

				if (runStepConfig.StepType == StepType.Halcon && executeResult.StepResult != null)
				{
					AppendStepLog(
						RuntimeLogCategory.Step,
						"Hdev outputs. Step=" + runStepConfig.StepName +
						", IsOK=" + executeResult.StepResult.IsOK +
						", Outputs=" + (executeResult.StepResult.Outputs == null ? 0 : executeResult.StepResult.Outputs.Count) +
						", Images=" + (executeResult.StepResult.OutputImages == null ? 0 : executeResult.StepResult.OutputImages.Count) +
						", Message=" + (executeResult.StepResult.Message ?? string.Empty).Replace("\r", " ").Replace("\n", " "));
				}
			}
			catch (Exception ex)
			{
				executeResult.StepResult = StepResult.NG(ex.Message);
				FillDefaultOutputs(runStepConfig, executeResult.StepResult);
				AppendStepLog(
					RuntimeLogCategory.Step,
					"Step failed. Job=" + context.JobName +
					", Task=" + context.TaskName +
					", Step=" + runStepConfig.StepName +
					", Error=" + ex.Message +
					". Flow will continue with default outputs.");
			}

			return executeResult;
		}

		private bool IsSignalFlowBlock(StepFlowItem flowItem)
		{
			return flowItem != null &&
				string.Equals(flowItem.BlockType, "Signal", StringComparison.OrdinalIgnoreCase);
		}

		private bool IsHardwareFlowBlock(StepFlowItem flowItem)
		{
			return flowItem != null &&
				string.Equals(flowItem.BlockType, "Hardware", StringComparison.OrdinalIgnoreCase);
		}

		private bool IsDatabaseFlowBlock(StepFlowItem flowItem)
		{
			return flowItem != null &&
				string.Equals(flowItem.BlockType, "Database", StringComparison.OrdinalIgnoreCase);
		}

		private bool AcquireHardwareFlowBlockImage(
			StepFlowItem flowItem,
			VisionRunContext context,
			Dictionary<string, VisionImage> outputImages,
			out string message)
		{
			message = string.Empty;

			if (flowItem == null)
			{
				message = "Hardware flow block is empty.";
				return false;
			}

			if (context == null)
			{
				message = "Runtime context is empty.";
				return false;
			}

			string sourceKey = ResolveHardwareFlowImageSourceKey(flowItem);
			if (string.IsNullOrWhiteSpace(sourceKey))
			{
				message = "Hardware image source is empty.";
				return false;
			}

			VisionImage existing;
			if (context.TryGetImage(sourceKey, out existing) && existing != null && existing.RawImage != null)
			{
				if (outputImages != null)
				{
					outputImages[sourceKey] = existing;
				}

				message = "Hardware image already available. Source=" + sourceKey;
				return true;
			}

			RuntimeImageAcquireResult acquireResult =
				new RuntimeImageAcquireService().Acquire(context.JobName, sourceKey);

			if (acquireResult == null)
			{
				message = "Hardware image acquire returned null. Source=" + sourceKey;
				return false;
			}

			if (!acquireResult.Success || acquireResult.Image == null || acquireResult.Image.RawImage == null)
			{
				message = "Hardware image acquire failed. Source=" + sourceKey +
					", Error=" + (acquireResult.Message ?? string.Empty);
				return false;
			}

			string acquiredSourceKey = string.IsNullOrWhiteSpace(acquireResult.SourceKey)
				? sourceKey
				: acquireResult.SourceKey.Trim();

			StoreRuntimeImage(context, acquiredSourceKey, acquireResult.Image);

			if (!string.Equals(acquiredSourceKey, sourceKey, StringComparison.OrdinalIgnoreCase))
			{
				StoreRuntimeImage(context, sourceKey, acquireResult.Image);
			}

			if (!string.IsNullOrWhiteSpace(acquireResult.Image.OutputImageKey))
			{
				StoreRuntimeImage(context, acquireResult.Image.OutputImageKey.Trim(), acquireResult.Image);
			}

			if (outputImages != null)
			{
				outputImages[sourceKey] = acquireResult.Image;
			}

			message = "Hardware image acquired. Source=" + sourceKey;
			return true;
		}

		private string ResolveHardwareFlowImageSourceKey(StepFlowItem flowItem)
		{
			if (flowItem == null)
			{
				return string.Empty;
			}

			if (!string.IsNullOrWhiteSpace(flowItem.BlockName))
			{
				return flowItem.BlockName.Trim();
			}

			if (!string.IsNullOrWhiteSpace(flowItem.StepName))
			{
				return flowItem.StepName.Trim();
			}

			return ConvertHardwarePathToImageSourceKey(flowItem.BlockPath);
		}

		private string ConvertHardwarePathToImageSourceKey(string blockPath)
		{
			if (string.IsNullOrWhiteSpace(blockPath))
			{
				return string.Empty;
			}

			string sourceKey = blockPath.Trim()
				.Replace(Path.DirectorySeparatorChar, '.')
				.Replace(Path.AltDirectorySeparatorChar, '.');

			while (sourceKey.Contains(".."))
			{
				sourceKey = sourceKey.Replace("..", ".");
			}

			return sourceKey.Trim('.');
		}

		private void StoreRuntimeImage(VisionRunContext context, string key, VisionImage image)
		{
			if (context == null || string.IsNullOrWhiteSpace(key) || image == null)
			{
				return;
			}

			string normalizedKey = key.Trim();
			context.SetImage(normalizedKey, image);
			context.SetData(normalizedKey, image.RawImage);
			context.SetData(normalizedKey + ".RawImage", image.RawImage);
		}

		private bool EnqueueDatabaseFlowBlockWrite(
			TaskConfig taskConfig,
			StepFlowItem flowItem,
			VisionRunContext context,
			out string message)
		{
			message = "Database write queued.";

			try
			{
				DatabaseConfig databaseConfig = DatabaseConfigStore.LoadOrCreateDefault();
				Dictionary<string, object> values = BuildDatabaseWriteValues(databaseConfig, taskConfig, flowItem, context);
				bool queued = DatabaseRecordWriter.Instance.Enqueue(values);
				message = queued ? "Database write queued." : "Database write skipped.";
				return true;
			}
			catch (Exception ex)
			{
				RuntimeLogStore.Append(
					DateTime.Now,
					RuntimeLogCategory.Step,
					"Database flow write skipped. Error=" + ex.Message,
					true);
				message = "Database write skipped. Error=" + ex.Message;
				return true;
			}
		}

		private Dictionary<string, object> BuildDatabaseWriteValues(
			DatabaseConfig databaseConfig,
			TaskConfig taskConfig,
			StepFlowItem flowItem,
			VisionRunContext context)
		{
			Dictionary<string, object> values = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
			AddDatabaseSystemValue(values, "TaskName", context == null ? (taskConfig == null ? string.Empty : taskConfig.TaskName) : context.TaskName);
			AddDatabaseSystemValue(values, "JobName", context == null ? string.Empty : context.JobName);
			AddDatabaseSystemValue(values, "TriggerName", context == null ? (taskConfig == null ? string.Empty : taskConfig.TriggerName) : context.TriggerName);

			if (databaseConfig == null || databaseConfig.Fields == null)
			{
				return values;
			}

			Dictionary<string, DatabaseInputBinding> bindingMap =
				new Dictionary<string, DatabaseInputBinding>(StringComparer.OrdinalIgnoreCase);
			if (flowItem != null && flowItem.DatabaseInputs != null)
			{
				foreach (DatabaseInputBinding binding in flowItem.DatabaseInputs)
				{
					if (binding == null || string.IsNullOrWhiteSpace(binding.InputName))
					{
						continue;
					}

					bindingMap[binding.InputName.Trim()] = binding;
				}
			}

			foreach (DatabaseFieldConfig field in databaseConfig.Fields.Where(x => x != null && x.Enabled))
			{
				if (string.IsNullOrWhiteSpace(field.InputName))
				{
					continue;
				}

				DatabaseInputBinding binding;
				if (bindingMap.TryGetValue(field.InputName, out binding) && binding != null)
				{
					if (!binding.Enabled)
					{
						continue;
					}

					if (binding.ForceValue)
					{
						values[field.InputName] = ResolveSignalOutputValue(binding.AssignedValue, context);
						continue;
					}

					object boundValue;
					if (!string.IsNullOrWhiteSpace(binding.GlobalVariableName) &&
						TryResolveDatabaseSourceValue(binding.GlobalVariableName, context, out boundValue))
					{
						values[field.InputName] = boundValue;
						continue;
					}
				}

				object implicitValue;
				if (TryResolveDatabaseSourceValue(field.InputName, context, out implicitValue))
				{
					values[field.InputName] = implicitValue;
				}
			}

			return values;
		}

		private void AddDatabaseSystemValue(Dictionary<string, object> values, string key, object value)
		{
			if (values == null || string.IsNullOrWhiteSpace(key))
			{
				return;
			}

			values[key] = value ?? string.Empty;
		}

		private bool TryResolveDatabaseSourceValue(string key, VisionRunContext context, out object value)
		{
			value = null;
			if (string.IsNullOrWhiteSpace(key))
			{
				return false;
			}

			string normalizedKey = key.Trim();
			if (context != null)
			{
				if (string.Equals(normalizedKey, "TaskName", StringComparison.OrdinalIgnoreCase))
				{
					value = context.TaskName;
					return true;
				}

				if (string.Equals(normalizedKey, "JobName", StringComparison.OrdinalIgnoreCase))
				{
					value = context.JobName;
					return true;
				}

				if (string.Equals(normalizedKey, "TriggerName", StringComparison.OrdinalIgnoreCase))
				{
					value = context.TriggerName;
					return true;
				}

				if (context.TryGetData(normalizedKey, out value))
				{
					return true;
				}

				try
				{
					if (context.Data != null)
					{
						foreach (KeyValuePair<string, object> pair in context.Data)
						{
							if (string.Equals(pair.Key, normalizedKey, StringComparison.OrdinalIgnoreCase))
							{
								value = pair.Value;
								return true;
							}
						}
					}
				}
				catch
				{
				}
			}

			return GlobalVariableStore.TryGetValue(normalizedKey, out value);
		}

		private bool SendSignalFlowBlockOutput(
			TaskConfig taskConfig,
			StepFlowItem flowItem,
			VisionRunContext context,
			Dictionary<string, object> outputValues)
		{
			if (!TaskRunContext.EnableCommunicationOutput)
			{
				return true;
			}

			if (flowItem == null || flowItem.SignalOutputs == null)
			{
				return true;
			}

			string protocolName = string.IsNullOrWhiteSpace(flowItem.SignalProtocol)
				? flowItem.CommunicationOutputProtocol
				: flowItem.SignalProtocol;
			string instanceName = string.IsNullOrWhiteSpace(flowItem.SignalInstanceName)
				? flowItem.CommunicationOutputInstanceName
				: flowItem.SignalInstanceName;

			if (string.IsNullOrWhiteSpace(protocolName) && taskConfig != null)
			{
				protocolName = taskConfig.CommunicationProtocol;
			}

			if (string.IsNullOrWhiteSpace(instanceName) && taskConfig != null)
			{
				instanceName = taskConfig.CommunicationInstanceName;
			}

			if (string.IsNullOrWhiteSpace(protocolName) ||
				protocolName.Equals("Not Use", StringComparison.OrdinalIgnoreCase) ||
				protocolName.Equals("None", StringComparison.OrdinalIgnoreCase))
			{
				return false;
			}

			foreach (SignalOutputBinding binding in flowItem.SignalOutputs)
			{
				if (binding == null || !binding.Enabled || string.IsNullOrWhiteSpace(binding.OutputName))
				{
					continue;
				}

				string outputName = binding.OutputName.Trim();
				outputValues[outputName] = binding.ForceValue
					? ResolveSignalOutputValue(binding.AssignedValue, context)
					: ResolveCurrentSignalOutputValue(protocolName, instanceName, outputName, context);
			}

			if (outputValues.Count <= 0)
			{
				AppendStepLog(
					RuntimeLogCategory.Communication,
					"Signal flow output skipped. Task=" + (taskConfig == null ? string.Empty : taskConfig.TaskName) +
					", Reason=No checked output values.");
				return true;
			}

			RuntimeCommunicationOutputService outputService = new RuntimeCommunicationOutputService();
			return outputService.SendConfiguredSignalOutput(protocolName, instanceName, outputValues);
		}

		private object ResolveCurrentSignalOutputValue(
			string protocolName,
			string instanceName,
			string outputName,
			VisionRunContext context)
		{
			CommOutputVariable outputVariable = FindSignalOutputVariable(protocolName, instanceName, outputName);
			object value;

			if (outputVariable != null && !string.IsNullOrWhiteSpace(outputVariable.GlobalVariableName) &&
				GlobalVariableStore.TryGetValue(outputVariable.GlobalVariableName, out value))
			{
				return value;
			}

			if (context != null)
			{
				if (context.TryGetData(outputName, out value))
				{
					return value;
				}

				if (outputVariable != null && !string.IsNullOrWhiteSpace(outputVariable.GlobalVariableName) &&
					context.TryGetData(outputVariable.GlobalVariableName, out value))
				{
					return value;
				}
			}

			if (RuntimeCommunicationOutputService.TryGetLatestOutputValue(protocolName, outputName, out value))
			{
				return value;
			}

			return string.Empty;
		}

		private CommOutputVariable FindSignalOutputVariable(string protocolName, string instanceName, string outputName)
		{
			if (string.IsNullOrWhiteSpace(outputName))
			{
				return null;
			}

			CommunicationConfig config = CommunicationConfigStore.LoadOrCreateDefault();
			protocolName = CommunicationRuntimeNaming.NormalizeProtocolName(protocolName);
			instanceName = CommunicationRuntimeNaming.NormalizeInstanceName(protocolName, instanceName, config);
			CommunicationInstanceConfig instance =
				CommunicationRuntimeNaming.FindInstance(config, protocolName, instanceName);

			IEnumerable<CommOutputVariable> outputs = null;
			if (protocolName.Equals("TCP/IP", StringComparison.OrdinalIgnoreCase))
			{
				outputs = instance != null && instance.TcpIp != null
					? instance.TcpIp.OutputVariables
					: (config.TcpIp == null ? null : config.TcpIp.OutputVariables);
			}
			else if (protocolName.Equals("Profinet", StringComparison.OrdinalIgnoreCase))
			{
				outputs = instance != null && instance.Profinet != null
					? instance.Profinet.OutputVariables
					: (config.Profinet == null ? null : config.Profinet.OutputVariables);
			}
			else if (protocolName.Equals("S7", StringComparison.OrdinalIgnoreCase))
			{
				outputs = instance != null && instance.S7 != null
					? instance.S7.OutputVariables
					: (config.S7 == null ? null : config.S7.OutputVariables);
			}

			if (outputs == null)
			{
				return null;
			}

			return outputs.FirstOrDefault(x =>
				x != null &&
				string.Equals(x.Name, outputName.Trim(), StringComparison.OrdinalIgnoreCase));
		}

		private object ResolveSignalOutputValue(string assignedValue, VisionRunContext context)
		{
			string text = assignedValue == null ? string.Empty : assignedValue.Trim();

			if (text.Length >= 2 && text.StartsWith("{", StringComparison.Ordinal) && text.EndsWith("}", StringComparison.Ordinal))
			{
				string key = text.Substring(1, text.Length - 2).Trim();
				object value;
				if (TryResolveSignalOutputToken(key, context, out value))
				{
					return value;
				}

				return string.Empty;
			}

			if (text.StartsWith("$", StringComparison.Ordinal) && text.Length > 1)
			{
				string key = text.Substring(1).Trim();
				object value;
				if (TryResolveSignalOutputToken(key, context, out value))
				{
					return value;
				}

				return string.Empty;
			}

			return assignedValue ?? string.Empty;
		}

		private bool TryResolveSignalOutputToken(string key, VisionRunContext context, out object value)
		{
			value = null;
			if (string.IsNullOrWhiteSpace(key))
			{
				return false;
			}

			if (context != null && context.TryGetData(key, out value))
			{
				return true;
			}

			if (context != null && context.Data != null)
			{
				foreach (KeyValuePair<string, object> pair in context.Data)
				{
					if (string.Equals(pair.Key, key, StringComparison.OrdinalIgnoreCase))
					{
						value = pair.Value;
						return true;
					}
				}
			}

			return GlobalVariableStore.TryGetValue(key, out value);
		}

		private bool GetPreviousStepsOK(VisionRunContext context)
		{
			if (context == null)
			{
				return true;
			}

			object value = context.GetData(CSharpScriptStepStore.PreviousStepsOkInputName);
			if (value is bool)
			{
				return (bool)value;
			}

			if (value != null)
			{
				bool parsed;
				if (bool.TryParse(Convert.ToString(value), out parsed))
				{
					return parsed;
				}
			}

			return true;
		}

		private void FillStepInputSnapshot(
			StepConfig stepConfig,
			StepResult result,
			bool previousStepsOK,
			VisionRunContext context)
		{
			if (result == null || result.Inputs == null)
			{
				return;
			}

			result.Inputs[CSharpScriptStepStore.PreviousStepsOkInputName] = previousStepsOK;

			if (stepConfig == null || stepConfig.InputPins == null)
			{
				return;
			}

			foreach (PinConfig pin in stepConfig.InputPins)
			{
				if (pin == null || string.IsNullOrWhiteSpace(pin.PinName))
				{
					continue;
				}

				object value = null;
				if (!string.IsNullOrWhiteSpace(pin.GlobalVariableName))
				{
					GlobalVariableStore.TryGetValue(pin.GlobalVariableName, out value);
				}

				if (value == null && context != null)
				{
					value = context.GetData(pin.PinName);
				}

				if (value != null)
				{
					result.Inputs[pin.PinName] = value;
					if (!string.IsNullOrWhiteSpace(pin.GlobalVariableName))
					{
						result.Inputs[pin.GlobalVariableName] = value;
					}
				}
			}
		}

		private bool ShouldSkipForMissingInputImage(StepConfig stepConfig, VisionRunContext context, out string message)
		{
			message = string.Empty;

			if (stepConfig == null ||
				(stepConfig.StepType != StepType.Vpp && stepConfig.StepType != StepType.Halcon))
			{
				return false;
			}

			List<string> sourceKeys = RuntimeImageSourceParser.SplitImageSources(stepConfig.InputImageKey);
			if (sourceKeys.Count <= 0)
			{
				return false;
			}

			foreach (string sourceKey in sourceKeys)
			{
				VisionImage image;
				if (context == null || !context.TryGetImage(sourceKey, out image) || image == null || image.RawImage == null)
				{
					message = "Step skipped because input image is null. Source=" + sourceKey;
					return true;
				}
			}

			return false;
		}

		private void FillDefaultOutputs(StepConfig stepConfig, StepResult result)
		{
			if (stepConfig == null || result == null || stepConfig.OutputPins == null)
			{
				return;
			}

			foreach (PinConfig pin in stepConfig.OutputPins)
			{
				if (pin == null || string.IsNullOrWhiteSpace(pin.PinName) || pin.DataType == PinDataType.Image)
				{
					continue;
				}

				if (!result.Outputs.ContainsKey(pin.PinName))
				{
					result.Outputs[pin.PinName] = 0;
				}

				if (!string.IsNullOrWhiteSpace(pin.GlobalVariableName) &&
					!result.Outputs.ContainsKey(pin.GlobalVariableName))
				{
					result.Outputs[pin.GlobalVariableName] = 0;
				}

				if (!string.IsNullOrWhiteSpace(pin.GlobalVariableName))
				{
					GlobalVariableStore.SetValue(pin.GlobalVariableName, 0);
				}
			}
		}

		private void AppendStepLog(RuntimeLogCategory category, string message)
		{
			RuntimeLogStore.Append(DateTime.Now, category, message);
		}

		private StepConfig CloneStepConfig(StepConfig source)
		{
			StepConfig target = new StepConfig();

			target.StepName = source.StepName;
			target.StepType = source.StepType;
			target.RunOrder = source.RunOrder;
			target.Enabled = source.Enabled;
			target.StopWhenNG = source.StopWhenNG;
			target.StepFolder = source.StepFolder;
			target.InputImageKey = source.InputImageKey;
			target.OutputImageKey = source.OutputImageKey;
			target.Remark = source.Remark;
			target.SourceFilePath = source.SourceFilePath;
			target.ProjectFilePath = source.ProjectFilePath;
			target.DisplayOutputKey = source.DisplayOutputKey;
			target.DisplaySlotName = source.DisplaySlotName;
			target.DisplayResultKey = source.DisplayResultKey;
			target.DisplayMode = source.DisplayMode;
			target.ScriptInputStepKeys = source.ScriptInputStepKeys;

			foreach (string file in source.VppFiles)
			{
				target.VppFiles.Add(file);
			}

			foreach (string file in source.ScriptFiles)
			{
				target.ScriptFiles.Add(file);
			}

			foreach (PinConfig pin in source.InputPins)
			{
				target.InputPins.Add(ClonePin(pin));
			}

			foreach (PinConfig pin in source.OutputPins)
			{
				target.OutputPins.Add(ClonePin(pin));
			}

			return target;
		}

		private PinConfig ClonePin(PinConfig source)
		{
			PinConfig target = new PinConfig();
			target.PinName = source.PinName;
			target.SourceKey = source.SourceKey;
			target.TargetKey = source.TargetKey;
			target.DataType = source.DataType;
			target.Length = source.Length;
			target.Description = source.Description;
			target.GlobalVariableName = source.GlobalVariableName;
			return target;
		}
	}

	public class OutputPinConfig
	{
		[XmlAttribute]
		public string PinName { get; set; }

		[XmlAttribute]
		public string SourceKey { get; set; }

		[XmlAttribute]
		public PinDataType DataType { get; set; }

		[XmlAttribute]
		public int ByteOffset { get; set; }

		[XmlAttribute]
		public int BitOffset { get; set; }

		[XmlAttribute]
		public int Length { get; set; }

		[XmlAttribute]
		public string Remark { get; set; }

		public OutputPinConfig()
		{
			PinName = string.Empty;
			SourceKey = string.Empty;
			DataType = PinDataType.String;
			ByteOffset = 0;
			BitOffset = 0;
			Length = 0;
			Remark = string.Empty;
		}
	}

	[XmlRoot("OutputMappingConfig")]
	public class OutputMappingConfig
	{
		[XmlArray("Pins")]
		[XmlArrayItem("Pin")]
		public List<OutputPinConfig> Pins { get; set; }

		public OutputMappingConfig()
		{
			Pins = new List<OutputPinConfig>();
		}
	}
}
