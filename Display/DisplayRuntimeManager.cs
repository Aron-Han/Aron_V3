using System;
using System.Collections.Generic;
using System.Drawing;

namespace Aron_V3
{
	public class StepImageOutput
	{
		public string JobName { get; set; }
		public string TaskName { get; set; }
		public string StepName { get; set; }
		public string OutputKey { get; set; }
		public string ImageType { get; set; }
		public object RawImage { get; set; }
		public Bitmap DisplayBitmap { get; set; }
		public DateTime Time { get; set; }

		public StepImageOutput()
		{
			JobName = "";
			TaskName = "";
			StepName = "";
			OutputKey = "";
			ImageType = "";
			Time = DateTime.Now;
		}
	}

	public class DisplayImageEventArgs : EventArgs
	{
		public string DisplaySlotName { get; set; }
		public Bitmap Image { get; set; }
		public string Title { get; set; }
		public string SourceInfo { get; set; }
		public string DisplayMode { get; set; }
		public object Record { get; set; }
		public string RecordKey { get; set; }

		public DisplayImageEventArgs()
		{
			DisplaySlotName = "";
			Title = "";
			SourceInfo = "";
			DisplayMode = "Fit";
			RecordKey = "";
		}
	}

	public static class DisplayRuntimeManager
	{
		private static readonly object SyncRoot = new object();
		private static readonly Dictionary<string, DisplayImageEventArgs> LatestImages =
			new Dictionary<string, DisplayImageEventArgs>(StringComparer.OrdinalIgnoreCase);

		public static event EventHandler<DisplayImageEventArgs> DisplayImageRequested;

		public static void ShowImage(
			string displaySlotName,
			Bitmap image,
			string sourceInfo,
			string displayMode,
			object record,
			string recordKey)
		{
			if (string.IsNullOrWhiteSpace(displaySlotName))
			{
				return;
			}

			if (string.Equals(displaySlotName, "Not Show", StringComparison.OrdinalIgnoreCase))
			{
				return;
			}

			if (image == null)
			{
				return;
			}

			DisplayImageEventArgs cachedArgs = CreateArgs(displaySlotName, image, sourceInfo, displayMode, record, recordKey);
			lock (SyncRoot)
			{
				DisplayImageEventArgs old;
				if (LatestImages.TryGetValue(displaySlotName, out old) && old.Image != null)
				{
					old.Image.Dispose();
				}

				LatestImages[displaySlotName] = cachedArgs;
			}

			EventHandler<DisplayImageEventArgs> handler = DisplayImageRequested;

			if (handler == null)
			{
				return;
			}

			handler(null, CreateArgs(displaySlotName, image, sourceInfo, displayMode, record, recordKey));
		}

		public static List<DisplayImageEventArgs> GetLatestImages()
		{
			List<DisplayImageEventArgs> result = new List<DisplayImageEventArgs>();
			lock (SyncRoot)
			{
				foreach (DisplayImageEventArgs item in LatestImages.Values)
				{
					if (item != null && item.Image != null)
					{
						result.Add(CreateArgs(
							item.DisplaySlotName,
							item.Image,
							item.SourceInfo,
							item.DisplayMode,
							item.Record,
							item.RecordKey));
					}
				}
			}
			return result;
		}

		private static DisplayImageEventArgs CreateArgs(
			string displaySlotName,
			Bitmap image,
			string sourceInfo,
			string displayMode,
			object record,
			string recordKey)
		{
			DisplayImageEventArgs args = new DisplayImageEventArgs();
			args.DisplaySlotName = displaySlotName;
			args.Image = new Bitmap(image);
			args.SourceInfo = sourceInfo;
			args.DisplayMode = string.IsNullOrWhiteSpace(displayMode) ? "Fit" : displayMode;
			args.Record = record;
			args.RecordKey = recordKey ?? string.Empty;

			return args;
		}
	}
}
