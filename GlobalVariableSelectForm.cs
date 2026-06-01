using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace Aron_V3
{
	public class GlobalVariableSelectForm : Form
	{
		private readonly TextBox _txtSearch;
		private readonly DataGridView _grid;
		private readonly Button _btnOk;
		private readonly Button _btnClear;
		private readonly Button _btnCancel;
		private readonly string _initialVariableName;
		private readonly bool _isEnglish;

		public string SelectedVariableName { get; private set; }

		public GlobalVariableSelectForm(string selectedVariableName)
		{
			_isEnglish = LanguagePreferenceStore.LoadIsEnglish();
			_initialVariableName = selectedVariableName ?? string.Empty;
			SelectedVariableName = _initialVariableName;

			Text = T("选择全局变量", "Select Global Variable");
			StartPosition = FormStartPosition.CenterParent;
			Size = new Size(820, 480);
			MinimumSize = new Size(660, 360);
			BackColor = Color.FromArgb(2, 10, 20);
			ForeColor = Color.White;
			Font = new Font("Microsoft YaHei UI", 9F);

			TableLayoutPanel root = new TableLayoutPanel
			{
				Dock = DockStyle.Fill,
				Padding = new Padding(14),
				ColumnCount = 1,
				RowCount = 3,
				BackColor = BackColor
			};
			root.RowStyles.Add(new RowStyle(SizeType.Absolute, 44F));
			root.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
			root.RowStyles.Add(new RowStyle(SizeType.Absolute, 50F));

			Panel searchPanel = new Panel { Dock = DockStyle.Fill, BackColor = BackColor };
			Label lblSearch = new Label
			{
				Text = T("关键字", "Keyword"),
				AutoSize = true,
				ForeColor = Color.White,
				Location = new Point(0, 10)
			};
			_txtSearch = new TextBox
			{
				Location = new Point(70, 6),
				Width = 360,
				BackColor = Color.FromArgb(8, 22, 38),
				ForeColor = Color.White,
				BorderStyle = BorderStyle.FixedSingle
			};
			_txtSearch.TextChanged += delegate { LoadVariables(); };
			searchPanel.Controls.Add(lblSearch);
			searchPanel.Controls.Add(_txtSearch);

			_grid = new BufferedDataGridView
			{
				Dock = DockStyle.Fill,
				AllowUserToAddRows = false,
				AllowUserToDeleteRows = false,
				ReadOnly = true,
				RowHeadersVisible = false,
				SelectionMode = DataGridViewSelectionMode.FullRowSelect,
				MultiSelect = false,
				AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
				BackgroundColor = BackColor,
				GridColor = Color.FromArgb(38, 62, 86),
				BorderStyle = BorderStyle.FixedSingle,
				EnableHeadersVisualStyles = false
			};
			_grid.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(8, 28, 48);
			_grid.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
			_grid.ColumnHeadersDefaultCellStyle.Font = new Font("Microsoft YaHei UI", 9F, FontStyle.Bold);
			_grid.DefaultCellStyle.BackColor = BackColor;
			_grid.DefaultCellStyle.ForeColor = Color.White;
			_grid.DefaultCellStyle.SelectionBackColor = Color.FromArgb(0, 120, 200);
			_grid.DefaultCellStyle.SelectionForeColor = Color.White;
			_grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Name", HeaderText = T("名称", "Name"), FillWeight = 130 });
			_grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "DataType", HeaderText = T("类型", "Type"), FillWeight = 80 });
			_grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "CurrentValue", HeaderText = T("当前值", "Current Value"), FillWeight = 120 });
			_grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Mark", HeaderText = T("备注", "Remark"), FillWeight = 150 });
			_grid.Columns.Add(new DataGridViewCheckBoxColumn { Name = "RememberValue", HeaderText = T("记忆", "Remember"), FillWeight = 55 });
			_grid.CellDoubleClick += delegate(object sender, DataGridViewCellEventArgs e)
			{
				if (e.RowIndex >= 0)
				{
					AcceptSelection();
				}
			};

			FlowLayoutPanel buttons = new FlowLayoutPanel
			{
				Dock = DockStyle.Fill,
				FlowDirection = FlowDirection.RightToLeft,
				Padding = new Padding(0, 10, 0, 0),
				BackColor = BackColor
			};
			_btnOk = CreateButton(T("确定", "OK"), true);
			_btnClear = CreateButton(T("清除关联", "Clear"), false);
			_btnCancel = CreateButton(T("取消", "Cancel"), false);
			_btnOk.Click += delegate { AcceptSelection(); };
			_btnClear.Click += delegate
			{
				SelectedVariableName = string.Empty;
				DialogResult = DialogResult.OK;
				Close();
			};
			_btnCancel.DialogResult = DialogResult.Cancel;
			buttons.Controls.Add(_btnOk);
			buttons.Controls.Add(_btnCancel);
			buttons.Controls.Add(_btnClear);

			root.Controls.Add(searchPanel, 0, 0);
			root.Controls.Add(_grid, 0, 1);
			root.Controls.Add(buttons, 0, 2);
			Controls.Add(root);
			AcceptButton = _btnOk;
			CancelButton = _btnCancel;

			LoadVariables();
		}

		private void LoadVariables()
		{
			string keyword = (_txtSearch.Text ?? string.Empty).Trim();
			GlobalVariableConfig config = GlobalVariableStore.LoadForEditing();
			_grid.Rows.Clear();
			foreach (GlobalVariableItem item in config.Variables.Where(item =>
				item != null &&
				(string.IsNullOrWhiteSpace(keyword) ||
				 item.Name.IndexOf(keyword, StringComparison.OrdinalIgnoreCase) >= 0 ||
				 item.DataType.ToString().IndexOf(keyword, StringComparison.OrdinalIgnoreCase) >= 0 ||
				 (item.Mark ?? string.Empty).IndexOf(keyword, StringComparison.OrdinalIgnoreCase) >= 0)))
			{
				int index = _grid.Rows.Add(item.Name, item.DataType.ToString().ToLowerInvariant(), item.CurrentValue, item.Mark, item.RememberValue);
				if (string.Equals(item.Name, _initialVariableName, StringComparison.OrdinalIgnoreCase))
				{
					_grid.Rows[index].Selected = true;
					_grid.CurrentCell = _grid.Rows[index].Cells["Name"];
				}
			}
		}

		private Button CreateButton(string text, bool primary)
		{
			return new Button
			{
				Text = text,
				Size = new Size(100, 32),
				Margin = new Padding(8, 0, 0, 0),
				FlatStyle = FlatStyle.Flat,
				BackColor = primary ? Color.FromArgb(0, 95, 220) : Color.FromArgb(3, 14, 27),
				ForeColor = Color.White
			};
		}

		private void AcceptSelection()
		{
			if (_grid.CurrentRow == null)
			{
				return;
			}

			SelectedVariableName = Convert.ToString(_grid.CurrentRow.Cells["Name"].Value);
			DialogResult = DialogResult.OK;
			Close();
		}

		private string T(string chinese, string english)
		{
			return _isEnglish ? english : chinese;
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

	public static class GlobalVariableBindingUi
	{
		public static string SelectText
		{
			get { return LanguagePreferenceStore.LoadIsEnglish() ? "Select..." : "选择..."; }
		}

		public static DataGridViewButtonColumn CreateButtonColumn(string name, string header, int width)
		{
			return new DataGridViewButtonColumn
			{
				Name = name,
				HeaderText = header,
				Width = width,
				Text = SelectText,
				UseColumnTextForButtonValue = false,
				FlatStyle = FlatStyle.Flat
			};
		}

		public static void SetCellValue(DataGridViewRow row, string columnName, string variableName)
		{
			if (row == null || row.DataGridView == null || !row.DataGridView.Columns.Contains(columnName))
			{
				return;
			}

			string name = variableName == null ? string.Empty : variableName.Trim();
			row.Cells[columnName].Tag = name;
			row.Cells[columnName].Value = string.IsNullOrWhiteSpace(name) ? SelectText : name;
		}

		public static string GetCellValue(DataGridViewRow row, string columnName)
		{
			if (row == null || row.DataGridView == null || !row.DataGridView.Columns.Contains(columnName))
			{
				return string.Empty;
			}

			string taggedValue = Convert.ToString(row.Cells[columnName].Tag);
			if (!string.IsNullOrWhiteSpace(taggedValue))
			{
				return taggedValue.Trim();
			}

			string value = Convert.ToString(row.Cells[columnName].Value).Trim();
			return IsSelectPlaceholder(value) ? string.Empty : value;
		}

		public static bool SelectForCell(IWin32Window owner, DataGridViewRow row, string columnName)
		{
			using (GlobalVariableSelectForm form = new GlobalVariableSelectForm(GetCellValue(row, columnName)))
			{
				if (form.ShowDialog(owner) != DialogResult.OK)
				{
					return false;
				}

				SetCellValue(row, columnName, form.SelectedVariableName);
				return true;
			}
		}

		private static bool IsSelectPlaceholder(string value)
		{
			return string.Equals(value, "选择...", StringComparison.OrdinalIgnoreCase) ||
				string.Equals(value, "Select...", StringComparison.OrdinalIgnoreCase);
		}
	}
}
