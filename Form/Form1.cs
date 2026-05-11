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
			ResetNavStyle(btnStop, underlineStop, true);

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

				case MainPageType.Stop:
					ApplyNavSelected(btnStop, underlineStop, true);
					if (changePage)
						MessageBox.Show("Stop clicked.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Warning);
					break;
			}
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
			if (_algorithmPage == null)
				_algorithmPage = CreatePlaceholderPage(_isEnglish ? "Algorithm Configuration" : "算法配置");

			ShowCachedPage(_algorithmPage);
		}

		private void ShowCommunicationPage()
		{
			if (_communicationPage == null)
				_communicationPage = CreatePlaceholderPage(_isEnglish ? "Communication Configuration" : "通讯配置");

			ShowCachedPage(_communicationPage);
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
		private void btnStop_Click(object sender, EventArgs e) { SelectMainPage(MainPageType.Stop, true); }
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
				btnAlgorithmConfig.Text = "▣  Algorithm";
				btnProcessConfig.Text = "⚙  Process";
				btnCommunicateConfig.Text = "◇  Comm";
				btnDatabase.Text = "▤  Database";
				btnSystemSetting.Text = "⚙  System";
				btnStop.Text = "□  Stop";
				btnHardwareConfig.Text = "📷  Hardware";

				lblLogTitle.Text = "Log";
				lblCameraStatus.Text = "▣  Camera: Connected";
				lblPlcStatus.Text = "▦  PLC: Connected";
				lblVersion.Text = "Version: 1.0.0.0";
				lblRunStatus.Text = "●  Running";
				lblUser.Text = "♟  admin ▾";
			}
			else
			{
				btnLanguage.Text = "中文 / EN";

				btnLogin.Text = "⌂  主页";
				btnAlgorithmConfig.Text = "▣  算法管理";
				btnProcessConfig.Text = "⚙  流程管理";
				btnCommunicateConfig.Text = "◇  通讯配置";
				btnDatabase.Text = "▤  数据库";
				btnSystemSetting.Text = "⚙  系统管理";
				btnStop.Text = "□  停止";
				btnHardwareConfig.Text = "📷 硬件配置";

				lblLogTitle.Text = "Log日志";
				lblCameraStatus.Text = "▣  相机:  已连接";
				lblPlcStatus.Text = "▦  PLC:  已连接";
				lblVersion.Text = "版本号:  1.0.0.0";
				lblRunStatus.Text = "●  运行中";
				lblUser.Text = "♟  admin ▾";
			}

			SelectMainPage(_currentPage, false);
			ApplyLanguageToPages();
			SelectMainPage(_currentPage, false);
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
	}
}
