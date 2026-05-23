using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;

namespace Aron_V3
{
	public static class CSharpScriptStepStore
	{
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
			string safeJob = NormalizeFileName(jobName, "Job_001");
			string safeTask = NormalizeFileName(taskName, "Task_New_01");

			return Path.Combine(
				ProjectPathStore.ProjectRoot,
				"Job",
				safeJob,
				"Task",
				safeTask,
				"Scripts");
		}

		public static string GetConfigPath(string jobName, string taskName, string stepName)
		{
			string folder = GetScriptFolder(jobName, taskName);
			string safeStep = NormalizeFileName(stepName, "CS_Script");

			return Path.Combine(folder, safeStep + ".script.xml");
		}

		public static string GetScriptPath(string jobName, string taskName, string stepName)
		{
			string folder = GetScriptFolder(jobName, taskName);
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

			// 默认不再创建 JobID / Measure1 / Barcode 示例输入。
			// 输入由流程管理中 Script 绑定的前序模块自动生成。
			config.References = new List<ScriptReferenceConfig>();
			config.Inputs = new List<ScriptPinConfig>();
			config.Outputs = new List<ScriptPinConfig>();

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
		// 输入示例：
		// double value = context.GetInputDouble(""InputName"");

		// 输出示例：
		// 编辑器会自动识别 context.SetOutput 的输出名，并显示到 Outputs 表格。
		double result = 0;

		context.SetOutput(""ResultSum"", result);
	}
}";
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

				if (pin.DefaultValue == null)
				{
					pin.DefaultValue = string.Empty;
				}

				if (pin.Description == null)
				{
					pin.Description = string.Empty;
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
