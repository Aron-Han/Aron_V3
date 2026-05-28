using System;
using System.IO;

namespace Aron_V3
{
	public enum RuntimeLogCategory
	{
		Task,
		Step,
		Communication
	}

	public static class RuntimeLogStore
	{
		private static readonly object _syncRoot = new object();
		public static event EventHandler<RuntimeFlowLogEventArgs> LogAppended;

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
			Append(time, category, message, IsErrorMessage(message));
		}

		public static void Append(DateTime time, RuntimeLogCategory category, string message, bool isError)
		{
			RuntimeFlowLogEventArgs args = new RuntimeFlowLogEventArgs(time, category, message, isError);
			string line = time.ToString("yyyy-MM-dd HH:mm:ss.fff") +
				" [" + GetCategoryText(category) + "]" +
				(isError ? " [Error]" : string.Empty) + " " +
				(message ?? string.Empty) + Environment.NewLine;
			string path = Path.Combine(LogFolder, time.ToString("yyyy-MM-dd") + ".log");
			lock (_syncRoot)
			{
				File.AppendAllText(path, line);
			}

			EventHandler<RuntimeFlowLogEventArgs> handler = LogAppended;
			if (handler != null)
			{
				handler(null, args);
			}
		}

		public static RuntimeLogCategory Classify(string message)
		{
			string text = (message ?? string.Empty).ToLowerInvariant();
			if (text.Contains("communication") || text.Contains("protocol=") || text.Contains("raw="))
			{
				return RuntimeLogCategory.Communication;
			}
			if (text.Contains("step") || text.Contains("image acquired") || text.Contains("image source"))
			{
				return RuntimeLogCategory.Step;
			}
			return RuntimeLogCategory.Task;
		}

		public static bool IsErrorMessage(string message)
		{
			string text = (message ?? string.Empty).ToLowerInvariant();
			return text.Contains("failed") ||
				text.Contains("error") ||
				text.Contains("alarm") ||
				text.Contains("exception") ||
				text.Contains("ok=false") ||
				text.Contains("ng");
		}

		public static string GetCategoryText(RuntimeLogCategory category)
		{
			switch (category)
			{
				case RuntimeLogCategory.Task:
					return "Task";
				case RuntimeLogCategory.Step:
					return "Step";
				case RuntimeLogCategory.Communication:
					return "Communication";
				default:
					return category.ToString();
			}
		}
	}
}
