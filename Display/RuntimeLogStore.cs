using System;
using System.IO;

namespace Aron_V3
{
	public enum RuntimeLogCategory
	{
		Operation,
		Alarm,
		System
	}

	public static class RuntimeLogStore
	{
		private static readonly object _syncRoot = new object();

		public static string LogFolder
		{
			get
			{
				string folder = Path.Combine(ProjectPathStore.ProjectRoot, "Log");
				Directory.CreateDirectory(folder);
				return folder;
			}
		}

		public static void Append(DateTime time, RuntimeLogCategory category, string message)
		{
			string line = time.ToString("yyyy-MM-dd HH:mm:ss.fff") +
				" [" + category.ToString().ToUpperInvariant() + "] " +
				(message ?? string.Empty) + Environment.NewLine;
			string path = Path.Combine(LogFolder, time.ToString("yyyy-MM-dd") + ".log");
			lock (_syncRoot)
			{
				File.AppendAllText(path, line);
			}
		}

		public static RuntimeLogCategory Classify(string message)
		{
			string text = (message ?? string.Empty).ToLowerInvariant();
			if (text.Contains("failed") || text.Contains("error") || text.Contains("alarm") || text.Contains("exception"))
			{
				return RuntimeLogCategory.Alarm;
			}
			if (text.Contains("communication") || text.Contains("task") || text.Contains("image acquired"))
			{
				return RuntimeLogCategory.Operation;
			}
			return RuntimeLogCategory.System;
		}
	}
}
