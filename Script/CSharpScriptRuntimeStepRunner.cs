using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace Aron_V3
{
	/// <summary>
	/// C# Script Step 运行器。
	/// 注意：
	/// 你当前工程中的统一 Step 接口是 IVisionStep，而不是 IVisionStepRunner。
	/// 因此这个类必须实现 IVisionStep，并由 StepFactory.Create() 创建。
	/// </summary>
	public class CSharpScriptRuntimeStepRunner : IVisionStep
	{
		private readonly StepConfig _config;

		public string StepName
		{
			get { return _config == null ? string.Empty : _config.StepName; }
		}

		public StepType StepType
		{
			get { return StepType.Script; }
		}

		public CSharpScriptRuntimeStepRunner(StepConfig config)
		{
			_config = config;
		}

		public StepResult Execute(VisionRunContext context)
		{
			Stopwatch sw = Stopwatch.StartNew();
			StepResult stepResult = new StepResult();

			try
			{
				if (_config == null)
				{
					return BuildNg("Script step config is null.", sw);
				}

				if (!_config.Enabled)
				{
					stepResult.IsOK = true;
					stepResult.Message = "Script step disabled.";
					stepResult.CostMs = sw.Elapsed.TotalMilliseconds;
					return stepResult;
				}

				string configPath = ResolveScriptConfigPath(_config, context);
				string scriptPath = ResolveScriptFilePath(_config, context);

				CSharpScriptStepConfig scriptConfig = null;

				if (!string.IsNullOrWhiteSpace(configPath) && File.Exists(configPath))
				{
					scriptConfig = CSharpScriptStepStore.Load(configPath);
				}
				else
				{
					// 如果还没保存 .script.xml，允许使用默认配置先跑通调试。
					scriptConfig = CSharpScriptStepStore.CreateDefaultConfig();
				}

				if (scriptConfig == null)
				{
					return BuildNg("Script config load failed: " + configPath, sw);
				}

				if (!scriptConfig.Enable)
				{
					stepResult.IsOK = true;
					stepResult.Message = "Script step disabled.";
					stepResult.CostMs = sw.Elapsed.TotalMilliseconds;
					return stepResult;
				}

				if (string.IsNullOrWhiteSpace(scriptPath))
				{
					scriptPath = scriptConfig.ScriptFilePath;
				}

				if (string.IsNullOrWhiteSpace(scriptPath) || !File.Exists(scriptPath))
				{
					return BuildNg("Script file not found: " + scriptPath, sw);
				}

				string code = File.ReadAllText(scriptPath, System.Text.Encoding.UTF8);

				Dictionary<string, object> runtimeInputs = BuildRuntimeInputs(scriptConfig, context);

				CSharpScriptStepRunner runner = new CSharpScriptStepRunner();
				CSharpScriptRunResult runResult = runner.CompileAndRun(scriptConfig, code, runtimeInputs);

				if (!runResult.IsCompileOK)
				{
					return BuildNg("Script compile failed: " + runResult.ErrorDetail, sw);
				}

				if (!runResult.IsRunOK)
				{
					return BuildNg("Script run failed: " + runResult.ErrorDetail, sw);
				}

				foreach (KeyValuePair<string, object> pair in runResult.Outputs)
				{
					string stepKey = _config.StepName + "." + pair.Key;

					if (context != null)
					{
						context.SetData(stepKey, pair.Value);
						context.SetData(pair.Key, pair.Value);

						ScriptPinConfig outputPin = FindOutputPin(scriptConfig, pair.Key);

						if (outputPin != null && !string.IsNullOrWhiteSpace(outputPin.BindingPath))
						{
							context.SetData(outputPin.BindingPath, pair.Value);
						}
					}

					stepResult.Outputs[pair.Key] = pair.Value;
					stepResult.Outputs[stepKey] = pair.Value;
				}

				stepResult.IsOK = true;
				stepResult.Message = "C# script run OK.";
			}
			catch (Exception ex)
			{
				stepResult.IsOK = false;
				stepResult.Message = ex.ToString();
			}
			finally
			{
				sw.Stop();
				stepResult.CostMs = sw.Elapsed.TotalMilliseconds;
			}

			return stepResult;
		}

		private StepResult BuildNg(string message, Stopwatch sw)
		{
			if (sw != null)
			{
				sw.Stop();
			}

			StepResult result = new StepResult();
			result.IsOK = false;
			result.Message = message;
			result.CostMs = sw == null ? 0 : sw.Elapsed.TotalMilliseconds;
			return result;
		}

		private Dictionary<string, object> BuildRuntimeInputs(CSharpScriptStepConfig config, VisionRunContext context)
		{
			Dictionary<string, object> result = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);

			if (config == null || config.Inputs == null)
			{
				return result;
			}

			foreach (ScriptPinConfig input in config.Inputs)
			{
				if (input == null || string.IsNullOrWhiteSpace(input.Name))
				{
					continue;
				}

				object value = null;

				if (context != null)
				{
					if (!string.IsNullOrWhiteSpace(input.BindingPath))
					{
						value = context.GetData(input.BindingPath);
					}

					if (value == null)
					{
						value = context.GetData(input.Name);
					}
				}

				if (value == null)
				{
					value = input.DefaultValue;
				}

				result[input.Name] = value;
			}

			return result;
		}

		private ScriptPinConfig FindOutputPin(CSharpScriptStepConfig config, string name)
		{
			if (config == null || config.Outputs == null)
			{
				return null;
			}

			foreach (ScriptPinConfig pin in config.Outputs)
			{
				if (pin != null &&
					pin.Name != null &&
					pin.Name.Equals(name, StringComparison.OrdinalIgnoreCase))
				{
					return pin;
				}
			}

			return null;
		}

		private string ResolveScriptConfigPath(StepConfig step, VisionRunContext context)
		{
			string scriptPath = ResolveScriptFilePath(step, context);

			if (!string.IsNullOrWhiteSpace(scriptPath))
			{
				string sameNameConfig = Path.ChangeExtension(scriptPath, ".script.xml");

				if (File.Exists(sameNameConfig))
				{
					return sameNameConfig;
				}
			}

			string folder = GetScriptFolder(step, context);

			if (!string.IsNullOrWhiteSpace(folder) && Directory.Exists(folder))
			{
				string[] files = Directory.GetFiles(folder, "*.script.xml", SearchOption.TopDirectoryOnly);

				if (files.Length > 0)
				{
					return files[0];
				}
			}

			return string.Empty;
		}

		private string ResolveScriptFilePath(StepConfig step, VisionRunContext context)
		{
			if (step == null)
			{
				return string.Empty;
			}

			// 1. 优先 ProjectFilePath。
			string p = ResolvePossiblePath(step.ProjectFilePath, step, context);

			if (IsScriptCodeFile(p) && File.Exists(p))
			{
				return p;
			}

			// 2. 再找 ScriptFiles。
			if (step.ScriptFiles != null)
			{
				foreach (string file in step.ScriptFiles)
				{
					p = ResolvePossiblePath(file, step, context);

					if (IsScriptCodeFile(p) && File.Exists(p))
					{
						return p;
					}
				}
			}

			// 3. 再从标准目录找。
			string folder = GetScriptFolder(step, context);

			if (!string.IsNullOrWhiteSpace(folder) && Directory.Exists(folder))
			{
				string[] csxFiles = Directory.GetFiles(folder, "*.csx", SearchOption.TopDirectoryOnly);

				if (csxFiles.Length > 0)
				{
					return csxFiles[0];
				}

				string[] csFiles = Directory.GetFiles(folder, "*.cs", SearchOption.TopDirectoryOnly);

				if (csFiles.Length > 0)
				{
					return csFiles[0];
				}
			}

			return string.Empty;
		}

		private string ResolvePossiblePath(string path, StepConfig step, VisionRunContext context)
		{
			if (string.IsNullOrWhiteSpace(path))
			{
				return string.Empty;
			}

			if (Path.IsPathRooted(path))
			{
				return path;
			}

			string folder = GetScriptFolder(step, context);
			string p1 = string.IsNullOrWhiteSpace(folder) ? string.Empty : Path.Combine(folder, path);

			if (File.Exists(p1))
			{
				return p1;
			}

			string p2 = Path.Combine(ProjectPathStore.ProjectRoot, path);

			if (File.Exists(p2))
			{
				return p2;
			}

			return p1;
		}

		private string GetScriptFolder(StepConfig step, VisionRunContext context)
		{
			string jobName = context == null ? string.Empty : context.JobName;
			string taskName = context == null ? string.Empty : context.TaskName;

			if (string.IsNullOrWhiteSpace(jobName))
			{
				jobName = "Job_001";
			}

			if (string.IsNullOrWhiteSpace(taskName))
			{
				taskName = "Task_New_01";
			}

			// 和 ProjectPathManager.EnsureStepFolder 保持一致：Scripts 复数。
			string taskFolder = Path.Combine(ProjectPathStore.ProjectRoot, "Job", MakeSafeName(jobName), "Task", MakeSafeName(taskName));
			string scriptsFolder = Path.Combine(taskFolder, "Scripts");

			if (Directory.Exists(scriptsFolder))
			{
				return scriptsFolder;
			}

			// 兼容之前我给你的 Script 单数目录。
			string scriptFolder = Path.Combine(taskFolder, "Script");

			if (Directory.Exists(scriptFolder))
			{
				return scriptFolder;
			}

			return scriptsFolder;
		}

		private bool IsScriptCodeFile(string path)
		{
			if (string.IsNullOrWhiteSpace(path))
			{
				return false;
			}

			string ext = Path.GetExtension(path);
			return ext.Equals(".csx", StringComparison.OrdinalIgnoreCase) ||
				   ext.Equals(".cs", StringComparison.OrdinalIgnoreCase) ||
				   ext.Equals(".txt", StringComparison.OrdinalIgnoreCase);
		}

		private string MakeSafeName(string name)
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
}
