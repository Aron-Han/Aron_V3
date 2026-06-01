using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace Aron_V3
{
	public enum DiagnosticLogLevel
	{
		Info,
		Warning,
		Error,
		Critical
	}

	public static class DiagnosticLogStore
	{
		private const int MaxRecentEvents = 200;
		private const long MaxLogFileBytes = 50L * 1024L * 1024L;
		private const int RetentionDays = 30;

		private static readonly object _syncRoot = new object();
		private static readonly Queue<string> _recentEvents = new Queue<string>();
		private static bool _initialized;

		public static string DiagnosticRoot
		{
			get
			{
				string path = Path.Combine(RuntimeLogStore.LogFolder, "Diagnostics");
				Directory.CreateDirectory(path);
				return path;
			}
		}

		public static string CrashFolder
		{
			get
			{
				string path = Path.Combine(DiagnosticRoot, "Crash");
				Directory.CreateDirectory(path);
				return path;
			}
		}

		public static string PackageFolder
		{
			get
			{
				string path = Path.Combine(ProjectPathStore.ProjectRoot, "DiagnosticPackages");
				Directory.CreateDirectory(path);
				return path;
			}
		}

		public static void Initialize()
		{
			if (_initialized)
			{
				return;
			}

			lock (_syncRoot)
			{
				if (_initialized)
				{
					return;
				}

				Directory.CreateDirectory(DiagnosticRoot);
				Directory.CreateDirectory(CrashFolder);
				Directory.CreateDirectory(PackageFolder);
				CleanupOldLogs();
				_initialized = true;
			}

			Append(DiagnosticLogLevel.Info, "Application", "Diagnostic logging initialized.", CollectEnvironmentData());
		}

		public static void AppendRuntimeLog(RuntimeFlowLogEventArgs args)
		{
			if (args == null)
			{
				return;
			}

			Dictionary<string, string> data = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
			data["runtimeCategory"] = RuntimeLogStore.GetCategoryText(args.Category);
			data["runtimeTime"] = args.Time.ToString("yyyy-MM-dd HH:mm:ss.fff", CultureInfo.InvariantCulture);
			data["isError"] = args.IsError ? "true" : "false";
			ExtractRuntimeTokens(args.Message, data);

			Append(
				args.IsError ? DiagnosticLogLevel.Error : DiagnosticLogLevel.Info,
				"RuntimeLog",
				args.Message,
				data);
		}

		public static void Append(
			DiagnosticLogLevel level,
			string category,
			string message,
			IDictionary<string, string> data)
		{
			try
			{
				EnsureInitialized();

				DateTime now = DateTime.Now;
				Dictionary<string, string> merged = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
				if (data != null)
				{
					foreach (KeyValuePair<string, string> pair in data)
					{
						merged[pair.Key ?? string.Empty] = pair.Value ?? string.Empty;
					}
				}

				AddProcessData(merged);
				string line = SerializeEvent(now, level, category, message, merged);

				lock (_syncRoot)
				{
					string path = GetCurrentLogPath(now);
					File.AppendAllText(path, line + Environment.NewLine, Encoding.UTF8);
					_recentEvents.Enqueue(line);
					while (_recentEvents.Count > MaxRecentEvents)
					{
						_recentEvents.Dequeue();
					}
				}
			}
			catch
			{
			}
		}

		public static void Append(DiagnosticLogLevel level, string category, string message)
		{
			Append(level, category, message, null);
		}

		public static string WriteCrashReport(Exception ex, string source, bool terminating)
		{
			try
			{
				EnsureInitialized();

				DateTime now = DateTime.Now;
				string fileName = "crash_" + now.ToString("yyyyMMdd_HHmmss_fff", CultureInfo.InvariantCulture) + ".txt";
				string path = Path.Combine(CrashFolder, fileName);
				StringBuilder sb = new StringBuilder();
				sb.AppendLine("Aron_V3 Crash Report");
				sb.AppendLine("Time=" + now.ToString("yyyy-MM-dd HH:mm:ss.fff", CultureInfo.InvariantCulture));
				sb.AppendLine("Source=" + (source ?? string.Empty));
				sb.AppendLine("Terminating=" + (terminating ? "true" : "false"));
				sb.AppendLine();
				sb.AppendLine("[Environment]");
				foreach (KeyValuePair<string, string> pair in CollectEnvironmentData())
				{
					sb.AppendLine(pair.Key + "=" + pair.Value);
				}
				sb.AppendLine();
				sb.AppendLine("[Exception]");
				sb.AppendLine(ex == null ? "unknown" : ex.ToString());
				sb.AppendLine();
				sb.AppendLine("[RecentEvents]");
				foreach (string line in GetRecentEventLines())
				{
					sb.AppendLine(line);
				}

				File.WriteAllText(path, sb.ToString(), Encoding.UTF8);

				Dictionary<string, string> data = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
				data["source"] = source ?? string.Empty;
				data["terminating"] = terminating ? "true" : "false";
				data["crashReport"] = path;
				data["exceptionType"] = ex == null ? string.Empty : ex.GetType().FullName;
				data["exceptionMessage"] = ex == null ? string.Empty : ex.Message;
				Append(DiagnosticLogLevel.Critical, "Crash", "Unexpected application exception captured.", data);

				return path;
			}
			catch
			{
				return string.Empty;
			}
		}

		public static string WriteStateSnapshot(string reason)
		{
			try
			{
				EnsureInitialized();

				DateTime now = DateTime.Now;
				string fileName = "snapshot_" + now.ToString("yyyyMMdd_HHmmss_fff", CultureInfo.InvariantCulture) + ".txt";
				string path = Path.Combine(DiagnosticRoot, fileName);
				StringBuilder sb = new StringBuilder();
				sb.AppendLine("Aron_V3 Diagnostic Snapshot");
				sb.AppendLine("Time=" + now.ToString("yyyy-MM-dd HH:mm:ss.fff", CultureInfo.InvariantCulture));
				sb.AppendLine("Reason=" + (reason ?? string.Empty));
				sb.AppendLine();
				sb.AppendLine("[Environment]");
				foreach (KeyValuePair<string, string> pair in CollectEnvironmentData())
				{
					sb.AppendLine(pair.Key + "=" + pair.Value);
				}
				sb.AppendLine();
				sb.AppendLine("[ConfigFiles]");
				foreach (KeyValuePair<string, string> pair in CollectConfigFingerprints())
				{
					sb.AppendLine(pair.Key + "=" + pair.Value);
				}
				sb.AppendLine();
				sb.AppendLine("[RecentEvents]");
				foreach (string line in GetRecentEventLines())
				{
					sb.AppendLine(line);
				}

				File.WriteAllText(path, sb.ToString(), Encoding.UTF8);

				Dictionary<string, string> data = CollectEnvironmentData();
				data["reason"] = reason ?? string.Empty;
				data["snapshot"] = path;
				Append(DiagnosticLogLevel.Info, "Snapshot", "Diagnostic state snapshot captured.", data);

				return path;
			}
			catch
			{
				return string.Empty;
			}
		}

		public static List<string> GetRecentEventLines()
		{
			lock (_syncRoot)
			{
				return new List<string>(_recentEvents);
			}
		}

		internal static Dictionary<string, string> CollectEnvironmentData()
		{
			Dictionary<string, string> data = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
			try
			{
				Process process = Process.GetCurrentProcess();
				Assembly entry = Assembly.GetEntryAssembly();
				Assembly current = typeof(DiagnosticLogStore).Assembly;
				Version version = current.GetName().Version;

				data["appVersion"] = version == null ? string.Empty : version.ToString();
				data["entryAssembly"] = entry == null ? string.Empty : entry.Location;
				data["currentAssembly"] = current.Location;
				data["machineName"] = Environment.MachineName;
				data["userName"] = Environment.UserName;
				data["osVersion"] = Environment.OSVersion.ToString();
				data["is64BitProcess"] = Environment.Is64BitProcess ? "true" : "false";
				data["is64BitOperatingSystem"] = Environment.Is64BitOperatingSystem ? "true" : "false";
				data["startupPath"] = Application.StartupPath;
				data["projectRoot"] = ProjectPathStore.ProjectRoot;
				data["processId"] = process.Id.ToString(CultureInfo.InvariantCulture);
				data["processName"] = process.ProcessName;
				data["privateMemoryMb"] = (process.PrivateMemorySize64 / 1024L / 1024L).ToString(CultureInfo.InvariantCulture);
				data["workingSetMb"] = (process.WorkingSet64 / 1024L / 1024L).ToString(CultureInfo.InvariantCulture);
				data["threadCount"] = process.Threads.Count.ToString(CultureInfo.InvariantCulture);
				data["handleCount"] = process.HandleCount.ToString(CultureInfo.InvariantCulture);
				data["currentCulture"] = CultureInfo.CurrentCulture.Name;
				data["uiCulture"] = CultureInfo.CurrentUICulture.Name;
			}
			catch
			{
			}

			return data;
		}

		internal static Dictionary<string, string> CollectConfigFingerprints()
		{
			Dictionary<string, string> data = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
			AddFileFingerprint(data, "flowConfig", SafeGetPath(delegate { return FlowConfigStore.FlowConfigFile; }));
			AddFileFingerprint(data, "communicationConfig", SafeGetPath(delegate { return CommunicationConfigStore.ConfigFile; }));
			AddFileFingerprint(data, "databaseConfig", SafeGetPath(delegate { return DatabaseConfigStore.ConfigFile; }));
			AddFileFingerprint(data, "displayLayoutConfig", SafeGetPath(delegate { return DisplayLayoutStore.ConfigFilePath; }));
			return data;
		}

		private static void EnsureInitialized()
		{
			if (!_initialized)
			{
				Initialize();
			}
		}

		private static string GetCurrentLogPath(DateTime time)
		{
			string dayFolder = Path.Combine(DiagnosticRoot, time.ToString("yyyyMMdd", CultureInfo.InvariantCulture));
			Directory.CreateDirectory(dayFolder);
			string day = time.ToString("yyyyMMdd", CultureInfo.InvariantCulture);

			for (int i = 0; i < 1000; i++)
			{
				string path = Path.Combine(dayFolder, "diagnostics_" + day + "_" + i.ToString("000", CultureInfo.InvariantCulture) + ".jsonl");
				if (!File.Exists(path))
				{
					return path;
				}

				FileInfo info = new FileInfo(path);
				if (info.Length < MaxLogFileBytes)
				{
					return path;
				}
			}

			return Path.Combine(dayFolder, "diagnostics_" + day + "_overflow.jsonl");
		}

		private static string SerializeEvent(DateTime time, DiagnosticLogLevel level, string category, string message, Dictionary<string, string> data)
		{
			StringBuilder sb = new StringBuilder();
			sb.Append('{');
			AppendJsonProperty(sb, "time", time.ToString("yyyy-MM-dd HH:mm:ss.fff", CultureInfo.InvariantCulture), true);
			AppendJsonProperty(sb, "utcTime", time.ToUniversalTime().ToString("o", CultureInfo.InvariantCulture), false);
			AppendJsonProperty(sb, "level", level.ToString(), false);
			AppendJsonProperty(sb, "category", category ?? string.Empty, false);
			AppendJsonProperty(sb, "message", message ?? string.Empty, false);
			sb.Append(",\"threadId\":").Append(Thread.CurrentThread.ManagedThreadId.ToString(CultureInfo.InvariantCulture));
			sb.Append(",\"data\":{");

			bool first = true;
			if (data != null)
			{
				foreach (KeyValuePair<string, string> pair in data)
				{
					string key = pair.Key ?? string.Empty;
					if (key.Length == 0)
					{
						continue;
					}

					if (!first)
					{
						sb.Append(',');
					}

					sb.Append('\"').Append(EscapeJson(key)).Append("\":\"").Append(EscapeJson(pair.Value ?? string.Empty)).Append('\"');
					first = false;
				}
			}

			sb.Append("}}");
			return sb.ToString();
		}

		private static void AppendJsonProperty(StringBuilder sb, string name, string value, bool first)
		{
			if (!first)
			{
				sb.Append(',');
			}

			sb.Append('\"').Append(EscapeJson(name)).Append("\":\"").Append(EscapeJson(value ?? string.Empty)).Append('\"');
		}

		private static string EscapeJson(string value)
		{
			if (string.IsNullOrEmpty(value))
			{
				return string.Empty;
			}

			StringBuilder sb = new StringBuilder(value.Length + 8);
			foreach (char c in value)
			{
				switch (c)
				{
					case '\\':
						sb.Append("\\\\");
						break;
					case '\"':
						sb.Append("\\\"");
						break;
					case '\r':
						sb.Append("\\r");
						break;
					case '\n':
						sb.Append("\\n");
						break;
					case '\t':
						sb.Append("\\t");
						break;
					default:
						if (c < 32)
						{
							sb.Append("\\u").Append(((int)c).ToString("x4", CultureInfo.InvariantCulture));
						}
						else
						{
							sb.Append(c);
						}
						break;
				}
			}

			return sb.ToString();
		}

		private static void AddProcessData(Dictionary<string, string> data)
		{
			try
			{
				Process process = Process.GetCurrentProcess();
				data["processId"] = process.Id.ToString(CultureInfo.InvariantCulture);
				data["privateMemoryMb"] = (process.PrivateMemorySize64 / 1024L / 1024L).ToString(CultureInfo.InvariantCulture);
				data["workingSetMb"] = (process.WorkingSet64 / 1024L / 1024L).ToString(CultureInfo.InvariantCulture);
				data["threadCount"] = process.Threads.Count.ToString(CultureInfo.InvariantCulture);
			}
			catch
			{
			}
		}

		private static void ExtractRuntimeTokens(string message, Dictionary<string, string> data)
		{
			AddToken(data, "job", message, "Job=");
			AddToken(data, "task", message, "Task=");
			AddToken(data, "step", message, "Step=");
			AddToken(data, "block", message, "Block=");
			AddToken(data, "blockType", message, "BlockType=");
			AddToken(data, "runOrder", message, "RunOrder=");
			AddToken(data, "communication", message, "Communication=");
			AddToken(data, "cost", message, "Cost=");
			AddToken(data, "values", message, "Values=");
			AddToken(data, "raw", message, "Raw=");
			AddToken(data, "parsed", message, "Parsed=");
		}

		private static void AddToken(Dictionary<string, string> data, string key, string message, string marker)
		{
			if (data == null || string.IsNullOrEmpty(message) || string.IsNullOrEmpty(marker))
			{
				return;
			}

			int start = message.IndexOf(marker, StringComparison.OrdinalIgnoreCase);
			if (start < 0)
			{
				return;
			}

			start += marker.Length;
			int end = message.IndexOf(',', start);
			if (end < 0)
			{
				end = message.Length;
			}

			string value = message.Substring(start, end - start).Trim();
			if (!string.IsNullOrEmpty(value))
			{
				data[key] = value;
			}
		}

		private static void AddFileFingerprint(Dictionary<string, string> data, string key, string path)
		{
			if (data == null || string.IsNullOrWhiteSpace(key))
			{
				return;
			}

			if (string.IsNullOrWhiteSpace(path) || !File.Exists(path))
			{
				data[key] = "missing|" + (path ?? string.Empty);
				return;
			}

			try
			{
				FileInfo info = new FileInfo(path);
				data[key] =
					path +
					"|length=" + info.Length.ToString(CultureInfo.InvariantCulture) +
					"|writeUtc=" + info.LastWriteTimeUtc.ToString("o", CultureInfo.InvariantCulture) +
					"|sha256=" + ComputeSha256(path);
			}
			catch (Exception ex)
			{
				data[key] = path + "|error=" + ex.Message;
			}
		}

		private static string ComputeSha256(string path)
		{
			using (FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
			using (SHA256 sha = SHA256.Create())
			{
				byte[] hash = sha.ComputeHash(fs);
				StringBuilder sb = new StringBuilder(hash.Length * 2);
				foreach (byte b in hash)
				{
					sb.Append(b.ToString("x2", CultureInfo.InvariantCulture));
				}

				return sb.ToString();
			}
		}

		private static string SafeGetPath(Func<string> getter)
		{
			try
			{
				return getter == null ? string.Empty : getter();
			}
			catch
			{
				return string.Empty;
			}
		}

		private static void CleanupOldLogs()
		{
			try
			{
				DateTime cutoff = DateTime.Now.Date.AddDays(-RetentionDays);
				string root = DiagnosticRoot;
				foreach (string folder in Directory.GetDirectories(root))
				{
					DirectoryInfo info = new DirectoryInfo(folder);
					if (info.LastWriteTime < cutoff)
					{
						info.Delete(true);
					}
				}
			}
			catch
			{
			}
		}
	}
}
