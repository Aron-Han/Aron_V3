using System;
using System.Collections.Generic;
using System.Drawing;

namespace Aron_V3
{
	public class TaskRunOptions
	{
		public bool IsTestMode { get; set; }
		public bool EnableCommunicationOutput { get; set; }
		public Dictionary<string, object> OverrideImageSources { get; set; }

		public TaskRunOptions()
		{
			IsTestMode = false;
			EnableCommunicationOutput = true;
			OverrideImageSources = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
		}

		public static TaskRunOptions Normal()
		{
			return new TaskRunOptions
			{
				IsTestMode = false,
				EnableCommunicationOutput = true
			};
		}

		public static TaskRunOptions Test(bool enableCommunicationOutput)
		{
			return new TaskRunOptions
			{
				IsTestMode = true,
				EnableCommunicationOutput = enableCommunicationOutput
			};
		}
	}

	public static class TaskRunContext
	{
		[ThreadStatic]
		private static TaskRunOptions _current;

		public static TaskRunOptions Current
		{
			get { return _current; }
		}

		public static bool IsTestMode
		{
			get { return _current != null && _current.IsTestMode; }
		}

		public static bool EnableCommunicationOutput
		{
			get
			{
				if (_current == null)
				{
					return true;
				}

				return _current.EnableCommunicationOutput;
			}
		}

		public static IDisposable Begin(TaskRunOptions options)
		{
			TaskRunOptions old = _current;
			_current = options;
			return new Scope(old);
		}

		public static bool TryGetOverrideImage(string imageSourceName, out object image)
		{
			image = null;

			if (_current == null || _current.OverrideImageSources == null)
			{
				return false;
			}

			if (string.IsNullOrWhiteSpace(imageSourceName))
			{
				return false;
			}

			return _current.OverrideImageSources.TryGetValue(imageSourceName, out image);
		}

		private class Scope : IDisposable
		{
			private readonly TaskRunOptions _old;

			public Scope(TaskRunOptions old)
			{
				_old = old;
			}

			public void Dispose()
			{
				_current = _old;
			}
		}
	}
}
