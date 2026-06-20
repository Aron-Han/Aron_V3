using System;
using System.Drawing;
using System.Reflection;
using System.Windows.Forms;

namespace Aron_V3
{
	public partial class DisplayLayoutControl : UserControl, ILocalizable
	{
		private readonly Color _back = Color.FromArgb(2, 10, 20);
		private readonly Color _panel = Color.FromArgb(3, 14, 27);
		private readonly Color _panel2 = Color.FromArgb(5, 18, 34);
		private readonly Color _border = Color.FromArgb(38, 62, 86);
		private readonly Color _accent = Color.FromArgb(0, 150, 220);
		private readonly Color _text = Color.FromArgb(220, 235, 245);

		private NumericUpDown numCount;
		private DataGridView dgvSlots;
		private Button btnSave;

		private DisplayLayoutConfig _config;

		public DisplayLayoutControl()
		{
			Dock = DockStyle.Fill;
			BackColor = _back;
			BuildUi();
			LoadConfigToUi();
		}

		public void ApplyLanguage(bool isEnglish)
		{
			if (btnSave != null)
			{
				btnSave.Text = isEnglish ? "Save" : "保存配置";
			}

			if (dgvSlots != null && dgvSlots.Columns.Contains("SlotName"))
			{
				dgvSlots.Columns["SlotName"].HeaderText = isEnglish ? "Display Slot" : "显示框";
				dgvSlots.Columns["Title"].HeaderText = isEnglish ? "Title" : "标题";
				dgvSlots.Columns["Enable"].HeaderText = isEnglish ? "Enable" : "启用";
			}
		}

		private void BuildUi()
		{
			Controls.Clear();

			TableLayoutPanel root = new TableLayoutPanel();
			root.Dock = DockStyle.Fill;
			root.BackColor = _back;
			root.Padding = new Padding(16);
			root.RowCount = 3;
			root.ColumnCount = 1;
			root.RowStyles.Add(new RowStyle(SizeType.Absolute, 68F));
			root.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
			root.RowStyles.Add(new RowStyle(SizeType.Absolute, 54F));

			Panel top = new Panel();
			top.Dock = DockStyle.Fill;
			top.BackColor = _panel2;
			top.Padding = new Padding(14);

			Label lblCount = CreateLabel("显示框数量", 14, 14, 90, 26);
			numCount = new NumericUpDown();
			numCount.Left = 110;
			numCount.Top = 15;
			numCount.Width = 110;
			numCount.Height = 24;
			numCount.Minimum = 1;
			numCount.Maximum = 16;
			numCount.BackColor = _back;
			numCount.ForeColor = Color.White;
			numCount.BorderStyle = BorderStyle.FixedSingle;
			numCount.Font = new Font("Microsoft YaHei UI", 9F, FontStyle.Regular);
			numCount.ValueChanged += delegate { ApplyCountToGrid(); };

			top.Controls.Add(lblCount);
			top.Controls.Add(numCount);

			dgvSlots = new DataGridView();
			dgvSlots.Dock = DockStyle.Fill;
			dgvSlots.BackgroundColor = _back;
			dgvSlots.BorderStyle = BorderStyle.FixedSingle;
			dgvSlots.AllowUserToAddRows = false;
			dgvSlots.AllowUserToDeleteRows = false;
			dgvSlots.RowHeadersVisible = false;
			dgvSlots.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
			dgvSlots.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
			dgvSlots.MultiSelect = false;
			dgvSlots.EnableHeadersVisualStyles = false;

			dgvSlots.ColumnHeadersDefaultCellStyle.BackColor = _panel2;
			dgvSlots.ColumnHeadersDefaultCellStyle.ForeColor = _text;
			dgvSlots.ColumnHeadersDefaultCellStyle.Font = new Font("Microsoft YaHei UI", 9F, FontStyle.Bold);
			dgvSlots.DefaultCellStyle.BackColor = _back;
			dgvSlots.DefaultCellStyle.ForeColor = _text;
			dgvSlots.DefaultCellStyle.SelectionBackColor = Color.FromArgb(0, 120, 190);
			dgvSlots.DefaultCellStyle.SelectionForeColor = Color.White;
			dgvSlots.GridColor = _border;
			SetDoubleBuffered(dgvSlots);

			DataGridViewTextBoxColumn colSlot = new DataGridViewTextBoxColumn();
			colSlot.Name = "SlotName";
			colSlot.HeaderText = "显示框";
			colSlot.FillWeight = 25;

			DataGridViewTextBoxColumn colTitle = new DataGridViewTextBoxColumn();
			colTitle.Name = "Title";
			colTitle.HeaderText = "标题";
			colTitle.FillWeight = 45;

			DataGridViewCheckBoxColumn colEnable = new DataGridViewCheckBoxColumn();
			colEnable.Name = "Enable";
			colEnable.HeaderText = "启用";
			colEnable.FillWeight = 15;

			dgvSlots.Columns.Add(colSlot);
			dgvSlots.Columns.Add(colTitle);
			dgvSlots.Columns.Add(colEnable);

			Panel bottom = new Panel();
			bottom.Dock = DockStyle.Fill;
			bottom.BackColor = _panel;
			bottom.Padding = new Padding(0, 10, 0, 0);

			btnSave = CreateButton("保存配置", 0, 10, 110, 32);
			btnSave.BackColor = Color.FromArgb(0, 95, 210);

			btnSave.Click += delegate { SaveConfigFromUi(); };

			bottom.Controls.Add(btnSave);

			root.Controls.Add(top, 0, 0);
			root.Controls.Add(dgvSlots, 0, 1);
			root.Controls.Add(bottom, 0, 2);

			Controls.Add(root);
		}

		private void LoadConfigToUi()
		{
			_config = DisplayLayoutStore.LoadOrCreateDefault();

			numCount.Value = Math.Max(numCount.Minimum, Math.Min(numCount.Maximum, _config.DisplayCount));

			LoadSlotsToGrid();
		}

		private void LoadSlotsToGrid()
		{
			dgvSlots.Rows.Clear();

			if (_config == null)
			{
				return;
			}

			for (int i = 0; i < _config.DisplayCount; i++)
			{
				DisplaySlotConfig slot = i < _config.Displays.Count ? _config.Displays[i] : null;

				if (slot == null)
				{
					slot = new DisplaySlotConfig();
					slot.SlotName = "Display" + (i + 1);
					slot.Title = "Display" + (i + 1);
					slot.Enable = true;
				}

				dgvSlots.Rows.Add(slot.SlotName, slot.Title, slot.Enable);
			}
		}

		private void ApplyCountToGrid()
		{
			int count;

			count = Convert.ToInt32(numCount.Value);

			if (_config == null)
			{
				_config = DisplayLayoutStore.CreateDefault();
			}

			CaptureRowsIntoConfig();
			_config.DisplayCount = count;

			while (_config.Displays.Count < count)
			{
				int index = _config.Displays.Count + 1;
				_config.Displays.Add(new DisplaySlotConfig
				{
					SlotName = "Display" + index,
					Title = "Display" + index,
					Enable = true
				});
			}

			LoadSlotsToGrid();
		}

		private void CaptureRowsIntoConfig()
		{
			if (_config == null || dgvSlots == null || dgvSlots.Rows.Count == 0)
			{
				return;
			}

			for (int i = 0; i < dgvSlots.Rows.Count; i++)
			{
				DataGridViewRow row = dgvSlots.Rows[i];
				if (row.IsNewRow) continue;
				while (_config.Displays.Count <= i)
				{
					_config.Displays.Add(new DisplaySlotConfig());
				}
				_config.Displays[i].SlotName = Convert.ToString(row.Cells["SlotName"].Value);
				_config.Displays[i].Title = Convert.ToString(row.Cells["Title"].Value);
				object value = row.Cells["Enable"].Value;
				_config.Displays[i].Enable = value == null || Convert.ToBoolean(value);
			}
		}

		private void SaveConfigFromUi()
		{
			if (_config == null)
			{
				_config = new DisplayLayoutConfig();
			}

			int count;

			count = Convert.ToInt32(numCount.Value);

			_config.DisplayCount = count;
			_config.LayoutMode = "AutoGrid";

			_config.Displays.Clear();

			foreach (DataGridViewRow row in dgvSlots.Rows)
			{
				if (row.IsNewRow)
				{
					continue;
				}

				DisplaySlotConfig slot = new DisplaySlotConfig();
				slot.SlotName = Convert.ToString(row.Cells["SlotName"].Value);
				slot.Title = Convert.ToString(row.Cells["Title"].Value);

				object enableValue = row.Cells["Enable"].Value;
				slot.Enable = enableValue == null || Convert.ToBoolean(enableValue);

				_config.Displays.Add(slot);
			}

			DisplayLayoutStore.Save(_config);

			MessageBox.Show("Display layout saved.", "Display Layout", MessageBoxButtons.OK, MessageBoxIcon.Information);
		}

		private Label CreateLabel(string text, int x, int y, int w, int h)
		{
			Label lbl = new Label();
			lbl.Text = text;
			lbl.Left = x;
			lbl.Top = y;
			lbl.Width = w;
			lbl.Height = h;
			lbl.ForeColor = _text;
			lbl.BackColor = Color.Transparent;
			lbl.TextAlign = ContentAlignment.MiddleLeft;
			lbl.Font = new Font("Microsoft YaHei UI", 9F, FontStyle.Bold);
			return lbl;
		}

		private void SetDoubleBuffered(Control control)
		{
			PropertyInfo property = typeof(Control).GetProperty("DoubleBuffered", BindingFlags.Instance | BindingFlags.NonPublic);
			if (property != null)
			{
				property.SetValue(control, true, null);
			}
		}

		private Button CreateButton(string text, int x, int y, int w, int h)
		{
			Button btn = new Button();
			btn.Text = text;
			btn.Left = x;
			btn.Top = y;
			btn.Width = w;
			btn.Height = h;
			btn.FlatStyle = FlatStyle.Flat;
			btn.FlatAppearance.BorderColor = _accent;
			btn.BackColor = _panel2;
			btn.ForeColor = _text;
			btn.Font = new Font("Microsoft YaHei UI", 9F, FontStyle.Bold);
			return btn;
		}
	}
}
