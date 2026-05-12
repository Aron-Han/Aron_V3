using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
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

	public class ProjectPathManager
	{
		public string ProjectRoot { get; private set; }

		public string ConfigRoot { get { return Path.Combine(ProjectRoot, "Config"); } }
		public string FlowConfigRoot { get { return Path.Combine(ConfigRoot, "Flow"); } }
		public string HardwareConfigRoot { get { return Path.Combine(ConfigRoot, "Hardware"); } }
		public string CommunicationConfigRoot { get { return Path.Combine(ConfigRoot, "Communication"); } }
		public string StepsRoot { get { return Path.Combine(ProjectRoot, "Steps"); } }
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
			Directory.CreateDirectory(FlowConfigRoot);
			Directory.CreateDirectory(HardwareConfigRoot);
			Directory.CreateDirectory(CommunicationConfigRoot);
			Directory.CreateDirectory(StepsRoot);
			Directory.CreateDirectory(Path.Combine(ImagesRoot, "Save"));
			Directory.CreateDirectory(Path.Combine(ImagesRoot, "Replay"));
			Directory.CreateDirectory(LogsRoot);
		}

		public string GetJobFolder(string jobName)
		{
			return Path.Combine(StepsRoot, jobName);
		}

		public string GetTaskFolder(string jobName, string taskName)
		{
			return Path.Combine(StepsRoot, jobName, taskName);
		}

		public string GetStepFolder(string jobName, string taskName, string stepName)
		{
			// 新目录结构不再使用 StepName 作为文件夹层级
			// Project/Steps/JobName/TaskName
			return Path.Combine(StepsRoot, jobName, taskName);
		}

		public void EnsureStepFolder(string jobName, string taskName, string stepName)
		{
			// 新目录结构：
			// Project/Steps/JobName/TaskName/VPP
			// Project/Steps/JobName/TaskName/Scripts
			string taskFolder = GetStepFolder(jobName, taskName, stepName);

			Directory.CreateDirectory(taskFolder);
			Directory.CreateDirectory(Path.Combine(taskFolder, "VPP"));
			Directory.CreateDirectory(Path.Combine(taskFolder, "Scripts"));
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

		public VisionImage()
		{
			ImageName = string.Empty;
			ImageType = string.Empty;
			SourceStep = string.Empty;
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

		public static StepResult NG(string message)
		{
			return new StepResult { IsOK = false, Message = message };
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

		public PinConfig()
		{
			PinName = string.Empty;
			SourceKey = string.Empty;
			TargetKey = string.Empty;
			Description = string.Empty;
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

		public StepFlowItem()
		{
			StepName = string.Empty;
			InputImageKey = string.Empty;
			RunOrder = 0;
			Enabled = true;
			Remark = string.Empty;
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

		// 旧字段保留，避免旧 XML 或旧代码报错。
		// 新逻辑里它可以不再代表 PLC 输入地址。
		[XmlAttribute]
		public string InputAddress { get; set; }

		// 新字段：图像源。
		// 例如：
		// 无
		// Cam1.Raw
		// Camera1.Raw
		// TopCamera.Raw
		[XmlAttribute]
		public string ImageSourceKey { get; set; }

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

		private static string _projectRoot = Path.Combine(Application.StartupPath, "Project", "DemoProject");

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
			PathManager.EnsureProjectFolders();

			ProjectFlowConfig config = XmlConfigHelper.Load<ProjectFlowConfig>(FlowConfigFile);

			if (config.Jobs.Count <= 0)
			{
				config = CreateDefaultConfig();
				Save(config);
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
			step.StepFolder = Path.Combine("Steps", jobName, taskName);
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

			return step;
		}

		public static StepType GetStepTypeByFilePath(string filePath)
		{
			string ext = Path.GetExtension(filePath).ToLower();

			if (ext == ".vpp") return StepType.Vpp;
			if (ext == ".cs" || ext == ".csx" || ext == ".txt") return StepType.Script;

			return StepType.Unknown;
		}

		private static ProjectFlowConfig CreateDefaultConfig()
		{
			ProjectFlowConfig config = new ProjectFlowConfig();

			JobConfig job = new JobConfig();
			job.JobName = "Job_001";
			job.Enabled = true;

			TaskConfig task = CreateDefaultTask("Job_001", "Task_Main", 1);
			job.Tasks.Add(task);

			config.Jobs.Add(job);
			return config;
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

			foreach (JobConfig job in config.Jobs)
			{
				foreach (TaskConfig task in job.Tasks)
				{
					foreach (StepConfig step in task.Steps)
					{
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
				// TODO:
				// 1. 使用 _config.StepFolder + _config.VppFiles[0] 定位 Project 内部 VPP
				// 2. 从 context.Images[_config.InputImageKey] 获取输入图像
				// 3. 设置 ToolBlock.Inputs
				// 4. Run
				// 5. 读取 ToolBlock.Outputs 到 result.Outputs
				// 6. 读取输出图像到 result.OutputImages

				result.Outputs["OK"] = true;
				result.Outputs["Score"] = 0.99;
				result.Message = "VPP step demo executed.";
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
					return new ScriptStep(config);

				default:
					throw new NotSupportedException("Unsupported step type: " + config.StepType);
			}
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
