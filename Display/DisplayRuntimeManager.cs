using System;
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

		public DisplayImageEventArgs()
		{
			DisplaySlotName = "";
			Title = "";
			SourceInfo = "";
			DisplayMode = "Fit";
		}
	}

	public static class DisplayRuntimeManager
	{
		public static event EventHandler<DisplayImageEventArgs> DisplayImageRequested;

		public static void ShowImage(string displaySlotName, Bitmap image, string title, string sourceInfo, string displayMode)
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

			EventHandler<DisplayImageEventArgs> handler = DisplayImageRequested;

			if (handler == null)
			{
				return;
			}

			DisplayImageEventArgs args = new DisplayImageEventArgs();
			args.DisplaySlotName = displaySlotName;
			args.Image = image;
			args.Title = title;
			args.SourceInfo = sourceInfo;
			args.DisplayMode = string.IsNullOrWhiteSpace(displayMode) ? "Fit" : displayMode;

			handler(null, args);
		}
	}
}
