using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;

namespace Aron_V3
{
	public static class CSharpScriptStepStore
	{
		public const string PreviousStepsOkInputName = "PreviousStepsOK";
		public const string PreviousStepsOkDescription = "Fixed input. True when all previous task steps finished OK.";

		public static CSharpScriptStepConfig Load(string configPath)
		{
			if (string.IsNullOrWhiteSpace(configPath) || !File.Exists(configPath))
			{
				return CreateDefaultConfig();
			}

			try
			{
				using (FileStream fs = new FileStream(configPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
				{
					XmlSerializer xs = new XmlSerializer(typeof(CSharpScriptStepConfig));
					object obj = xs.Deserialize(fs);

					CSharpScriptStepConfig config = obj as CSharpScriptStepConfig;

					if (config == null)
					{
						return CreateDefaultConfig();
					}

					EnsureConfigNotNull(config);
					return config;
				}
			}
			catch
			{
				return CreateDefaultConfig();
			}
		}

		public static void Save(string configPath, CSharpScriptStepConfig config)
		{
			if (config == null)
			{
				throw new ArgumentNullException("config");
			}

			if (string.IsNullOrWhiteSpace(configPath))
			{
				throw new ArgumentException("configPath is empty.", "configPath");
			}

			EnsureConfigNotNull(config);

			string dir = Path.GetDirectoryName(configPath);

			if (!string.IsNullOrWhiteSpace(dir) && !Directory.Exists(dir))
			{
				Directory.CreateDirectory(dir);
			}

			using (FileStream fs = new FileStream(configPath, FileMode.Create, FileAccess.Write, FileShare.Read))
			{
				XmlSerializer xs = new XmlSerializer(typeof(CSharpScriptStepConfig));
				xs.Serialize(fs, config);
			}
		}

		public static string GetScriptFolder(string jobName, string taskName)
		{
			return GetScriptFolder("TCP/IP", "Channel01", jobName, taskName);
		}

		public static string GetScriptFolder(string protocolName, string channelName, string jobName, string taskName)
		{
			return Path.Combine(
				FlowConfigStore.PathManager.GetTaskFolder(protocolName, channelName, jobName, taskName),
				"Script");
		}

		public static string GetConfigPath(string jobName, string taskName, string stepName)
		{
			return GetConfigPath("TCP/IP", "Channel01", jobName, taskName, stepName);
		}

		public static string GetConfigPath(string protocolName, string channelName, string jobName, string taskName, string stepName)
		{
			string folder = GetScriptFolder(protocolName, channelName, jobName, taskName);
			string safeStep = NormalizeFileName(stepName, "CS_Script");

			return Path.Combine(folder, safeStep + ".script.xml");
		}

		public static string GetScriptPath(string jobName, string taskName, string stepName)
		{
			return GetScriptPath("TCP/IP", "Channel01", jobName, taskName, stepName);
		}

		public static string GetScriptPath(string protocolName, string channelName, string jobName, string taskName, string stepName)
		{
			string folder = GetScriptFolder(protocolName, channelName, jobName, taskName);
			string safeStep = NormalizeFileName(stepName, "CS_Script");

			return Path.Combine(folder, safeStep + ".csx");
		}

		public static void EnsureScriptFile(string scriptPath)
		{
			if (string.IsNullOrWhiteSpace(scriptPath))
			{
				return;
			}

			string dir = Path.GetDirectoryName(scriptPath);

			if (!string.IsNullOrWhiteSpace(dir) && !Directory.Exists(dir))
			{
				Directory.CreateDirectory(dir);
			}

			if (File.Exists(scriptPath))
			{
				return;
			}

			File.WriteAllText(scriptPath, GetDefaultScriptTemplate(), System.Text.Encoding.UTF8);
		}

		public static CSharpScriptStepConfig CreateDefaultConfig()
		{
			CSharpScriptStepConfig config = new CSharpScriptStepConfig();

			config.StepName = "CS_Script";
			config.Enable = true;
			config.ScriptFileName = string.Empty;
			config.ScriptFilePath = string.Empty;
			config.LastCompileStatus = string.Empty;
			config.LastErrorMessage = string.Empty;

			config.References = new List<ScriptReferenceConfig>();
			config.Inputs = new List<ScriptPinConfig>();
			config.Outputs = new List<ScriptPinConfig>();
			EnsureRequiredInputs(config);

			return config;
		}

		public static string GetDefaultScriptTemplate()
		{
			return
@"using System;
using System.Collections.Generic;

public class ScriptMain : IScriptMain
{
	public void Execute(IScriptContext context)
	{
		bool previousStepsOK = context.GetInputBool(""PreviousStepsOK"");

		// 输入示例：
		// double value = context.GetInputDouble(""InputName"");

		// 输出示例：
		// 编辑器会自动识别 context.SetOutput 的输出名，并显示到 Outputs 表格。
		double result = 0;

		context.SetOutput(""ResultSum"", result);
	}
}";
		}

		public static void EnsureRequiredInputs(CSharpScriptStepConfig config)
		{
			if (config == null)
			{
				return;
			}

			if (config.Inputs == null)
			{
				config.Inputs = new List<ScriptPinConfig>();
			}

			ScriptPinConfig required = null;
			foreach (ScriptPinConfig pin in config.Inputs)
			{
				if (pin != null &&
					string.Equals(pin.Name, PreviousStepsOkInputName, StringComparison.OrdinalIgnoreCase))
				{
					required = pin;
					break;
				}
			}

			if (required == null)
			{
				required = new ScriptPinConfig();
				config.Inputs.Insert(0, required);
			}

			required.Name = PreviousStepsOkInputName;
			required.DataType = ScriptPinDataType.Bool;
			required.BindingPath = PreviousStepsOkInputName;
			required.GlobalVariableName = string.Empty;
			required.DefaultValue = "True";
			required.Description = PreviousStepsOkDescription;

			int firstIndex = config.Inputs.IndexOf(required);
			for (int i = config.Inputs.Count - 1; i >= 0; i--)
			{
				if (i == firstIndex)
				{
					continue;
				}

				ScriptPinConfig pin = config.Inputs[i];
				if (pin != null &&
					string.Equals(pin.Name, PreviousStepsOkInputName, StringComparison.OrdinalIgnoreCase))
				{
					config.Inputs.RemoveAt(i);
				}
			}

			if (firstIndex > 0)
			{
				config.Inputs.Remove(required);
				config.Inputs.Insert(0, required);
			}
		}

		private static void EnsureConfigNotNull(CSharpScriptStepConfig config)
		{
			if (config == null)
			{
				return;
			}

			if (config.StepName == null)
			{
				config.StepName = "CS_Script";
			}

			if (config.ScriptFileName == null)
			{
				config.ScriptFileName = string.Empty;
			}

			if (config.ScriptFilePath == null)
			{
				config.ScriptFilePath = string.Empty;
			}

			if (config.LastCompileStatus == null)
			{
				config.LastCompileStatus = string.Empty;
			}

			if (config.LastErrorMessage == null)
			{
				config.LastErrorMessage = string.Empty;
			}

			if (config.References == null)
			{
				config.References = new List<ScriptReferenceConfig>();
			}

			if (config.Inputs == null)
			{
				config.Inputs = new List<ScriptPinConfig>();
			}

			if (config.Outputs == null)
			{
				config.Outputs = new List<ScriptPinConfig>();
			}

			NormalizePins(config.Inputs);
			NormalizePins(config.Outputs);
			EnsureRequiredInputs(config);
		}

		private static void NormalizePins(List<ScriptPinConfig> pins)
		{
			if (pins == null)
			{
				return;
			}

			foreach (ScriptPinConfig pin in pins)
			{
				if (pin == null)
				{
					continue;
				}

				if (pin.Name == null)
				{
					pin.Name = string.Empty;
				}

				if (pin.BindingPath == null)
				{
					pin.BindingPath = string.Empty;
				}

				if (pin.GlobalVariableName == null)
				{
					pin.GlobalVariableName = string.Empty;
				}

				if (pin.DefaultValue == null)
				{
					pin.DefaultValue = string.Empty;
				}

				if (pin.Description == null)
				{
					pin.Description = string.Empty;
				}

				if (pin.DataType == ScriptPinDataType.Int)
				{
					pin.DataType = ScriptPinDataType.Int32;
				}
			}
		}

		private static string NormalizeFileName(string value, string defaultValue)
		{
			string name = string.IsNullOrWhiteSpace(value) ? defaultValue : value.Trim();

			foreach (char c in Path.GetInvalidFileNameChars())
			{
				name = name.Replace(c, '_');
			}

			return name;
		}
	}
}
