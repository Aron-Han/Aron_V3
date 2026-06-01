using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using System.Xml.Serialization;

namespace Aron_V3
{
	public partial class MainDisplayControl : UserControl
	{
		private readonly Color _back = Color.FromArgb(2, 10, 20);
		private readonly Color _panel = Color.FromArgb(3, 14, 27);
		private readonly Color _text = Color.FromArgb(210, 230, 245);

		private TableLayoutPanel _layout;
		private Dictionary<string, InspectionDisplayPanel> _displayPanels;
		private string _layoutSignature = string.Empty;

		public MainDisplayControl()
		{
			DoubleBuffered = true;
			Dock = DockStyle.Fill;
			BackColor = _back;

			_displayPanels = new Dictionary<string, InspectionDisplayPanel>(StringComparer.OrdinalIgnoreCase);

			BuildLayout(DisplayLayoutStore.LoadOrCreateDefault());

			DisplayRuntimeManager.DisplayImageRequested += DisplayRuntimeManager_DisplayImageRequested;
			DisplayRuntimeManager.DisplayInfoRequested += DisplayRuntimeManager_DisplayInfoRequested;
			Disposed += MainDisplayControl_Disposed;
		}

		private void MainDisplayControl_Disposed(object sender, EventArgs e)
		{
			DisplayRuntimeManager.DisplayImageRequested -= DisplayRuntimeManager_DisplayImageRequested;
			DisplayRuntimeManager.DisplayInfoRequested -= DisplayRuntimeManager_DisplayInfoRequested;

			foreach (InspectionDisplayPanel panel in _displayPanels.Values)
			{
				panel.ClearImage();
			}
		}

		public void ReloadLayout()
		{
			BuildLayout(DisplayLayoutStore.LoadOrCreateDefault());
		}

		public void BuildLayout(DisplayLayoutConfig config)
		{
			if (config == null)
			{
				config = DisplayLayoutStore.CreateDefault();
			}

			string signature = BuildLayoutSignature(config);
			if (string.Equals(_layoutSignature, signature, StringComparison.OrdinalIgnoreCase) &&
				_layout != null &&
				_layout.Parent == this)
			{
				return;
			}

			SuspendLayout();
			try
			{
				ReleaseCurrentImages();
				Controls.Clear();
				_displayPanels.Clear();

				_layout = new TableLayoutPanel();
				_layout.Dock = DockStyle.Fill;
				_layout.BackColor = _back;
				_layout.Padding = new Padding(8);
				_layout.Margin = new Padding(0);

				int count = Math.Max(1, config.DisplayCount);
				int columns;
				int rows;

				CalculateGrid(count, out rows, out columns);

				_layout.RowCount = rows;
				_layout.ColumnCount = columns;

				_layout.RowStyles.Clear();
				_layout.ColumnStyles.Clear();

				for (int r = 0; r < rows; r++)
				{
					_layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F / rows));
				}

				for (int c = 0; c < columns; c++)
				{
					_layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F / columns));
				}

				int index = 0;

				for (int r = 0; r < rows; r++)
				{
					for (int c = 0; c < columns; c++)
					{
						if (index >= count)
						{
							break;
						}

						DisplaySlotConfig slot = index < config.Displays.Count ? config.Displays[index] : null;

						if (slot == null)
						{
							slot = new DisplaySlotConfig();
							slot.SlotName = "Display" + (index + 1);
							slot.Title = "Display" + (index + 1);
							slot.Enable = true;
						}

						Control panel = CreateDisplayPanel(slot);
						_layout.Controls.Add(panel, c, r);
						index++;
					}
				}

				Controls.Add(_layout);
				_layoutSignature = signature;
			}
			finally
			{
				ResumeLayout(true);
			}

			ReplayLatestImages();
		}

		public void RefreshAfterWindowRestore()
		{
			try
			{
				if (_layout != null)
				{
					_layout.PerformLayout();
				}

				Invalidate(true);
				Update();
			}
			catch
			{
			}
		}

		private string BuildLayoutSignature(DisplayLayoutConfig config)
		{
			if (config == null)
			{
				return string.Empty;
			}

			List<string> parts = new List<string>();
			parts.Add(Math.Max(1, config.DisplayCount).ToString());

			if (config.Displays != null)
			{
				foreach (DisplaySlotConfig slot in config.Displays)
				{
					if (slot == null)
					{
						parts.Add(string.Empty);
						continue;
					}

					parts.Add((slot.SlotName ?? string.Empty) + "|" +
						(slot.Title ?? string.Empty) + "|" +
						slot.Enable.ToString());
				}
			}

			return string.Join(";", parts.ToArray());
		}

		private void ReplayLatestImages()
		{
			foreach (DisplayImageEventArgs args in DisplayRuntimeManager.GetLatestImages())
			{
				ShowDisplayImage(args, false);
			}
		}

		private void ReleaseCurrentImages()
		{
			foreach (InspectionDisplayPanel panel in _displayPanels.Values)
			{
				panel.ClearImage();
			}

		}

		private Control CreateDisplayPanel(DisplaySlotConfig slot)
		{
			InspectionDisplayPanel panel = new InspectionDisplayPanel();
			panel.Dock = DockStyle.Fill;
			panel.Margin = new Padding(6);
			panel.Title = slot.Title;
			panel.DisplaySlotName = slot.SlotName;

			if (!string.IsNullOrWhiteSpace(slot.SlotName))
			{
				_displayPanels[slot.SlotName] = panel;
			}

			return panel;
		}

		private void DisplayRuntimeManager_DisplayImageRequested(object sender, DisplayImageEventArgs e)
		{
			if (e == null || e.Image == null || string.IsNullOrWhiteSpace(e.DisplaySlotName))
			{
				return;
			}

			if (InvokeRequired)
			{
				BeginInvoke(new EventHandler<DisplayImageEventArgs>(DisplayRuntimeManager_DisplayImageRequested), sender, e);
				return;
			}

			ShowDisplayImage(e, true);
		}

		private void DisplayRuntimeManager_DisplayInfoRequested(object sender, DisplayInfoEventArgs e)
		{
			if (e == null || string.IsNullOrWhiteSpace(e.DisplaySlotName))
			{
				return;
			}

			if (InvokeRequired)
			{
				BeginInvoke(new EventHandler<DisplayInfoEventArgs>(DisplayRuntimeManager_DisplayInfoRequested), sender, e);
				return;
			}

			InspectionDisplayPanel panel;
			if (_displayPanels.TryGetValue(e.DisplaySlotName, out panel))
			{
				panel.UpdateInfo(e.JobName, e.JobID, e.PosID, e.EngineID);
			}
		}

		private void ShowDisplayImage(DisplayImageEventArgs e, bool countResult)
		{
			InspectionDisplayPanel panel;

			if (!_displayPanels.TryGetValue(e.DisplaySlotName, out panel))
			{
				e.Image.Dispose();
				return;
			}

			Bitmap newImage = null;

			try
			{
				newImage = TryRenderRecordBitmap(e, panel.ImageBox) ?? new Bitmap(e.Image);

				panel.SetImage(newImage, e, countResult);
				newImage = null;

				ApplyDisplayMode(panel.ImageBox, e.DisplayMode);

			}
			finally
			{
				if (newImage != null)
				{
					newImage.Dispose();
				}

				e.Image.Dispose();
			}
		}

		private Bitmap TryRenderRecordBitmap(DisplayImageEventArgs e, ZoomableImageBox box)
		{
			return VisionProRecordBitmapRenderer.TryRender(e.Record, e.RecordKey, box.ClientSize);
		}

		private void ApplyDisplayMode(ZoomableImageBox box, string displayMode)
		{
			if (box == null)
			{
				return;
			}

			box.DisplayMode = displayMode;
		}

		private void CalculateGrid(int count, out int rows, out int columns)
		{
			if (count <= 1)
			{
				rows = 1;
				columns = 1;
				return;
			}

			if (count == 2)
			{
				rows = 1;
				columns = 2;
				return;
			}

			if (count <= 4)
			{
				rows = 2;
				columns = 2;
				return;
			}

			if (count <= 6)
			{
				rows = 2;
				columns = 3;
				return;
			}

			if (count <= 8)
			{
				rows = 2;
				columns = 4;
				return;
			}

			if (count <= 9)
			{
				rows = 3;
				columns = 3;
				return;
			}

			rows = 3;
			columns = 4;
		}
	}

	internal sealed class InspectionDisplayPanel : UserControl
	{
		private readonly Color _panelBack = Color.FromArgb(3, 14, 27);
		private readonly Color _headerBack = Color.FromArgb(5, 18, 34);
		private readonly Color _infoBack = Color.FromArgb(6, 22, 38);
		private readonly Color _text = Color.FromArgb(210, 230, 245);
		private readonly Color _muted = Color.FromArgb(145, 170, 190);
		private readonly Color _divider = Color.FromArgb(24, 58, 88);
		private readonly Color _neutral = Color.FromArgb(88, 104, 118);
		private readonly Color _ok = Color.FromArgb(0, 180, 80);
		private readonly Color _ng = Color.FromArgb(220, 55, 70);

		private readonly Label _title;
		private readonly ResultFramePanel _imageFrame;
		private readonly ZoomableImageBox _imageBox;
		private readonly Label _totalLabel;
		private readonly Label _passLabel;
		private readonly Label _failLabel;
		private readonly Label _rateLabel;
		private readonly Label _jobLabel;
		private readonly Label _engineLabel;
		private readonly Label _resetButton;

		private int _passCount;
		private int _failCount;
		private bool _lastOK = true;
		private string _displaySlotName = string.Empty;
		private string _activeJobName = string.Empty;
		private string _lastJobID = string.Empty;
		private string _lastPosID = string.Empty;
		private string _lastEngineID = string.Empty;

		public InspectionDisplayPanel()
		{
			DoubleBuffered = true;
			BackColor = _panelBack;
			Padding = new Padding(1);

			TableLayoutPanel root = new TableLayoutPanel();
			root.Dock = DockStyle.Fill;
			root.RowCount = 3;
			root.ColumnCount = 1;
			root.BackColor = _panelBack;
			root.RowStyles.Add(new RowStyle(SizeType.Absolute, 28F));
			root.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
			root.RowStyles.Add(new RowStyle(SizeType.Absolute, 58F));
			Controls.Add(root);

			_title = new Label();
			_title.Dock = DockStyle.Fill;
			_title.ForeColor = _text;
			_title.BackColor = _headerBack;
			_title.Font = new Font("Microsoft YaHei UI", 9F, FontStyle.Bold);
			_title.TextAlign = ContentAlignment.MiddleLeft;
			_title.Padding = new Padding(8, 0, 0, 0);
			root.Controls.Add(_title, 0, 0);

			_imageFrame = new ResultFramePanel();
			_imageFrame.Dock = DockStyle.Fill;
			_imageFrame.BackColor = Color.FromArgb(1, 8, 16);
			_imageFrame.Padding = new Padding(3);
			root.Controls.Add(_imageFrame, 0, 1);

			_imageBox = new ZoomableImageBox();
			_imageBox.Dock = DockStyle.Fill;
			_imageBox.BackColor = Color.Black;
			_imageFrame.Controls.Add(_imageBox);

			TableLayoutPanel info = new TableLayoutPanel();
			info.Dock = DockStyle.Fill;
			info.BackColor = _infoBack;
			info.Padding = new Padding(6, 3, 6, 3);
			info.RowCount = 2;
			info.ColumnCount = 5;
			info.CellPaint += Info_CellPaint;
			info.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));
			info.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));
			info.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 18F));
			info.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 18F));
			info.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 18F));
			info.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 26F));
			info.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 20F));
			root.Controls.Add(info, 0, 2);

			_totalLabel = CreateInfoLabel();
			_passLabel = CreateInfoLabel();
			_failLabel = CreateInfoLabel();
			_rateLabel = CreateInfoLabel();
			_jobLabel = CreateInfoLabel();
			_engineLabel = CreateInfoLabel();

			_resetButton = new Label();
			_resetButton.Text = "Reset";
			_resetButton.Dock = DockStyle.Fill;
			_resetButton.Margin = new Padding(6, 1, 2, 1);
			_resetButton.Font = new Font("Microsoft YaHei UI", 7.6F, FontStyle.Bold);
			_resetButton.BackColor = Color.FromArgb(2, 10, 20);
			_resetButton.ForeColor = _text;
			_resetButton.BorderStyle = BorderStyle.FixedSingle;
			_resetButton.TextAlign = ContentAlignment.MiddleCenter;
			_resetButton.Cursor = Cursors.Hand;
			_resetButton.Click += delegate { ResetStats(); };

			info.Controls.Add(_totalLabel, 0, 0);
			info.Controls.Add(_passLabel, 1, 0);
			info.Controls.Add(_failLabel, 2, 0);
			info.Controls.Add(_rateLabel, 3, 0);
			info.Controls.Add(_resetButton, 4, 0);
			info.Controls.Add(_jobLabel, 0, 1);
			info.Controls.Add(_engineLabel, 1, 1);
			info.SetColumnSpan(_engineLabel, 3);

			UpdateLabels(string.Empty, string.Empty, string.Empty);
			UpdateResultFrameNeutral();
		}

		public ZoomableImageBox ImageBox
		{
			get { return _imageBox; }
		}

		public string Title
		{
			get { return _title.Text; }
			set { _title.Text = value ?? string.Empty; }
		}

		public string DisplaySlotName
		{
			get { return _displaySlotName; }
			set
			{
				_displaySlotName = value ?? string.Empty;
				_activeJobName = string.Empty;
				LoadStatsForJob("Default");
			}
		}

		public void SetImage(Bitmap image, DisplayImageEventArgs args, bool countResult)
		{
			if (args == null)
			{
				return;
			}

			_imageBox.Image = image;
			string jobName = ResolveJobName(args);

			if (!string.Equals(_activeJobName, jobName, StringComparison.OrdinalIgnoreCase))
			{
				LoadStatsForJob(jobName);
			}

			_lastOK = args.InspectionOK;
			if (!string.IsNullOrWhiteSpace(args.JobID)) _lastJobID = args.JobID;
			if (!string.IsNullOrWhiteSpace(args.PosID)) _lastPosID = args.PosID;
			if (!string.IsNullOrWhiteSpace(args.EngineID)) _lastEngineID = args.EngineID;

			if (countResult)
			{
				if (args.InspectionOK) _passCount++;
				else _failCount++;

				DisplayInspectionStatsStore.SaveStats(CreateStatsItem());
			}

			UpdateResultFrame(args.InspectionOK);
			UpdateLabels(_lastJobID, _lastPosID, _lastEngineID);
		}

		public void UpdateInfo(string jobName, string jobID, string posID, string engineID)
		{
			string resolvedJobName = string.IsNullOrWhiteSpace(jobName) ? "Default" : jobName;

			if (!string.Equals(_activeJobName, resolvedJobName, StringComparison.OrdinalIgnoreCase))
			{
				LoadStatsForJob(resolvedJobName);
			}

			_lastJobID = jobID ?? string.Empty;
			_lastPosID = posID ?? string.Empty;
			_lastEngineID = engineID ?? string.Empty;

			DisplayInspectionStatsStore.SaveStats(CreateStatsItem());
			UpdateLabels(_lastJobID, _lastPosID, _lastEngineID);
		}

		public void ClearImage()
		{
			_imageBox.Image = null;
			UpdateResultFrameNeutral();
		}

		private Label CreateInfoLabel()
		{
			Label label = new Label();
			label.Dock = DockStyle.Fill;
			label.ForeColor = _text;
			label.BackColor = Color.Transparent;
			label.Font = new Font("Microsoft YaHei UI", 7.4F, FontStyle.Bold);
			label.TextAlign = ContentAlignment.MiddleLeft;
			label.AutoEllipsis = true;
			label.Padding = new Padding(6, 0, 4, 0);
			return label;
		}

		private void Info_CellPaint(object sender, TableLayoutCellPaintEventArgs e)
		{
			using (Pen pen = new Pen(_divider))
			{
				Rectangle bounds = e.CellBounds;
				e.Graphics.DrawLine(pen, bounds.Right - 1, bounds.Top + 4, bounds.Right - 1, bounds.Bottom - 4);

				if (e.Row == 0)
				{
					e.Graphics.DrawLine(pen, bounds.Left + 4, bounds.Bottom - 1, bounds.Right - 4, bounds.Bottom - 1);
				}
			}
		}

		private void ResetStats()
		{
			_passCount = 0;
			_failCount = 0;
			DisplayInspectionStatsStore.ResetStats(_activeJobName, _displaySlotName);
			UpdateResultFrame(_lastOK);
			UpdateLabels(_lastJobID, _lastPosID, _lastEngineID);
		}

		private string ExtractValue(string text)
		{
			if (string.IsNullOrWhiteSpace(text))
			{
				return string.Empty;
			}

			int index = text.IndexOf(':');
			return index >= 0 && index + 1 < text.Length ? text.Substring(index + 1).Trim() : string.Empty;
		}

		private void UpdateLabels(string jobID, string posID, string engineID)
		{
			int total = _passCount + _failCount;
			double rate = total <= 0 ? 0.0 : _passCount * 100.0 / total;
			_totalLabel.Text = "Total: " + total.ToString();
			_passLabel.Text = "Pass: " + _passCount.ToString();
			_failLabel.Text = "Fail: " + _failCount.ToString();
			_rateLabel.Text = "PassRate: " + rate.ToString("0.00") + "%";
			_jobLabel.Text = "JobID: " + (jobID ?? string.Empty);
			_engineLabel.Text = "Channel: " + (engineID ?? string.Empty);
			_passLabel.ForeColor = _text;
			_failLabel.ForeColor = _text;
		}

		private void UpdateResultFrame(bool ok)
		{
			_imageFrame.ResultColor = ok ? _ok : _ng;
		}

		private void UpdateResultFrameNeutral()
		{
			_imageFrame.ResultColor = _neutral;
		}

		private void LoadStatsForJob(string jobName)
		{
			_activeJobName = string.IsNullOrWhiteSpace(jobName) ? "Default" : jobName;
			DisplayInspectionStatsItem stats = DisplayInspectionStatsStore.LoadStats(_activeJobName, _displaySlotName);
			_passCount = stats == null ? 0 : Math.Max(0, stats.PassCount);
			_failCount = stats == null ? 0 : Math.Max(0, stats.FailCount);
			_lastOK = stats == null ? true : stats.LastOK;
			_lastJobID = stats == null ? string.Empty : (stats.JobID ?? string.Empty);
			_lastPosID = stats == null ? string.Empty : (stats.PosID ?? string.Empty);
			_lastEngineID = stats == null ? string.Empty : (stats.EngineID ?? string.Empty);
			UpdateResultFrameNeutral();
			UpdateLabels(_lastJobID, _lastPosID, _lastEngineID);
		}

		private DisplayInspectionStatsItem CreateStatsItem()
		{
			DisplayInspectionStatsItem item = new DisplayInspectionStatsItem();
			item.JobName = string.IsNullOrWhiteSpace(_activeJobName) ? "Default" : _activeJobName;
			item.DisplaySlotName = _displaySlotName ?? string.Empty;
			item.PassCount = _passCount;
			item.FailCount = _failCount;
			item.LastOK = _lastOK;
			item.JobID = _lastJobID ?? string.Empty;
			item.PosID = _lastPosID ?? string.Empty;
			item.EngineID = _lastEngineID ?? string.Empty;
			return item;
		}

		private static string ResolveJobName(DisplayImageEventArgs args)
		{
			if (args == null)
			{
				return "Default";
			}

			if (!string.IsNullOrWhiteSpace(args.JobName))
			{
				return args.JobName;
			}

			if (!string.IsNullOrWhiteSpace(args.SourceInfo))
			{
				string[] parts = args.SourceInfo.Split(new string[] { " / " }, StringSplitOptions.None);
				if (parts.Length > 0 && !string.IsNullOrWhiteSpace(parts[0]))
				{
					return parts[0].Trim();
				}
			}

			return "Default";
		}
	}

	internal sealed class ResultFramePanel : Panel
	{
		private Color _resultColor = Color.FromArgb(88, 104, 118);

		public ResultFramePanel()
		{
			DoubleBuffered = true;
			SetStyle(ControlStyles.AllPaintingInWmPaint |
				ControlStyles.UserPaint |
				ControlStyles.OptimizedDoubleBuffer |
				ControlStyles.ResizeRedraw, true);
		}

		public Color ResultColor
		{
			get { return _resultColor; }
			set
			{
				if (_resultColor.ToArgb() == value.ToArgb())
				{
					return;
				}

				_resultColor = value;
				Invalidate();
			}
		}

		protected override void OnPaint(PaintEventArgs e)
		{
			base.OnPaint(e);

			Rectangle rect = ClientRectangle;
			if (rect.Width <= 1 || rect.Height <= 1)
			{
				return;
			}

			rect.Inflate(-1, -1);
			using (Pen pen = new Pen(_resultColor, 3F))
			{
				e.Graphics.DrawRectangle(pen, rect);
			}
		}
	}

	[Serializable]
	public class DisplayInspectionStatsConfig
	{
		public List<DisplayInspectionStatsItem> Items { get; set; }

		public DisplayInspectionStatsConfig()
		{
			Items = new List<DisplayInspectionStatsItem>();
		}
	}

	[Serializable]
	public class DisplayInspectionStatsItem
	{
		public string JobName { get; set; }
		public string DisplaySlotName { get; set; }
		public int PassCount { get; set; }
		public int FailCount { get; set; }
		public bool LastOK { get; set; }
		public string JobID { get; set; }
		public string PosID { get; set; }
		public string EngineID { get; set; }

		public DisplayInspectionStatsItem()
		{
			JobName = "Default";
			DisplaySlotName = "";
			LastOK = true;
			JobID = "";
			PosID = "";
			EngineID = "";
		}
	}

	public static class DisplayInspectionStatsStore
	{
		private static readonly object SyncRoot = new object();

		public static string ConfigFilePath
		{
			get
			{
				string folder = ProjectPathStore.SystemConfigRoot;
				Directory.CreateDirectory(folder);
				return Path.Combine(folder, "DisplayInspectionStats.xml");
			}
		}

		public static DisplayInspectionStatsItem LoadStats(string jobName, string displaySlotName)
		{
			lock (SyncRoot)
			{
				DisplayInspectionStatsConfig config = LoadOrCreateDefault();
				return Find(config, jobName, displaySlotName);
			}
		}

		public static void SaveStats(DisplayInspectionStatsItem item)
		{
			if (item == null)
			{
				return;
			}

			lock (SyncRoot)
			{
				DisplayInspectionStatsConfig config = LoadOrCreateDefault();
				DisplayInspectionStatsItem existing = Find(config, item.JobName, item.DisplaySlotName);

				if (existing == null)
				{
					existing = new DisplayInspectionStatsItem();
					config.Items.Add(existing);
				}

				existing.JobName = NormalizeKey(item.JobName, "Default");
				existing.DisplaySlotName = NormalizeKey(item.DisplaySlotName, "Display");
				existing.PassCount = Math.Max(0, item.PassCount);
				existing.FailCount = Math.Max(0, item.FailCount);
				existing.LastOK = item.LastOK;
				existing.JobID = item.JobID ?? string.Empty;
				existing.PosID = item.PosID ?? string.Empty;
				existing.EngineID = item.EngineID ?? string.Empty;
				Save(config);
			}
		}

		public static void ResetStats(string jobName, string displaySlotName)
		{
			lock (SyncRoot)
			{
				DisplayInspectionStatsConfig config = LoadOrCreateDefault();
				DisplayInspectionStatsItem item = Find(config, jobName, displaySlotName);

				if (item != null)
				{
					item.PassCount = 0;
					item.FailCount = 0;
					Save(config);
				}
			}
		}

		private static DisplayInspectionStatsConfig LoadOrCreateDefault()
		{
			try
			{
				if (File.Exists(ConfigFilePath))
				{
					XmlSerializer serializer = new XmlSerializer(typeof(DisplayInspectionStatsConfig));

					using (FileStream fs = new FileStream(ConfigFilePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
					{
						DisplayInspectionStatsConfig config = serializer.Deserialize(fs) as DisplayInspectionStatsConfig;
						if (config != null)
						{
							Normalize(config);
							return config;
						}
					}
				}
			}
			catch
			{
			}

			return new DisplayInspectionStatsConfig();
		}

		private static void Save(DisplayInspectionStatsConfig config)
		{
			if (config == null)
			{
				config = new DisplayInspectionStatsConfig();
			}

			Normalize(config);
			string folder = Path.GetDirectoryName(ConfigFilePath);
			if (!Directory.Exists(folder))
			{
				Directory.CreateDirectory(folder);
			}

			XmlSerializer serializer = new XmlSerializer(typeof(DisplayInspectionStatsConfig));
			using (FileStream fs = new FileStream(ConfigFilePath, FileMode.Create, FileAccess.Write, FileShare.Read))
			{
				serializer.Serialize(fs, config);
			}
		}

		private static DisplayInspectionStatsItem Find(DisplayInspectionStatsConfig config, string jobName, string displaySlotName)
		{
			if (config == null || config.Items == null)
			{
				return null;
			}

			string normalizedJob = NormalizeKey(jobName, "Default");
			string normalizedSlot = NormalizeKey(displaySlotName, "Display");

			foreach (DisplayInspectionStatsItem item in config.Items)
			{
				if (item != null &&
					string.Equals(NormalizeKey(item.JobName, "Default"), normalizedJob, StringComparison.OrdinalIgnoreCase) &&
					string.Equals(NormalizeKey(item.DisplaySlotName, "Display"), normalizedSlot, StringComparison.OrdinalIgnoreCase))
				{
					return item;
				}
			}

			return null;
		}

		private static void Normalize(DisplayInspectionStatsConfig config)
		{
			if (config.Items == null)
			{
				config.Items = new List<DisplayInspectionStatsItem>();
			}

			foreach (DisplayInspectionStatsItem item in config.Items)
			{
				if (item == null)
				{
					continue;
				}

				item.JobName = NormalizeKey(item.JobName, "Default");
				item.DisplaySlotName = NormalizeKey(item.DisplaySlotName, "Display");
				item.PassCount = Math.Max(0, item.PassCount);
				item.FailCount = Math.Max(0, item.FailCount);
				item.JobID = item.JobID ?? string.Empty;
				item.PosID = item.PosID ?? string.Empty;
				item.EngineID = item.EngineID ?? string.Empty;
			}
		}

		private static string NormalizeKey(string value, string defaultValue)
		{
			return string.IsNullOrWhiteSpace(value) ? defaultValue : value.Trim();
		}
	}
}
