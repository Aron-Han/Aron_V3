using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace Aron_V3
{
	/// <summary>
	/// C# Script Step 配置文件。
	/// 建议保存路径：
	/// Project\Job\<JobName>\Task\<TaskName>\Script\<ScriptName>.script.xml
	/// 脚本文件：
	/// Project\Job\<JobName>\Task\<TaskName>\Script\<ScriptName>.csx
	/// </summary>
	[Serializable]
	public class CSharpScriptStepConfig
	{
		public string StepName { get; set; }
		public bool Enable { get; set; }
		public string ScriptFileName { get; set; }
		public string ScriptFilePath { get; set; }
		public string LastCompileStatus { get; set; }
		public string LastErrorMessage { get; set; }

		[XmlArrayItem("Reference")]
		public List<ScriptReferenceConfig> References { get; set; }

		[XmlArrayItem("Input")]
		public List<ScriptPinConfig> Inputs { get; set; }

		[XmlArrayItem("Output")]
		public List<ScriptPinConfig> Outputs { get; set; }

		public CSharpScriptStepConfig()
		{
			StepName = "CS_Script";
			Enable = true;
			ScriptFileName = "CS_Script.csx";
			ScriptFilePath = string.Empty;
			LastCompileStatus = string.Empty;
			LastErrorMessage = string.Empty;
			References = new List<ScriptReferenceConfig>();
			Inputs = new List<ScriptPinConfig>();
			Outputs = new List<ScriptPinConfig>();
		}
	}

	[Serializable]
	public class ScriptReferenceConfig
	{
		public bool Enable { get; set; }
		public string ReferenceName { get; set; }
		public string DllPath { get; set; }
		public string Description { get; set; }

		public ScriptReferenceConfig()
		{
			Enable = true;
			ReferenceName = string.Empty;
			DllPath = string.Empty;
			Description = string.Empty;
		}
	}

	[Serializable]
	public class ScriptPinConfig
	{
		public string Name { get; set; }
		public ScriptPinDataType DataType { get; set; }

		/// <summary>
		/// 输入时：从 Context 读取的绑定路径，例如 Vpp_01.ResultX / Halcon_01.Code / Comm.ProductCode。
		/// 输出时：写到 Context 或通讯输出的目标名，例如 ResultOK / SendData / PLC.ResultCode。
		/// </summary>
		public string BindingPath { get; set; }

		public string GlobalVariableName { get; set; }

		public string DefaultValue { get; set; }
		public string Description { get; set; }

		public ScriptPinConfig()
		{
			Name = string.Empty;
			DataType = ScriptPinDataType.String;
			BindingPath = string.Empty;
			GlobalVariableName = string.Empty;
			DefaultValue = string.Empty;
			Description = string.Empty;
		}
	}

	public enum ScriptPinDataType
	{
		String = 0,
		Bool = 1,
		Int = 2,
		Double = 3,
		Decimal = 4,
		Object = 5,
		Int16 = 6,
		Int32 = 7,
		Float = 8
	}

	public class CSharpScriptRunResult
	{
		public bool IsCompileOK { get; set; }
		public bool IsRunOK { get; set; }
		public string Message { get; set; }
		public string ErrorDetail { get; set; }
		public Dictionary<string, object> Outputs { get; private set; }
		public TimeSpan CompileCost { get; set; }
		public TimeSpan RunCost { get; set; }

		public CSharpScriptRunResult()
		{
			Message = string.Empty;
			ErrorDetail = string.Empty;
			Outputs = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
		}

		public static CSharpScriptRunResult CompileError(string msg)
		{
			CSharpScriptRunResult r = new CSharpScriptRunResult();
			r.IsCompileOK = false;
			r.IsRunOK = false;
			r.Message = "Compile failed";
			r.ErrorDetail = msg ?? string.Empty;
			return r;
		}

		public static CSharpScriptRunResult RunError(string msg)
		{
			CSharpScriptRunResult r = new CSharpScriptRunResult();
			r.IsCompileOK = true;
			r.IsRunOK = false;
			r.Message = "Run failed";
			r.ErrorDetail = msg ?? string.Empty;
			return r;
		}
	}
}
