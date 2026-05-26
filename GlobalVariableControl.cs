using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace Aron_V3
{
	public class GlobalVariableControl : UserControl, ILocalizable
	{
		private readonly Color _back = Color.FromArgb(2, 10, 20);
		private readonly Color _panel = Color.FromArgb(3, 14, 27);
		private readonly Color _border = Color.FromArgb(38, 62, 86);
		private DataGridView _grid;
		private Button _btnAdd;
		private Button _btnDelete;
		private Button _btnSave;
		private bool _isEnglish;

		public GlobalVariableControl()
		{
			Dock = DockStyle.Fill;
			BackColor = _back;
			DoubleBuffered = true;
			SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.AllPaintingInWmPaint, true);
			BuildUi();
			LoadVariables();
		}

		public void ReloadVariables()
		{
			LoadVariables();
		}

		public void ApplyLanguage(bool isEnglish)
		{
			_isEnglish = isEnglish;
			_grid.Columns["Name"].HeaderText = isEnglish ? "Name" : "名称";
			_grid.Columns["DataType"].HeaderText = isEnglish ? "Type" : "类型";
			_grid.Columns["CurrentValue"].HeaderText = isEnglish ? "Current Value" : "当前值";
			_grid.Columns["Mark"].HeaderText = isEnglish ? "Mark" : "备注";
			_grid.Columns["RememberValue"].HeaderText = isEnglish ? "Remember Value" : "记忆当前值";
			_btnAdd.Text = isEnglish ? "+ Add" : "+ 新增";
			_btnDelete.Text = isEnglish ? "Delete" : "删除选中";
			_btnSave.Text = isEnglish ? "Save" : "保存配置";
		}

		private void BuildUi()
		{
			TableLayoutPanel root = new TableLayoutPanel
			{
				Dock = DockStyle.Fill,
				Padding = new Padding(16),
				RowCount = 2,
				ColumnCount = 1,
				BackColor = _back
			};
			root.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
			root.RowStyles.Add(new RowStyle(SizeType.Absolute, 58F));
			SetDoubleBuffered(root);

			_grid = new BufferedDataGridView
			{
				Dock = DockStyle.Fill,
				AllowUserToAddRows = false,
				AllowUserToDeleteRows = false,
				RowHeadersVisible = false,
				SelectionMode = DataGridViewSelectionMode.FullRowSelect,
				MultiSelect = false,
				AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
				BackgroundColor = _back,
				GridColor = _border,
				BorderStyle = BorderStyle.FixedSingle,
				EnableHeadersVisualStyles = false
			};
			_grid.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(8, 28, 48);
			_grid.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
			_grid.ColumnHeadersDefaultCellStyle.Font = new Font("Microsoft YaHei UI", 9F, FontStyle.Bold);
			_grid.DefaultCellStyle.BackColor = _back;
			_grid.DefaultCellStyle.ForeColor = Color.White;
			_grid.DefaultCellStyle.SelectionBackColor = Color.FromArgb(0, 120, 200);
			_grid.DefaultCellStyle.SelectionForeColor = Color.White;
			_grid.RowTemplate.Height = 32;
			_grid.DataError += delegate(object sender, DataGridViewDataErrorEventArgs e) { e.ThrowException = false; };
			_grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Name", HeaderText = "名称", FillWeight = 150 });

			DataGridViewComboBoxColumn typeColumn = new DataGridViewComboBoxColumn
			{
				Name = "DataType",
				HeaderText = "类型",
				FillWeight = 100,
				FlatStyle = FlatStyle.Flat
			};
			typeColumn.Items.AddRange(
				"int16",
				"int32",
				"byte",
				"bit",
				"string",
				"float",
				"double");
			_grid.Columns.Add(typeColumn);
			_grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "CurrentValue", HeaderText = "当前值", FillWeight = 150 });
			_grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Mark", HeaderText = "备注", FillWeight = 180 });
			_grid.Columns.Add(new DataGridViewCheckBoxColumn { Name = "RememberValue", HeaderText = "记忆当前值", FillWeight = 90 });

			FlowLayoutPanel buttons = new FlowLayoutPanel
			{
				Dock = DockStyle.Fill,
				FlowDirection = FlowDirection.LeftToRight,
				Padding = new Padding(0, 12, 0, 0),
				BackColor = _panel
			};
			SetDoubleBuffered(buttons);
			_btnAdd = CreateButton("+ 新增", false);
			_btnDelete = CreateButton("删除选中", false);
			_btnSave = CreateButton("保存配置", true);
			_btnAdd.Click += delegate { AddRow(); };
			_btnDelete.Click += delegate { DeleteSelected(); };
			_btnSave.Click += delegate { SaveVariables(); };
			buttons.Controls.Add(_btnAdd);
			buttons.Controls.Add(_btnDelete);
			buttons.Controls.Add(_btnSave);
			root.Controls.Add(_grid, 0, 0);
			root.Controls.Add(buttons, 0, 1);
			Controls.Add(root);
		}

		private Button CreateButton(string text, bool primary)
		{
			Button button = new Button
			{
				Text = text,
				Size = new Size(120, 34),
				Margin = new Padding(0, 0, 12, 0),
				FlatStyle = FlatStyle.Flat,
				BackColor = primary ? Color.FromArgb(0, 95, 220) : _panel,
				ForeColor = Color.White,
				Font = new Font("Microsoft YaHei UI", 9F, FontStyle.Bold)
			};
			button.FlatAppearance.BorderColor = Color.FromArgb(0, 150, 220);
			return button;
		}

		private void LoadVariables()
		{
			_grid.SuspendLayout();
			try
			{
				_grid.Rows.Clear();
				foreach (GlobalVariableItem item in GlobalVariableStore.LoadForEditing().Variables)
				{
					_grid.Rows.Add(item.Name, item.DataType.ToString().ToLowerInvariant(), item.CurrentValue, item.Mark, item.RememberValue);
				}
			}
			finally
			{
				_grid.ResumeLayout();
			}
		}

		private void AddRow()
		{
			int index = _grid.Rows.Add("Variable_" + (_grid.Rows.Count + 1).ToString("00"),
				"string", string.Empty, string.Empty, false);
			_grid.CurrentCell = _grid.Rows[index].Cells["Name"];
			_grid.BeginEdit(true);
		}

		private void DeleteSelected()
		{
			if (_grid.SelectedRows.Count > 0 && !_grid.SelectedRows[0].IsNewRow)
			{
				_grid.Rows.Remove(_grid.SelectedRows[0]);
			}
		}

		private void SaveVariables()
		{
			GlobalVariableConfig config = new GlobalVariableConfig();
			HashSet<string> names = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
			foreach (DataGridViewRow row in _grid.Rows)
			{
				string name = Convert.ToString(row.Cells["Name"].Value).Trim();
				if (string.IsNullOrWhiteSpace(name))
				{
					continue;
				}
				if (!names.Add(name))
				{
					MessageBox.Show(_isEnglish ? "Variable names must be unique." : "全局变量名称不能重复。",
						"Global Variables", MessageBoxButtons.OK, MessageBoxIcon.Warning);
					return;
				}

				GlobalVariableDataType type = GlobalVariableDataType.String;
				Enum.TryParse(Convert.ToString(row.Cells["DataType"].Value), true, out type);
				config.Variables.Add(new GlobalVariableItem
				{
					Name = name,
					DataType = type,
					CurrentValue = Convert.ToString(row.Cells["CurrentValue"].Value),
					Mark = Convert.ToString(row.Cells["Mark"].Value),
					RememberValue = Convert.ToBoolean(row.Cells["RememberValue"].Value ?? false)
				});
			}

			GlobalVariableStore.Save(config);
			LoadVariables();
			MessageBox.Show(_isEnglish ? "Global variables saved." : "全局变量已保存。",
				"Global Variables", MessageBoxButtons.OK, MessageBoxIcon.Information);
		}

		private void SetDoubleBuffered(Control control)
		{
			System.Reflection.PropertyInfo property = typeof(Control).GetProperty(
				"DoubleBuffered",
				System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
			if (property != null)
			{
				property.SetValue(control, true, null);
			}
		}

		private class BufferedDataGridView : DataGridView
		{
			public BufferedDataGridView()
			{
				DoubleBuffered = true;
				SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.AllPaintingInWmPaint, true);
			}
		}
	}
}
