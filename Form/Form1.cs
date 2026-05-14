using System;
using System.Collections.Generic;
using System.Drawing;
using System.Reflection;
using System.Windows.Forms;

namespace Aron_V3
{
	public partial class Form1 : Form
	{
		private readonly List<CameraViewControl> _cameraViews = new List<CameraViewControl>();
		private bool _isEnglish = false;

		private Control _runtimePage;
		private Control _hardwareConfigPage;
		private FlowConfigForm _processConfigPage;
		private Control _algorithmPage;
		private Control _communicationPage;
		private Control _databasePage;
		private Control _systemPage;

		private bool _dragging;
		private Point _dragStartPoint;
		private Point _formStartPoint;

		private Timer _autoLogoutTimer;
		private ContextMenuStrip _userMenu;
		private UserActivityMessageFilter _activityMessageFilter;


		#region Run State

		private enum RunState
		{
			Offline = 0,
			RunningNoCommunication = 1,
			RunningReady = 2
		}

		private RunState _runState = RunState.Offline;

		// 这个变量代表“通讯是否已建立”。
		// 后续需要和真实 PLC / TCP / Profinet / S7 连接状态绑定。
		private bool _communicationConnected = false;

		// 定时刷新运行按钮状态。
		private Timer _runStatusTimer;

		private void InitRunStatusButton()
		{
			// 顶部导航中的 btnStop 不再显示，避免和右上角 lblRunStatus 重复。
			if (panelNavStop != null)
			{
				panelNavStop.Visible = false;
				panelNavStop.Width = 0;
			}

			if (btnStop != null)
			{
				btnStop.Visible = false;
				btnStop.Enabled = false;
				btnStop.Click -= btnRunStatus_Click;
			}

			if (underlineStop != null)
			{
				underlineStop.Visible = false;
			}

			// 右上角运行状态作为唯一运行/离线状态按钮。
			lblRunStatus.Cursor = Cursors.Hand;
			lblRunStatus.Click -= lblRunStatus_Click;
			lblRunStatus.Click += lblRunStatus_Click;

			lblRunStatus.Font = new Font("Microsoft YaHei UI", 10.5F, FontStyle.Bold);
			lblRunStatus.TextAlign = ContentAlignment.MiddleCenter;

			_runStatusTimer = new Timer();
			_runStatusTimer.Interval = 500;
			_runStatusTimer.Tick += runStatusTimer_Tick;
			_runStatusTimer.Start();

			SetRunState(RunState.Offline);
		}

		private void lblRunStatus_Click(object sender, EventArgs e)
		{
			ToggleRunState();
		}
		private void ToggleRunState()
		{
			if (_runState == RunState.Offline)
			{
				if (IsCommunicationConnected())
				{
					SetRunState(RunState.RunningReady);
				}
				else
				{
					SetRunState(RunState.RunningNoCommunication);
				}

				// TODO：这里放启动运行逻辑
				// StartRun();
			}
			else
			{
				SetRunState(RunState.Offline);

				// TODO：这里放停止/离线逻辑
				// StopRun();
			}
		}


		private void runStatusTimer_Tick(object sender, EventArgs e)
		{
			RefreshRunButtonByCommunicationState();
		}

		private void RefreshRunButtonByCommunicationState()
		{
			if (_runState == RunState.Offline)
			{
				return;
			}

			bool connected = IsCommunicationConnected();

			if (connected && _runState != RunState.RunningReady)
			{
				SetRunState(RunState.RunningReady);
			}
			else if (!connected && _runState != RunState.RunningNoCommunication)
			{
				SetRunState(RunState.RunningNoCommunication);
			}
			else
			{
				// 通讯状态没变化时也刷新一次文字，避免中英文切换后文字残留。
				ApplyRunStateStyle(_runState);
			}
		}

		private void SetRunState(RunState state)
		{
			_runState = state;

			if (btnStop.InvokeRequired)
			{
				btnStop.BeginInvoke(new Action(delegate
				{
					ApplyRunStateStyle(state);
				}));
			}
			else
			{
				ApplyRunStateStyle(state);
			}
		}

		private string GetRunButtonText(RunState state)
		{
			if (state == RunState.Offline)
			{
				return _isEnglish ? "● Offline" : "● 离线";
			}

			return _isEnglish ? "● Running" : "● 运行";
		}

		private string GetRunStatusText(RunState state)
		{
			if (state == RunState.Offline)
			{
				return _isEnglish ? "● Offline" : "● 离线";
			}

			return _isEnglish ? "● Running" : "● 运行中";
		}

		private void ApplyRunStateStyle(RunState state)
		{
			if (lblRunStatus == null)
			{
				return;
			}

			if (state == RunState.Offline)
			{
				lblRunStatus.Text = _isEnglish ? "● Offline" : "● 离线";
				lblRunStatus.ForeColor = Color.FromArgb(255, 85, 110);
				lblRunStatus.BackColor = Color.FromArgb(55, 10, 18);
			}
			else if (state == RunState.RunningNoCommunication)
			{
				lblRunStatus.Text = _isEnglish ? "● Running" : "● 运行";
				lblRunStatus.ForeColor = Color.FromArgb(255, 205, 70);
				lblRunStatus.BackColor = Color.FromArgb(60, 45, 10);
			}
			else
			{
				lblRunStatus.Text = _isEnglish ? "● Running" : "● 运行";
				lblRunStatus.ForeColor = Color.FromArgb(65, 220, 100);
				lblRunStatus.BackColor = Color.FromArgb(10, 55, 28);
			}

			lblRunStatus.Invalidate();
		}


		private bool IsCommunicationConnected()
		{
			// 这里先用 _communicationConnected。
			// 后续把它接到真实通讯模块即可。
			return _communicationConnected;
		}

		// 外部通讯模块连接成功/断开时调用这个方法即可。
		// 例如：PLC连接成功后 SetCommunicationConnected(true)
		//      PLC断开后 SetCommunicationConnected(false)
		public void SetCommunicationConnected(bool connected)
		{
			_communicationConnected = connected;
			RefreshRunButtonByCommunicationState();
		}

		#endregion


		private enum MainPageType
		{
			Login,
			HardwareConfig,
			AlgorithmConfig,
			ProcessConfig,
			CommunicationConfig,
			Database,
			SystemSetting,
			Stop
		}

		private MainPageType _currentPage = MainPageType.Login;

		public Form1()
		{
			InitializeComponent();

			EnableDoubleBuffer(this);

			_runtimePage = mainLayout;
			_runtimePage.Dock = DockStyle.Fill;

			BindTopBarDragEvents();

			LoadDemoData();
			BuildCameraLayout(4);

			SelectMainPage(MainPageType.Login, false);

			InitLoginSystem();
			InitRunStatusButton();
			PreCreateAlgorithmPageIfEnabled();
		}

		private void Form1_Load(object sender, EventArgs e)
		{
			this.WindowState = FormWindowState.Maximized;
			ShowRuntimePage();
		}

		#region Top Bar / Window

		private void BindTopBarDragEvents()
		{
			titlePanel.MouseDown += TopBar_MouseDown;
			titlePanel.MouseMove += TopBar_MouseMove;
			titlePanel.MouseUp += TopBar_MouseUp;
			titlePanel.DoubleClick += TopBar_DoubleClick;

			panelBrand.MouseDown += TopBar_MouseDown;
			panelBrand.MouseMove += TopBar_MouseMove;
			panelBrand.MouseUp += TopBar_MouseUp;
			panelBrand.DoubleClick += TopBar_DoubleClick;

			lblLogo.MouseDown += TopBar_MouseDown;
			lblLogo.MouseMove += TopBar_MouseMove;
			lblLogo.MouseUp += TopBar_MouseUp;
			lblLogo.DoubleClick += TopBar_DoubleClick;

			lblTitle.MouseDown += TopBar_MouseDown;
			lblTitle.MouseMove += TopBar_MouseMove;
			lblTitle.MouseUp += TopBar_MouseUp;
			lblTitle.DoubleClick += TopBar_DoubleClick;
		}

		private void TopBar_MouseDown(object sender, MouseEventArgs e)
		{
			if (e.Button != MouseButtons.Left)
				return;

			_dragging = true;
			_dragStartPoint = Cursor.Position;
			_formStartPoint = this.Location;
		}

		private void TopBar_MouseMove(object sender, MouseEventArgs e)
		{
			if (!_dragging)
				return;

			if (this.WindowState == FormWindowState.Maximized)
				return;

			Point offset = new Point(Cursor.Position.X - _dragStartPoint.X, Cursor.Position.Y - _dragStartPoint.Y);
			this.Location = new Point(_formStartPoint.X + offset.X, _formStartPoint.Y + offset.Y);
		}

		private void TopBar_MouseUp(object sender, MouseEventArgs e)
		{
			_dragging = false;
		}

		private void TopBar_DoubleClick(object sender, EventArgs e)
		{
			ToggleMaximize();
		}

		private void ToggleMaximize()
		{
			this.WindowState = this.WindowState == FormWindowState.Maximized
				? FormWindowState.Normal
				: FormWindowState.Maximized;
		}

		private void btnMinimize_Click(object sender, EventArgs e)
		{
			this.WindowState = FormWindowState.Minimized;
		}

		private void btnMaximize_Click(object sender, EventArgs e)
		{
			ToggleMaximize();
		}

		private void btnClose_Click(object sender, EventArgs e)
		{
			DialogResult result = MessageBox.Show(
				"Are you sure you want to exit the software?",
				"Exit",
				MessageBoxButtons.YesNo,
				MessageBoxIcon.Question);

			if (result == DialogResult.Yes)
				this.Close();
		}

		#endregion

		#region Smooth Page Switch

		private void SelectMainPage(MainPageType page, bool changePage)
		{
			_currentPage = page;

			ResetNavStyle(btnLogin, underlineLogin, false);
			ResetNavStyle(btnHardwareConfig, underlineHardwareConfig, false);
			ResetNavStyle(btnAlgorithmConfig, underlineAlgorithmConfig, false);
			ResetNavStyle(btnProcessConfig, underlineProcessConfig, false);
			ResetNavStyle(btnCommunicateConfig, underlineCommunicateConfig, false);
			ResetNavStyle(btnDatabase, underlineDatabase, false);
			ResetNavStyle(btnSystemSetting, underlineSystemSetting, false);

			// btnStop 现在是运行状态按钮，不再参与导航选中/取消选中。
			// 否则切换页面或切换语言时会把运行状态按钮颜色和文字覆盖掉。
			ApplyRunStateStyle(_runState);

			switch (page)
			{
				case MainPageType.Login:
					ApplyNavSelected(btnLogin, underlineLogin, false);
					if (changePage) ShowRuntimePage();
					break;

				case MainPageType.HardwareConfig:
					ApplyNavSelected(btnHardwareConfig, underlineHardwareConfig, false);
					if (changePage) ShowHardwareConfigPage();
					break;

				case MainPageType.AlgorithmConfig:
					ApplyNavSelected(btnAlgorithmConfig, underlineAlgorithmConfig, false);
					if (changePage) ShowAlgorithmPage();
					break;

				case MainPageType.ProcessConfig:
					ApplyNavSelected(btnProcessConfig, underlineProcessConfig, false);
					if (changePage) ShowProcessConfigPage();
					break;

				case MainPageType.CommunicationConfig:
					ApplyNavSelected(btnCommunicateConfig, underlineCommunicateConfig, false);
					if (changePage) ShowCommunicationPage();
					break;

				case MainPageType.Database:
					ApplyNavSelected(btnDatabase, underlineDatabase, false);
					if (changePage) ShowDatabasePage();
					break;

				case MainPageType.SystemSetting:
					ApplyNavSelected(btnSystemSetting, underlineSystemSetting, false);
					if (changePage) ShowSystemPage();
					break;
			}

			// 导航样式处理完后，再恢复一次运行状态按钮样式，防止被其它代码覆盖。
			ApplyRunStateStyle(_runState);
		}

		private void ResetNavStyle(Button button, Panel underline, bool isStopButton)
		{
			button.BackColor = Color.FromArgb(3, 10, 20);
			button.FlatAppearance.BorderSize = 0;
			button.ForeColor = isStopButton
				? Color.FromArgb(235, 54, 65)
				: Color.FromArgb(155, 170, 195);

			underline.Visible = false;
		}

		private void ApplyNavSelected(Button button, Panel underline, bool isStopButton)
		{
			button.BackColor = Color.FromArgb(5, 16, 32);
			button.ForeColor = isStopButton
				? Color.FromArgb(255, 80, 95)
				: Color.FromArgb(70, 170, 255);

			underline.Visible = true;
			underline.BringToFront();
		}

		private void HideAllPages()
		{
			pageHost.SuspendLayout();

			foreach (Control ctrl in pageHost.Controls)
				ctrl.Visible = false;

			pageHost.ResumeLayout(false);
		}

		private void ShowCachedPage(Control page)
		{
			if (page == null)
				return;

			pageHost.SuspendLayout();

			HideAllPages();

			if (page.Parent != pageHost)
				pageHost.Controls.Add(page);

			page.Dock = DockStyle.Fill;
			page.Visible = true;
			page.BringToFront();

			pageHost.ResumeLayout(true);
		}

		private void ShowRuntimePage()
		{
			ShowCachedPage(_runtimePage);
		}

		private void ShowHardwareConfigPage()
		{
			if (_hardwareConfigPage == null)
			{
				_hardwareConfigPage = CreatePlaceholderPage(_isEnglish ? "Hardware Configuration" : "硬件配置");
			}

			ShowCachedPage(_hardwareConfigPage);
		}

		private void ShowProcessConfigPage()
		{
			if (_processConfigPage == null || _processConfigPage.IsDisposed)
			{
				_processConfigPage = new FlowConfigForm();
				_processConfigPage.TopLevel = false;
				_processConfigPage.FormBorderStyle = FormBorderStyle.None;
				_processConfigPage.Dock = DockStyle.Fill;
				_processConfigPage.WindowState = FormWindowState.Normal;

				EnableDoubleBuffer(_processConfigPage);

				pageHost.Controls.Add(_processConfigPage);
				_processConfigPage.Show();
				_processConfigPage.Visible = false;

				_processConfigPage.ApplyLanguage(_isEnglish);
			}

			ShowCachedPage(_processConfigPage);
		}

		private void ShowAlgorithmPage()
		{
			if (_algorithmPage == null || _algorithmPage.IsDisposed)
			{
				_algorithmPage = new AlgorithmModuleControl();
			}

			ShowCachedPage(_algorithmPage);
		}

		private void ShowCommunicationPage()
		{
			if (_communicationPage == null)
			{
				CommunicationConfigControl page = new CommunicationConfigControl();
				page.Dock = DockStyle.Fill;
				_communicationPage = page;
				pageHost.Controls.Add(_communicationPage);
			}

			ShowCachedPage(_communicationPage);
			ApplyLanguageToPages();
		}

		private void ShowDatabasePage()
		{
			if (_databasePage == null)
				_databasePage = CreatePlaceholderPage(_isEnglish ? "Database" : "数据库");

			ShowCachedPage(_databasePage);
		}

		private void ShowSystemPage()
		{
			if (_systemPage == null)
				_systemPage = CreatePlaceholderPage(_isEnglish ? "System Settings" : "系统设置");

			ShowCachedPage(_systemPage);
		}

		private Control CreatePlaceholderPage(string title)
		{
			Panel panel = new Panel();
			panel.Dock = DockStyle.Fill;
			panel.BackColor = Color.FromArgb(5, 14, 28);

			Label lbl = new Label();
			lbl.Dock = DockStyle.Fill;
			lbl.Text = title + "\r\n\r\nPage content will be added here.";
			lbl.TextAlign = ContentAlignment.MiddleCenter;
			lbl.Font = new Font("Microsoft YaHei UI", 22F, FontStyle.Bold);
			lbl.ForeColor = Color.FromArgb(160, 195, 230);

			panel.Controls.Add(lbl);

			EnableDoubleBuffer(panel);
			return panel;
		}

		#endregion

		#region Double Buffer

		private void EnableDoubleBuffer(Control control)
		{
			if (control == null)
				return;

			try
			{
				PropertyInfo property = typeof(Control).GetProperty(
					"DoubleBuffered",
					BindingFlags.Instance | BindingFlags.NonPublic);

				if (property != null)
					property.SetValue(control, true, null);
			}
			catch
			{
			}

			foreach (Control child in control.Controls)
				EnableDoubleBuffer(child);
		}

		#endregion

		#region 中间相机区域：根据相机数量动态生成

		private void BuildCameraLayout(int cameraCount)
		{
			tableLayoutPanelCameras.SuspendLayout();

			tableLayoutPanelCameras.Controls.Clear();
			tableLayoutPanelCameras.RowStyles.Clear();
			tableLayoutPanelCameras.ColumnStyles.Clear();
			_cameraViews.Clear();

			int rows;
			int cols;
			GetCameraGridSize(cameraCount, out rows, out cols);

			tableLayoutPanelCameras.RowCount = rows;
			tableLayoutPanelCameras.ColumnCount = cols;

			for (int r = 0; r < rows; r++)
				tableLayoutPanelCameras.RowStyles.Add(new RowStyle(SizeType.Percent, 100f / rows));

			for (int c = 0; c < cols; c++)
				tableLayoutPanelCameras.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f / cols));

			for (int i = 0; i < cameraCount; i++)
			{
				CameraViewControl camView = CreateCameraView(i + 1);
				_cameraViews.Add(camView);

				int row = i / cols;
				int col = i % cols;

				tableLayoutPanelCameras.Controls.Add(camView, col, row);
			}

			tableLayoutPanelCameras.ResumeLayout();
		}

		private void GetCameraGridSize(int cameraCount, out int rows, out int cols)
		{
			if (cameraCount <= 1) { rows = 1; cols = 1; }
			else if (cameraCount == 2) { rows = 1; cols = 2; }
			else if (cameraCount <= 4) { rows = 2; cols = 2; }
			else if (cameraCount <= 6) { rows = 2; cols = 3; }
			else if (cameraCount <= 9) { rows = 3; cols = 3; }
			else if (cameraCount <= 12) { rows = 3; cols = 4; }
			else { rows = 4; cols = 4; }
		}

		private CameraViewControl CreateCameraView(int index)
		{
			CameraViewControl view = new CameraViewControl();
			view.Dock = DockStyle.Fill;
			view.Margin = new Padding(0, 0, 8, 8);

			switch (index)
			{
				case 1:
					view.SetTitle("相机01 - 读码");
					view.SetDisplayText("读码");
					view.SetResult(true);
					view.SetStatistics(32, 6);
					view.SetInfo("Job1", "Pos1", "Cam1");
					break;
				case 2:
					view.SetTitle("相机02 - 定位");
					view.SetDisplayText("定位");
					view.SetResult(true);
					view.SetStatistics(22, 22);
					view.SetInfo("Job1", "Pos2", "Cam1");
					break;
				case 3:
					view.SetTitle("相机03 - 表面检测");
					view.SetDisplayText("表面");
					view.SetResult(false);
					view.SetStatistics(25, 5);
					view.SetInfo("Job1", "Pos1", "Cam2");
					break;
				case 4:
					view.SetTitle("相机04 - 表面检测");
					view.SetDisplayText("表面");
					view.SetResult(false);
					view.SetStatistics(40, 4);
					view.SetInfo("Job1", "Pos1", "Cam2");
					break;
				case 5:
					view.SetTitle("相机05 - 备用视图");
					view.SetNoImage();
					view.SetStatistics(0, 0);
					view.SetInfo("Job--", "Pos--", "Cam--");
					break;
				case 6:
					view.SetTitle("相机06 - 读码");
					view.SetDisplayText("读码");
					view.SetResult(true);
					view.SetStatistics(35, 32);
					view.SetInfo("Job1", "Pos2", "Cam2");
					break;
				case 7:
					view.SetTitle("相机07 - 拔针检测");
					view.SetDisplayText("拔针");
					view.SetResult(true);
					view.SetStatistics(30, 29);
					view.SetInfo("Job1", "Pos1", "Cam1");
					break;
				case 8:
					view.SetTitle("相机08 - 定位");
					view.SetDisplayText("定位");
					view.SetResult(true);
					view.SetStatistics(28, 26);
					view.SetInfo("Job1", "Pos2", "Cam1");
					break;
				case 9:
					view.SetTitle("相机09 - 表面检测");
					view.SetDisplayText("表面");
					view.SetResult(true);
					view.SetStatistics(40, 38);
					view.SetInfo("Job1", "Pos1", "Cam2");
					break;
				default:
					view.SetTitle("相机" + index.ToString("00"));
					view.SetDisplayText("检测");
					view.SetResult(true);
					view.SetStatistics(0, 0);
					view.SetInfo("Job1", "Pos1", "Cam" + index);
					break;
			}

			return view;
		}

		private void ReloadCameraLayoutFromConfig()
		{
			BuildCameraLayout(9);
		}

		#endregion

		#region Demo Data

		private void LoadDemoData()
		{
			dgvResults.Rows.Clear();

			string[,] rows =
			{
				{ "Cam1", "读码结果", "OK", "09:31:16" },
				{ "Cam1", "二维码内容", "SN-20542", "09:31:15" },
				{ "Cam2", "定位X", "0.021 mm", "09:31:14" },
				{ "Cam2", "定位Y", "0.018 mm", "09:31:13" },
				{ "Cam2", "角度", "0.12°", "09:31:12" },
				{ "Cam3", "表面检测", "NG", "09:31:11" },
				{ "Cam3", "缺陷数量", "3", "09:31:10" },
				{ "Cam3", "最大面积", "0.86 mm²", "09:31:09" },
				{ "Cam4", "表面检测", "NG", "09:31:08" },
				{ "Cam6", "读码结果", "OK", "09:31:07" },
				{ "Cam7", "拔针检测", "OK", "09:31:06" },
				{ "Cam8", "定位结果", "OK", "09:31:05" },
				{ "Cam9", "表面检测", "OK", "09:31:04" }
			};

			for (int i = 0; i < rows.GetLength(0); i++)
			{
				int rowIndex = dgvResults.Rows.Add(rows[i, 0], rows[i, 1], rows[i, 2], rows[i, 3]);

				if (rows[i, 2] == "OK")
					dgvResults.Rows[rowIndex].Cells[2].Style.ForeColor = Color.FromArgb(65, 210, 70);
				else if (rows[i, 2] == "NG")
					dgvResults.Rows[rowIndex].Cells[2].Style.ForeColor = Color.FromArgb(235, 54, 65);
				else
					dgvResults.Rows[rowIndex].Cells[2].Style.ForeColor = Color.WhiteSmoke;
			}

			if (dgvResults.Rows.Count > 0)
				dgvResults.Rows[0].Selected = true;

			lstLog.Items.Clear();
			lstLog.Items.Add("2025-05-24   09:31:16.121   [INFO]  系统启动完成");
			lstLog.Items.Add("2025-05-24   09:31:16.132   [INFO]  打开项目: D:\\Projects\\DemoProject\\DemoProject.vision");
			lstLog.Items.Add("2025-05-24   09:31:16.256   [INFO]  相机 Cam1 连接成功");
			lstLog.Items.Add("2025-05-24   09:31:16.352   [INFO]  Task 1 图像采集与定位 执行完成 (10.00 ms)");
			lstLog.Items.Add("2025-05-24   09:31:16.421   [INFO]  Task 2 检测分析 执行完成");
			lstLog.Items.Add("2025-05-24   09:31:16.507   [OK]    Blob 分析: OK (数量: 0)");
			lstLog.Items.Add("2025-05-24   09:31:16.612   [INFO]  Hough 直线检测: OK (长度: 64.4 ms)");
			lstLog.Items.Add("2025-05-24   09:31:16.721   [NG]    Task 3 表面检测 发现缺陷 (数量: 3, 面积: 0.86 mm², 占比: 0.72%)");
		}

		#endregion

		#region Toolbar Events

		private void btnLogin_Click(object sender, EventArgs e) { SelectMainPage(MainPageType.Login, true); }
		private void btnAlgorithmConfig_Click(object sender, EventArgs e) { SelectMainPage(MainPageType.AlgorithmConfig, true); }
		private void btnProcessConfig_Click(object sender, EventArgs e) { SelectMainPage(MainPageType.ProcessConfig, true); }
		private void btnCommunicateConfig_Click(object sender, EventArgs e) { SelectMainPage(MainPageType.CommunicationConfig, true); }
		private void btnDatabase_Click(object sender, EventArgs e) { SelectMainPage(MainPageType.Database, true); }
		private void btnSystemSetting_Click(object sender, EventArgs e) { SelectMainPage(MainPageType.SystemSetting, true); }
		private void btnRunStatus_Click(object sender, EventArgs e)
		{
			ToggleRunState();
		}
		private void btnHardwareConfig_Click(object sender, EventArgs e)
		{
			SelectMainPage(MainPageType.HardwareConfig, true);
		}

		#endregion

		#region language function

		private void btnLanguage_Click(object sender, EventArgs e)
		{
			_isEnglish = !_isEnglish;

			if (_isEnglish)
			{
				btnLanguage.Text = "EN / 中文";

				btnLogin.Text = "⌂  Home";
				btnHardwareConfig.Text = "📷  Hardware";
				btnAlgorithmConfig.Text = "▣  Algorithm";
				btnProcessConfig.Text = "⚙  Process";
				btnCommunicateConfig.Text = "◇  Comm";
				btnDatabase.Text = "▤  Database";
				btnSystemSetting.Text = "⚙  System";

				lblLogTitle.Text = "Log";
				lblCameraStatus.Text = "▣  Camera: Connected";
				lblPlcStatus.Text = "▦  PLC: Connected";
				lblVersion.Text = "Version: 1.0.0.0";
			}
			else
			{
				btnLanguage.Text = "中文 / EN";

				btnLogin.Text = "⌂  主页";
				btnHardwareConfig.Text = "📷  硬件配置";
				btnAlgorithmConfig.Text = "▣  算法模块";
				btnProcessConfig.Text = "⚙  流程管理";
				btnCommunicateConfig.Text = "◇  通讯配置";
				btnDatabase.Text = "▤  数据库";
				btnSystemSetting.Text = "⚙  系统管理";

				lblLogTitle.Text = "Log日志";
				lblCameraStatus.Text = "▣  相机:  已连接";
				lblPlcStatus.Text = "▦  PLC:  已连接";
				lblVersion.Text = "版本号:  1.0.0.0";
			}

			// 不要在语言切换里直接写 btnStop.Text / lblRunStatus.Text。
			// 运行状态按钮必须由当前 _runState 统一刷新，否则会出现颜色和文字不匹配。
			UpdateLoginUi();
			ApplyRunStateStyle(_runState);

			SelectMainPage(_currentPage, false);
			ApplyLanguageToPages();

			// 页面语言刷新后再恢复一次运行状态，避免子页面刷新或导航刷新覆盖按钮样式。
			ApplyRunStateStyle(_runState);

		}

		private void ApplyLanguageToPages()
		{
			foreach (Control ctrl in pageHost.Controls)
			{
				ILocalizable localizable = ctrl as ILocalizable;
				if (localizable != null)
				{
					localizable.ApplyLanguage(_isEnglish);
				}

				foreach (Control child in ctrl.Controls)
				{
					ILocalizable childLocalizable = child as ILocalizable;
					if (childLocalizable != null)
					{
						childLocalizable.ApplyLanguage(_isEnglish);
					}
				}
			}
		}

		#endregion


		#region Login System

		private void InitLoginSystem()
		{
			UserAccountStore.LoadOrCreateDefault();

			_userMenu = new ContextMenuStrip();
			_userMenu.Items.Add("Change Password", null, menuChangePassword_Click);
			_userMenu.Items.Add("User Management", null, menuUserManagement_Click);
			_userMenu.Items.Add(new ToolStripSeparator());
			_userMenu.Items.Add("Logout", null, menuLogout_Click);

			_autoLogoutTimer = new Timer();
			_autoLogoutTimer.Interval = 10000;
			_autoLogoutTimer.Tick += autoLogoutTimer_Tick;
			_autoLogoutTimer.Start();

			// 全局鼠标/键盘操作监听。
			// 只要有鼠标或键盘操作，就会更新 LoginSession.LastActiveTime。
			// 自动注销只判断“连续无操作时间”。
			_activityMessageFilter = new UserActivityMessageFilter();
			Application.AddMessageFilter(_activityMessageFilter);

			// 你当前顶部右侧是 lblUser。
			lblUser.Cursor = Cursors.Hand;
			lblUser.Text = "♟  Guest ▾";
			lblUser.Click -= lblUser_Click;
			lblUser.Click += lblUser_Click;

			this.FormClosed -= Form1_FormClosed_RemoveLoginFilter;
			this.FormClosed += Form1_FormClosed_RemoveLoginFilter;

			UpdateLoginUi();
		}

		private void Form1_FormClosed_RemoveLoginFilter(object sender, FormClosedEventArgs e)
		{
			if (_activityMessageFilter != null)
			{
				Application.RemoveMessageFilter(_activityMessageFilter);
				_activityMessageFilter = null;
			}
		}

		private void lblUser_Click(object sender, EventArgs e)
		{
			if (!LoginSession.IsLoggedIn)
			{
				LoginForm form = new LoginForm();

				if (form.ShowDialog(this) == DialogResult.OK)
				{
					LoginSession.Login(form.LoginUser);
					UpdateLoginUi();
				}

				return;
			}

			_userMenu.Items[1].Enabled = LoginSession.Permission.CanUserManagement;
			_userMenu.Show(lblUser, new Point(0, lblUser.Height));
		}

		private void menuChangePassword_Click(object sender, EventArgs e)
		{
			ChangePasswordForm form = new ChangePasswordForm();
			form.ShowDialog(this);
		}

		private void menuUserManagement_Click(object sender, EventArgs e)
		{
			if (!LoginSession.Permission.CanUserManagement)
			{
				MessageBox.Show("No permission.", "User Management", MessageBoxButtons.OK, MessageBoxIcon.Warning);
				return;
			}

			UserManagerForm form = new UserManagerForm();
			form.ShowDialog(this);
		}

		private void menuLogout_Click(object sender, EventArgs e)
		{
			LogoutCurrentUser(false);
		}

		private void autoLogoutTimer_Tick(object sender, EventArgs e)
		{
			if (!LoginSession.IsLoggedIn)
			{
				return;
			}

			UserAccountConfig config = UserAccountStore.LoadOrCreateDefault();
			int minutes = config.AutoLogoutMinutes <= 0 ? 30 : config.AutoLogoutMinutes;

			TimeSpan idleTime = DateTime.Now - LoginSession.LastActiveTime;

			if (idleTime.TotalMinutes >= minutes)
			{
				LogoutCurrentUser(true);
			}
		}

		private void LogoutCurrentUser(bool autoLogout)
		{
			LoginSession.Logout();
			UpdateLoginUi();
		}

		private void UpdateLoginUi()
		{
			if (LoginSession.IsLoggedIn)
			{
				lblUser.Text = "♟  " + LoginSession.CurrentUser.UserName + " ▾";
			}
			else
			{
				lblUser.Text = "♟  Guest ▾";
			}

			ApplyPermissionToUi();
		}

		private void ApplyPermissionToUi()
		{
			UserPermission p = LoginSession.Permission;

			btnHardwareConfig.Enabled = p.CanHardwareConfig;
			btnAlgorithmConfig.Enabled = p.CanAlgorithmConfig;
			btnProcessConfig.Enabled = p.CanFlowConfig;
			btnCommunicateConfig.Enabled = p.CanCommunicationConfig;
			btnDatabase.Enabled = p.CanDatabaseConfig;
			btnSystemSetting.Enabled = p.CanSystemConfig;
		}


		#endregion

		#region Algorithm Page Preload
		private void PreCreateAlgorithmPageIfEnabled()
		{
			try
			{
				AlgorithmModuleConfig config = AlgorithmModuleConfigStore.LoadOrCreateDefault();

				if (config.EnableVpp || config.EnableScript || config.EnableHdev || config.EnableVM)
				{
					if (_algorithmPage == null || _algorithmPage.IsDisposed)
					{
						_algorithmPage = new AlgorithmModuleControl();
					}

					AlgorithmModuleControl algorithmControl = _algorithmPage as AlgorithmModuleControl;

					if (algorithmControl != null)
					{
						algorithmControl.StartPreloadIfNeeded();
					}
				}
			}
			catch
			{
				// 预加载失败不能影响主程序启动
			}
		}
		#endregion

	}
}
