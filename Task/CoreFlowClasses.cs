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

	public interface ICommunicationRuntimeValueProvider
	{
		string GetInputValue(string protocol, string tagName);
	}

	public class ProjectPathManager
	{
		public string ProjectRoot { get; private set; }

		public string ConfigRoot { get { return Path.Combine(ProjectRoot, "Config"); } }
		public string FlowConfigRoot { get { return Path.Combine(ProjectRoot, "Job"); } }
		public string HardwareConfigRoot { get { return Path.Combine(ConfigRoot, "Hardware"); } }
		public string CommunicationConfigRoot { get { return Path.Combine(ConfigRoot, "Communication"); } }

		public string JobRoot { get { return Path.Combine(ProjectRoot, "Job"); } }

		// 保留 StepsRoot 属性，兼容旧代码调用。
		// 但新路径不再使用 Project\Steps，而是统一放在 Project\Job\<JobName>\Task\<TaskName> 下。
		public string StepsRoot { get { return JobRoot; } }

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
			Directory.CreateDirectory(ConfigRoot);
			Directory.CreateDirectory(JobRoot);
			Directory.CreateDirectory(FlowConfigRoot);
			Directory.CreateDirectory(CommunicationConfigRoot);
			Directory.CreateDirectory(Path.Combine(ImagesRoot, "Save"));
			Directory.CreateDirectory(Path.Combine(ImagesRoot, "Replay"));
			Directory.CreateDirectory(LogsRoot);

			// 注意：
			// 不在这里创建 Config\Hardware。
			// Hardware 已经改为每个 Job 内部：
			// Project\Job\<JobName>\Hardware
		}

		public string GetJobFolder(string jobName)
		{
			if (string.IsNullOrWhiteSpace(jobName))
			{
				jobName = "Job_001";
			}

			return Path.Combine(JobRoot, MakeSafeName(jobName));
		}

		public string GetJobHardwareFolder(string jobName)
		{
			return Path.Combine(GetJobFolder(jobName), "Hardware");
		}

		public string GetTaskRootFolder(string jobName)
		{
			return Path.Combine(GetJobFolder(jobName), "Task");
		}

		public string GetTaskFolder(string jobName, string taskName)
		{
			if (string.IsNullOrWhiteSpace(taskName))
			{
				taskName = "Task_New_01";
			}

			return Path.Combine(GetTaskRootFolder(jobName), MakeSafeName(taskName));
		}

		public string GetStepFolder(string jobName, string taskName, string stepName)
		{
			// 新目录结构不再使用 StepName 作为文件夹层级。
			// 所有当前 Task 使用到的 VPP / Script 放在：
			// Project\Job\<JobName>\Task\<TaskName>\VPP
			// Project\Job\<JobName>\Task\<TaskName>\Scripts
			return GetTaskFolder(jobName, taskName);
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
			string taskFolder = GetStepFolder(jobName, taskName, stepName);

			Directory.CreateDirectory(taskFolder);
			Directory.CreateDirectory(Path.Combine(taskFolder, "VPP"));
			Directory.CreateDirectory(Path.Combine(taskFolder, "Scripts"));
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

		public Dictionary<string, object> Outputs { get; private set; }
		public Dictionary<string, VisionImage> OutputImages { get; private set; }

		public StepResult()
		{
			IsOK = true;
			Message = string.Empty;
			Outputs = new Dictionary<string, object>();
			OutputImages = new Dictionary<string, VisionImage>();
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

		// 复制到 Project 后的相对路径，例如 VPP\Camera.vpp 或 Scripts\Output.csx
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
			DisplayMode = "Fit";
			ScriptInputStepKeys = string.Empty;
		}
	}

	// StepFlowItem 表示右侧“当前 task 中实际执行的算子流程”。
	// RunOrder 允许重复：1、1、2 代表 RunOrder=1 的 Step 并行执行，全部完成后执行 RunOrder=2。
	public class StepFlowItem
	{
		[XmlAttribute]
		public string StepName { get; set; }

		[XmlAttribute]
		public string InputImageKey { get; set; }

		[XmlAttribute]
		public int RunOrder { get; set; }

		[XmlAttribute]
		public bool Enabled { get; set; }

		[XmlAttribute]
		public string Remark { get; set; }

		public string DisplayOutputKey { get; set; }
		public string DisplaySlotName { get; set; }
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

		public StepFlowItem()
		{
			StepName = "";
			InputImageKey = "";
			RunOrder = 1;
			Enabled = true;
			Remark = "";

			DisplayOutputKey = "Not Use";
			DisplaySlotName = "Not Show";
			DisplayMode = "Fit";
			ScriptInputStepKeys = string.Empty;
		}
	}

	public class TaskConfig
	{
		[XmlAttribute]
		public string TaskName { get; set; }

		[XmlAttribute]
		public string CommunicationProtocol { get; set; }

		[XmlAttribute]
		public int RunOrder { get; set; }

		[XmlAttribute]
		public bool Enabled { get; set; }

		[XmlAttribute]
		public string TriggerName { get; set; }

		// 新字段：触发源值。
		// 只有通讯运行时读取到的 TriggerName 实际值满足 TriggerValue，才允许执行当前 Task。
		[XmlAttribute]
		public string TriggerValue { get; set; }

		[XmlAttribute]
		public TriggerCompareType TriggerCompare { get; set; }

		// 新字段：位置号。
		// 原“标志位”改为“位置号”，旧字段 FlagBit 继续保留用于兼容旧 XML。
		[XmlAttribute]
		public string PositionName { get; set; }

		// 新字段：位置号值。
		// 原“标志值”改为“位置号值”，旧字段 FlagValue 继续保留用于兼容旧 XML。
		[XmlAttribute]
		public string PositionValue { get; set; }

		[XmlAttribute]
		public TriggerCompareType PositionCompare { get; set; }

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

		public TaskConfig()
		{
			CommunicationProtocol = string.Empty;
			TaskName = string.Empty;
			RunOrder = 0;
			Enabled = true;
			TriggerName = string.Empty;
			TriggerValue = "1";
			TriggerCompare = TriggerCompareType.Equal;
			PositionName = "Not Use";
			PositionValue = "1";
			PositionCompare = TriggerCompareType.Equal;
			InputAddress = string.Empty;
			ImageSourceKey = "Not Use";
			FlagBit = 0;
			FlagValue = string.Empty;
			Remark = string.Empty;
			Steps = new List<StepConfig>();
			StepFlow = new List<StepFlowItem>();
		}
	}


	public class JobConfig
	{
		[XmlAttribute]
		public string JobName { get; set; }

		[XmlAttribute]
		public bool Enabled { get; set; }

		[XmlArray("Tasks")]
		[XmlArrayItem("Task")]
		public List<TaskConfig> Tasks { get; set; }

		public JobConfig()
		{
			JobName = string.Empty;
			Enabled = true;
			Tasks = new List<TaskConfig>();
		}
	}

	[XmlRoot("ProjectFlowConfig")]
	public class ProjectFlowConfig
	{
		[XmlArray("Jobs")]
		[XmlArrayItem("Job")]
		public List<JobConfig> Jobs { get; set; }

		public ProjectFlowConfig()
		{
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

		public static ProjectFlowConfig LoadOrCreateDefault()
		{
			ProjectFlowConfig config = new ProjectFlowConfig();

			string filePath = FlowConfigFile;

			if (File.Exists(filePath))
			{
				config = XmlConfigHelper.Load<ProjectFlowConfig>(filePath);

				if (config == null)
				{
					config = new ProjectFlowConfig();
				}
			}

			NormalizeConfig(config);
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
			XmlConfigHelper.Save(FlowConfigFile, config);
			EnsureStepFolders(config);

			EventHandler handler = FlowConfigSaved;
			if (handler != null)
			{
				handler(null, EventArgs.Empty);
			}
		}

		public static JobConfig GetOrCreateJob(ProjectFlowConfig config, string jobName)
		{
			JobConfig job = config.Jobs.FirstOrDefault(j => string.Equals(j.JobName, jobName, StringComparison.OrdinalIgnoreCase));

			if (job == null)
			{
				job = new JobConfig();
				job.JobName = jobName;
				job.Enabled = true;
				config.Jobs.Add(job);
			}

			return job;
		}

		public static TaskConfig CreateDefaultTask(string jobName, string taskName, int runOrder)
		{
			TaskConfig task = new TaskConfig();
			task.TaskName = taskName;
			task.RunOrder = runOrder;
			task.Enabled = true;
			task.TriggerName = "Trigger_" + (runOrder - 1).ToString();
			task.TriggerValue = "1";
			task.TriggerCompare = TriggerCompareType.Equal;
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
			step.StepFolder = Path.Combine("Job", jobName, "Task", taskName);
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
			// 只有用户点击“+”新增 Job 时，才创建 Project\Job\Job_xxx。
			return new ProjectFlowConfig();
		}


		private static void NormalizeConfig(ProjectFlowConfig config)
		{
			if (config.Jobs == null) config.Jobs = new List<JobConfig>();

			foreach (JobConfig job in config.Jobs)
			{
				if (job.Tasks == null) job.Tasks = new List<TaskConfig>();

				foreach (TaskConfig task in job.Tasks)
				{
					if (task.Steps == null) task.Steps = new List<StepConfig>();
					if (task.StepFlow == null) task.StepFlow = new List<StepFlowItem>();

					if (string.IsNullOrEmpty(task.TriggerValue)) task.TriggerValue = "1";
					if (string.IsNullOrEmpty(task.PositionName)) task.PositionName = task.FlagBit.ToString();
					if (string.IsNullOrEmpty(task.PositionValue)) task.PositionValue = task.FlagValue;
					if (string.IsNullOrEmpty(task.PositionValue)) task.PositionValue = "1";

					// 旧字段同步，保证旧代码仍能读取。
					int oldFlagBit;
					if (int.TryParse(task.PositionName, out oldFlagBit)) task.FlagBit = oldFlagBit;
					task.FlagValue = task.PositionValue;

					foreach (StepConfig step in task.Steps)
					{
						if (step.VppFiles == null) step.VppFiles = new List<string>();
						if (step.ScriptFiles == null) step.ScriptFiles = new List<string>();
						if (step.InputPins == null) step.InputPins = new List<PinConfig>();
						if (step.OutputPins == null) step.OutputPins = new List<PinConfig>();
					}
				}
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

				string jobFolder = path.GetJobFolder(job.JobName);
				Directory.CreateDirectory(jobFolder);

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
					// Project\Job\<JobName>\Task\<TaskName>
					string taskFolder = path.GetTaskFolder(job.JobName, task.TaskName);
					Directory.CreateDirectory(taskFolder);

					// 兼容迁移旧路径：
					// Project\Job\<JobName>\<TaskName>
					// 如果旧目录存在，则迁移到：
					// Project\Job\<JobName>\Task\<TaskName>
					string legacyTaskFolder = Path.Combine(jobFolder, task.TaskName);

					if (Directory.Exists(legacyTaskFolder) &&
						!string.Equals(legacyTaskFolder, taskFolder, StringComparison.OrdinalIgnoreCase))
					{
						try
						{
							MoveDirectoryContent(legacyTaskFolder, taskFolder);
							Directory.Delete(legacyTaskFolder, true);
						}
						catch
						{
							// 迁移失败不影响软件启动和保存，后续可以手动处理旧目录。
						}
					}

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
							path.EnsureStepFolder(job.JobName, task.TaskName, step.StepName);
						}
					}
				}
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
			string taskFolder = FlowConfigStore.PathManager.GetTaskFolder(context.JobName, context.TaskName);
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
				outputKey.Equals("Not Use", StringComparison.OrdinalIgnoreCase) ||
				result.OutputImages.ContainsKey(outputKey))
			{
				return;
			}

			object displayRecord;
			object value = TryGetLastRunImage(toolBlock, outputKey, out displayRecord);
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
			image.DisplayRecordKey = outputKey.Substring("LastRun.".Length);
			result.OutputImages[outputKey] = image;
		}

		private object TryGetLastRunImage(object toolBlock, string outputKey, out object displayRecord)
		{
			displayRecord = null;
			if (toolBlock == null || string.IsNullOrWhiteSpace(outputKey) ||
				!outputKey.StartsWith("LastRun.", StringComparison.OrdinalIgnoreCase))
			{
				return null;
			}

			object record = null;
			MethodInfo createRecord = toolBlock.GetType().GetMethod("CreateLastRunRecord", Type.EmptyTypes);
			if (createRecord != null)
			{
				record = createRecord.Invoke(toolBlock, null);
			}

			if (record == null)
			{
				record = GetPropertyValue(toolBlock, "LastRunRecord");
			}

			if (record == null)
			{
				return null;
			}

			object rootRecord = record;
			string relativeKey = outputKey.Substring("LastRun.".Length);
			object imageRecord = FindRecordByKey(record, relativeKey);
			if (imageRecord != null)
			{
				displayRecord = rootRecord;
				object imageContent = GetPropertyValue(imageRecord, "Content");
				return imageContent ?? GetPropertyValue(imageRecord, "Image");
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
			object content = GetPropertyValue(record, "Content");
			return content ?? GetPropertyValue(record, "Image");
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
				// 后续这里执行 Step 文件夹 Scripts 内部脚本。
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
		private static bool _halconAssembliesLoaded;
		private static string _halconAssemblyLoadMessage = string.Empty;

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

				object program = Activator.CreateInstance(programType, new object[] { filePath });
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

		private string ResolveHdevPath(VisionRunContext context)
		{
			string taskFolder = FlowConfigStore.PathManager.GetTaskFolder(context.JobName, context.TaskName);
			List<string> candidates = new List<string>();
			AddFileCandidate(candidates, _config.ProjectFilePath, taskFolder);

			if (string.IsNullOrWhiteSpace(_config.ProjectFilePath) && !string.IsNullOrWhiteSpace(_config.StepName))
			{
				AddFileCandidate(candidates, Path.Combine("Hdev", _config.StepName + ".hdev"), taskFolder);
			}

			AddFileCandidate(candidates, _config.SourceFilePath, taskFolder);
			return candidates.FirstOrDefault(File.Exists) ?? (candidates.Count > 0 ? candidates[0] : string.Empty);
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

		private Type FindHalconType(string typeName)
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

		private Type FindLoadedHalconType(string typeName)
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

		private void LoadHalconAssemblies()
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

		private void ConfigureHalconNativeSearchPath(List<string> messages)
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

		private bool TryLoadAssemblyByName(string assemblyName, List<string> messages)
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

		private bool TryLoadAssemblyFromFile(string filePath, List<string> messages)
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

		private IEnumerable<string> GetHalconAssemblyCandidates(string fileName)
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

		private void AddHalconAssemblyCandidate(HashSet<string> candidates, string folder, string fileName)
		{
			if (string.IsNullOrWhiteSpace(folder))
			{
				return;
			}

			candidates.Add(Path.Combine(folder, fileName));
		}

		private IEnumerable<string> GetHalconRoots()
		{
			HashSet<string> roots = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
			AddHalconRoot(roots, Environment.GetEnvironmentVariable("HALCONROOT"));
			AddHalconRoot(roots, Environment.GetEnvironmentVariable("HALCON_ROOT"));
			AddHalconRoot(roots, Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Programs", "MVTec", "HALCON-25.05-Progress"));
			return roots;
		}

		private void AddHalconRoot(HashSet<string> roots, string root)
		{
			if (!string.IsNullOrWhiteSpace(root))
			{
				roots.Add(root);
			}
		}

		private IEnumerable<string> GetHalconNativeDirs(string root)
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

			string triggerActualValue = valueProvider.GetInputValue(taskConfig.CommunicationProtocol, taskConfig.TriggerName);
			bool triggerOk = CompareValue(triggerActualValue, taskConfig.TriggerValue, taskConfig.TriggerCompare);

			if (string.IsNullOrWhiteSpace(taskConfig.PositionName) ||
				taskConfig.PositionName.Equals("Not Use", StringComparison.OrdinalIgnoreCase) ||
				taskConfig.PositionName.Equals("None", StringComparison.OrdinalIgnoreCase))
			{
				return triggerOk;
			}

			string positionActualValue = valueProvider.GetInputValue(taskConfig.CommunicationProtocol, taskConfig.PositionName);
			bool positionOk = CompareValue(positionActualValue, taskConfig.PositionValue, taskConfig.PositionCompare);

			return triggerOk && positionOk;
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

			List<IGrouping<int, StepFlowItem>> groups = taskConfig.StepFlow
				.Where(x => x.Enabled)
				.OrderBy(x => x.RunOrder)
				.GroupBy(x => x.RunOrder)
				.ToList();

			foreach (IGrouping<int, StepFlowItem> group in groups)
			{
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

				bool hasNgAndStop = false;

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

					if (!stepResult.IsOK && stepConfig.StopWhenNG)
					{
						hasNgAndStop = true;
					}
				}

				// 当前 RunOrder 这一组全部完成后，如果有 NG 且 StopWhenNG=true，后续 RunOrder 不再执行。
				if (hasNgAndStop)
				{
					break;
				}
			}

			return finalResult;
		}

		private class StepExecuteResult
		{
			public StepConfig StepConfig { get; set; }
			public StepResult StepResult { get; set; }
		}

		private StepExecuteResult ExecuteOneStep(TaskConfig taskConfig, StepFlowItem flowItem, VisionRunContext context)
		{
			StepExecuteResult executeResult = new StepExecuteResult();

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

			if (!string.IsNullOrEmpty(flowItem.ScriptInputStepKeys))
			{
				runStepConfig.ScriptInputStepKeys = flowItem.ScriptInputStepKeys;
			}

			runStepConfig.DisplayOutputKey = flowItem.DisplayOutputKey;
			runStepConfig.DisplaySlotName = flowItem.DisplaySlotName;
			runStepConfig.DisplayMode = flowItem.DisplayMode;

			executeResult.StepConfig = runStepConfig;

			try
			{
				IVisionStep step = StepFactory.Create(runStepConfig);
				executeResult.StepResult = step.Execute(context);
			}
			catch (Exception ex)
			{
				executeResult.StepResult = StepResult.NG(ex.Message);
			}

			return executeResult;
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
