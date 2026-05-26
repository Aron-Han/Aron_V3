using System;
using System.Drawing;
using System.Windows.Forms;

namespace Aron_V3
{
	public class DataDisplayControl : UserControl, ILocalizable
	{
		private readonly Color _back = Color.FromArgb(2, 10, 20);
		private readonly Color _panel = Color.FromArgb(3, 14, 27);
		private readonly Color _header = Color.FromArgb(5, 18, 34);
		private readonly Color _accent = Color.FromArgb(0, 150, 220);
		private DataGridView _grid;
		private Button _add;
		private Button _delete;
		private Button _save;

		public DataDisplayControl()
		{
			Dock = DockStyle.Fill;
			BackColor = _back;
			DoubleBuffered = true;
			BuildUi();
			LoadConfig();
		}

		public void ApplyLanguage(bool isEnglish)
		{
			_grid.Columns["GroupName"].HeaderText = isEnglish ? "Group / Camera" : "分组 / 相机";
			_grid.Columns["ItemName"].HeaderText = isEnglish ? "Item" : "项目名称";
			_grid.Columns["GlobalVariableName"].HeaderText = isEnglish ? "Global Variable" : "关联全局变量";
			_add.Text = isEnglish ? "+ Add" : "+ 新增";
			_delete.Text = isEnglish ? "Delete" : "删除选中";
			_save.Text = isEnglish ? "Save" : "保存配置";
		}

		private void BuildUi()
		{
			TableLayoutPanel root = new TableLayoutPanel();
			root.Dock = DockStyle.Fill;
			root.Padding = new Padding(10);
			root.RowCount = 2;
			root.ColumnCount = 1;
			root.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
			root.RowStyles.Add(new RowStyle(SizeType.Absolute, 52F));

			_grid = new DataGridView();
			_grid.Dock = DockStyle.Fill;
			_grid.BackgroundColor = _back;
			_grid.BorderStyle = BorderStyle.FixedSingle;
			_grid.AllowUserToAddRows = false;
			_grid.RowHeadersVisible = false;
			_grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
			_grid.EnableHeadersVisualStyles = false;
			_grid.ColumnHeadersDefaultCellStyle.BackColor = _header;
			_grid.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
			_grid.DefaultCellStyle.BackColor = _back;
			_grid.DefaultCellStyle.ForeColor = Color.White;
			_grid.DefaultCellStyle.SelectionBackColor = Color.FromArgb(0, 120, 190);
			_grid.GridColor = Color.FromArgb(38, 62, 86);
			_grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "GroupName", HeaderText = "分组 / 相机", FillWeight = 30 });
			_grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "ItemName", HeaderText = "项目名称", FillWeight = 40 });
			_grid.Columns.Add(GlobalVariableBindingUi.CreateButtonColumn("GlobalVariableName", "关联全局变量", 200));
			_grid.CellContentClick += Grid_CellContentClick;

			Panel buttons = new Panel();
			buttons.Dock = DockStyle.Fill;
			buttons.BackColor = _panel;
			_add = CreateButton("+ 新增", 0);
			_delete = CreateButton("删除选中", 120);
			_save = CreateButton("保存配置", 240);
			_save.BackColor = Color.FromArgb(0, 95, 210);
			_add.Click += delegate { AddRow(); };
			_delete.Click += delegate { DeleteSelected(); };
			_save.Click += delegate { SaveConfig(); };
			buttons.Controls.Add(_add);
			buttons.Controls.Add(_delete);
			buttons.Controls.Add(_save);

			root.Controls.Add(_grid, 0, 0);
			root.Controls.Add(buttons, 0, 1);
			Controls.Add(root);
		}

		private Button CreateButton(string text, int left)
		{
			Button button = new Button();
			button.Text = text;
			button.Left = left;
			button.Top = 10;
			button.Width = 110;
			button.Height = 32;
			button.FlatStyle = FlatStyle.Flat;
			button.FlatAppearance.BorderColor = _accent;
			button.BackColor = _header;
			button.ForeColor = Color.White;
			return button;
		}

		private void LoadConfig()
		{
			_grid.Rows.Clear();
			foreach (DataDisplayItem item in DataDisplayStore.LoadOrCreateDefault().Items)
			{
				int index = _grid.Rows.Add(item.GroupName, item.ItemName, GlobalVariableBindingUi.SelectText);
				GlobalVariableBindingUi.SetCellValue(_grid.Rows[index], "GlobalVariableName", item.GlobalVariableName);
			}
		}

		private void AddRow()
		{
			_grid.Rows.Add(string.Empty, string.Empty, GlobalVariableBindingUi.SelectText);
		}

		private void DeleteSelected()
		{
			foreach (DataGridViewRow row in _grid.SelectedRows)
			{
				if (!row.IsNewRow) _grid.Rows.Remove(row);
			}
		}

		private void Grid_CellContentClick(object sender, DataGridViewCellEventArgs e)
		{
			if (e.RowIndex >= 0 && e.ColumnIndex >= 0 &&
				_grid.Columns[e.ColumnIndex].Name == "GlobalVariableName")
			{
				GlobalVariableBindingUi.SelectForCell(this, _grid.Rows[e.RowIndex], "GlobalVariableName");
			}
		}

		private void SaveConfig()
		{
			DataDisplayConfig config = new DataDisplayConfig();
			foreach (DataGridViewRow row in _grid.Rows)
			{
				if (row.IsNewRow) continue;
				config.Items.Add(new DataDisplayItem
				{
					GroupName = Convert.ToString(row.Cells["GroupName"].Value),
					ItemName = Convert.ToString(row.Cells["ItemName"].Value),
					GlobalVariableName = GlobalVariableBindingUi.GetCellValue(row, "GlobalVariableName")
				});
			}
			DataDisplayStore.Save(config);
			MessageBox.Show("Data display configuration saved.", "Data Display", MessageBoxButtons.OK, MessageBoxIcon.Information);
		}
	}
}
