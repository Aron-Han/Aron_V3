using System;
using System.Drawing;
using System.Windows.Forms;

namespace Aron_V3
{
	public partial class SystemManagementControl : UserControl, ILocalizable
	{
		private readonly Color _back = Color.FromArgb(2, 10, 20);
		private readonly Color _panel = Color.FromArgb(3, 14, 27);
		private readonly Color _panel2 = Color.FromArgb(5, 18, 34);
		private readonly Color _border = Color.FromArgb(38, 62, 86);
		private readonly Color _accent = Color.FromArgb(0, 150, 220);
		private readonly Color _selected = Color.FromArgb(0, 95, 170);
		private readonly Color _text = Color.FromArgb(220, 235, 245);
		private readonly Color _muted = Color.FromArgb(130, 155, 175);

		private TableLayoutPanel _root;
		private Panel _menuPanel;
		private Panel _contentPanel;

		private Button _btnDisplayLayout;
		private Button _btnGlobalVariables;
		private Button _btnDataDisplay;
		private Button _btnUserManager;
		private Button _btnSystemInfo;

		private Control _currentPage;
		private DisplayLayoutControl _displayLayoutPage;
		private GlobalVariableControl _globalVariablePage;
		private DataDisplayControl _dataDisplayPage;

		public SystemManagementControl()
		{
			Dock = DockStyle.Fill;
			BackColor = _back;
			DoubleBuffered = true;

			BuildUi();
			ShowDisplayLayoutPage();
		}

		public void ApplyLanguage(bool isEnglish)
		{
			if (_btnDisplayLayout != null)
			{
				_btnDisplayLayout.Text = isEnglish ? "Display Layout" : "显示布局";
			}

			if (_btnUserManager != null)
			{
				_btnUserManager.Text = isEnglish ? "User Manager" : "用户管理";
			}

			if (_btnGlobalVariables != null)
			{
				_btnGlobalVariables.Text = isEnglish ? "Global Variables" : "全局变量";
			}

			if (_btnDataDisplay != null)
			{
				_btnDataDisplay.Text = isEnglish ? "Data Display" : "界面数据显示";
			}

			if (_btnSystemInfo != null)
			{
				_btnSystemInfo.Text = isEnglish ? "System Info" : "系统信息";
			}

			if (_displayLayoutPage != null)
			{
				_displayLayoutPage.ApplyLanguage(isEnglish);
			}

			if (_globalVariablePage != null)
			{
				_globalVariablePage.ApplyLanguage(isEnglish);
			}

			if (_dataDisplayPage != null)
			{
				_dataDisplayPage.ApplyLanguage(isEnglish);
			}
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

			_menuPanel = new Panel();
			_menuPanel.Dock = DockStyle.Fill;
			_menuPanel.BackColor = _panel;
			_menuPanel.Padding = new Padding(10);

			_contentPanel = new Panel();
			_contentPanel.Dock = DockStyle.Fill;
			_contentPanel.BackColor = _panel;
			_contentPanel.Padding = new Padding(10);

			BuildLeftMenu();

			_root.Controls.Add(_menuPanel, 0, 0);
			_root.Controls.Add(_contentPanel, 1, 0);

			Controls.Add(_root);
		}

		private void BuildLeftMenu()
		{
			_menuPanel.Controls.Clear();

			Label title = new Label();
			title.Text = "系统管理";
			title.ForeColor = _text;
			title.Font = new Font("Microsoft YaHei UI", 10F, FontStyle.Bold);
			title.TextAlign = ContentAlignment.MiddleLeft;
			title.Dock = DockStyle.Top;
			title.Height = 34;
			title.Padding = new Padding(4, 0, 0, 0);

			_btnDisplayLayout = CreateMenuButton("显示布局");
			_btnGlobalVariables = CreateMenuButton("全局变量");
			_btnDataDisplay = CreateMenuButton("界面数据显示");
			_btnUserManager = CreateMenuButton("用户管理");
			_btnSystemInfo = CreateMenuButton("系统信息");

			_btnDisplayLayout.Top = 44;
			_btnGlobalVariables.Top = 98;
			_btnDataDisplay.Top = 152;
			_btnUserManager.Top = 206;
			_btnSystemInfo.Top = 260;

			_btnDisplayLayout.Click += delegate { ShowDisplayLayoutPage(); };
			_btnGlobalVariables.Click += delegate { ShowGlobalVariablePage(); };
			_btnDataDisplay.Click += delegate { ShowDataDisplayPage(); };
			_btnUserManager.Click += delegate { ShowPlaceholderPage("用户管理", "后续可接入 UserAccounts.xml、权限、自动登出等设置。"); };
			_btnSystemInfo.Click += delegate { ShowPlaceholderPage("系统信息", "后续可显示软件版本、项目路径、日志路径、授权状态等信息。"); };

			_menuPanel.Controls.Add(title);
			_menuPanel.Controls.Add(_btnDisplayLayout);
			_menuPanel.Controls.Add(_btnGlobalVariables);
			_menuPanel.Controls.Add(_btnDataDisplay);
			_menuPanel.Controls.Add(_btnUserManager);
			_menuPanel.Controls.Add(_btnSystemInfo);
		}

		private Button CreateMenuButton(string text)
		{
			Button btn = new Button();
			btn.Left = 0;
			btn.Width = _menuPanel.Width - 20;
			btn.Height = 44;
			btn.Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top;
			btn.Text = text;
			btn.TextAlign = ContentAlignment.MiddleCenter;
			btn.FlatStyle = FlatStyle.Flat;
			btn.FlatAppearance.BorderColor = _border;
			btn.FlatAppearance.BorderSize = 1;
			btn.BackColor = _panel2;
			btn.ForeColor = _text;
			btn.Font = new Font("Microsoft YaHei UI", 9F, FontStyle.Bold);
			btn.Cursor = Cursors.Hand;

			return btn;
		}

		private void ShowDisplayLayoutPage()
		{
			if (_displayLayoutPage == null || _displayLayoutPage.IsDisposed)
			{
				_displayLayoutPage = new DisplayLayoutControl();
				_displayLayoutPage.Dock = DockStyle.Fill;
			}

			ShowPage(_displayLayoutPage);
			SetSelectedButton(_btnDisplayLayout);
		}

		private void ShowGlobalVariablePage()
		{
			if (_globalVariablePage == null || _globalVariablePage.IsDisposed)
			{
				_globalVariablePage = new GlobalVariableControl();
				_globalVariablePage.Dock = DockStyle.Fill;
			}

			ShowPage(_globalVariablePage);
			SetSelectedButton(_btnGlobalVariables);
		}

		private void ShowDataDisplayPage()
		{
			if (_dataDisplayPage == null || _dataDisplayPage.IsDisposed)
			{
				_dataDisplayPage = new DataDisplayControl();
				_dataDisplayPage.Dock = DockStyle.Fill;
			}
			ShowPage(_dataDisplayPage);
			SetSelectedButton(_btnDataDisplay);
		}

		private void ShowPlaceholderPage(string title, string message)
		{
			Panel page = new Panel();
			page.Dock = DockStyle.Fill;
			page.BackColor = _back;
			page.Padding = new Padding(20);

			Label lblTitle = new Label();
			lblTitle.Text = title;
			lblTitle.ForeColor = _text;
			lblTitle.Font = new Font("Microsoft YaHei UI", 12F, FontStyle.Bold);
			lblTitle.Dock = DockStyle.Top;
			lblTitle.Height = 40;

			Label lblMessage = new Label();
			lblMessage.Text = message;
			lblMessage.ForeColor = _muted;
			lblMessage.Font = new Font("Microsoft YaHei UI", 10F, FontStyle.Regular);
			lblMessage.Dock = DockStyle.Fill;
			lblMessage.TextAlign = ContentAlignment.MiddleCenter;

			page.Controls.Add(lblMessage);
			page.Controls.Add(lblTitle);

			ShowPage(page);

			if (title == "用户管理")
			{
				SetSelectedButton(_btnUserManager);
			}
			else
			{
				SetSelectedButton(_btnSystemInfo);
			}
		}

		private void ShowPage(Control page)
		{
			if (page == null)
			{
				return;
			}

			if (_currentPage == page && page.Parent == _contentPanel)
			{
				return;
			}

			_contentPanel.SuspendLayout();
			_contentPanel.Controls.Clear();

			page.Dock = DockStyle.Fill;
			_contentPanel.Controls.Add(page);

			_currentPage = page;
			_contentPanel.ResumeLayout(true);
		}

		private void SetSelectedButton(Button selectedButton)
		{
			Button[] buttons = new Button[]
			{
				_btnDisplayLayout,
				_btnGlobalVariables,
				_btnDataDisplay,
				_btnUserManager,
				_btnSystemInfo
			};

			foreach (Button btn in buttons)
			{
				if (btn == null)
				{
					continue;
				}

				if (btn == selectedButton)
				{
					btn.BackColor = _selected;
					btn.FlatAppearance.BorderColor = _accent;
				}
				else
				{
					btn.BackColor = _panel2;
					btn.FlatAppearance.BorderColor = _border;
				}
			}
		}

		protected override void OnResize(EventArgs e)
		{
			base.OnResize(e);

			if (_btnDisplayLayout != null)
			{
				int width = Math.Max(120, _menuPanel.ClientSize.Width - 20);
				_btnDisplayLayout.Width = width;
				_btnGlobalVariables.Width = width;
				_btnDataDisplay.Width = width;
				_btnUserManager.Width = width;
				_btnSystemInfo.Width = width;
			}
		}
	}
}
