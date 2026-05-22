using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using Microsoft.CSharp;

namespace Aron_V3
{
	/// <summary>
	/// C# Script Step 入口接口。
	/// 脚本代码里可以直接写：
	/// public class ScriptMain : IScriptMain
	/// </summary>
	public interface IScriptMain
	{
		void Execute(IScriptContext context);
	}

	/// <summary>
	/// C# Script 运行上下文。
	/// 只用于数据输入/输出，不传递图像。
	/// </summary>
	public interface IScriptContext
	{
		object GetInput(string name);
		string GetInputString(string name);
		int GetInputInt(string name);
		double GetInputDouble(string name);
		bool GetInputBool(string name);

		void SetOutput(string name, object value);
		object GetOutput(string name);
	}

	public class ScriptRuntimeContext : IScriptContext
	{
		private readonly Dictionary<string, object> _inputs;
		private readonly Dictionary<string, object> _outputs;

		public Dictionary<string, object> Inputs { get { return _inputs; } }
		public Dictionary<string, object> Outputs { get { return _outputs; } }

		public ScriptRuntimeContext()
		{
			_inputs = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
			_outputs = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
		}

		public object GetInput(string name)
		{
			if (string.IsNullOrWhiteSpace(name))
			{
				return null;
			}

			object value;
			if (_inputs.TryGetValue(name, out value))
			{
				return value;
			}

			return null;
		}

		public string GetInputString(string name)
		{
			object value = GetInput(name);
			return value == null ? string.Empty : Convert.ToString(value, CultureInfo.InvariantCulture);
		}

		public int GetInputInt(string name)
		{
			object value = GetInput(name);
			if (value == null)
			{
				return 0;
			}

			try
			{
				if (value is int)
				{
					return (int)value;
				}

				if (value is double)
				{
					return Convert.ToInt32((double)value);
				}

				if (value is float)
				{
					return Convert.ToInt32((float)value);
				}

				if (value is decimal)
				{
					return Convert.ToInt32((decimal)value);
				}

				int v;
				if (int.TryParse(Convert.ToString(value, CultureInfo.InvariantCulture), NumberStyles.Any, CultureInfo.InvariantCulture, out v))
				{
					return v;
				}

				double d;
				if (double.TryParse(Convert.ToString(value, CultureInfo.InvariantCulture), NumberStyles.Any, CultureInfo.InvariantCulture, out d))
				{
					return Convert.ToInt32(d);
				}

				return Convert.ToInt32(value, CultureInfo.InvariantCulture);
			}
			catch
			{
				return 0;
			}
		}

		public double GetInputDouble(string name)
		{
			object value = GetInput(name);
			if (value == null)
			{
				return 0.0;
			}

			try
			{
				if (value is double)
				{
					return (double)value;
				}

				if (value is float)
				{
					return Convert.ToDouble((float)value);
				}

				if (value is decimal)
				{
					return Convert.ToDouble((decimal)value);
				}

				double v;
				if (double.TryParse(Convert.ToString(value, CultureInfo.InvariantCulture), NumberStyles.Any, CultureInfo.InvariantCulture, out v))
				{
					return v;
				}

				return Convert.ToDouble(value, CultureInfo.InvariantCulture);
			}
			catch
			{
				return 0.0;
			}
		}

		public bool GetInputBool(string name)
		{
			object value = GetInput(name);
			if (value == null)
			{
				return false;
			}

			try
			{
				if (value is bool)
				{
					return (bool)value;
				}

				bool v;
				if (bool.TryParse(Convert.ToString(value, CultureInfo.InvariantCulture), out v))
				{
					return v;
				}

				int i;
				if (int.TryParse(Convert.ToString(value, CultureInfo.InvariantCulture), NumberStyles.Any, CultureInfo.InvariantCulture, out i))
				{
					return i != 0;
				}

				string text = Convert.ToString(value, CultureInfo.InvariantCulture);
				return string.Equals(text, "Y", StringComparison.OrdinalIgnoreCase) ||
					   string.Equals(text, "YES", StringComparison.OrdinalIgnoreCase) ||
					   string.Equals(text, "OK", StringComparison.OrdinalIgnoreCase) ||
					   string.Equals(text, "NG", StringComparison.OrdinalIgnoreCase) == false && string.Equals(text, "TRUE", StringComparison.OrdinalIgnoreCase);
			}
			catch
			{
				return false;
			}
		}

		public void SetOutput(string name, object value)
		{
			if (string.IsNullOrWhiteSpace(name))
			{
				return;
			}

			_outputs[name] = value;
		}

		public object GetOutput(string name)
		{
			if (string.IsNullOrWhiteSpace(name))
			{
				return null;
			}

			object value;
			if (_outputs.TryGetValue(name, out value))
			{
				return value;
			}

			return null;
		}
	}


	/// <summary>
	/// 全局 C# Script DLL 引用管理。
	/// 
	/// 目录：
	/// Project\Config\Algorithm\ScriptReferences
	/// 
	/// 用法：
	/// 1. 把第三方 DLL 放到这个目录。
	/// 2. 编译 Script 时会自动引用该目录下所有 DLL。
	/// 3. ScriptUsings.txt 中每行写一个 namespace，编译时自动补 using。
	/// </summary>
	public static class CSharpScriptReferenceManager
	{
		public static string ReferenceFolder
		{
			get
			{
				string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Project", "Config", "Algorithm", "ScriptReferences");
				Directory.CreateDirectory(path);
				return path;
			}
		}

		public static string UsingConfigFile
		{
			get { return Path.Combine(ReferenceFolder, "ScriptUsings.txt"); }
		}

		public static void EnsureReferenceFolder()
		{
			Directory.CreateDirectory(ReferenceFolder);

			if (!File.Exists(UsingConfigFile))
			{
				StringBuilder sb = new StringBuilder();
				sb.AppendLine("// 每行写一个需要自动 using 的 namespace，例如：");
				sb.AppendLine("// MyCompany.MyLibrary");
				File.WriteAllText(UsingConfigFile, sb.ToString(), Encoding.UTF8);
			}
		}

		public static List<string> GetReferenceDllPaths()
		{
			EnsureReferenceFolder();

			List<string> result = new List<string>();
			try
			{
				foreach (string file in Directory.GetFiles(ReferenceFolder, "*.dll", SearchOption.TopDirectoryOnly))
				{
					if (File.Exists(file))
					{
						result.Add(file);
					}
				}
			}
			catch
			{
			}

			return result;
		}

		public static void PreloadAllReferenceDlls()
		{
			foreach (string dll in GetReferenceDllPaths())
			{
				try
				{
					AssemblyName name = AssemblyName.GetAssemblyName(dll);
					bool loaded = AppDomain.CurrentDomain.GetAssemblies().Any(a =>
					{
						try
						{
							return string.Equals(a.GetName().Name, name.Name, StringComparison.OrdinalIgnoreCase);
						}
						catch
						{
							return false;
						}
					});

					if (!loaded)
					{
						Assembly.LoadFrom(dll);
					}
				}
				catch
				{
					// 某些 DLL 不是 .NET 托管程序集，仍可作为原生依赖存在；这里忽略加载错误。
				}
			}
		}

		public static List<string> GetAutoUsingNamespaces()
		{
			EnsureReferenceFolder();

			List<string> result = new List<string>();

			try
			{
				if (File.Exists(UsingConfigFile))
				{
					foreach (string line in File.ReadAllLines(UsingConfigFile, Encoding.UTF8))
					{
						string text = (line ?? string.Empty).Trim();
						if (string.IsNullOrWhiteSpace(text)) continue;
						if (text.StartsWith("//")) continue;
						if (text.StartsWith("#")) continue;
						if (text.StartsWith("using ", StringComparison.OrdinalIgnoreCase))
						{
							text = text.Substring(6).Trim();
						}
						if (text.EndsWith(";")) text = text.Substring(0, text.Length - 1).Trim();
						if (IsValidNamespace(text) && !ContainsIgnoreCase(result, text))
						{
							result.Add(text);
						}
					}
				}
			}
			catch
			{
			}

			// 尝试把 DLL 程序集名也作为默认 using。真实 namespace 不一定等于程序集名，
			// 所以需要更准确时仍建议写入 ScriptUsings.txt。
			foreach (string dll in GetReferenceDllPaths())
			{
				try
				{
					string asmName = AssemblyName.GetAssemblyName(dll).Name;
					if (IsValidNamespace(asmName) && !result.Contains(asmName)) result.Add(asmName);
				}
				catch
				{
				}
			}

			return result;
		}

		private static bool ContainsIgnoreCase(List<string> list, string value)
		{
			if (list == null || string.IsNullOrWhiteSpace(value))
			{
				return false;
			}

			foreach (string item in list)
			{
				if (string.Equals(item, value, StringComparison.OrdinalIgnoreCase))
				{
					return true;
				}
			}

			return false;
		}

		private static bool IsValidNamespace(string text)
		{
			if (string.IsNullOrWhiteSpace(text)) return false;

			string[] parts = text.Split('.');
			foreach (string part in parts)
			{
				if (string.IsNullOrWhiteSpace(part)) return false;
				if (!(char.IsLetter(part[0]) || part[0] == '_')) return false;
				for (int i = 1; i < part.Length; i++)
				{
					char c = part[i];
					if (!(char.IsLetterOrDigit(c) || c == '_')) return false;
				}
			}

			return true;
		}
	}

	/// <summary>
	/// C# Script Step 编译 + 运行器。
	/// 
	/// 当前版本重点：
	/// 1. 动态编译时自动引用当前程序程序集，解决 IScriptMain / IScriptContext 找不到。
	/// 2. 自动补常用 using，包括 System.Windows.Forms，脚本里可直接写 MessageBox.Show。
	/// 3. 自动引用常用程序集，包括 Linq / Drawing / WinForms / Xml / Data / Microsoft.CSharp。
	/// 4. 自动带上当前 AppDomain 已加载的 Cognex / Halcon / Aron_V3 相关程序集。
	/// 5. 支持 ScriptMain 写在 namespace 内，自动查找实现 IScriptMain 的类型。
	/// 6. 输入支持 Name 和 BindingPath 两种 key。
	/// </summary>
	public class CSharpScriptStepRunner
	{
		public CSharpScriptRunResult CompileAndRun(
			CSharpScriptStepConfig config,
			string scriptCode,
			Dictionary<string, object> runtimeInputs)
		{
			CSharpScriptRunResult result = new CSharpScriptRunResult();

			if (config == null)
			{
				return CSharpScriptRunResult.RunError("Script config is null.");
			}

			if (string.IsNullOrWhiteSpace(scriptCode))
			{
				return CSharpScriptRunResult.CompileError("Script code is empty.");
			}

			Stopwatch compileWatch = Stopwatch.StartNew();
			CompilerResults compilerResults = Compile(config, scriptCode);
			compileWatch.Stop();
			result.CompileCost = compileWatch.Elapsed;

			if (compilerResults == null)
			{
				result.IsCompileOK = false;
				result.IsRunOK = false;
				result.Message = "Compile failed";
				result.ErrorDetail = "Compiler result is null.";
				return result;
			}

			if (compilerResults.Errors.HasErrors)
			{
				result.IsCompileOK = false;
				result.IsRunOK = false;
				result.Message = "Compile failed";
				result.ErrorDetail = FormatCompileErrors(compilerResults.Errors);
				return result;
			}

			result.IsCompileOK = true;

			try
			{
				Stopwatch runWatch = Stopwatch.StartNew();

				Assembly asm = compilerResults.CompiledAssembly;
				if (asm == null)
				{
					return CSharpScriptRunResult.RunError("Compiled assembly is null.");
				}

				Type mainType = FindScriptMainType(asm);
				if (mainType == null)
				{
					return CSharpScriptRunResult.RunError("ScriptMain class was not found, or no class implements IScriptMain.");
				}

				object instance = Activator.CreateInstance(mainType);
				IScriptMain scriptMain = instance as IScriptMain;

				if (scriptMain == null)
				{
					return CSharpScriptRunResult.RunError("ScriptMain must implement Aron_V3.IScriptMain.");
				}

				ScriptRuntimeContext context = BuildRuntimeContext(config, runtimeInputs);
				scriptMain.Execute(context);

				foreach (KeyValuePair<string, object> pair in context.Outputs)
				{
					result.Outputs[pair.Key] = pair.Value;
				}

				runWatch.Stop();
				result.RunCost = runWatch.Elapsed;
				result.IsRunOK = true;
				result.Message = "Script run OK";

				return result;
			}
			catch (TargetInvocationException ex)
			{
				result.IsRunOK = false;
				result.Message = "Run failed";
				result.ErrorDetail = ex.InnerException == null ? ex.ToString() : ex.InnerException.ToString();
				return result;
			}
			catch (Exception ex)
			{
				result.IsRunOK = false;
				result.Message = "Run failed";
				result.ErrorDetail = ex.ToString();
				return result;
			}
		}

		public CompilerResults Compile(CSharpScriptStepConfig config, string scriptCode)
		{
			CSharpScriptReferenceManager.PreloadAllReferenceDlls();

			CSharpCodeProvider provider = new CSharpCodeProvider();
			CompilerParameters parameters = new CompilerParameters();

			parameters.GenerateExecutable = false;
			parameters.GenerateInMemory = true;
			parameters.IncludeDebugInformation = true;
			parameters.WarningLevel = 4;
			parameters.TreatWarningsAsErrors = false;
			parameters.CompilerOptions = "/optimize- /debug+";

			AddDefaultReferences(parameters, config);

			string finalCode = PrepareScriptCode(scriptCode);

			return provider.CompileAssemblyFromSource(parameters, finalCode);
		}

		private void AddDefaultReferences(CompilerParameters parameters, CSharpScriptStepConfig config)
		{
			if (parameters == null)
			{
				return;
			}

			// .NET Framework 常用程序集：这些是“编译引用”，不会自动显示在 UI 的引用 DLL 列表里。
			AddReference(parameters, "System.dll");
			AddReference(parameters, "System.Core.dll");
			AddReference(parameters, "System.Data.dll");
			AddReference(parameters, "System.Drawing.dll");
			AddReference(parameters, "System.Windows.Forms.dll");
			AddReference(parameters, "System.Xml.dll");
			AddReference(parameters, "System.Xml.Linq.dll");
			AddReference(parameters, "Microsoft.CSharp.dll");

			// 用实际 Assembly.Location 再补一遍，避免某些现场环境只靠短名称解析失败。
			AddReferenceByType(parameters, typeof(object));
			AddReferenceByType(parameters, typeof(Uri));
			AddReferenceByType(parameters, typeof(Enumerable));
			AddReferenceByType(parameters, typeof(System.Data.DataTable));
			AddReferenceByType(parameters, typeof(System.Drawing.Bitmap));
			AddReferenceByType(parameters, typeof(System.Windows.Forms.MessageBox));
			AddReferenceByType(parameters, typeof(System.Xml.XmlDocument));
			AddReferenceByType(parameters, typeof(Microsoft.CSharp.RuntimeBinder.Binder));

			// 关键：引用定义 IScriptMain / IScriptContext 的程序集。
			AddReference(parameters, typeof(IScriptMain).Assembly.Location);
			AddReference(parameters, typeof(IScriptContext).Assembly.Location);
			AddReference(parameters, typeof(CSharpScriptStepRunner).Assembly.Location);

			// 当前 exe 也加入，避免接口和运行器在不同程序集时漏引用。
			try
			{
				Assembly entry = Assembly.GetEntryAssembly();
				if (entry != null && !string.IsNullOrWhiteSpace(entry.Location))
				{
					AddReference(parameters, entry.Location);
				}
			}
			catch
			{
			}

			// 当前 AppDomain 已加载的项目 / 视觉相关程序集加入，接近 VisionPro 高级脚本体验。
			AddLoadedAssembliesByPrefix(parameters, "Aron_V3");
			AddLoadedAssembliesByPrefix(parameters, "Cognex.");
			AddLoadedAssembliesByPrefix(parameters, "MVTec.");
			AddLoadedAssembliesByPrefix(parameters, "Halcon");

			// 全局 ScriptReferences 文件夹下的 DLL：所有脚本自动引用。
			foreach (string dllPath in CSharpScriptReferenceManager.GetReferenceDllPaths())
			{
				AddReference(parameters, dllPath);
			}

			// 当前 Script 配置里的引用 DLL：保留兼容旧配置。
			if (config != null && config.References != null)
			{
				foreach (ScriptReferenceConfig reference in config.References)
				{
					if (reference == null || !reference.Enable)
					{
						continue;
					}

					if (string.IsNullOrWhiteSpace(reference.DllPath))
					{
						continue;
					}

					if (File.Exists(reference.DllPath))
					{
						AddReference(parameters, reference.DllPath);
					}
				}
			}
		}

		private string PrepareScriptCode(string scriptCode)
		{
			if (string.IsNullOrWhiteSpace(scriptCode))
			{
				return string.Empty;
			}

			StringBuilder sb = new StringBuilder();

			// 这些 using 不要求用户手写；#line 1 保证报错行号仍对应编辑器里的用户代码行号。
			sb.AppendLine("using System;");
			sb.AppendLine("using System.IO;");
			sb.AppendLine("using System.Text;");
			sb.AppendLine("using System.Linq;");
			sb.AppendLine("using System.Data;");
			sb.AppendLine("using System.Drawing;");
			sb.AppendLine("using System.Collections;");
			sb.AppendLine("using System.Collections.Generic;");
			sb.AppendLine("using System.Text.RegularExpressions;");
			sb.AppendLine("using System.Windows.Forms;");
			sb.AppendLine("using Aron_V3;");

			foreach (string ns in CSharpScriptReferenceManager.GetAutoUsingNamespaces())
			{
				sb.AppendLine("using " + ns + ";");
			}

			if (IsAssemblyLoaded("Cognex.VisionPro"))
			{
				sb.AppendLine("using Cognex.VisionPro;");
				sb.AppendLine("using Cognex.VisionPro.ToolBlock;");
				sb.AppendLine("using Cognex.VisionPro.PMAlign;");
				sb.AppendLine("using Cognex.VisionPro.ImageProcessing;");
			}

			sb.AppendLine();
			sb.AppendLine("#line 1");
			sb.AppendLine(scriptCode);

			return sb.ToString();
		}

		private bool IsAssemblyLoaded(string assemblyName)
		{
			Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
			foreach (Assembly asm in assemblies)
			{
				try
				{
					if (asm == null)
					{
						continue;
					}

					if (string.Equals(asm.GetName().Name, assemblyName, StringComparison.OrdinalIgnoreCase))
					{
						return true;
					}
				}
				catch
				{
				}
			}

			return false;
		}

		private void AddReferenceByType(CompilerParameters parameters, Type type)
		{
			if (type == null)
			{
				return;
			}

			try
			{
				Assembly asm = type.Assembly;
				if (asm != null && !asm.IsDynamic && !string.IsNullOrWhiteSpace(asm.Location))
				{
					AddReference(parameters, asm.Location);
				}
			}
			catch
			{
			}
		}

		private void AddLoadedAssembliesByPrefix(CompilerParameters parameters, string prefix)
		{
			if (parameters == null || string.IsNullOrWhiteSpace(prefix))
			{
				return;
			}

			Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
			foreach (Assembly asm in assemblies)
			{
				try
				{
					if (asm == null || asm.IsDynamic)
					{
						continue;
					}

					string name = asm.GetName().Name;
					if (string.IsNullOrWhiteSpace(name))
					{
						continue;
					}

					if (!name.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
					{
						continue;
					}

					string location = asm.Location;
					if (!string.IsNullOrWhiteSpace(location) && File.Exists(location))
					{
						AddReference(parameters, location);
					}
				}
				catch
				{
				}
			}
		}

		private void AddReference(CompilerParameters parameters, string reference)
		{
			if (parameters == null || string.IsNullOrWhiteSpace(reference))
			{
				return;
			}

			string referenceFileName = Path.GetFileName(reference);

			foreach (string item in parameters.ReferencedAssemblies)
			{
				if (string.Equals(item, reference, StringComparison.OrdinalIgnoreCase))
				{
					return;
				}

				if (!string.IsNullOrWhiteSpace(referenceFileName) &&
					string.Equals(Path.GetFileName(item), referenceFileName, StringComparison.OrdinalIgnoreCase))
				{
					return;
				}
			}

			parameters.ReferencedAssemblies.Add(reference);
		}

		private Type FindScriptMainType(Assembly asm)
		{
			if (asm == null)
			{
				return null;
			}

			Type mainType = asm.GetType("ScriptMain");
			if (mainType != null && typeof(IScriptMain).IsAssignableFrom(mainType))
			{
				return mainType;
			}

			Type[] types = asm.GetTypes();
			foreach (Type type in types)
			{
				if (type == null)
				{
					continue;
				}

				if (typeof(IScriptMain).IsAssignableFrom(type) &&
					!type.IsAbstract &&
					type.GetConstructor(Type.EmptyTypes) != null)
				{
					return type;
				}
			}

			return null;
		}

		private ScriptRuntimeContext BuildRuntimeContext(
			CSharpScriptStepConfig config,
			Dictionary<string, object> runtimeInputs)
		{
			ScriptRuntimeContext context = new ScriptRuntimeContext();

			if (config == null || config.Inputs == null)
			{
				return context;
			}

			foreach (ScriptPinConfig input in config.Inputs)
			{
				if (input == null || string.IsNullOrWhiteSpace(input.Name))
				{
					continue;
				}

				object value = null;

				if (runtimeInputs != null)
				{
					if (!string.IsNullOrWhiteSpace(input.BindingPath))
					{
						runtimeInputs.TryGetValue(input.BindingPath, out value);
					}

					if (value == null)
					{
						runtimeInputs.TryGetValue(input.Name, out value);
					}
				}

				if (value == null)
				{
					value = ConvertDefaultValue(input.DefaultValue, input.DataType);
				}
				else
				{
					value = ConvertRuntimeValue(value, input.DataType);
				}

				context.Inputs[input.Name] = value;

				// 同时按 BindingPath 存一份，便于脚本高级用法。
				if (!string.IsNullOrWhiteSpace(input.BindingPath))
				{
					context.Inputs[input.BindingPath] = value;
				}
			}

			return context;
		}

		private object ConvertRuntimeValue(object value, ScriptPinDataType type)
		{
			if (value == null)
			{
				return ConvertDefaultValue(string.Empty, type);
			}

			if (type == ScriptPinDataType.Object)
			{
				return value;
			}

			return ConvertDefaultValue(Convert.ToString(value, CultureInfo.InvariantCulture), type);
		}

		private object ConvertDefaultValue(string value, ScriptPinDataType type)
		{
			if (value == null)
			{
				value = string.Empty;
			}

			try
			{
				switch (type)
				{
					case ScriptPinDataType.Bool:
						bool b;
						if (bool.TryParse(value, out b))
						{
							return b;
						}

						int bi;
						if (int.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out bi))
						{
							return bi != 0;
						}

						return string.Equals(value, "Y", StringComparison.OrdinalIgnoreCase) ||
							   string.Equals(value, "YES", StringComparison.OrdinalIgnoreCase) ||
							   string.Equals(value, "OK", StringComparison.OrdinalIgnoreCase) ||
							   string.Equals(value, "TRUE", StringComparison.OrdinalIgnoreCase);

					case ScriptPinDataType.Int:
						int i;
						if (int.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out i))
						{
							return i;
						}

						double di;
						if (double.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out di))
						{
							return Convert.ToInt32(di);
						}

						return 0;

					case ScriptPinDataType.Double:
						double d;
						if (double.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out d))
						{
							return d;
						}

						return 0.0;

					case ScriptPinDataType.Decimal:
						decimal m;
						if (decimal.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out m))
						{
							return m;
						}

						return decimal.Zero;

					case ScriptPinDataType.Object:
						return value;

					default:
						return value;
				}
			}
			catch
			{
				return value;
			}
		}

		private string FormatCompileErrors(CompilerErrorCollection errors)
		{
			if (errors == null || errors.Count <= 0)
			{
				return string.Empty;
			}

			StringBuilder sb = new StringBuilder();

			foreach (CompilerError error in errors)
			{
				if (error == null)
				{
					continue;
				}

				sb.AppendLine(
					"Line " + error.Line +
					", Column " + error.Column +
					", " + error.ErrorNumber +
					": " + error.ErrorText);
			}

			return sb.ToString();
		}
	}
}
