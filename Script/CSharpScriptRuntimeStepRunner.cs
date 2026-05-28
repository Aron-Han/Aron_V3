using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

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
			CSharpScriptStepConfig scriptConfig = null;

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

				CSharpScriptStepStore.EnsureRequiredInputs(scriptConfig);

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
				CopyRuntimeInputsToStepResult(runtimeInputs, stepResult);

				if (!runResult.IsCompileOK)
				{
					stepResult = BuildNg("Script compile failed: " + runResult.ErrorDetail, sw);
					FillDefaultScriptOutputs(scriptConfig, stepResult);
					return stepResult;
				}

				if (!runResult.IsRunOK)
				{
					stepResult = BuildNg("Script run failed: " + runResult.ErrorDetail, sw);
					FillDefaultScriptOutputs(scriptConfig, stepResult);
					return stepResult;
				}

				foreach (KeyValuePair<string, object> pair in runResult.Outputs)
				{
					string stepKey = _config.StepName + "." + pair.Key;

					if (context != null)
					{
						context.SetData(stepKey, pair.Value);
						context.SetData(pair.Key, pair.Value);

					}

					ScriptPinConfig boundOutputPin = FindOutputPin(scriptConfig, pair.Key);
					if (boundOutputPin != null && !string.IsNullOrWhiteSpace(boundOutputPin.GlobalVariableName))
					{
						GlobalVariableStore.SetValue(boundOutputPin.GlobalVariableName, pair.Value);

						if (context != null)
						{
							context.SetData(boundOutputPin.GlobalVariableName, pair.Value);
						}

						stepResult.Outputs[boundOutputPin.GlobalVariableName] = pair.Value;
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
				FillDefaultScriptOutputs(scriptConfig, stepResult);
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

		private void CopyRuntimeInputsToStepResult(Dictionary<string, object> runtimeInputs, StepResult stepResult)
		{
			if (runtimeInputs == null || stepResult == null || stepResult.Inputs == null)
			{
				return;
			}

			foreach (KeyValuePair<string, object> pair in runtimeInputs)
			{
				if (string.IsNullOrWhiteSpace(pair.Key))
				{
					continue;
				}

				stepResult.Inputs[pair.Key] = pair.Value;
			}
		}

		private Dictionary<string, object> BuildRuntimeInputs(CSharpScriptStepConfig config, VisionRunContext context)
		{
			Dictionary<string, object> result = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
			bool previousStepsOK = true;

			if (context != null)
			{
				object value = context.GetData(CSharpScriptStepStore.PreviousStepsOkInputName);
				if (value is bool)
				{
					previousStepsOK = (bool)value;
				}
				else if (value != null)
				{
					bool parsed;
					if (bool.TryParse(Convert.ToString(value), out parsed))
					{
						previousStepsOK = parsed;
					}
				}
			}

			result[CSharpScriptStepStore.PreviousStepsOkInputName] = previousStepsOK;

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

				if (string.Equals(input.Name, CSharpScriptStepStore.PreviousStepsOkInputName, StringComparison.OrdinalIgnoreCase))
				{
					value = previousStepsOK;
				}
				else if (!string.IsNullOrWhiteSpace(input.GlobalVariableName))
				{
					GlobalVariableStore.TryGetValue(input.GlobalVariableName, out value);
				}

				if (context != null)
				{
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

		private void FillDefaultScriptOutputs(CSharpScriptStepConfig config, StepResult result)
		{
			if (config == null || config.Outputs == null || result == null)
			{
				return;
			}

			foreach (ScriptPinConfig pin in config.Outputs)
			{
				if (pin == null || string.IsNullOrWhiteSpace(pin.Name))
				{
					continue;
				}

				if (!result.Outputs.ContainsKey(pin.Name))
				{
					result.Outputs[pin.Name] = 0;
				}

				if (!string.IsNullOrWhiteSpace(_config.StepName))
				{
					string stepKey = _config.StepName + "." + pin.Name;
					if (!result.Outputs.ContainsKey(stepKey))
					{
						result.Outputs[stepKey] = 0;
					}
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


		private void AddSelectedStepOutputsToRuntimeInputs(Dictionary<string, object> result, VisionRunContext context)
		{
			if (result == null || context == null || _config == null)
			{
				return;
			}

			List<string> stepNames = ParseSeparatedKeys(_config.ScriptInputStepKeys);
			if (stepNames.Count <= 0)
			{
				return;
			}

			foreach (string stepName in stepNames)
			{
				if (string.IsNullOrWhiteSpace(stepName))
				{
					continue;
				}

				StepResult stepResult = null;
				if (context.StepResults != null)
				{
					context.StepResults.TryGetValue(stepName, out stepResult);
				}

				if (stepResult != null && stepResult.Outputs != null)
				{
					foreach (KeyValuePair<string, object> pair in stepResult.Outputs)
					{
						SetInputIfMissing(result, stepName + "." + pair.Key, pair.Value);
						SetInputIfMissing(result, pair.Key, pair.Value);
					}
				}

				if (context.Data != null)
				{
					string prefix = stepName + ".";
					foreach (KeyValuePair<string, object> pair in context.Data)
					{
						if (pair.Key != null && pair.Key.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
						{
							SetInputIfMissing(result, pair.Key, pair.Value);
						}
					}
				}
			}
		}

		private void SetInputIfMissing(Dictionary<string, object> inputs, string key, object value)
		{
			if (inputs == null || string.IsNullOrWhiteSpace(key))
			{
				return;
			}

			if (!inputs.ContainsKey(key))
			{
				inputs[key] = value;
			}
		}

		private List<string> ParseSeparatedKeys(string text)
		{
			List<string> result = new List<string>();

			if (string.IsNullOrWhiteSpace(text))
			{
				return result;
			}

			string[] parts = text.Split(new char[] { ';', ',', '|', '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
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

			// 1. 优先当前 Task 下的项目脚本。不同 Task 可以有同名 Script，不能先用旧的 SourceFilePath。
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

			// 3. 最后才允许使用 SourceFilePath，避免跨 Task 复用旧文件。
			p = ResolvePossiblePath(step.SourceFilePath, step, context);

			if (IsScriptCodeFile(p) && File.Exists(p))
			{
				return p;
			}

			// 4. 再从标准目录找同名文件，最后才允许使用文件夹内第一个脚本。
			string folder = GetScriptFolder(step, context);

			if (!string.IsNullOrWhiteSpace(folder) && Directory.Exists(folder))
			{
				string stepName = MakeSafeName(step.StepName);
				string directCsx = Path.Combine(folder, stepName + ".csx");
				if (File.Exists(directCsx))
				{
					return directCsx;
				}

				string directCs = Path.Combine(folder, stepName + ".cs");
				if (File.Exists(directCs))
				{
					return directCs;
				}

				string directTxt = Path.Combine(folder, stepName + ".txt");
				if (File.Exists(directTxt))
				{
					return directTxt;
				}

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
			string taskFolder = GetTaskFolder(context);
			string p0 = string.IsNullOrWhiteSpace(taskFolder) ? string.Empty : Path.Combine(taskFolder, path);

			if (File.Exists(p0))
			{
				return p0;
			}

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

		private string GetTaskFolder(VisionRunContext context)
		{
			string jobName = context == null ? string.Empty : context.JobName;
			string taskName = context == null ? string.Empty : context.TaskName;
			string protocolName = context == null ? string.Empty : Convert.ToString(context.GetData("Comm.Protocol"));
			string channelName = context == null ? string.Empty : Convert.ToString(context.GetData("Comm.Channel"));

			return FlowConfigStore.PathManager.GetTaskFolder(protocolName, channelName, jobName, taskName);
		}

		private string GetScriptFolder(StepConfig step, VisionRunContext context)
		{
			string taskFolder = GetTaskFolder(context);
			string scriptsFolder = Path.Combine(taskFolder, "Script");

			if (Directory.Exists(scriptsFolder))
			{
				return scriptsFolder;
			}

			string scriptFolder = Path.Combine(taskFolder, "Scripts");

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
