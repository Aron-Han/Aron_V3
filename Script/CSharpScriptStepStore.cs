using System;
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

			using (FileStream fs = new FileStream(configPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
			{
				XmlSerializer xs = new XmlSerializer(typeof(CSharpScriptStepConfig));
				object obj = xs.Deserialize(fs);
				CSharpScriptStepConfig config = obj as CSharpScriptStepConfig;
				return config ?? CreateDefaultConfig();
			}
		}

		public static void Save(string configPath, CSharpScriptStepConfig config)
		{
			if (config == null)
			{
				throw new ArgumentNullException("config");
			}

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

			config.Inputs.Add(new ScriptPinConfig
			{
				Name = "JobID",
				DataType = ScriptPinDataType.Int,
				BindingPath = "Comm.JobID",
				DefaultValue = "0",
				Description = "当前作业号"
			});

			config.Inputs.Add(new ScriptPinConfig
			{
				Name = "Measure1",
				DataType = ScriptPinDataType.Double,
				BindingPath = "Vpp_01.Result1",
				DefaultValue = "0.0",
				Description = "测量值"
			});

			config.Inputs.Add(new ScriptPinConfig
			{
				Name = "Barcode",
				DataType = ScriptPinDataType.String,
				BindingPath = "Halcon_01.Code",
				DefaultValue = "",
				Description = "条码字符串"
			});

			config.Outputs.Add(new ScriptPinConfig
			{
				Name = "ResultSum",
				DataType = ScriptPinDataType.Double,
				BindingPath = "ResultSum",
				DefaultValue = "0",
				Description = "计算汇总值"
			});

			config.Outputs.Add(new ScriptPinConfig
			{
				Name = "ResultOK",
				DataType = ScriptPinDataType.Bool,
				BindingPath = "ResultOK",
				DefaultValue = "false",
				Description = "最终结果"
			});

			config.Outputs.Add(new ScriptPinConfig
			{
				Name = "SendData",
				DataType = ScriptPinDataType.String,
				BindingPath = "Comm.SendData",
				DefaultValue = "",
				Description = "发送给通讯模块的报文"
			});

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
		// 读取输入
		int jobId = context.GetInputInt(""JobID"");
		double measure1 = context.GetInputDouble(""Measure1"");
		string barcode = context.GetInputString(""Barcode"");

		// 数据处理
		double resultSum = measure1 + jobId;
		bool resultOK = resultSum > 100.0;
		string finalCode = string.IsNullOrEmpty(barcode)
			? ""NO_CODE_"" + jobId.ToString()
			: barcode;

		// 汇总发送数据。后续可直接给 TCP/IP / Profinet / S7 输出模块使用。
		string sendData =
			""JOB:"" + jobId.ToString() +
			"";SUM:"" + resultSum.ToString(""F2"") +
			"";OK:"" + resultOK.ToString() +
			"";CODE:"" + finalCode;

		// 写输出
		context.SetOutput(""ResultSum"", resultSum);
		context.SetOutput(""ResultOK"", resultOK);
		context.SetOutput(""FinalCode"", finalCode);
		context.SetOutput(""SendData"", sendData);
	}
}";
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
