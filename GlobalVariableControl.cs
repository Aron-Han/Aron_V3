using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace Aron_V3
{
	public class GlobalVariableControl : UserControl, ILocalizable
	{
		private readonly Color _back = Color.FromArgb(2, 10, 20);
		private readonly Color _panel = Color.FromArgb(3, 14, 27);
		private readonly Color _border = Color.FromArgb(38, 62, 86);
		private DataGridView _grid;
		private TextBox _txtFilter;
		private Label _lblFilter;
		private Button _btnAdd;
		private Button _btnDelete;
		private Button _btnSave;
		private bool _isEnglish;
		private bool _loading;
		private List<GlobalVariableItem> _allVariables = new List<GlobalVariableItem>();
		private HashSet<string> _loadedVariableNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
		private Dictionary<string, string> _pendingRenameMap = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

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
				RowCount = 3,
				ColumnCount = 1,
				BackColor = _back
			};
			root.RowStyles.Add(new RowStyle(SizeType.Absolute, 44F));
			root.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
			root.RowStyles.Add(new RowStyle(SizeType.Absolute, 58F));
			SetDoubleBuffered(root);

			Panel filterPanel = new Panel
			{
				Dock = DockStyle.Fill,
				BackColor = _panel
			};
			SetDoubleBuffered(filterPanel);

			_lblFilter = new Label
			{
				Text = "Filter",
				AutoSize = false,
				Location = new Point(8, 8),
				Size = new Size(70, 26),
				TextAlign = ContentAlignment.MiddleLeft,
				ForeColor = Color.White,
				BackColor = Color.Transparent,
				Font = new Font("Microsoft YaHei UI", 9F, FontStyle.Bold)
			};

			_txtFilter = new TextBox
			{
				Location = new Point(84, 8),
				Size = new Size(260, 26),
				BackColor = _back,
				ForeColor = Color.White,
				BorderStyle = BorderStyle.FixedSingle
			};
			_txtFilter.TextChanged += FilterTextChanged;
			filterPanel.Controls.Add(_lblFilter);
			filterPanel.Controls.Add(_txtFilter);

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
			root.Controls.Add(filterPanel, 0, 0);
			root.Controls.Add(_grid, 0, 1);
			root.Controls.Add(buttons, 0, 2);
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
			_loading = true;
			try
			{
				_allVariables = GlobalVariableStore.LoadForEditing().Variables
					.Select(CloneVariable)
					.OrderBy(x => x.Name, StringComparer.OrdinalIgnoreCase)
					.ThenBy(x => x.Name, StringComparer.Ordinal)
					.ToList();
				_loadedVariableNames = new HashSet<string>(
					_allVariables
						.Where(x => x != null && !string.IsNullOrWhiteSpace(x.Name))
						.Select(x => x.Name),
					StringComparer.OrdinalIgnoreCase);
				_pendingRenameMap = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
				BindFilteredVariables();
			}
			finally
			{
				_loading = false;
			}
		}

		private void BindFilteredVariables()
		{
			_grid.SuspendLayout();
			try
			{
				_grid.Rows.Clear();
				foreach (GlobalVariableItem item in GetFilteredVariables())
				{
					int rowIndex = _grid.Rows.Add(item.Name, item.DataType.ToString().ToLowerInvariant(), item.CurrentValue, item.Mark, item.RememberValue);
					_grid.Rows[rowIndex].Tag = item.Name;
				}
			}
			finally
			{
				_grid.ResumeLayout();
			}
		}

		private IEnumerable<GlobalVariableItem> GetFilteredVariables()
		{
			string filter = _txtFilter == null ? string.Empty : _txtFilter.Text.Trim();
			IEnumerable<GlobalVariableItem> query = _allVariables;
			if (!string.IsNullOrWhiteSpace(filter))
			{
				query = query.Where(x =>
					x != null &&
					!string.IsNullOrWhiteSpace(x.Name) &&
					x.Name.StartsWith(filter, StringComparison.OrdinalIgnoreCase));
			}

			return query
				.OrderBy(x => x.Name, StringComparer.OrdinalIgnoreCase)
				.ThenBy(x => x.Name, StringComparer.Ordinal);
		}

		private void FilterTextChanged(object sender, EventArgs e)
		{
			if (_loading)
			{
				return;
			}

			CaptureVisibleRowsToAllVariables();
			BindFilteredVariables();
		}

		private void AddRow()
		{
			CaptureVisibleRowsToAllVariables();

			if (_txtFilter != null && _txtFilter.Text.Length > 0)
			{
				_txtFilter.Text = string.Empty;
			}

			string name = GetNextVariableName();
			_allVariables.Add(new GlobalVariableItem
			{
				Name = name,
				DataType = GlobalVariableDataType.String,
				CurrentValue = string.Empty,
				Mark = string.Empty,
				RememberValue = false
			});
			BindFilteredVariables();

			foreach (DataGridViewRow row in _grid.Rows)
			{
				if (string.Equals(Convert.ToString(row.Cells["Name"].Value), name, StringComparison.OrdinalIgnoreCase))
				{
					_grid.CurrentCell = row.Cells["Name"];
					row.Selected = true;
					_grid.BeginEdit(true);
					break;
				}
			}
		}

		private void DeleteSelected()
		{
			if (_grid.SelectedRows.Count > 0 && !_grid.SelectedRows[0].IsNewRow)
			{
				DataGridViewRow row = _grid.SelectedRows[0];
				string originalName = Convert.ToString(row.Tag);
				string currentName = Convert.ToString(row.Cells["Name"].Value).Trim();
				RemovePendingRename(originalName, currentName);
				_allVariables.RemoveAll(x =>
					x != null &&
					(string.Equals(x.Name, originalName, StringComparison.OrdinalIgnoreCase) ||
					 string.Equals(x.Name, currentName, StringComparison.OrdinalIgnoreCase)));
				BindFilteredVariables();
			}
		}

		private void SaveVariables()
		{
			_grid.EndEdit();
			BindingContext[_grid.DataSource ?? _grid].EndCurrentEdit();
			Dictionary<string, string> visibleRenameMap = BuildVisibleRenameMap();
			CaptureVisibleRowsToAllVariables();
			Dictionary<string, string> renameMap = new Dictionary<string, string>(_pendingRenameMap, StringComparer.OrdinalIgnoreCase);
			foreach (KeyValuePair<string, string> pair in visibleRenameMap)
			{
				renameMap[pair.Key] = pair.Value;
			}
			GlobalVariableConfig config = new GlobalVariableConfig();
			HashSet<string> names = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
			foreach (GlobalVariableItem variable in _allVariables
				.OrderBy(x => x.Name, StringComparer.OrdinalIgnoreCase)
				.ThenBy(x => x.Name, StringComparer.Ordinal))
			{
				string name = variable == null ? string.Empty : variable.Name;
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

				config.Variables.Add(new GlobalVariableItem
				{
					Name = name,
					DataType = variable.DataType,
					CurrentValue = variable.CurrentValue,
					Mark = variable.Mark,
					RememberValue = variable.RememberValue
				});
			}

			GlobalVariableStore.Save(config);
			HashSet<string> finalNames = new HashSet<string>(
				config.Variables
					.Where(x => x != null && !string.IsNullOrWhiteSpace(x.Name))
					.Select(x => x.Name),
				StringComparer.OrdinalIgnoreCase);
			HashSet<string> deletedNames = new HashSet<string>(_loadedVariableNames, StringComparer.OrdinalIgnoreCase);
			deletedNames.ExceptWith(renameMap.Keys);
			deletedNames.ExceptWith(finalNames);
			GlobalVariableReferenceUpdater.Apply(renameMap, deletedNames);
			LoadVariables();
			MessageBox.Show(_isEnglish ? "Global variables saved." : "全局变量已保存。",
				"Global Variables", MessageBoxButtons.OK, MessageBoxIcon.Information);
		}

		private Dictionary<string, string> BuildVisibleRenameMap()
		{
			Dictionary<string, string> result = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

			foreach (DataGridViewRow row in _grid.Rows)
			{
				if (row.IsNewRow)
				{
					continue;
				}

				string oldName = Convert.ToString(row.Tag).Trim();
				string newName = Convert.ToString(row.Cells["Name"].Value).Trim();
				if (string.IsNullOrWhiteSpace(oldName) ||
					string.IsNullOrWhiteSpace(newName) ||
					string.Equals(oldName, newName, StringComparison.OrdinalIgnoreCase))
				{
					continue;
				}

				result[oldName] = newName;
			}

			return result;
		}

		private void CaptureVisibleRowsToAllVariables()
		{
			foreach (DataGridViewRow row in _grid.Rows)
			{
				if (row.IsNewRow)
				{
					continue;
				}

				string name = Convert.ToString(row.Cells["Name"].Value).Trim();
				if (string.IsNullOrWhiteSpace(name))
				{
					continue;
				}

				string originalName = Convert.ToString(row.Tag);
				RegisterPendingRename(originalName, name);
				GlobalVariableItem item = _allVariables.FirstOrDefault(x =>
					x != null && string.Equals(x.Name, originalName, StringComparison.OrdinalIgnoreCase));

				if (item == null)
				{
					item = _allVariables.FirstOrDefault(x =>
						x != null && string.Equals(x.Name, name, StringComparison.OrdinalIgnoreCase));
				}

				if (item == null)
				{
					item = new GlobalVariableItem();
					_allVariables.Add(item);
				}

				GlobalVariableDataType type = GlobalVariableDataType.String;
				Enum.TryParse(Convert.ToString(row.Cells["DataType"].Value), true, out type);
				item.Name = name;
				item.DataType = type;
				item.CurrentValue = Convert.ToString(row.Cells["CurrentValue"].Value);
				item.Mark = Convert.ToString(row.Cells["Mark"].Value);
				item.RememberValue = Convert.ToBoolean(row.Cells["RememberValue"].Value ?? false);
			}
		}

		private void RegisterPendingRename(string oldName, string newName)
		{
			oldName = (oldName ?? string.Empty).Trim();
			newName = (newName ?? string.Empty).Trim();
			if (string.IsNullOrWhiteSpace(oldName) ||
				string.IsNullOrWhiteSpace(newName) ||
				string.Equals(oldName, newName, StringComparison.OrdinalIgnoreCase))
			{
				return;
			}

			string chainedOldName = null;
			foreach (KeyValuePair<string, string> pair in _pendingRenameMap)
			{
				if (string.Equals(pair.Value, oldName, StringComparison.OrdinalIgnoreCase))
				{
					chainedOldName = pair.Key;
					break;
				}
			}

			if (!string.IsNullOrWhiteSpace(chainedOldName))
			{
				if (string.Equals(chainedOldName, newName, StringComparison.OrdinalIgnoreCase))
				{
					_pendingRenameMap.Remove(chainedOldName);
				}
				else
				{
					_pendingRenameMap[chainedOldName] = newName;
				}
				return;
			}

			_pendingRenameMap[oldName] = newName;
		}

		private void RemovePendingRename(string oldName, string currentName)
		{
			oldName = (oldName ?? string.Empty).Trim();
			currentName = (currentName ?? string.Empty).Trim();
			List<string> keysToRemove = new List<string>();
			foreach (KeyValuePair<string, string> pair in _pendingRenameMap)
			{
				if (string.Equals(pair.Key, oldName, StringComparison.OrdinalIgnoreCase) ||
					string.Equals(pair.Key, currentName, StringComparison.OrdinalIgnoreCase) ||
					string.Equals(pair.Value, oldName, StringComparison.OrdinalIgnoreCase) ||
					string.Equals(pair.Value, currentName, StringComparison.OrdinalIgnoreCase))
				{
					keysToRemove.Add(pair.Key);
				}
			}

			foreach (string key in keysToRemove)
			{
				_pendingRenameMap.Remove(key);
			}
		}

		private string GetNextVariableName()
		{
			int index = _allVariables.Count + 1;
			string name;
			do
			{
				name = "Variable_" + index.ToString("00");
				index++;
			}
			while (_allVariables.Any(x => x != null && string.Equals(x.Name, name, StringComparison.OrdinalIgnoreCase)));

			return name;
		}

		private GlobalVariableItem CloneVariable(GlobalVariableItem source)
		{
			if (source == null)
			{
				return new GlobalVariableItem();
			}

			return new GlobalVariableItem
			{
				Name = source.Name,
				DataType = source.DataType,
				CurrentValue = source.CurrentValue,
				Mark = source.Mark,
				RememberValue = source.RememberValue
			};
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
