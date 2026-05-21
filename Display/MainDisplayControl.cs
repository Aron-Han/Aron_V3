using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace Aron_V3
{
	public partial class MainDisplayControl : UserControl
	{
		private readonly Color _back = Color.FromArgb(2, 10, 20);
		private readonly Color _panel = Color.FromArgb(3, 14, 27);
		private readonly Color _text = Color.FromArgb(210, 230, 245);

		private TableLayoutPanel _layout;
		private Dictionary<string, PictureBox> _pictureBoxes;
		private Dictionary<string, Label> _titleLabels;

		public MainDisplayControl()
		{
			DoubleBuffered = true;
			Dock = DockStyle.Fill;
			BackColor = _back;

			_pictureBoxes = new Dictionary<string, PictureBox>(StringComparer.OrdinalIgnoreCase);
			_titleLabels = new Dictionary<string, Label>(StringComparer.OrdinalIgnoreCase);

			BuildLayout(DisplayLayoutStore.LoadOrCreateDefault());

			DisplayRuntimeManager.DisplayImageRequested += DisplayRuntimeManager_DisplayImageRequested;
			Disposed += MainDisplayControl_Disposed;
		}

		private void MainDisplayControl_Disposed(object sender, EventArgs e)
		{
			DisplayRuntimeManager.DisplayImageRequested -= DisplayRuntimeManager_DisplayImageRequested;

			foreach (PictureBox box in _pictureBoxes.Values)
			{
				if (box.Image != null)
				{
					box.Image.Dispose();
					box.Image = null;
				}
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

			Controls.Clear();
			_pictureBoxes.Clear();
			_titleLabels.Clear();

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
		}

		private Control CreateDisplayPanel(DisplaySlotConfig slot)
		{
			Panel panel = new Panel();
			panel.Dock = DockStyle.Fill;
			panel.BackColor = _panel;
			panel.Margin = new Padding(6);
			panel.Padding = new Padding(1);

			TableLayoutPanel inner = new TableLayoutPanel();
			inner.Dock = DockStyle.Fill;
			inner.RowCount = 2;
			inner.ColumnCount = 1;
			inner.BackColor = _panel;
			inner.RowStyles.Add(new RowStyle(SizeType.Absolute, 28F));
			inner.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));

			Label title = new Label();
			title.Dock = DockStyle.Fill;
			title.Text = slot.Title;
			title.ForeColor = _text;
			title.BackColor = Color.FromArgb(5, 18, 34);
			title.Font = new Font("Microsoft YaHei UI", 9F, FontStyle.Bold);
			title.TextAlign = ContentAlignment.MiddleLeft;
			title.Padding = new Padding(8, 0, 0, 0);

			PictureBox picture = new PictureBox();
			picture.Dock = DockStyle.Fill;
			picture.BackColor = Color.Black;
			picture.SizeMode = PictureBoxSizeMode.Zoom;
			picture.BorderStyle = BorderStyle.FixedSingle;

			inner.Controls.Add(title, 0, 0);
			inner.Controls.Add(picture, 0, 1);

			panel.Controls.Add(inner);

			if (!string.IsNullOrWhiteSpace(slot.SlotName))
			{
				_pictureBoxes[slot.SlotName] = picture;
				_titleLabels[slot.SlotName] = title;
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

			PictureBox box;

			if (!_pictureBoxes.TryGetValue(e.DisplaySlotName, out box))
			{
				return;
			}

			Bitmap newImage = null;

			try
			{
				newImage = new Bitmap(e.Image);

				Image old = box.Image;
				box.Image = newImage;
				newImage = null;

				if (old != null)
				{
					old.Dispose();
				}

				ApplyDisplayMode(box, e.DisplayMode);

				Label title;

				if (_titleLabels.TryGetValue(e.DisplaySlotName, out title))
				{
					if (!string.IsNullOrWhiteSpace(e.Title))
					{
						title.Text = e.Title;
					}
				}
			}
			finally
			{
				if (newImage != null)
				{
					newImage.Dispose();
				}
			}
		}

		private void ApplyDisplayMode(PictureBox box, string displayMode)
		{
			if (box == null)
			{
				return;
			}

			if (string.Equals(displayMode, "Center", StringComparison.OrdinalIgnoreCase))
			{
				box.SizeMode = PictureBoxSizeMode.CenterImage;
			}
			else if (string.Equals(displayMode, "Original", StringComparison.OrdinalIgnoreCase))
			{
				box.SizeMode = PictureBoxSizeMode.AutoSize;
			}
			else if (string.Equals(displayMode, "Stretch", StringComparison.OrdinalIgnoreCase))
			{
				box.SizeMode = PictureBoxSizeMode.StretchImage;
			}
			else
			{
				box.SizeMode = PictureBoxSizeMode.Zoom;
			}
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
}
