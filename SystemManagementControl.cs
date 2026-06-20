using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Reflection;
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
		private Label _titleLabel;
		private bool _isEnglish;

		private Button _btnDisplayLayout;
		private Button _btnGlobalVariables;
		private Button _btnDataDisplay;
		private Button _btnDiagnostics;
		private Button _btnProgramManager;
		private Button _btnUserManager;
		private Button _btnSystemInfo;

		private Control _currentPage;
		private DisplayLayoutControl _displayLayoutPage;
		private GlobalVariableControl _globalVariablePage;
		private DataDisplayControl _dataDisplayPage;
		private DiagnosticManagementControl _diagnosticPage;
		private ProgramManagementControl _programManagementPage;
		private Panel _systemInfoPage;

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
			_isEnglish = isEnglish;

			if (_titleLabel != null)
			{
				_titleLabel.Text = isEnglish ? "System Management" : "系统管理";
			}

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

			if (_btnDiagnostics != null)
			{
				_btnDiagnostics.Text = isEnglish ? "Diagnostics" : "诊断日志";
			}

			if (_btnProgramManager != null)
			{
				_btnProgramManager.Text = isEnglish ? "Program Management" : "程序号管理";
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

			if (_diagnosticPage != null)
			{
				_diagnosticPage.ApplyLanguage(isEnglish);
			}

			if (_programManagementPage != null)
			{
				_programManagementPage.ApplyLanguage(isEnglish);
			}

			if (_systemInfoPage != null && !_systemInfoPage.IsDisposed)
			{
				BuildSystemInfoContent();
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

			_titleLabel = new Label();
			_titleLabel.Text = _isEnglish ? "System Management" : "系统管理";
			_titleLabel.ForeColor = _text;
			_titleLabel.Font = new Font("Microsoft YaHei UI", 10F, FontStyle.Bold);
			_titleLabel.TextAlign = ContentAlignment.MiddleLeft;
			_titleLabel.Dock = DockStyle.Top;
			_titleLabel.Height = 34;
			_titleLabel.Padding = new Padding(4, 0, 0, 0);

			_btnDisplayLayout = CreateMenuButton("显示布局");
			_btnGlobalVariables = CreateMenuButton("全局变量");
			_btnDataDisplay = CreateMenuButton("界面数据显示");
			_btnDiagnostics = CreateMenuButton("诊断日志");
			_btnProgramManager = CreateMenuButton("程序号管理");
			_btnUserManager = CreateMenuButton("用户管理");
			_btnSystemInfo = CreateMenuButton("系统信息");

			_btnDisplayLayout.Top = 44;
			_btnGlobalVariables.Top = 98;
			_btnDataDisplay.Top = 152;
			_btnDiagnostics.Top = 206;
			_btnProgramManager.Top = 260;
			_btnUserManager.Top = 314;
			_btnSystemInfo.Top = 368;

			_btnDisplayLayout.Click += delegate { ShowDisplayLayoutPage(); };
			_btnGlobalVariables.Click += delegate { ShowGlobalVariablePage(); };
			_btnDataDisplay.Click += delegate { ShowDataDisplayPage(); };
			_btnDiagnostics.Click += delegate { ShowDiagnosticPage(); };
			_btnProgramManager.Click += delegate { ShowProgramManagementPage(); };
			_btnUserManager.Click += delegate { ShowUserManagerPlaceholder(); };
			_btnSystemInfo.Click += delegate { ShowSystemInfoPage(); };

			_menuPanel.Controls.Add(_titleLabel);
			_menuPanel.Controls.Add(_btnDisplayLayout);
			_menuPanel.Controls.Add(_btnGlobalVariables);
			_menuPanel.Controls.Add(_btnDataDisplay);
			_menuPanel.Controls.Add(_btnDiagnostics);
			_menuPanel.Controls.Add(_btnProgramManager);
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
				_displayLayoutPage.ApplyLanguage(_isEnglish);
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
				_globalVariablePage.ApplyLanguage(_isEnglish);
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
				_dataDisplayPage.ApplyLanguage(_isEnglish);
			}
			ShowPage(_dataDisplayPage);
			SetSelectedButton(_btnDataDisplay);
		}

		private void ShowDiagnosticPage()
		{
			if (_diagnosticPage == null || _diagnosticPage.IsDisposed)
			{
				_diagnosticPage = new DiagnosticManagementControl();
				_diagnosticPage.Dock = DockStyle.Fill;
				_diagnosticPage.ApplyLanguage(_isEnglish);
			}

			ShowPage(_diagnosticPage);
			SetSelectedButton(_btnDiagnostics);
		}

		private void ShowProgramManagementPage()
		{
			if (_programManagementPage == null || _programManagementPage.IsDisposed)
			{
				_programManagementPage = new ProgramManagementControl();
				_programManagementPage.Dock = DockStyle.Fill;
				_programManagementPage.ApplyLanguage(_isEnglish);
			}

			ShowPage(_programManagementPage);
			SetSelectedButton(_btnProgramManager);
		}

		private void ShowUserManagerPlaceholder()
		{
			ShowPlaceholderPage(
				_isEnglish ? "User Management" : "用户管理",
				_isEnglish
					? "User account, permission, and auto logout settings are opened from the user menu."
					: "用户账号、权限、自动登出等设置可从右上角用户菜单进入。",
				_btnUserManager);
		}

		private void ShowSystemInfoPage()
		{
			if (_systemInfoPage == null || _systemInfoPage.IsDisposed)
			{
				_systemInfoPage = new Panel();
				_systemInfoPage.Dock = DockStyle.Fill;
				_systemInfoPage.BackColor = _back;
				_systemInfoPage.Padding = new Padding(20);
			}

			BuildSystemInfoContent();
			ShowPage(_systemInfoPage);
			SetSelectedButton(_btnSystemInfo);
		}

		private void BuildSystemInfoContent()
		{
			if (_systemInfoPage == null || _systemInfoPage.IsDisposed)
			{
				return;
			}

			_systemInfoPage.SuspendLayout();
			_systemInfoPage.Controls.Clear();

			Label lblTitle = new Label();
			lblTitle.Text = _isEnglish ? "System Info" : "系统信息";
			lblTitle.ForeColor = _text;
			lblTitle.Font = new Font("Microsoft YaHei UI", 12F, FontStyle.Bold);
			lblTitle.Dock = DockStyle.Top;
			lblTitle.Height = 42;
			lblTitle.TextAlign = ContentAlignment.MiddleLeft;

			Panel body = new Panel();
			body.Dock = DockStyle.Fill;
			body.BackColor = _back;
			body.AutoScroll = true;
			body.Padding = new Padding(0, 8, 0, 0);

			TableLayoutPanel infoPanel = CreateSystemInfoTable();
			RuntimeLibraryInfo visionPro = GetVisionProInfo();
			RuntimeLibraryInfo halcon = GetHalconInfo();

			AddInfoSection(infoPanel, _isEnglish ? "Software" : "软件信息");
			AddInfoRow(infoPanel, _isEnglish ? "Software Version" : "软件版本", GetSoftwareVersion());

			AddInfoSection(infoPanel, _isEnglish ? "Project" : "项目信息");
			AddInfoRow(infoPanel, _isEnglish ? "Project Folder" : "项目目录", ProjectPathStore.ProjectRoot);
			AddInfoRow(infoPanel, _isEnglish ? "Config Folder" : "配置目录", ProjectPathStore.ConfigRoot);
			AddInfoRow(infoPanel, _isEnglish ? "Log Folder" : "日志目录", Path.Combine(ProjectPathStore.ProjectRoot, "Log"));
			AddInfoRow(infoPanel, _isEnglish ? "Project Total Size" : "项目文件总容量", GetProjectTotalSizeText());

			AddInfoSection(infoPanel, _isEnglish ? "License" : "授权信息");
			AddInfoRow(infoPanel, _isEnglish ? "VisionPro Version" : "VisionPro版本", visionPro.VersionText);
			AddInfoRow(infoPanel, _isEnglish ? "HALCON Version" : "Halcon版本", halcon.VersionText);
			AddInfoRow(infoPanel, _isEnglish ? "VisionPro License Status" : "VisionPro授权状态", visionPro.StatusText);
			AddInfoRow(infoPanel, _isEnglish ? "HALCON License Status" : "Halcon授权状态", halcon.StatusText);

			AddInfoSection(infoPanel, _isEnglish ? "Runtime" : "运行环境");
			AddInfoRow(infoPanel, _isEnglish ? "Operating System" : "操作系统", GetOperatingSystemText());
			AddInfoRow(infoPanel, _isEnglish ? ".NET Runtime" : ".NET运行时", Environment.Version.ToString());
			AddInfoRow(infoPanel, _isEnglish ? "Process Architecture" : "进程架构", Environment.Is64BitProcess ? "x64" : "x86");

			body.Controls.Add(infoPanel);
			_systemInfoPage.Controls.Add(body);
			_systemInfoPage.Controls.Add(lblTitle);
			_systemInfoPage.ResumeLayout(true);
		}

		private TableLayoutPanel CreateSystemInfoTable()
		{
			TableLayoutPanel table = new TableLayoutPanel();
			table.Dock = DockStyle.Top;
			table.AutoSize = true;
			table.AutoSizeMode = AutoSizeMode.GrowAndShrink;
			table.GrowStyle = TableLayoutPanelGrowStyle.AddRows;
			table.BackColor = _panel2;
			table.Padding = new Padding(18, 10, 18, 16);
			table.Margin = new Padding(0);
			table.ColumnCount = 2;
			table.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 190F));
			table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));

			return table;
		}

		private void AddInfoSection(TableLayoutPanel table, string text)
		{
			int row = table.RowCount++;
			table.RowStyles.Add(new RowStyle(SizeType.Absolute, 42F));

			Label label = new Label();
			label.Text = text;
			label.Dock = DockStyle.Fill;
			label.ForeColor = Color.FromArgb(120, 210, 255);
			label.Font = new Font("Microsoft YaHei UI", 10F, FontStyle.Bold);
			label.TextAlign = ContentAlignment.BottomLeft;
			label.Padding = new Padding(0, 0, 0, 8);

			table.Controls.Add(label, 0, row);
			table.SetColumnSpan(label, 2);
		}

		private void AddInfoRow(TableLayoutPanel table, string name, string value)
		{
			int row = table.RowCount++;
			table.RowStyles.Add(new RowStyle(SizeType.Absolute, 34F));

			Label nameLabel = new Label();
			nameLabel.Text = name;
			nameLabel.Dock = DockStyle.Fill;
			nameLabel.ForeColor = _muted;
			nameLabel.Font = new Font("Microsoft YaHei UI", 9F, FontStyle.Regular);
			nameLabel.TextAlign = ContentAlignment.MiddleLeft;
			nameLabel.Padding = new Padding(8, 0, 8, 0);
			nameLabel.AutoEllipsis = true;

			Label valueLabel = new Label();
			valueLabel.Text = string.IsNullOrWhiteSpace(value) ? "-" : value;
			valueLabel.Dock = DockStyle.Fill;
			valueLabel.ForeColor = _text;
			valueLabel.Font = new Font("Microsoft YaHei UI", 9F, FontStyle.Regular);
			valueLabel.TextAlign = ContentAlignment.MiddleLeft;
			valueLabel.Padding = new Padding(10, 0, 8, 0);
			valueLabel.AutoEllipsis = true;

			table.Controls.Add(nameLabel, 0, row);
			table.Controls.Add(valueLabel, 1, row);
		}

		private void ShowPlaceholderPage(string title, string message, Button selectedButton)
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
			SetSelectedButton(selectedButton);
		}

		private string GetSoftwareVersion()
		{
			try
			{
				Assembly assembly = Assembly.GetEntryAssembly() ?? typeof(SystemManagementControl).Assembly;
				if (assembly != null && !string.IsNullOrWhiteSpace(assembly.Location) && File.Exists(assembly.Location))
				{
					FileVersionInfo versionInfo = FileVersionInfo.GetVersionInfo(assembly.Location);
					if (!string.IsNullOrWhiteSpace(versionInfo.FileVersion))
					{
						return versionInfo.FileVersion;
					}
				}

				if (assembly != null && assembly.GetName() != null && assembly.GetName().Version != null)
				{
					return assembly.GetName().Version.ToString();
				}
			}
			catch
			{
			}

			return "-";
		}

		private string GetProjectTotalSizeText()
		{
			try
			{
				long bytes = CalculateDirectorySize(ProjectPathStore.ProjectRoot);
				double gb = bytes / 1024D / 1024D / 1024D;
				return gb.ToString("0.00", CultureInfo.InvariantCulture) + " G";
			}
			catch
			{
				return _isEnglish ? "Unavailable" : "无法读取";
			}
		}

		private long CalculateDirectorySize(string root)
		{
			if (string.IsNullOrWhiteSpace(root) || !Directory.Exists(root))
			{
				return 0L;
			}

			long total = 0L;
			Stack<string> pending = new Stack<string>();
			pending.Push(root);

			while (pending.Count > 0)
			{
				string folder = pending.Pop();

				string[] files = new string[0];
				try
				{
					files = Directory.GetFiles(folder);
				}
				catch
				{
				}

				foreach (string file in files)
				{
					try
					{
						total += new FileInfo(file).Length;
					}
					catch
					{
					}
				}

				string[] folders = new string[0];
				try
				{
					folders = Directory.GetDirectories(folder);
				}
				catch
				{
				}

				foreach (string child in folders)
				{
					pending.Push(child);
				}
			}

			return total;
		}

		private RuntimeLibraryInfo GetVisionProInfo()
		{
			return ResolveRuntimeLibrary(
				"Cognex.VisionPro",
				"Cognex.VisionPro.dll",
				GetVisionProSearchFolders(),
				FormatVisionProVersion);
		}

		private RuntimeLibraryInfo GetHalconInfo()
		{
			return ResolveRuntimeLibrary(
				"HalconDotNet",
				"halcondotnet.dll",
				GetHalconSearchFolders(),
				FormatVersion);
		}

		private RuntimeLibraryInfo ResolveRuntimeLibrary(string assemblyName, string fileName, IEnumerable<string> searchFolders, Func<Version, string> versionFormatter)
		{
			Version version;
			if (TryGetLoadedAssemblyVersion(assemblyName, out version))
			{
				return new RuntimeLibraryInfo(FormatRuntimeVersion(version, versionFormatter), _isEnglish ? "Available" : "可用");
			}

			if (TryLoadAssemblyVersion(assemblyName, out version))
			{
				return new RuntimeLibraryInfo(FormatRuntimeVersion(version, versionFormatter), _isEnglish ? "Available" : "可用");
			}

			string path;
			if (TryFindAssemblyFile(fileName, searchFolders, out path))
			{
				try
				{
					AssemblyName name = AssemblyName.GetAssemblyName(path);
					return new RuntimeLibraryInfo(FormatRuntimeVersion(name.Version, versionFormatter), _isEnglish ? "Runtime found" : "已找到运行库");
				}
				catch
				{
					return new RuntimeLibraryInfo("-", _isEnglish ? "Runtime found" : "已找到运行库");
				}
			}

			return new RuntimeLibraryInfo("-", _isEnglish ? "Not detected" : "未检测到");
		}

		private bool TryGetLoadedAssemblyVersion(string assemblyName, out Version version)
		{
			version = null;

			try
			{
				Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
				foreach (Assembly assembly in assemblies)
				{
					if (assembly == null || assembly.GetName() == null)
					{
						continue;
					}

					if (string.Equals(assembly.GetName().Name, assemblyName, StringComparison.OrdinalIgnoreCase))
					{
						version = assembly.GetName().Version;
						return true;
					}
				}
			}
			catch
			{
			}

			return false;
		}

		private bool TryLoadAssemblyVersion(string assemblyName, out Version version)
		{
			version = null;

			try
			{
				Assembly assembly = Assembly.Load(assemblyName);
				if (assembly != null && assembly.GetName() != null)
				{
					version = assembly.GetName().Version;
					return true;
				}
			}
			catch
			{
			}

			return false;
		}

		private bool TryFindAssemblyFile(string fileName, IEnumerable<string> searchFolders, out string path)
		{
			path = string.Empty;
			if (string.IsNullOrWhiteSpace(fileName) || searchFolders == null)
			{
				return false;
			}

			foreach (string folder in searchFolders)
			{
				if (string.IsNullOrWhiteSpace(folder))
				{
					continue;
				}

				try
				{
					if (!Directory.Exists(folder))
					{
						continue;
					}

					string candidate = Path.Combine(folder, fileName);
					if (File.Exists(candidate))
					{
						path = candidate;
						return true;
					}
				}
				catch
				{
				}
			}

			return false;
		}

		private IEnumerable<string> GetVisionProSearchFolders()
		{
			List<string> folders = new List<string>();
			AddSearchFolder(folders, AppDomain.CurrentDomain.BaseDirectory);

			string programFiles = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);
			string programFilesX86 = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86);
			AddSearchFolder(folders, Path.Combine(programFiles, "Cognex", "VisionPro", "bin"));
			AddSearchFolder(folders, Path.Combine(programFiles, "Cognex", "VisionPro", "bin", "x64"));
			AddSearchFolder(folders, Path.Combine(programFiles, "Cognex", "VisionPro", "ReferencedAssemblies"));
			AddSearchFolder(folders, Path.Combine(programFilesX86, "Cognex", "VisionPro", "bin"));
			AddSearchFolder(folders, Path.Combine(programFilesX86, "Cognex", "VisionPro", "bin", "x64"));
			AddSearchFolder(folders, Path.Combine(programFilesX86, "Cognex", "VisionPro", "ReferencedAssemblies"));
			return folders;
		}

		private IEnumerable<string> GetHalconSearchFolders()
		{
			List<string> folders = new List<string>();
			AddSearchFolder(folders, AppDomain.CurrentDomain.BaseDirectory);
			AddHalconRootSearchFolders(folders, Environment.GetEnvironmentVariable("HALCONROOT"));
			AddHalconRootSearchFolders(folders, Environment.GetEnvironmentVariable("HALCON_ROOT"));
			AddHalconRootSearchFolders(folders, Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Programs", "MVTec", "HALCON-25.05-Progress"));
			return folders;
		}

		private void AddHalconRootSearchFolders(List<string> folders, string root)
		{
			if (string.IsNullOrWhiteSpace(root))
			{
				return;
			}

			AddSearchFolder(folders, Path.Combine(root, "bin", "dotnet35"));
			AddSearchFolder(folders, Path.Combine(root, "bin", "dotnet20"));
			AddSearchFolder(folders, Path.Combine(root, "bin"));
		}

		private void AddSearchFolder(List<string> folders, string folder)
		{
			if (folders == null || string.IsNullOrWhiteSpace(folder))
			{
				return;
			}

			if (!folders.Contains(folder))
			{
				folders.Add(folder);
			}
		}

		private string FormatVersion(Version version)
		{
			return version == null ? "-" : version.ToString();
		}

		private string FormatVisionProVersion(Version version)
		{
			if (version == null)
			{
				return "-";
			}

			if (version.Major == 71 && version.Minor == 2)
			{
				return "9.6SR2";
			}

			return version.ToString();
		}

		private string FormatRuntimeVersion(Version version, Func<Version, string> formatter)
		{
			if (formatter != null)
			{
				return formatter(version);
			}

			return FormatVersion(version);
		}

		private string GetOperatingSystemText()
		{
			try
			{
				return Environment.OSVersion.VersionString + (Environment.Is64BitOperatingSystem ? " x64" : " x86");
			}
			catch
			{
				return "-";
			}
		}

		private class RuntimeLibraryInfo
		{
			public string VersionText { get; private set; }
			public string StatusText { get; private set; }

			public RuntimeLibraryInfo(string versionText, string statusText)
			{
				VersionText = string.IsNullOrWhiteSpace(versionText) ? "-" : versionText;
				StatusText = string.IsNullOrWhiteSpace(statusText) ? "-" : statusText;
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
				_btnDiagnostics,
				_btnProgramManager,
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
				_btnDiagnostics.Width = width;
				_btnProgramManager.Width = width;
				_btnUserManager.Width = width;
				_btnSystemInfo.Width = width;
			}
		}
	}
}
