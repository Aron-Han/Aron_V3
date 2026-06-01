using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Aron_V3
{
	public class DatabaseManagementControl : UserControl, ILocalizable
	{
		private readonly Color _back = Color.FromArgb(2, 10, 20);
		private readonly Color _panel = Color.FromArgb(3, 14, 27);
		private readonly Color _panel2 = Color.FromArgb(5, 18, 34);
		private readonly Color _border = Color.FromArgb(38, 62, 86);
		private readonly Color _accent = Color.FromArgb(0, 150, 220);
		private readonly Color _selected = Color.FromArgb(0, 95, 170);
		private readonly Color _text = Color.FromArgb(220, 235, 245);
		private readonly Color _muted = Color.FromArgb(130, 155, 175);

		private bool _isEnglish;
		private bool _loading;
		private bool _querying;
		private DatabaseQueryResult _lastQueryResult = new DatabaseQueryResult();

		private TableLayoutPanel _root;
		private Panel _menuPanel;
		private Panel _contentPanel;
		private Button _btnSettings;
		private Button _btnQuery;
		private Control _currentPage;

		private Panel _settingsPage;
		private TextBox _txtDatabasePath;
		private NumericUpDown _numRetentionDays;
		private TextBox _txtTableName;
		private DataGridView _fieldsGrid;
		private Label _lblSettingsTitle;
		private Label _lblStorageTitle;
		private Label _lblFieldsTitle;
		private Label _lblFieldsHint;
		private Label _lblDatabasePath;
		private Label _lblRetentionDays;
		private Label _lblTableName;
		private Button _btnTestStorage;
		private Button _btnSaveStorage;
		private Button _btnAddField;
		private Button _btnDeleteField;
		private Button _btnMoveFieldUp;
		private Button _btnMoveFieldDown;
		private Button _btnSaveFields;

		private Panel _queryPage;
		private Label _lblQueryTitle;
		private Label _lblQueryStart;
		private Label _lblQueryEnd;
		private Label _lblQueryKeyword;
		private DateTimePicker _dtpStart;
		private DateTimePicker _dtpEnd;
		private TextBox _txtKeyword;
		private Button _btnSearch;
		private Button _btnExport;
		private Button _btnOpenFolder;
		private DataGridView _queryGrid;

		public DatabaseManagementControl()
		{
			Dock = DockStyle.Fill;
			BackColor = _back;
			DoubleBuffered = true;
			SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.AllPaintingInWmPaint | ControlStyles.ResizeRedraw, true);

			BuildUi();
			ShowSettingsPage();
		}

		public void ApplyLanguage(bool isEnglish)
		{
			_isEnglish = isEnglish;

			if (_btnSettings != null)
			{
				_btnSettings.Text = isEnglish ? "▣  Database Settings" : "▣  数据库设置";
			}

			if (_btnQuery != null)
			{
				_btnQuery.Text = isEnglish ? "▣  Database Query" : "▣  数据库查询";
			}

			if (_lblSettingsTitle != null)
			{
				_lblSettingsTitle.Text = isEnglish ? "Database Settings" : "数据库设置";
			}

			if (_lblStorageTitle != null)
			{
				_lblStorageTitle.Text = isEnglish ? "Storage" : "存储配置";
			}

			if (_lblFieldsTitle != null)
			{
				_lblFieldsTitle.Text = isEnglish ? "Input Definitions" : "数据库输入参数定义";
			}

			if (_lblFieldsHint != null)
			{
				_lblFieldsHint.Text = isEnglish
					? "Database Step will use these input definitions and write mapped global variables later."
					: "Step 中的 Database 功能块后续会读取这里的字段定义，并把全局变量写入对应参数。";
			}

			if (_lblDatabasePath != null)
			{
				_lblDatabasePath.Text = isEnglish ? "Database Path" : "数据库路径";
			}

			if (_lblRetentionDays != null)
			{
				_lblRetentionDays.Text = isEnglish ? "Retention Days" : "保留天数";
			}

			if (_lblTableName != null)
			{
				_lblTableName.Text = isEnglish ? "Table Name" : "表名称";
			}

			if (_btnTestStorage != null)
			{
				_btnTestStorage.Text = isEnglish ? "Test" : "测试连接";
			}

			if (_btnSaveStorage != null)
			{
				_btnSaveStorage.Text = isEnglish ? "Save" : "保存";
			}

			if (_btnAddField != null)
			{
				_btnAddField.Text = isEnglish ? "+ Add Field" : "+ 新增参数";
			}

			if (_btnDeleteField != null)
			{
				_btnDeleteField.Text = isEnglish ? "Delete" : "删除选中";
			}

			if (_btnMoveFieldUp != null)
			{
				_btnMoveFieldUp.Text = isEnglish ? "Move Up" : "上移选中";
			}

			if (_btnMoveFieldDown != null)
			{
				_btnMoveFieldDown.Text = isEnglish ? "Move Down" : "下移选中";
			}

			if (_btnSaveFields != null)
			{
				_btnSaveFields.Text = isEnglish ? "Save Definition" : "保存定义";
			}

			if (_lblQueryTitle != null)
			{
				_lblQueryTitle.Text = isEnglish ? "Database Query" : "数据库查询";
			}

			if (_lblQueryStart != null)
			{
				_lblQueryStart.Text = isEnglish ? "Start" : "开始时间";
			}

			if (_lblQueryEnd != null)
			{
				_lblQueryEnd.Text = isEnglish ? "End" : "结束时间";
			}

			if (_lblQueryKeyword != null)
			{
				_lblQueryKeyword.Text = isEnglish ? "Field Keyword" : "数据名称关键字";
			}

			if (_btnSearch != null)
			{
				_btnSearch.Text = isEnglish ? "Query" : "查询";
			}

			if (_btnExport != null)
			{
				_btnExport.Text = isEnglish ? "Export" : "导出数据";
			}

			if (_btnOpenFolder != null)
			{
				_btnOpenFolder.Text = isEnglish ? "Open Folder" : "打开数据库文件夹";
			}

			ApplyFieldGridLanguage();
			ApplyQueryGridLanguage();
		}

		private void BuildUi()
		{
			Controls.Clear();

			_root = new TableLayoutPanel();
			_root.Dock = DockStyle.Fill;
			_root.BackColor = _back;
			_root.Padding = new Padding(10);
			_root.RowCount = 1;
			_root.ColumnCount = 2;
			_root.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 230F));
			_root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
			SetDoubleBuffered(_root);

			_menuPanel = new Panel();
			_menuPanel.Dock = DockStyle.Fill;
			_menuPanel.BackColor = _panel;
			_menuPanel.Padding = new Padding(10);
			SetDoubleBuffered(_menuPanel);

			_contentPanel = new Panel();
			_contentPanel.Dock = DockStyle.Fill;
			_contentPanel.BackColor = _panel;
			_contentPanel.Padding = new Padding(16);
			SetDoubleBuffered(_contentPanel);

			BuildMenu();

			_root.Controls.Add(_menuPanel, 0, 0);
			_root.Controls.Add(_contentPanel, 1, 0);
			Controls.Add(_root);
		}

		private void BuildMenu()
		{
			_btnSettings = CreateMenuButton("▣  数据库设置");
			_btnQuery = CreateMenuButton("▣  数据库查询");

			_btnSettings.Top = 12;
			_btnQuery.Top = 76;
			_btnSettings.Click += delegate { ShowSettingsPage(); };
			_btnQuery.Click += delegate { ShowQueryPage(); };

			_menuPanel.Controls.Add(_btnSettings);
			_menuPanel.Controls.Add(_btnQuery);
		}

		private Button CreateMenuButton(string text)
		{
			Button button = new Button();
			button.Left = 0;
			button.Width = _menuPanel.Width - 20;
			button.Height = 52;
			button.Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top;
			button.Text = text;
			button.TextAlign = ContentAlignment.MiddleCenter;
			button.FlatStyle = FlatStyle.Flat;
			button.FlatAppearance.BorderColor = _border;
			button.FlatAppearance.BorderSize = 1;
			button.FlatAppearance.MouseOverBackColor = Color.FromArgb(8, 34, 56);
			button.FlatAppearance.MouseDownBackColor = Color.FromArgb(0, 80, 135);
			button.BackColor = _panel2;
			button.ForeColor = _text;
			button.Font = new Font("Microsoft YaHei UI", 9F, FontStyle.Bold);
			button.Cursor = Cursors.Hand;
			return button;
		}

		private void ShowSettingsPage()
		{
			if (_settingsPage == null || _settingsPage.IsDisposed)
			{
				_settingsPage = BuildSettingsPage();
				LoadConfigToSettingsPage();
			}

			ShowPage(_settingsPage);
			SetSelectedButton(_btnSettings);
		}

		private void ShowQueryPage()
		{
			if (_queryPage == null || _queryPage.IsDisposed)
			{
				_queryPage = BuildQueryPage();
			}

			ShowPage(_queryPage);
			SetSelectedButton(_btnQuery);
			StartQueryRecordsAsync();
		}

		private void ShowPage(Control page)
		{
			if (page == null)
			{
				return;
			}

			_contentPanel.SuspendLayout();
			try
			{
				if (page.Parent != _contentPanel)
				{
					page.Dock = DockStyle.Fill;
					page.Visible = false;
					_contentPanel.Controls.Add(page);
				}

				foreach (Control child in _contentPanel.Controls)
				{
					child.Visible = false;
				}

				page.Visible = true;
				page.BringToFront();
				_currentPage = page;
			}
			finally
			{
				_contentPanel.ResumeLayout(true);
			}
		}

		private void SetSelectedButton(Button selectedButton)
		{
			Button[] buttons = new Button[] { _btnSettings, _btnQuery };
			foreach (Button button in buttons)
			{
				if (button == null)
				{
					continue;
				}

				bool selected = object.ReferenceEquals(button, selectedButton);
				button.BackColor = selected ? _selected : _panel2;
				button.FlatAppearance.BorderColor = selected ? _accent : _border;
				button.ForeColor = Color.White;
			}
		}

		private Panel BuildSettingsPage()
		{
			Panel page = new Panel();
			page.BackColor = _panel;
			page.Dock = DockStyle.Fill;
			SetDoubleBuffered(page);

			TableLayoutPanel layout = new TableLayoutPanel();
			layout.Dock = DockStyle.Fill;
			layout.BackColor = _panel;
			layout.RowCount = 3;
			layout.ColumnCount = 1;
			layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 42F));
			layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 108F));
			layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
			SetDoubleBuffered(layout);

			_lblSettingsTitle = CreateTitleLabel("数据库设置");

			Panel storagePanel = CreateBorderPanel();
			BuildStoragePanel(storagePanel);

			Panel fieldsPanel = CreateBorderPanel();
			BuildFieldsPanel(fieldsPanel);

			layout.Controls.Add(_lblSettingsTitle, 0, 0);
			layout.Controls.Add(storagePanel, 0, 1);
			layout.Controls.Add(fieldsPanel, 0, 2);
			page.Controls.Add(layout);
			return page;
		}

		private void BuildStoragePanel(Panel panel)
		{
			_lblStorageTitle = CreateSectionLabel("存储配置", 16, 8, 180, 26);
			_lblDatabasePath = CreateSmallLabel("数据库路径", 16, 42, 120, 24);
			_txtDatabasePath = CreateTextBox(150, 42, 330);
			_lblRetentionDays = CreateSmallLabel("保留天数", 504, 42, 100, 24);
			_numRetentionDays = new NumericUpDown();
			_numRetentionDays.Location = new Point(620, 42);
			_numRetentionDays.Size = new Size(110, 24);
			_numRetentionDays.Minimum = 1;
			_numRetentionDays.Maximum = 36500;
			_numRetentionDays.BackColor = _back;
			_numRetentionDays.ForeColor = Color.White;
			_numRetentionDays.BorderStyle = BorderStyle.FixedSingle;

			_lblTableName = CreateSmallLabel("表名称", 756, 42, 80, 24);
			_txtTableName = CreateTextBox(850, 42, 180);
			_btnTestStorage = CreateButton("测试连接", false);
			_btnSaveStorage = CreateButton("保存", true);
			_btnTestStorage.SetBounds(996, 40, 104, 30);
			_btnSaveStorage.SetBounds(1116, 40, 90, 30);
			_btnTestStorage.Anchor = AnchorStyles.Top | AnchorStyles.Right;
			_btnSaveStorage.Anchor = AnchorStyles.Top | AnchorStyles.Right;
			_btnTestStorage.Click += delegate { TestStorage(); };
			_btnSaveStorage.Click += delegate { SaveConfigFromSettingsPage(); };

			panel.Resize += delegate
			{
				_btnSaveStorage.Left = panel.ClientSize.Width - _btnSaveStorage.Width - 18;
				_btnTestStorage.Left = _btnSaveStorage.Left - _btnTestStorage.Width - 12;
			};

			panel.Controls.Add(_lblStorageTitle);
			panel.Controls.Add(_lblDatabasePath);
			panel.Controls.Add(_txtDatabasePath);
			panel.Controls.Add(_lblRetentionDays);
			panel.Controls.Add(_numRetentionDays);
			panel.Controls.Add(_lblTableName);
			panel.Controls.Add(_txtTableName);
			panel.Controls.Add(_btnTestStorage);
			panel.Controls.Add(_btnSaveStorage);
		}

		private void BuildFieldsPanel(Panel panel)
		{
			TableLayoutPanel layout = new TableLayoutPanel();
			layout.Dock = DockStyle.Fill;
			layout.BackColor = _panel2;
			layout.Padding = new Padding(14);
			layout.RowCount = 3;
			layout.ColumnCount = 1;
			layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 54F));
			layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
			layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 56F));
			SetDoubleBuffered(layout);

			Panel titlePanel = new Panel();
			titlePanel.Dock = DockStyle.Fill;
			titlePanel.BackColor = _panel2;
			_lblFieldsTitle = CreateSectionLabel("数据库输入参数定义", 0, 0, 260, 28);
			_lblFieldsHint = CreateSmallLabel(
				"Step 中的 Database 功能块后续会读取这里的字段定义，并把全局变量写入对应参数。",
				0,
				28,
				820,
				22);
			_lblFieldsHint.ForeColor = _muted;
			titlePanel.Controls.Add(_lblFieldsTitle);
			titlePanel.Controls.Add(_lblFieldsHint);

			_fieldsGrid = CreateGrid();
			_fieldsGrid.AllowUserToAddRows = false;
			_fieldsGrid.MultiSelect = false;
			_fieldsGrid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
			_fieldsGrid.DataError += delegate(object sender, DataGridViewDataErrorEventArgs e) { e.ThrowException = false; };
			BuildFieldGridColumns();

			FlowLayoutPanel buttons = new FlowLayoutPanel();
			buttons.Dock = DockStyle.Fill;
			buttons.FlowDirection = FlowDirection.LeftToRight;
			buttons.Padding = new Padding(0, 12, 0, 0);
			buttons.BackColor = _panel2;
			SetDoubleBuffered(buttons);

			_btnAddField = CreateButton("+ 新增参数", false);
			_btnDeleteField = CreateButton("删除选中", false);
			_btnMoveFieldUp = CreateButton("上移选中", false);
			_btnMoveFieldDown = CreateButton("下移选中", false);
			_btnSaveFields = CreateButton("保存定义", true);

			_btnAddField.Click += delegate { AddFieldRow(); };
			_btnDeleteField.Click += delegate { DeleteSelectedFieldRow(); };
			_btnMoveFieldUp.Click += delegate { MoveSelectedFieldRow(-1); };
			_btnMoveFieldDown.Click += delegate { MoveSelectedFieldRow(1); };
			_btnSaveFields.Click += delegate { SaveConfigFromSettingsPage(); };

			buttons.Controls.Add(_btnAddField);
			buttons.Controls.Add(_btnDeleteField);
			buttons.Controls.Add(_btnMoveFieldUp);
			buttons.Controls.Add(_btnMoveFieldDown);
			buttons.Controls.Add(_btnSaveFields);

			layout.Controls.Add(titlePanel, 0, 0);
			layout.Controls.Add(_fieldsGrid, 0, 1);
			layout.Controls.Add(buttons, 0, 2);
			panel.Controls.Add(layout);
		}

		private Panel BuildQueryPage()
		{
			Panel page = new Panel();
			page.BackColor = _panel;
			page.Dock = DockStyle.Fill;
			SetDoubleBuffered(page);

			TableLayoutPanel layout = new TableLayoutPanel();
			layout.Dock = DockStyle.Fill;
			layout.BackColor = _panel;
			layout.RowCount = 4;
			layout.ColumnCount = 1;
			layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 42F));
			layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 88F));
			layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
			layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 58F));
			SetDoubleBuffered(layout);

			_lblQueryTitle = CreateTitleLabel("数据库查询");

			Panel filterPanel = CreateBorderPanel();
			BuildQueryFilterPanel(filterPanel);

			_queryGrid = CreateGrid();
			_queryGrid.ReadOnly = true;
			_queryGrid.AllowUserToAddRows = false;
			_queryGrid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
			_queryGrid.DataError += delegate(object sender, DataGridViewDataErrorEventArgs e) { e.ThrowException = false; };

			FlowLayoutPanel bottomButtons = new FlowLayoutPanel();
			bottomButtons.Dock = DockStyle.Fill;
			bottomButtons.FlowDirection = FlowDirection.LeftToRight;
			bottomButtons.Padding = new Padding(0, 12, 0, 0);
			bottomButtons.BackColor = _panel;
			_btnExport = CreateButton("导出数据", false);
			_btnOpenFolder = CreateButton("打开数据库文件夹", false);
			_btnExport.Click += delegate { ExportQueryResult(); };
			_btnOpenFolder.Click += delegate { OpenDatabaseFolder(); };
			bottomButtons.Controls.Add(_btnExport);
			bottomButtons.Controls.Add(_btnOpenFolder);

			layout.Controls.Add(_lblQueryTitle, 0, 0);
			layout.Controls.Add(filterPanel, 0, 1);
			layout.Controls.Add(_queryGrid, 0, 2);
			layout.Controls.Add(bottomButtons, 0, 3);
			page.Controls.Add(layout);
			return page;
		}

		private void BuildQueryFilterPanel(Panel panel)
		{
			_lblQueryStart = CreateSmallLabel("开始时间", 16, 28, 80, 24);
			_dtpStart = CreateDatePicker(96, 28);
			_dtpStart.Value = DateTime.Today;
			_lblQueryEnd = CreateSmallLabel("结束时间", 310, 28, 80, 24);
			_dtpEnd = CreateDatePicker(390, 28);
			_dtpEnd.Value = DateTime.Today.AddDays(1).AddMilliseconds(-1);
			_lblQueryKeyword = CreateSmallLabel("数据名称关键字", 604, 28, 120, 24);
			_txtKeyword = CreateTextBox(724, 28, 200);
			_btnSearch = CreateButton("查询", true);
			_btnSearch.SetBounds(946, 26, 90, 30);
			_btnSearch.Click += async delegate { await QueryRecordsAsync(); };

			panel.Controls.Add(_lblQueryStart);
			panel.Controls.Add(_dtpStart);
			panel.Controls.Add(_lblQueryEnd);
			panel.Controls.Add(_dtpEnd);
			panel.Controls.Add(_lblQueryKeyword);
			panel.Controls.Add(_txtKeyword);
			panel.Controls.Add(_btnSearch);
		}

		private DateTimePicker CreateDatePicker(int x, int y)
		{
			DateTimePicker picker = new DateTimePicker();
			picker.Format = DateTimePickerFormat.Custom;
			picker.CustomFormat = "yyyy-MM-dd HH:mm:ss";
			picker.Location = new Point(x, y);
			picker.Size = new Size(190, 24);
			picker.CalendarForeColor = Color.White;
			picker.CalendarMonthBackground = _back;
			picker.CalendarTitleBackColor = _panel2;
			picker.CalendarTitleForeColor = Color.White;
			return picker;
		}

		private void BuildFieldGridColumns()
		{
			_fieldsGrid.Columns.Clear();
			_fieldsGrid.Columns.Add(new DataGridViewCheckBoxColumn { Name = "Enabled", HeaderText = "启用", FillWeight = 45 });
			_fieldsGrid.Columns.Add(new DataGridViewTextBoxColumn { Name = "InputName", HeaderText = "输入名称", FillWeight = 120 });

			DataGridViewComboBoxColumn formatColumn = new DataGridViewComboBoxColumn();
			formatColumn.Name = "DataFormat";
			formatColumn.HeaderText = "数据格式";
			formatColumn.FillWeight = 85;
			formatColumn.FlatStyle = FlatStyle.Flat;
			formatColumn.Items.AddRange(GetDatabaseFormatNames());
			_fieldsGrid.Columns.Add(formatColumn);

			_fieldsGrid.Columns.Add(new DataGridViewTextBoxColumn { Name = "DefaultValue", HeaderText = "默认值", FillWeight = 88 });
			_fieldsGrid.Columns.Add(new DataGridViewCheckBoxColumn { Name = "Required", HeaderText = "必填", FillWeight = 52 });
			_fieldsGrid.Columns.Add(new DataGridViewCheckBoxColumn { Name = "Indexed", HeaderText = "索引", FillWeight = 52 });
			_fieldsGrid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Remark", HeaderText = "备注", FillWeight = 150 });
			ApplyFieldGridLanguage();
		}

		private string[] GetDatabaseFormatNames()
		{
			return new string[] { "String", "Int", "Double", "Bool", "DateTime" };
		}

		private string GetDatabaseFormatName(DatabaseFieldDataFormat format)
		{
			if (format == DatabaseFieldDataFormat.String || format == DatabaseFieldDataFormat.Text)
			{
				return "String";
			}

			return format.ToString();
		}

		private DatabaseFieldDataFormat ParseDatabaseFormat(string text)
		{
			if (string.Equals(text, "TXT", StringComparison.OrdinalIgnoreCase) ||
				string.Equals(text, "Text", StringComparison.OrdinalIgnoreCase) ||
				string.Equals(text, "String", StringComparison.OrdinalIgnoreCase))
			{
				return DatabaseFieldDataFormat.String;
			}

			DatabaseFieldDataFormat format;
			if (!Enum.TryParse(text, true, out format))
			{
				return DatabaseFieldDataFormat.String;
			}

			return format;
		}

		private void ApplyFieldGridLanguage()
		{
			if (_fieldsGrid == null || _fieldsGrid.Columns.Count == 0)
			{
				return;
			}

			_fieldsGrid.Columns["Enabled"].HeaderText = _isEnglish ? "Enabled" : "启用";
			_fieldsGrid.Columns["InputName"].HeaderText = _isEnglish ? "Input Name" : "输入名称";
			_fieldsGrid.Columns["DataFormat"].HeaderText = _isEnglish ? "Data Format" : "数据格式";
			_fieldsGrid.Columns["DefaultValue"].HeaderText = _isEnglish ? "Default" : "默认值";
			_fieldsGrid.Columns["Required"].HeaderText = _isEnglish ? "Required" : "必填";
			_fieldsGrid.Columns["Indexed"].HeaderText = _isEnglish ? "Indexed" : "索引";
			_fieldsGrid.Columns["Remark"].HeaderText = _isEnglish ? "Remark" : "备注";
		}

		private void ApplyQueryGridLanguage()
		{
			if (_queryGrid == null || _queryGrid.Columns.Count == 0)
			{
				return;
			}

			if (_queryGrid.Columns.Contains("RecordTime"))
			{
				_queryGrid.Columns["RecordTime"].HeaderText = _isEnglish ? "Record Time" : "记录时间";
			}
		}

		private void LoadConfigToSettingsPage()
		{
			if (_settingsPage == null)
			{
				return;
			}

			_loading = true;
			try
			{
				DatabaseConfig config = DatabaseConfigStore.LoadOrCreateDefault();
				_txtDatabasePath.Text = config.DatabasePath;
				ResetTextBoxViewStart(_txtDatabasePath);
				_numRetentionDays.Value = Math.Max(_numRetentionDays.Minimum, Math.Min(_numRetentionDays.Maximum, config.RetentionDays));
				_txtTableName.Text = config.TableName;
				ResetTextBoxViewStart(_txtTableName);

				_fieldsGrid.Rows.Clear();
				foreach (DatabaseFieldConfig field in config.Fields)
				{
					AddFieldRow(field);
				}
			}
			finally
			{
				_loading = false;
			}
		}

		private void ResetTextBoxViewStart(TextBox textBox)
		{
			if (textBox == null)
			{
				return;
			}

			try
			{
				textBox.SelectionStart = 0;
				textBox.SelectionLength = 0;
			}
			catch
			{
			}
		}

		private void AddFieldRow()
		{
			AddFieldRow(new DatabaseFieldConfig
			{
				Enabled = true,
				InputName = NextFieldName(),
				DataFormat = DatabaseFieldDataFormat.String,
				LengthPrecision = string.Empty,
				DefaultValue = string.Empty,
				Required = false,
				Indexed = false,
				Remark = string.Empty
			});
		}

		private void AddFieldRow(DatabaseFieldConfig field)
		{
			if (_fieldsGrid == null)
			{
				return;
			}

			int index = _fieldsGrid.Rows.Add();
			DataGridViewRow row = _fieldsGrid.Rows[index];
			row.Cells["Enabled"].Value = field == null || field.Enabled;
			row.Cells["InputName"].Value = field == null ? string.Empty : field.InputName;
			row.Cells["DataFormat"].Value = GetDatabaseFormatName(field == null ? DatabaseFieldDataFormat.String : field.DataFormat);
			row.Cells["DefaultValue"].Value = field == null ? string.Empty : field.DefaultValue;
			row.Cells["Required"].Value = field != null && field.Required;
			row.Cells["Indexed"].Value = field != null && field.Indexed;
			row.Cells["Remark"].Value = field == null ? string.Empty : field.Remark;
			_fieldsGrid.ClearSelection();
			row.Selected = true;
		}

		private string NextFieldName()
		{
			int index = 1;
			HashSet<string> names = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
			foreach (DataGridViewRow row in _fieldsGrid.Rows)
			{
				if (row == null || row.IsNewRow)
				{
					continue;
				}
				names.Add(GetCellText(row, "InputName"));
			}

			while (names.Contains("Field_" + index.ToString("00")))
			{
				index++;
			}

			return "Field_" + index.ToString("00");
		}

		private void DeleteSelectedFieldRow()
		{
			if (_fieldsGrid == null || _fieldsGrid.SelectedRows.Count <= 0)
			{
				return;
			}

			foreach (DataGridViewRow row in _fieldsGrid.SelectedRows)
			{
				if (!row.IsNewRow)
				{
					_fieldsGrid.Rows.Remove(row);
				}
			}
		}

		private void MoveSelectedFieldRow(int direction)
		{
			if (_fieldsGrid == null || _fieldsGrid.SelectedRows.Count <= 0)
			{
				return;
			}

			DataGridViewRow row = _fieldsGrid.SelectedRows[0];
			int oldIndex = row.Index;
			int newIndex = oldIndex + direction;
			if (newIndex < 0 || newIndex >= _fieldsGrid.Rows.Count)
			{
				return;
			}

			_fieldsGrid.Rows.RemoveAt(oldIndex);
			_fieldsGrid.Rows.Insert(newIndex, row);
			_fieldsGrid.ClearSelection();
			row.Selected = true;
		}

		private DatabaseConfig CollectSettingsConfig()
		{
			DatabaseConfig config = new DatabaseConfig();
			config.DatabasePath = _txtDatabasePath == null ? string.Empty : _txtDatabasePath.Text.Trim();
			config.RetentionDays = _numRetentionDays == null ? 365 : (int)_numRetentionDays.Value;
			config.TableName = _txtTableName == null ? "TaskRecord" : _txtTableName.Text.Trim();

			foreach (DataGridViewRow row in _fieldsGrid.Rows)
			{
				if (row == null || row.IsNewRow)
				{
					continue;
				}

				string inputName = GetCellText(row, "InputName");
				if (string.IsNullOrWhiteSpace(inputName))
				{
					continue;
				}

				DatabaseFieldDataFormat format = ParseDatabaseFormat(GetCellText(row, "DataFormat"));

				config.Fields.Add(new DatabaseFieldConfig
				{
					Enabled = GetCellBool(row, "Enabled"),
					InputName = inputName,
					DataFormat = format,
					LengthPrecision = string.Empty,
					DefaultValue = GetCellText(row, "DefaultValue"),
					Required = GetCellBool(row, "Required"),
					Indexed = GetCellBool(row, "Indexed"),
					Remark = GetCellText(row, "Remark")
				});
			}

			return config;
		}

		private bool ValidateConfig(DatabaseConfig config, out string message)
		{
			message = string.Empty;
			if (config == null)
			{
				message = _isEnglish ? "Configuration is empty." : "配置为空。";
				return false;
			}

			if (string.IsNullOrWhiteSpace(config.DatabasePath))
			{
				message = _isEnglish ? "Database path is required." : "数据库路径不能为空。";
				return false;
			}

			HashSet<string> names = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
			foreach (DatabaseFieldConfig field in config.Fields)
			{
				if (field == null || string.IsNullOrWhiteSpace(field.InputName))
				{
					continue;
				}

				if (!names.Add(field.InputName.Trim()))
				{
					message = _isEnglish ? "Duplicate input name: " + field.InputName : "输入名称重复：" + field.InputName;
					return false;
				}
			}

			return true;
		}

		private void SaveConfigFromSettingsPage()
		{
			if (_loading)
			{
				return;
			}

			DatabaseConfig config = CollectSettingsConfig();
			string message;
			if (!ValidateConfig(config, out message))
			{
				ThemedDialog.ShowWarning(this, _isEnglish ? "Database" : "数据库", message, _isEnglish);
				return;
			}

			try
			{
				DatabaseConfigStore.Save(config);
				DatabaseLocalRecordStore.EnsureStorage(config);
				ThemedDialog.ShowInformation(
					this,
					_isEnglish ? "Database" : "数据库",
					_isEnglish ? "Database settings saved." : "数据库配置已保存。",
					_isEnglish);
			}
			catch (Exception ex)
			{
				ThemedDialog.ShowError(
					this,
					_isEnglish ? "Database" : "数据库",
					(_isEnglish ? "Save failed: " : "保存失败：") + ex.Message,
					_isEnglish);
			}
		}

		private void TestStorage()
		{
			try
			{
				DatabaseConfig config = CollectSettingsConfig();
				DatabaseLocalRecordStore.EnsureStorage(config);
				ThemedDialog.ShowInformation(
					this,
					_isEnglish ? "Database" : "数据库",
					_isEnglish ? "Database folder is ready." : "数据库文件夹已准备完成。",
					_isEnglish);
			}
			catch (Exception ex)
			{
				ThemedDialog.ShowError(
					this,
					_isEnglish ? "Database" : "数据库",
					(_isEnglish ? "Storage test failed: " : "测试失败：") + ex.Message,
					_isEnglish);
			}
		}

		private async Task QueryRecordsAsync()
		{
			if (_btnSearch == null || _querying)
			{
				return;
			}

			_querying = true;
			_btnSearch.Enabled = false;
			try
			{
				DatabaseQueryOptions options = new DatabaseQueryOptions();
				options.StartTime = _dtpStart.Value;
				options.EndTime = _dtpEnd.Value;
				options.FieldKeyword = _txtKeyword.Text;
				DatabaseConfig config = DatabaseConfigStore.LoadOrCreateDefault();

				DatabaseQueryResult result = await Task.Run(delegate
				{
					return DatabaseLocalRecordStore.Query(config, options);
				});

				_lastQueryResult = result;
				LoadQueryResult(result);
			}
			catch (Exception ex)
			{
				ThemedDialog.ShowError(
					this,
					_isEnglish ? "Database" : "数据库",
					(_isEnglish ? "Query failed: " : "查询失败：") + ex.Message,
					_isEnglish);
			}
			finally
			{
				_querying = false;
				if (_btnSearch != null)
				{
					_btnSearch.Enabled = true;
				}
			}
		}

		private void StartQueryRecordsAsync()
		{
			if (_queryPage == null || _queryPage.IsDisposed || _btnSearch == null)
			{
				return;
			}

			Task ignored = QueryRecordsAsync();
		}

		private void LoadQueryResult(DatabaseQueryResult result)
		{
			_queryGrid.Columns.Clear();
			_queryGrid.Rows.Clear();

			if (result == null)
			{
				return;
			}

			foreach (string column in result.Columns)
			{
				_queryGrid.Columns.Add(new DataGridViewTextBoxColumn
				{
					Name = column,
					HeaderText = column,
					FillWeight = string.Equals(column, "RecordTime", StringComparison.OrdinalIgnoreCase) ? 140 : 100
				});
			}

			foreach (Dictionary<string, string> rowData in result.Rows)
			{
				int rowIndex = _queryGrid.Rows.Add();
				DataGridViewRow row = _queryGrid.Rows[rowIndex];
				foreach (string column in result.Columns)
				{
					string value;
					row.Cells[column].Value = rowData.TryGetValue(column, out value) ? value : string.Empty;
				}
			}

			ApplyQueryGridLanguage();
		}

		private void ExportQueryResult()
		{
			if (_lastQueryResult == null || _lastQueryResult.Columns.Count <= 0)
			{
				ThemedDialog.ShowWarning(
					this,
					_isEnglish ? "Database" : "数据库",
					_isEnglish ? "Please query data before export." : "请先查询数据再导出。",
					_isEnglish);
				return;
			}

			using (SaveFileDialog dialog = new SaveFileDialog())
			{
				dialog.Filter = _isEnglish ? "CSV File (*.csv)|*.csv" : "CSV 文件 (*.csv)|*.csv";
				dialog.FileName = "database_export_" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".csv";
				if (dialog.ShowDialog(this) != DialogResult.OK)
				{
					return;
				}

				try
				{
					DatabaseLocalRecordStore.ExportCsv(_lastQueryResult, dialog.FileName);
					ThemedDialog.ShowInformation(
						this,
						_isEnglish ? "Database" : "数据库",
						_isEnglish ? "Export finished." : "导出完成。",
						_isEnglish);
				}
				catch (Exception ex)
				{
					ThemedDialog.ShowError(
						this,
						_isEnglish ? "Database" : "数据库",
						(_isEnglish ? "Export failed: " : "导出失败：") + ex.Message,
						_isEnglish);
				}
			}
		}

		private void OpenDatabaseFolder()
		{
			try
			{
				DatabaseConfig config = DatabaseConfigStore.LoadOrCreateDefault();
				string folder = DatabaseLocalRecordStore.GetStorageFolder(config);
				Directory.CreateDirectory(folder);
				Process.Start(folder);
			}
			catch (Exception ex)
			{
				ThemedDialog.ShowError(
					this,
					_isEnglish ? "Database" : "数据库",
					(_isEnglish ? "Open folder failed: " : "打开文件夹失败：") + ex.Message,
					_isEnglish);
			}
		}

		private Panel CreateBorderPanel()
		{
			Panel panel = new Panel();
			panel.Dock = DockStyle.Fill;
			panel.BackColor = _panel2;
			panel.Padding = new Padding(1);
			panel.Paint += delegate(object sender, PaintEventArgs e)
			{
				Control control = sender as Control;
				if (control == null || e == null)
				{
					return;
				}
				using (Pen pen = new Pen(_border))
				{
					e.Graphics.DrawRectangle(pen, 0, 0, control.Width - 1, control.Height - 1);
				}
			};
			SetDoubleBuffered(panel);
			return panel;
		}

		private Label CreateTitleLabel(string text)
		{
			Label label = new Label();
			label.Dock = DockStyle.Fill;
			label.Text = text;
			label.ForeColor = _text;
			label.BackColor = _panel;
			label.Font = new Font("Microsoft YaHei UI", 11F, FontStyle.Bold);
			label.TextAlign = ContentAlignment.MiddleLeft;
			label.Padding = new Padding(2, 0, 0, 0);
			return label;
		}

		private Label CreateSectionLabel(string text, int x, int y, int width, int height)
		{
			Label label = new Label();
			label.Text = text;
			label.SetBounds(x, y, width, height);
			label.ForeColor = _text;
			label.BackColor = Color.Transparent;
			label.Font = new Font("Microsoft YaHei UI", 9.5F, FontStyle.Bold);
			label.TextAlign = ContentAlignment.MiddleLeft;
			return label;
		}

		private Label CreateSmallLabel(string text, int x, int y, int width, int height)
		{
			Label label = new Label();
			label.Text = text;
			label.SetBounds(x, y, width, height);
			label.ForeColor = _text;
			label.BackColor = Color.Transparent;
			label.Font = new Font("Microsoft YaHei UI", 8.5F, FontStyle.Bold);
			label.TextAlign = ContentAlignment.MiddleLeft;
			return label;
		}

		private TextBox CreateTextBox(int x, int y, int width)
		{
			TextBox textBox = new TextBox();
			textBox.Location = new Point(x, y);
			textBox.Size = new Size(width, 24);
			textBox.BackColor = _back;
			textBox.ForeColor = Color.White;
			textBox.BorderStyle = BorderStyle.FixedSingle;
			textBox.Font = new Font("Microsoft YaHei UI", 8.5F, FontStyle.Regular);
			return textBox;
		}

		private Button CreateButton(string text, bool primary)
		{
			Button button = new Button();
			button.Text = text;
			button.Size = new Size(primary ? 120 : 120, 34);
			button.Margin = new Padding(0, 0, 12, 0);
			button.FlatStyle = FlatStyle.Flat;
			button.BackColor = primary ? Color.FromArgb(0, 95, 220) : _panel;
			button.ForeColor = Color.White;
			button.Font = new Font("Microsoft YaHei UI", 9F, FontStyle.Bold);
			button.Cursor = Cursors.Hand;
			button.FlatAppearance.BorderColor = _accent;
			button.FlatAppearance.BorderSize = 1;
			button.FlatAppearance.MouseOverBackColor = primary ? Color.FromArgb(20, 120, 235) : Color.FromArgb(8, 34, 56);
			button.FlatAppearance.MouseDownBackColor = primary ? Color.FromArgb(0, 80, 190) : Color.FromArgb(0, 80, 135);
			return button;
		}

		private DataGridView CreateGrid()
		{
			BufferedDataGridView grid = new BufferedDataGridView();
			grid.Dock = DockStyle.Fill;
			grid.BackgroundColor = _back;
			grid.GridColor = _border;
			grid.BorderStyle = BorderStyle.FixedSingle;
			grid.EnableHeadersVisualStyles = false;
			grid.RowHeadersVisible = false;
			grid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
			grid.MultiSelect = false;
			grid.AllowUserToResizeRows = false;
			grid.ColumnHeadersHeight = 32;
			grid.RowTemplate.Height = 30;
			grid.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(8, 28, 48);
			grid.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
			grid.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
			grid.ColumnHeadersDefaultCellStyle.Font = new Font("Microsoft YaHei UI", 9F, FontStyle.Bold);
			grid.DefaultCellStyle.BackColor = _back;
			grid.DefaultCellStyle.ForeColor = Color.White;
			grid.DefaultCellStyle.SelectionBackColor = Color.FromArgb(0, 120, 200);
			grid.DefaultCellStyle.SelectionForeColor = Color.White;
			grid.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
			grid.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(4, 18, 34);
			grid.AlternatingRowsDefaultCellStyle.ForeColor = Color.White;
			return grid;
		}

		private string GetCellText(DataGridViewRow row, string columnName)
		{
			if (row == null || row.DataGridView == null || !row.DataGridView.Columns.Contains(columnName))
			{
				return string.Empty;
			}

			object value = row.Cells[columnName].Value;
			return value == null ? string.Empty : Convert.ToString(value).Trim();
		}

		private bool GetCellBool(DataGridViewRow row, string columnName)
		{
			string text = GetCellText(row, columnName);
			bool value;
			if (bool.TryParse(text, out value))
			{
				return value;
			}
			return text == "1" || text.Equals("yes", StringComparison.OrdinalIgnoreCase) || text == "√";
		}

		private void SetDoubleBuffered(Control control)
		{
			if (control == null)
			{
				return;
			}

			try
			{
				System.Reflection.PropertyInfo property = typeof(Control).GetProperty(
					"DoubleBuffered",
					System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
				if (property != null)
				{
					property.SetValue(control, true, null);
				}
			}
			catch
			{
			}
		}

		private class BufferedDataGridView : DataGridView
		{
			public BufferedDataGridView()
			{
				DoubleBuffered = true;
				SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.AllPaintingInWmPaint | ControlStyles.ResizeRedraw, true);
			}
		}
	}
}
