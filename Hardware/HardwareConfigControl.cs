using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Aron_V3
{
	public partial class HardwareConfigControl : UserControl, ILocalizable
	{
		private readonly Color _back = Color.FromArgb(2, 10, 20);
		private readonly Color _panel = Color.FromArgb(3, 14, 27);
		private readonly Color _panel2 = Color.FromArgb(5, 18, 34);
		private readonly Color _accent = Color.FromArgb(0, 150, 220);
		private readonly Color _text = Color.FromArgb(220, 235, 245);
		private readonly Color _muted = Color.FromArgb(130, 155, 175);
		private readonly Color _green = Color.FromArgb(55, 210, 95);

		private HardwareProjectConfig _config;
		private CameraDeviceConfig _currentCamera;
		private object _currentVproAcqTool;
		private bool _loading;
		private bool _isLoadingVpro;
		private bool _isEnglish;

		private TableLayoutPanel _root;
		private Panel _leftPanel;
		private Panel _cameraListHost;
		private GroupBox _rightGroup;
		private Label _lblCurrentCamera;
		private Panel _modeHost;
		private Label _lblBottomStatus;
		private Label _lblCameraTitle;
		private Button _btnAddCamera;
		private Button _btnDeleteCamera;
		private Button _btnRefreshCamera;
		private Button _btnBottomRefresh;
		private Button _btnBottomSave;
		private Button _btnBottomApply;

		private Panel _vproEditorHost;
		private Button _btnLoadVpro;
		private Button _btnNewVpro;
		private Button _btnSaveVpro;
		private Button _btnLoadSdk;
		private Button _btnNewSdk;
		private Button _btnSaveSdk;

		private ISdkCameraConfigPanel _currentSdkPanel;

		public HardwareConfigControl()
		{
			InitializeUi();
			EnableDoubleBuffer(this);
			LoadConfigToUi();
		}

		private void InitializeUi()
		{
			this.BackColor = _back;
			this.Dock = DockStyle.Fill;
			this.Font = new Font("Microsoft YaHei UI", 9F);

			_root = new TableLayoutPanel();
			_root.Dock = DockStyle.Fill;
			_root.Margin = new Padding(0);
			_root.Padding = new Padding(8);
			_root.BackColor = _back;
			_root.ColumnCount = 3;
			_root.RowCount = 2;
			_root.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 290F));
			_root.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 10F));
			_root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
			_root.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
			_root.RowStyles.Add(new RowStyle(SizeType.Absolute, 48F));

			_leftPanel = CreatePanel();
			_leftPanel.Padding = new Padding(12);
			BuildLeftCameraPanel();

			_rightGroup = CreateGroupBox(T("相机配置", "Camera Config"));
			_rightGroup.Padding = new Padding(10);
			BuildRightContent();

			Panel bottom = BuildBottomBar();

			_root.Controls.Add(_leftPanel, 0, 0);
			_root.Controls.Add(_rightGroup, 2, 0);
			_root.Controls.Add(bottom, 0, 1);
			_root.SetColumnSpan(bottom, 3);

			this.Controls.Add(_root);
		}

		private void BuildLeftCameraPanel()
		{
			TableLayoutPanel layout = new TableLayoutPanel();
			layout.Dock = DockStyle.Fill;
			layout.ColumnCount = 1;
			layout.RowCount = 4;
			layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 36F));
			layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 38F));
			layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
			layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 28F));
			layout.BackColor = _panel;

			_lblCameraTitle = CreateTitleLabel(T("相机管理", "Camera Management"));

			Panel toolbar = new Panel();
			toolbar.Dock = DockStyle.Fill;
			toolbar.BackColor = _panel;

			_btnAddCamera = CreateActionButton(T("+ 添加相机", "+ Add"), 0, 0, 92);
			_btnDeleteCamera = CreateActionButton(T("删除", "Delete"), 98, 0, 72);
			_btnRefreshCamera = CreateActionButton(T("刷新", "Refresh"), 176, 0, 72);

			_btnAddCamera.Click += btnAddCamera_Click;
			_btnDeleteCamera.Click += btnDeleteCamera_Click;
			_btnRefreshCamera.Click += delegate { LoadConfigToUi(); };

			toolbar.Controls.Add(_btnAddCamera);
			toolbar.Controls.Add(_btnDeleteCamera);
			toolbar.Controls.Add(_btnRefreshCamera);

			_cameraListHost = new Panel();
			_cameraListHost.Dock = DockStyle.Fill;
			_cameraListHost.BackColor = _panel;
			_cameraListHost.AutoScroll = true;

			Label foot = new Label();
			foot.Dock = DockStyle.Fill;
			foot.ForeColor = _muted;
			foot.TextAlign = ContentAlignment.MiddleLeft;
			foot.Name = "lblCameraCount";

			layout.Controls.Add(_lblCameraTitle, 0, 0);
			layout.Controls.Add(toolbar, 0, 1);
			layout.Controls.Add(_cameraListHost, 0, 2);
			layout.Controls.Add(foot, 0, 3);

			_leftPanel.Controls.Add(layout);
		}

		private void BuildRightContent()
		{
			TableLayoutPanel root = new TableLayoutPanel();
			root.Dock = DockStyle.Fill;
			root.BackColor = _panel;
			root.ColumnCount = 1;
			root.RowCount = 3;
			root.RowStyles.Add(new RowStyle(SizeType.Absolute, 60F));
			root.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
			root.RowStyles.Add(new RowStyle(SizeType.Absolute, 34F));

			Panel header = new Panel();
			header.Dock = DockStyle.Fill;
			header.BackColor = _panel2;
			header.Padding = new Padding(12, 8, 12, 8);

			_lblCurrentCamera = new Label();
			_lblCurrentCamera.Dock = DockStyle.Fill;
			_lblCurrentCamera.TextAlign = ContentAlignment.MiddleLeft;
			_lblCurrentCamera.Font = new Font("Microsoft YaHei UI", 10F, FontStyle.Bold);
			_lblCurrentCamera.ForeColor = _text;

			header.Controls.Add(_lblCurrentCamera);

			_modeHost = new Panel();
			_modeHost.Dock = DockStyle.Fill;
			_modeHost.BackColor = _panel;

			_lblBottomStatus = new Label();
			_lblBottomStatus.Dock = DockStyle.Fill;
			_lblBottomStatus.TextAlign = ContentAlignment.MiddleLeft;
			_lblBottomStatus.ForeColor = _muted;
			_lblBottomStatus.Font = new Font("Microsoft YaHei UI", 9F, FontStyle.Bold);

			root.Controls.Add(header, 0, 0);
			root.Controls.Add(_modeHost, 0, 1);
			root.Controls.Add(_lblBottomStatus, 0, 2);

			_rightGroup.Controls.Add(root);
		}

		private Panel BuildBottomBar()
		{
			Panel bottom = new Panel();
			bottom.Dock = DockStyle.Fill;
			bottom.BackColor = _panel;
			bottom.Padding = new Padding(12, 6, 12, 6);

			_btnBottomRefresh = CreateBottomButton(T("刷新设备", "Refresh"));
			_btnBottomRefresh.Click += delegate { LoadConfigToUi(); };

			_btnBottomSave = CreateBottomButton(T("保存配置", "Save"));
			_btnBottomSave.Click += delegate
			{
				if (!HardwareConfigStore.HasCurrentJob)
				{
					MessageBox.Show(T("请先创建或选择程序号。硬件文件会保存到 Project\\Job\\[JobName]\\Hardware。", "Please create or select a Job first. Hardware files are saved under Project\\Job\\[JobName]\\Hardware."), "Hardware", MessageBoxButtons.OK, MessageBoxIcon.Information);
					return;
				}

				SaveCurrentCameraFromUi();
				HardwareConfigStore.Save(_config);
				MessageBox.Show(T("硬件配置已保存。", "Hardware configuration saved."), "Hardware", MessageBoxButtons.OK, MessageBoxIcon.Information);
			};

			_btnBottomApply = CreateBottomButton(T("应用", "Apply"));
			_btnBottomApply.BackColor = Color.FromArgb(0, 95, 220);
			_btnBottomApply.Click += delegate { SaveCurrentCameraFromUi(); };

			bottom.Controls.Add(_btnBottomApply);
			bottom.Controls.Add(CreateRightGap());
			bottom.Controls.Add(_btnBottomSave);
			bottom.Controls.Add(CreateRightGap());
			bottom.Controls.Add(_btnBottomRefresh);

			return bottom;
		}

		private void LoadConfigToUi()
		{
			_loading = true;

			try
			{
				HardwareConfigStore.AutoSelectFirstJobIfNeeded();
				_config = HardwareConfigStore.LoadOrCreateDefault();
				RebuildCameraCards();

				if (_config.Cameras.Count > 0)
				{
					SelectCamera(_config.Cameras[0], false);
				}
				else
				{
					_currentCamera = null;
					_currentVproAcqTool = null;
					_currentSdkPanel = null;
					ShowEmptyHardwarePlaceholder();
				}
			}
			finally
			{
				_loading = false;
			}
		}


		private void ShowEmptyHardwarePlaceholder()
		{
			string jobName = HardwareConfigStore.CurrentJobName;

			if (_lblCurrentCamera != null)
			{
				if (string.IsNullOrWhiteSpace(jobName))
				{
					_lblCurrentCamera.Text = T("当前未选择 Job，请先在流程管理中新增或选择 Job。", "No Job is selected. Please create or select a Job in Process Management first.");
				}
				else
				{
					_lblCurrentCamera.Text = T("当前 Job： ", "Current Job: ") + jobName + T("    |    尚未创建相机", "    |    No camera created");
				}
			}

			if (_modeHost != null)
			{
				_modeHost.Controls.Clear();

				Label label = new Label();
				label.Dock = DockStyle.Fill;
				label.TextAlign = ContentAlignment.MiddleCenter;
				label.ForeColor = _muted;
				label.Font = new Font("Microsoft YaHei UI", 11F, FontStyle.Bold);

				if (string.IsNullOrWhiteSpace(jobName))
				{
					label.Text = string.Concat(
						T("当前 Project\\Job 下没有可用 Job。", "No available Job under Project\\Job."),
						Environment.NewLine,
						T("硬件配置不会自动创建 Cam1，也不会自动创建 Job_001。", "Hardware configuration will not auto-create Cam1 or Job_001."),
						Environment.NewLine,
						T("请先在流程管理中点击 + 新增 Job，再进入硬件配置添加相机。", "Please add a Job in Process Management first, then add cameras here."));
				}
				else
				{
					label.Text = string.Concat(
						T("当前 Job：", "Current Job: "),
						jobName,
						Environment.NewLine,
						T("尚未创建相机。", "No camera created."),
						Environment.NewLine,
						T("点击左侧“+ 添加相机”后，配置会保存到：", "After clicking \"+ Add\", the configuration will be saved to:"),
						Environment.NewLine,
						"Project\\Job\\",
						jobName,
						"\\Hardware");
				}

				_modeHost.Controls.Add(label);
			}

			if (_lblBottomStatus != null)
			{
				if (string.IsNullOrWhiteSpace(jobName))
				{
					_lblBottomStatus.Text = T("当前未选择 Job，Hardware 不会生成默认 Cam1。", "No Job is selected. Hardware will not create default Cam1.");
				}
				else
				{
					_lblBottomStatus.Text = T("当前 Job：", "Current Job: ") + jobName + T("    |    Hardware 路径：", "    |    Hardware Path: ") + HardwareConfigStore.ConfigFolder;
				}
			}
		}

		private void RebuildCameraCards()
		{
			_cameraListHost.Controls.Clear();

			if (_config == null || _config.Cameras == null)
			{
				return;
			}

			for (int i = _config.Cameras.Count - 1; i >= 0; i--)
			{
				Panel card = CreateCameraCard(_config.Cameras[i]);
				_cameraListHost.Controls.Add(card);
			}

			Label foot = FindLabel(_leftPanel, "lblCameraCount");
			if (foot != null)
			{
				foot.Text = _isEnglish ? _config.Cameras.Count + " camera(s)" : "共 " + _config.Cameras.Count + " 台相机";
			}
		}

		private Panel CreateCameraCard(CameraDeviceConfig camera)
		{
			Panel card = new Panel();
			card.Dock = DockStyle.Top;
			card.Height = camera.AcquisitionMode == CameraAcquisitionMode.SDK ? 142 : 112;
			card.Padding = new Padding(10);
			card.BackColor = camera == _currentCamera ? Color.FromArgb(6, 32, 58) : _panel2;
			card.BorderStyle = BorderStyle.FixedSingle;
			card.Tag = camera;
			card.DoubleClick += cameraCard_DoubleClick;

			Label icon = new Label();
			icon.Text = "▣";
			icon.ForeColor = _text;
			icon.Font = new Font("Microsoft YaHei UI", 13F, FontStyle.Bold);
			icon.Location = new Point(16, 14);
			icon.Size = new Size(24, 24);
			icon.Tag = camera;
			icon.DoubleClick += cameraCard_DoubleClick;

			Label name = new Label();
			name.Text = camera.CameraName;
			name.ForeColor = _text;
			name.Font = new Font("Microsoft YaHei UI", 11F, FontStyle.Bold);
			name.Location = new Point(44, 14);
			name.Size = new Size(115, 26);
			name.Tag = camera;
			name.DoubleClick += cameraCard_DoubleClick;

			Label statusDot = new Label();
			statusDot.Text = "●";
			statusDot.ForeColor = camera.Status == "Connected" ? _green : Color.Gray;
			statusDot.Location = new Point(card.Width - 30, 18);
			statusDot.Anchor = AnchorStyles.Top | AnchorStyles.Right;
			statusDot.Size = new Size(20, 20);

			Label lblMode = SmallLabel(T("采集模式", "Mode"), 18, 50, 75);
			ComboBox cmbMode = SmallCombo(95, 47, 145);
			cmbMode.Items.Add(CameraAcquisitionMode.VPro.ToString());
			cmbMode.Items.Add(CameraAcquisitionMode.SDK.ToString());
			cmbMode.SelectedItem = camera.AcquisitionMode.ToString();
			cmbMode.Tag = camera;
			cmbMode.SelectedIndexChanged += cmbMode_SelectedIndexChanged;

			Label lblStatusName = SmallLabel(T("状态", "Status"), 18, 82, 75);
			Label lblStatusValue = SmallLabel(camera.Status == "Connected" ? T("● 已连接", "● Connected") : T("● 未连接", "● Disconnected"), 95, 82, 145);
			lblStatusValue.ForeColor = camera.Status == "Connected" ? _green : Color.Gray;

			card.Controls.Add(icon);
			card.Controls.Add(name);
			card.Controls.Add(statusDot);
			card.Controls.Add(lblMode);
			card.Controls.Add(cmbMode);
			card.Controls.Add(lblStatusName);
			card.Controls.Add(lblStatusValue);

			if (camera.AcquisitionMode == CameraAcquisitionMode.SDK)
			{
				Label lblBrand = SmallLabel(T("SDK品牌", "SDK Brand"), 18, 78, 75);
				ComboBox cmbBrand = SmallCombo(95, 75, 145);
				cmbBrand.Items.Add(CameraSdkBrand.LMI.ToString());
				cmbBrand.Items.Add(CameraSdkBrand.Keyence.ToString());
				cmbBrand.Items.Add(CameraSdkBrand.Hikvision.ToString());
				cmbBrand.Items.Add(CameraSdkBrand.Dahua.ToString());
				cmbBrand.SelectedItem = camera.SdkBrand.ToString();
				cmbBrand.Tag = camera;
				cmbBrand.SelectedIndexChanged += cmbBrand_SelectedIndexChanged;

				lblStatusName.Location = new Point(18, 112);
				lblStatusValue.Location = new Point(95, 112);

				card.Controls.Add(lblBrand);
				card.Controls.Add(cmbBrand);
			}

			return card;
		}

		private void cameraCard_DoubleClick(object sender, EventArgs e)
		{
			Control c = sender as Control;
			CameraDeviceConfig camera = c == null ? null : c.Tag as CameraDeviceConfig;

			if (camera == null)
			{
				return;
			}

			SelectCamera(camera, false);
		}

		private void SelectCamera(CameraDeviceConfig camera, bool loadTool)
		{
			if (camera == null)
			{
				return;
			}

			SaveCurrentCameraFromUi();

			_currentCamera = camera;
			RebuildCameraCards();

			_lblCurrentCamera.Text = T("当前选中： ", "Selected: ") + camera.CameraName + T("    |    采集模式： ", "    |    Mode: ") + camera.AcquisitionMode;

			if (camera.AcquisitionMode == CameraAcquisitionMode.VPro)
			{
				ShowVproMode(camera);
			}
			else
			{
				ShowSdkMode(camera);
			}

			_lblBottomStatus.Text =
				T("当前相机：", "Current Camera: ") + camera.CameraName +
				T("    |    图像源：", "    |    Image Source: ") + camera.CameraName + ".Raw" +
				"    |    ImageSources：" + HardwareConfigStore.ImageSourceConfigPath;

			if (loadTool && camera.AcquisitionMode == CameraAcquisitionMode.VPro)
			{
				LoadVproEditorForCurrentCamera(false);
			}
		}

		private void ShowVproMode(CameraDeviceConfig camera)
		{
			_modeHost.SuspendLayout();
			_modeHost.Controls.Clear();

			TableLayoutPanel root = new TableLayoutPanel();
			root.Dock = DockStyle.Fill;
			root.BackColor = _panel;
			root.Padding = new Padding(8);
			root.ColumnCount = 1;
			root.RowCount = 2;
			root.RowStyles.Add(new RowStyle(SizeType.Absolute, 42F));
			root.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));

			Panel toolBar = new Panel();
			toolBar.Dock = DockStyle.Fill;
			toolBar.BackColor = _panel;

			_btnLoadVpro = CreateActionButton(T("加载 CogAcq", "Load CogAcq"), 0, 5, 120);
			_btnNewVpro = CreateActionButton(T("新建工具", "New Tool"), 130, 5, 100);
			_btnSaveVpro = CreateActionButton(T("保存工具", "Save Tool"), 240, 5, 100);

			_btnLoadVpro.Click += delegate { LoadVproEditorForCurrentCamera(true); };
			_btnNewVpro.Click += delegate { CreateNewVproToolForCurrentCamera(); };
			_btnSaveVpro.Click += delegate { SaveCurrentVproAcqTool(); };

			toolBar.Controls.Add(_btnLoadVpro);
			toolBar.Controls.Add(_btnNewVpro);
			toolBar.Controls.Add(_btnSaveVpro);

			_vproEditorHost = CreatePanel();
			_vproEditorHost.Padding = new Padding(10);

			ShowVproPlaceholder(
				string.Concat(
					T("已选择 VPro 相机。", "VPro camera selected."),
					Environment.NewLine,
					T("右侧仅显示 VisionPro 取像工具区域。", "The right panel only shows the VisionPro acquisition tool area."),
					Environment.NewLine,
					T("点击加载 CogAcq 后从本地选择 VPP，并导入当前相机目录后加载。", "Click Load CogAcq to select a local VPP, import it into the current camera folder, and load it.")));

			root.Controls.Add(toolBar, 0, 0);
			root.Controls.Add(_vproEditorHost, 0, 1);

			_modeHost.Controls.Add(root);
			_modeHost.ResumeLayout(true);
		}

		private void ShowSdkMode(CameraDeviceConfig camera)
		{
			_modeHost.SuspendLayout();
			_modeHost.Controls.Clear();

			TableLayoutPanel root = new TableLayoutPanel();
			root.Dock = DockStyle.Fill;
			root.BackColor = _panel;
			root.Padding = new Padding(8);
			root.ColumnCount = 1;
			root.RowCount = 2;
			root.RowStyles.Add(new RowStyle(SizeType.Absolute, 42F));
			root.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));

			Panel toolBar = new Panel();
			toolBar.Dock = DockStyle.Fill;
			toolBar.BackColor = _panel;

			_btnLoadSdk = CreateActionButton(T("加载取像参数", "Load Params"), 0, 5, 125);
			_btnNewSdk = CreateActionButton(T("新建工具", "New Tool"), 135, 5, 100);
			_btnSaveSdk = CreateActionButton(T("保存工具", "Save Tool"), 245, 5, 100);

			_btnLoadSdk.Click += delegate { LoadSdkToolForCurrentCamera(); };
			_btnNewSdk.Click += delegate { CreateNewSdkToolForCurrentCamera(); };
			_btnSaveSdk.Click += delegate { SaveSdkToolForCurrentCamera(); };

			toolBar.Controls.Add(_btnLoadSdk);
			toolBar.Controls.Add(_btnNewSdk);
			toolBar.Controls.Add(_btnSaveSdk);

			_currentSdkPanel = SdkCameraPanelFactory.CreatePanel(camera.SdkBrand);
			_currentSdkPanel.LoadCamera(camera);

			Control sdkView = _currentSdkPanel.View;
			sdkView.Dock = DockStyle.Fill;

			root.Controls.Add(toolBar, 0, 0);
			root.Controls.Add(sdkView, 0, 1);

			_modeHost.Controls.Add(root);
			_modeHost.ResumeLayout(true);
		}

		private void SaveCurrentCameraFromUi()
		{
			if (_loading || _currentCamera == null)
			{
				return;
			}

			if (_currentSdkPanel != null && _currentCamera.AcquisitionMode == CameraAcquisitionMode.SDK)
			{
				_currentSdkPanel.SaveCamera(_currentCamera);
				HardwareConfigStore.SaveSdkConfig(_currentCamera);
			}

			HardwareConfigStore.Save(_config);
			RebuildCameraCards();
		}

		private async void LoadVproEditorForCurrentCamera(bool showError)
		{
			if (_isLoadingVpro || _currentCamera == null)
			{
				return;
			}

			if (_currentCamera.AcquisitionMode != CameraAcquisitionMode.VPro)
			{
				return;
			}

			string selectedFile = string.Empty;

			using (OpenFileDialog dialog = new OpenFileDialog())
			{
				dialog.Title = "Select VisionPro CogAcq VPP";
				dialog.Filter = "VisionPro VPP (*.vpp)|*.vpp|All files (*.*)|*.*";
				dialog.Multiselect = false;

				string initDir = HardwareConfigStore.GetVisionProFolder(_currentCamera.CameraName, _currentCamera.AcqProfileName);
				if (Directory.Exists(initDir))
				{
					dialog.InitialDirectory = initDir;
				}

				if (dialog.ShowDialog(this) != DialogResult.OK)
				{
					return;
				}

				selectedFile = dialog.FileName;
			}

			string toolName = Path.GetFileNameWithoutExtension(selectedFile);
			string targetFolder = HardwareConfigStore.GetVisionProFolder(_currentCamera.CameraName, _currentCamera.AcqProfileName);
			string projectPath;

			try
			{
				projectPath = CopyImportedFileToProjectLocal(
					selectedFile,
					targetFolder,
					toolName,
					".vpp",
					true);
			}
			catch (Exception ex)
			{
				MessageBox.Show("Import VPP failed: " + ex.Message, "Load CogAcq", MessageBoxButtons.OK, MessageBoxIcon.Warning);
				return;
			}

			_currentCamera.VisionPro.ToolName = Path.GetFileNameWithoutExtension(projectPath);
			_currentCamera.VisionPro.AcqVppPath = projectPath;
			HardwareConfigStore.Save(_config);

			_isLoadingVpro = true;

			ShowVproPlaceholder(
				string.Concat(
					"已导入 VPP 到当前相机目录，正在后台加载 VisionPro CogAcq 工具，请稍候。",
					Environment.NewLine,
					Environment.NewLine,
					projectPath));

			try
			{
				object loadedTool = await Task.Run<object>(delegate
				{
					return VisionProReflectionHelper.LoadObjectFromFile(projectPath);
				});

				if (loadedTool == null)
				{
					ShowVproPlaceholder(
						string.Concat(
							"VPP 已导入，但未能加载 CogAcqFifoTool。",
							Environment.NewLine,
							"请确认选择的 VPP 是 VisionPro 取像工具文件。"));
					return;
				}

				_currentVproAcqTool = loadedTool;

				ShowVproPlaceholder(
					string.Concat(
						"CogAcq 工具对象已加载。",
						Environment.NewLine,
						"正在初始化 Cognex 原生编辑控件。"));

				await Task.Delay(80);

				Control editor = VisionProReflectionHelper.CreateCogAcqFifoEditor(_currentVproAcqTool);

				if (editor == null)
				{
					ShowVproPlaceholder(
						string.Concat(
							"VPP 已导入并加载，但未能创建 CogAcqFifoEditV2 控件。",
							Environment.NewLine,
							"请确认项目引用 Cognex.VisionPro.AcqFifo 相关 DLL。",
							Environment.NewLine,
							Environment.NewLine,
							"当前路径：",
							Environment.NewLine,
							projectPath));
					return;
				}

				_vproEditorHost.SuspendLayout();
				_vproEditorHost.Controls.Clear();
				editor.Dock = DockStyle.Fill;
				_vproEditorHost.Controls.Add(editor);
				_vproEditorHost.ResumeLayout(true);

				_lblBottomStatus.Text = "当前相机：" + _currentCamera.CameraName + "    |    VPro CogAcq 已加载";
			}
			catch (Exception ex)
			{
				ShowVproPlaceholder(
					string.Concat(
						"加载 VPro CogAcq 失败：",
						Environment.NewLine,
						ex.Message,
						Environment.NewLine,
						Environment.NewLine,
						"VPP 已导入：",
						Environment.NewLine,
						projectPath));

				if (showError)
				{
					MessageBox.Show(ex.Message, "Load CogAcq", MessageBoxButtons.OK, MessageBoxIcon.Warning);
				}
			}
			finally
			{
				_isLoadingVpro = false;
			}
		}

		private void CreateNewVproToolForCurrentCamera()
		{
			if (_currentCamera == null)
			{
				return;
			}

			_currentVproAcqTool = VisionProReflectionHelper.CreateCogAcqFifoTool();

			if (_currentVproAcqTool == null)
			{
				MessageBox.Show("Failed to create CogAcqFifoTool. Please check VisionPro references.", "VisionPro", MessageBoxButtons.OK, MessageBoxIcon.Warning);
				return;
			}

			Control editor = VisionProReflectionHelper.CreateCogAcqFifoEditor(_currentVproAcqTool);
			_vproEditorHost.Controls.Clear();

			if (editor != null)
			{
				editor.Dock = DockStyle.Fill;
				_vproEditorHost.Controls.Add(editor);
			}
			else
			{
				ShowVproPlaceholder("已创建 CogAcqFifoTool，但未能创建编辑控件。");
			}
		}

		private void SaveCurrentVproAcqTool()
		{
			if (_currentCamera == null) return;
			if (_currentVproAcqTool == null)
			{
				MessageBox.Show("Please load or create CogAcq tool first.", "Save", MessageBoxButtons.OK, MessageBoxIcon.Warning);
				return;
			}

			string defaultName = _currentCamera.VisionPro == null || string.IsNullOrWhiteSpace(_currentCamera.VisionPro.ToolName)
				? _currentCamera.CameraName + "_Acq"
				: _currentCamera.VisionPro.ToolName;

			using (ToolNameDialog dialog = new ToolNameDialog(T("保存 VisionPro 工具", "Save VisionPro Tool"), defaultName, ".vpp"))
			{
				if (dialog.ShowDialog(this) != DialogResult.OK) return;

				_currentCamera.VisionPro.ToolName = dialog.ToolName;
				_currentCamera.VisionPro.AcqVppPath = HardwareConfigStore.GetVisionProAcqPath(_currentCamera.CameraName, _currentCamera.AcqProfileName, _currentCamera.VisionPro.ToolName);

				string path = _currentCamera.VisionPro.AcqVppPath;
				try
				{
					string folder = Path.GetDirectoryName(path);
					if (!string.IsNullOrEmpty(folder) && !Directory.Exists(folder)) Directory.CreateDirectory(folder);
					VisionProReflectionHelper.SaveObjectToFile(_currentVproAcqTool, path);
					HardwareConfigStore.Save(_config);
					ShowVproMode(_currentCamera);
					MessageBox.Show("CogAcq tool saved successfully.\n" + path, "Save", MessageBoxButtons.OK, MessageBoxIcon.Information);
				}
				catch (Exception ex)
				{
					MessageBox.Show("Save CogAcq failed: " + ex.Message, "Save", MessageBoxButtons.OK, MessageBoxIcon.Warning);
				}
			}
		}

		private void TestGrabCurrentCamera()
		{
			if (_currentVproAcqTool == null)
			{
				LoadVproEditorForCurrentCamera(false);
				return;
			}

			try
			{
				VisionProReflectionHelper.RunTool(_currentVproAcqTool);
				object outputImage = VisionProReflectionHelper.GetProperty(_currentVproAcqTool, "OutputImage");
				_lblBottomStatus.Text = "当前相机：" + _currentCamera.CameraName + "    |    Test Grab OK    |    OutputImage: " + (outputImage == null ? "null" : outputImage.GetType().Name);
			}
			catch (Exception ex)
			{
				MessageBox.Show("Test grab failed: " + ex.Message, "Grab", MessageBoxButtons.OK, MessageBoxIcon.Warning);
			}
		}

		private void ShowVproPlaceholder(string text)
		{
			if (_vproEditorHost == null)
			{
				return;
			}

			_vproEditorHost.Controls.Clear();

			Label label = new Label();
			label.Dock = DockStyle.Fill;
			label.TextAlign = ContentAlignment.MiddleCenter;
			label.ForeColor = _muted;
			label.Font = new Font("Microsoft YaHei UI", 11F, FontStyle.Bold);
			label.Text = text;

			_vproEditorHost.Controls.Add(label);
		}

		private string GetCurrentSdkConfigPath(CameraDeviceConfig camera)
		{
			if (camera == null) return string.Empty;
			if (camera.Sdk == null) camera.Sdk = new SdkCameraConfig();
			if (!string.IsNullOrWhiteSpace(camera.Sdk.ConfigPath)) return camera.Sdk.ConfigPath;
			return HardwareConfigStore.GetSdkConfigPath(camera.CameraName, camera.AcqProfileName, camera.SdkBrand, camera.Sdk.ToolName);
		}

		private void LoadSdkToolForCurrentCamera()
		{
			if (_currentCamera == null || _currentCamera.AcquisitionMode != CameraAcquisitionMode.SDK)
			{
				return;
			}

			string selectedFile = string.Empty;

			using (OpenFileDialog dialog = new OpenFileDialog())
			{
				dialog.Title = "Select SDK acquisition parameter file";
				dialog.Filter = "SDK Config (*.xml)|*.xml|All files (*.*)|*.*";
				dialog.Multiselect = false;

				string initDir = HardwareConfigStore.GetSdkFolder(_currentCamera.CameraName, _currentCamera.AcqProfileName);
				if (Directory.Exists(initDir))
				{
					dialog.InitialDirectory = initDir;
				}

				if (dialog.ShowDialog(this) != DialogResult.OK)
				{
					return;
				}

				selectedFile = dialog.FileName;
			}

			string toolName = Path.GetFileNameWithoutExtension(selectedFile);
			string targetFolder = HardwareConfigStore.GetSdkFolder(_currentCamera.CameraName, _currentCamera.AcqProfileName);
			string projectPath;

			try
			{
				projectPath = CopyImportedFileToProjectLocal(
					selectedFile,
					targetFolder,
					toolName,
					".xml",
					true);
			}
			catch (Exception ex)
			{
				MessageBox.Show("Import SDK config failed: " + ex.Message, "SDK", MessageBoxButtons.OK, MessageBoxIcon.Warning);
				return;
			}

			try
			{
				SdkCameraConfig loaded = HardwareConfigStore.LoadSdkConfig(projectPath);

				if (loaded != null)
				{
					_currentCamera.Sdk = loaded;
					_currentCamera.Sdk.ConfigPath = projectPath;
					_currentCamera.Sdk.ToolName = Path.GetFileNameWithoutExtension(projectPath);

					if (_currentCamera.Sdk.Brand != CameraSdkBrand.None)
					{
						_currentCamera.SdkBrand = _currentCamera.Sdk.Brand;
					}

					HardwareConfigStore.Save(_config);
					ShowSdkMode(_currentCamera);
					_lblBottomStatus.Text = "SDK 参数已导入并加载：" + projectPath;
				}
				else
				{
					_currentCamera.Sdk.ConfigPath = projectPath;
					_currentCamera.Sdk.ToolName = Path.GetFileNameWithoutExtension(projectPath);
					HardwareConfigStore.Save(_config);
					ShowSdkMode(_currentCamera);
					_lblBottomStatus.Text = "SDK 参数文件已导入，但反序列化为空：" + projectPath;
				}
			}
			catch (Exception ex)
			{
				MessageBox.Show("Load SDK config failed: " + ex.Message, "SDK", MessageBoxButtons.OK, MessageBoxIcon.Warning);
			}
		}

		private void CreateNewSdkToolForCurrentCamera()
		{
			if (_currentCamera == null || _currentCamera.AcquisitionMode != CameraAcquisitionMode.SDK) return;
			if (MessageBox.Show("Create a new SDK acquisition parameter tool?", "New SDK Tool", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes) return;
			_currentCamera.Sdk = new SdkCameraConfig();
			_currentCamera.Sdk.Brand = _currentCamera.SdkBrand;
			_currentCamera.Sdk.ToolName = _currentCamera.CameraName + "_" + _currentCamera.SdkBrand + "_Sdk";
			_currentCamera.Sdk.ConfigPath = HardwareConfigStore.GetSdkConfigPath(_currentCamera.CameraName, _currentCamera.AcqProfileName, _currentCamera.SdkBrand, _currentCamera.Sdk.ToolName);
			ShowSdkMode(_currentCamera);
			_lblBottomStatus.Text = "已新建 SDK 取像参数：" + _currentCamera.Sdk.ToolName;
		}

		private void SaveSdkToolForCurrentCamera()
		{
			if (_currentCamera == null || _currentCamera.AcquisitionMode != CameraAcquisitionMode.SDK) return;
			if (_currentSdkPanel != null) _currentSdkPanel.SaveCamera(_currentCamera);

			string defaultName = _currentCamera.Sdk == null || string.IsNullOrWhiteSpace(_currentCamera.Sdk.ToolName)
				? _currentCamera.CameraName + "_" + _currentCamera.SdkBrand + "_Sdk"
				: _currentCamera.Sdk.ToolName;

			using (ToolNameDialog dialog = new ToolNameDialog(T("保存 SDK 工具", "Save SDK Tool"), defaultName, ".xml"))
			{
				if (dialog.ShowDialog(this) != DialogResult.OK) return;
				_currentCamera.Sdk.ToolName = dialog.ToolName;
				_currentCamera.Sdk.Brand = _currentCamera.SdkBrand;
				_currentCamera.Sdk.ConfigPath = HardwareConfigStore.GetSdkConfigPath(_currentCamera.CameraName, _currentCamera.AcqProfileName, _currentCamera.SdkBrand, _currentCamera.Sdk.ToolName);
				try
				{
					HardwareConfigStore.SaveSdkConfig(_currentCamera);
					HardwareConfigStore.Save(_config);
					ShowSdkMode(_currentCamera);
					MessageBox.Show("SDK tool saved successfully.\n" + _currentCamera.Sdk.ConfigPath, "SDK", MessageBoxButtons.OK, MessageBoxIcon.Information);
				}
				catch (Exception ex)
				{
					MessageBox.Show("Save SDK tool failed: " + ex.Message, "SDK", MessageBoxButtons.OK, MessageBoxIcon.Warning);
				}
			}
		}


		private string CopyImportedFileToProjectLocal(string sourceFile, string targetFolder, string targetFileNameWithoutExtension, string extensionWithDot, bool overwrite)
		{
			if (string.IsNullOrWhiteSpace(sourceFile) || !File.Exists(sourceFile))
			{
				throw new FileNotFoundException("Source file does not exist.", sourceFile);
			}

			if (string.IsNullOrWhiteSpace(targetFolder))
			{
				throw new ArgumentException("Target folder is empty.");
			}

			if (!Directory.Exists(targetFolder))
			{
				Directory.CreateDirectory(targetFolder);
			}

			string safeName = HardwareConfigStore.NormalizeFileName(targetFileNameWithoutExtension, Path.GetFileNameWithoutExtension(sourceFile));

			if (string.IsNullOrWhiteSpace(extensionWithDot))
			{
				extensionWithDot = Path.GetExtension(sourceFile);
			}

			if (!extensionWithDot.StartsWith("."))
			{
				extensionWithDot = "." + extensionWithDot;
			}

			string targetPath = Path.Combine(targetFolder, safeName + extensionWithDot);

			if (File.Exists(targetPath) && !overwrite)
			{
				throw new IOException("Target file already exists: " + targetPath);
			}

			if (string.Equals(Path.GetFullPath(sourceFile), Path.GetFullPath(targetPath), StringComparison.OrdinalIgnoreCase))
			{
				return targetPath;
			}

			File.Copy(sourceFile, targetPath, true);
			return targetPath;
		}


		private void cmbMode_SelectedIndexChanged(object sender, EventArgs e)
		{
			ComboBox cmb = sender as ComboBox;
			CameraDeviceConfig camera = cmb == null ? null : cmb.Tag as CameraDeviceConfig;

			if (camera == null || cmb.SelectedItem == null)
			{
				return;
			}

			camera.AcquisitionMode = (CameraAcquisitionMode)Enum.Parse(typeof(CameraAcquisitionMode), cmb.SelectedItem.ToString());

			if (camera.AcquisitionMode == CameraAcquisitionMode.SDK && camera.SdkBrand == CameraSdkBrand.None)
			{
				camera.SdkBrand = CameraSdkBrand.Hikvision;
				camera.Sdk.Brand = CameraSdkBrand.Hikvision;
			}

			HardwareConfigStore.Save(_config);
			RebuildCameraCards();

			if (_currentCamera == camera)
			{
				SelectCamera(camera, false);
			}
		}

		private void cmbBrand_SelectedIndexChanged(object sender, EventArgs e)
		{
			ComboBox cmb = sender as ComboBox;
			CameraDeviceConfig camera = cmb == null ? null : cmb.Tag as CameraDeviceConfig;

			if (camera == null || cmb.SelectedItem == null)
			{
				return;
			}

			camera.SdkBrand = (CameraSdkBrand)Enum.Parse(typeof(CameraSdkBrand), cmb.SelectedItem.ToString());
			camera.Sdk.Brand = camera.SdkBrand;
			HardwareConfigStore.Save(_config);

			if (_currentCamera == camera)
			{
				SelectCamera(camera, false);
			}
		}

		private void btnAddCamera_Click(object sender, EventArgs e)
		{
			HardwareConfigStore.AutoSelectFirstJobIfNeeded();

			if (!HardwareConfigStore.HasCurrentJob)
			{
				MessageBox.Show(
					"Please create or select a Job first. Camera hardware files are saved under Project\\Job\\[JobName]\\Hardware.",
					"Add Camera",
					MessageBoxButtons.OK,
					MessageBoxIcon.Information);
				return;
			}

			if (_config == null)
			{
				_config = new HardwareProjectConfig();
			}

			using (AddCameraDialog dialog = new AddCameraDialog(GetNextCameraName()))
			{
				if (dialog.ShowDialog(this) != DialogResult.OK)
				{
					return;
				}

				CameraDeviceConfig camera = new CameraDeviceConfig();
				camera.CameraName = dialog.CameraName;
				camera.AcquisitionMode = dialog.AcquisitionMode;
				camera.SdkBrand = dialog.SdkBrand;
				camera.Sdk.Brand = dialog.SdkBrand;
				camera.Status = "Disconnected";
				camera.AcqProfileName = "Default";
				camera.VisionPro.ToolName = string.Empty;
				camera.VisionPro.AcqVppPath = string.Empty;

				_config.Cameras.Add(camera);
				HardwareConfigStore.Save(_config);

				_currentCamera = camera;
				RebuildCameraCards();
				SelectCamera(camera, false);
			}
		}

		private void btnDeleteCamera_Click(object sender, EventArgs e)
		{
			if (_currentCamera == null)
			{
				return;
			}

			string cameraName = _currentCamera.CameraName;
			string cameraFolder = HardwareConfigStore.GetCameraFolder(cameraName);

			string message = string.Concat(
				"Delete selected camera?",
				Environment.NewLine,
				Environment.NewLine,
				"Camera: ",
				cameraName,
				Environment.NewLine,
				"Local folder will also be deleted:",
				Environment.NewLine,
				cameraFolder);

			if (MessageBox.Show(message, "Delete Camera", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes)
			{
				return;
			}

			try
			{
				_config.Cameras.Remove(_currentCamera);
				_currentCamera = null;
				_currentVproAcqTool = null;
				_currentSdkPanel = null;

				HardwareConfigStore.Save(_config);
				HardwareConfigStore.DeleteCameraFolder(cameraName);

				LoadConfigToUi();
			}
			catch (Exception ex)
			{
				MessageBox.Show("Delete camera failed: " + ex.Message, "Delete Camera", MessageBoxButtons.OK, MessageBoxIcon.Warning);
			}
		}

		private string GetNextCameraName()
		{
			int idx = 1;

			while (true)
			{
				string name = "Cam" + idx;
				bool exists = false;

				foreach (CameraDeviceConfig camera in _config.Cameras)
				{
					if (string.Equals(camera.CameraName, name, StringComparison.OrdinalIgnoreCase))
					{
						exists = true;
						break;
					}
				}

				if (!exists)
				{
					return name;
				}

				idx++;
			}
		}

		private Panel CreatePanel()
		{
			Panel panel = new Panel();
			panel.Dock = DockStyle.Fill;
			panel.BackColor = _panel;
			panel.BorderStyle = BorderStyle.FixedSingle;
			return panel;
		}

		private GroupBox CreateGroupBox(string text)
		{
			GroupBox group = new GroupBox();
			group.Dock = DockStyle.Fill;
			group.Text = text;
			group.ForeColor = _text;
			group.BackColor = _panel;
			group.Font = new Font("Microsoft YaHei UI", 9F, FontStyle.Bold);
			return group;
		}

		private Label CreateTitleLabel(string text)
		{
			Label label = new Label();
			label.Dock = DockStyle.Fill;
			label.Text = text;
			label.TextAlign = ContentAlignment.MiddleLeft;
			label.ForeColor = _text;
			label.Font = new Font("Microsoft YaHei UI", 10F, FontStyle.Bold);
			return label;
		}

		private Button CreateActionButton(string text, int x, int y, int width)
		{
			Button btn = new Button();
			btn.Text = text;
			btn.Location = new Point(x, y);
			btn.Size = new Size(width, 30);
			btn.FlatStyle = FlatStyle.Flat;
			btn.FlatAppearance.BorderSize = 1;
			btn.FlatAppearance.BorderColor = _accent;
			btn.FlatAppearance.MouseOverBackColor = Color.FromArgb(15, 45, 78);
			btn.FlatAppearance.MouseDownBackColor = Color.FromArgb(20, 70, 135);
			btn.BackColor = _panel2;
			btn.ForeColor = _text;
			btn.Font = new Font("Microsoft YaHei UI", 9F, FontStyle.Bold);
			btn.TextAlign = ContentAlignment.MiddleCenter;
			btn.UseVisualStyleBackColor = false;
			return btn;
		}

		private Button CreateBottomButton(string text)
		{
			Button btn = CreateActionButton(text, 0, 0, 120);
			btn.Dock = DockStyle.Right;
			return btn;
		}

		private Control CreateRightGap()
		{
			Panel p = new Panel();
			p.Width = 10;
			p.Dock = DockStyle.Right;
			p.BackColor = _panel;
			return p;
		}

		private Label SmallLabel(string text, int x, int y, int width)
		{
			Label label = new Label();
			label.Text = text;
			label.Location = new Point(x, y);
			label.Size = new Size(width, 24);
			label.ForeColor = _text;
			label.TextAlign = ContentAlignment.MiddleLeft;
			return label;
		}

		private ComboBox SmallCombo(int x, int y, int width)
		{
			ComboBox cmb = new ComboBox();
			cmb.Location = new Point(x, y);
			cmb.Size = new Size(width, 26);
			cmb.DropDownStyle = ComboBoxStyle.DropDownList;
			cmb.BackColor = Color.FromArgb(1, 8, 16);
			cmb.ForeColor = _text;
			return cmb;
		}

		private Label FindLabel(Control root, string name)
		{
			if (root == null)
			{
				return null;
			}

			foreach (Control c in root.Controls)
			{
				if (c.Name == name && c is Label)
				{
					return (Label)c;
				}

				Label child = FindLabel(c, name);

				if (child != null)
				{
					return child;
				}
			}

			return null;
		}

		private void EnableDoubleBuffer(Control control)
		{
			try
			{
				PropertyInfo property = typeof(Control).GetProperty("DoubleBuffered", BindingFlags.Instance | BindingFlags.NonPublic);

				if (property != null)
				{
					property.SetValue(control, true, null);
				}

				foreach (Control child in control.Controls)
				{
					EnableDoubleBuffer(child);
				}
			}
			catch
			{
			}
		}

		public void ApplyLanguage(bool isEnglish)
		{
			_isEnglish = isEnglish;

			if (_lblCameraTitle != null) _lblCameraTitle.Text = T("相机管理", "Camera Management");
			if (_rightGroup != null) _rightGroup.Text = T("相机配置", "Camera Config");
			if (_btnAddCamera != null) _btnAddCamera.Text = T("+ 添加相机", "+ Add");
			if (_btnDeleteCamera != null) _btnDeleteCamera.Text = T("删除", "Delete");
			if (_btnRefreshCamera != null) _btnRefreshCamera.Text = T("刷新", "Refresh");
			if (_btnBottomRefresh != null) _btnBottomRefresh.Text = T("刷新设备", "Refresh");
			if (_btnBottomSave != null) _btnBottomSave.Text = T("保存配置", "Save");
			if (_btnBottomApply != null) _btnBottomApply.Text = T("应用", "Apply");
			if (_btnLoadVpro != null) _btnLoadVpro.Text = T("加载 CogAcq", "Load CogAcq");
			if (_btnNewVpro != null) _btnNewVpro.Text = T("新建工具", "New Tool");
			if (_btnSaveVpro != null) _btnSaveVpro.Text = T("保存工具", "Save Tool");
			if (_btnLoadSdk != null) _btnLoadSdk.Text = T("加载取像参数", "Load Params");
			if (_btnNewSdk != null) _btnNewSdk.Text = T("新建工具", "New Tool");
			if (_btnSaveSdk != null) _btnSaveSdk.Text = T("保存工具", "Save Tool");

			if (_config != null)
			{
				RebuildCameraCards();
			}

			if (_currentCamera != null)
			{
				_lblCurrentCamera.Text = T("当前选中： ", "Selected: ") + _currentCamera.CameraName + T("    |    采集模式： ", "    |    Mode: ") + _currentCamera.AcquisitionMode;
				_lblBottomStatus.Text =
					T("当前相机：", "Current Camera: ") + _currentCamera.CameraName +
					T("    |    图像源：", "    |    Image Source: ") + _currentCamera.CameraName + ".Raw" +
					"    |    ImageSources：" + HardwareConfigStore.ImageSourceConfigPath;
			}
			else
			{
				ShowEmptyHardwarePlaceholder();
			}
		}

		private string T(string chinese, string english)
		{
			return _isEnglish ? english : chinese;
		}
	}

	public interface ISdkCameraConfigPanel
	{
		CameraSdkBrand Brand { get; }
		Control View { get; }
		void LoadCamera(CameraDeviceConfig camera);
		void SaveCamera(CameraDeviceConfig camera);
	}

	public static class SdkCameraPanelFactory
	{
		public static ISdkCameraConfigPanel CreatePanel(CameraSdkBrand brand)
		{
			switch (brand)
			{
				case CameraSdkBrand.LMI:
					return new LmiSdkCameraConfigPanel();
				case CameraSdkBrand.Keyence:
					return new KeyenceSdkCameraConfigPanel();
				case CameraSdkBrand.Hikvision:
					return new HikvisionSdkCameraConfigPanel();
				case CameraSdkBrand.Dahua:
					return new DahuaSdkCameraConfigPanel();
				default:
					return new GenericSdkCameraConfigPanel(CameraSdkBrand.Hikvision);
			}
		}
	}

	public class GenericSdkCameraConfigPanel : UserControl, ISdkCameraConfigPanel
	{
		protected readonly Color Back = Color.FromArgb(3, 14, 27);
		protected readonly Color Panel2 = Color.FromArgb(5, 18, 34);
		protected readonly Color TextColor = Color.FromArgb(220, 235, 245);
		protected readonly Color Muted = Color.FromArgb(130, 155, 175);
		protected readonly Color Accent = Color.FromArgb(0, 150, 220);

		protected CameraDeviceConfig CurrentCamera;
		protected TextBox txtIp;
		protected NumericUpDown numPort;
		protected TextBox txtSerial;
		protected ComboBox cmbTrigger;
		protected NumericUpDown numExposure;
		protected NumericUpDown numGain;
		protected ComboBox cmbPixel;
		protected ICameraSdkAdapter SdkAdapter;

		public virtual CameraSdkBrand Brand { get; private set; }
		public Control View { get { return this; } }

		public GenericSdkCameraConfigPanel(CameraSdkBrand brand)
		{
			Brand = brand;
			SdkAdapter = CameraSdkAdapterFactory.Create(brand);
			InitializePanel();
		}

		protected virtual void InitializePanel()
		{
			this.Dock = DockStyle.Fill;
			this.BackColor = Back;
			this.Font = new Font("Microsoft YaHei UI", 9F);

			TableLayoutPanel root = new TableLayoutPanel();
			root.Dock = DockStyle.Fill;
			root.BackColor = Back;
			root.Padding = new Padding(10);
			root.ColumnCount = 2;
			root.RowCount = 1;
			root.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 430F));
			root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));

			Panel left = new Panel();
			left.Dock = DockStyle.Fill;
			left.BackColor = Back;
			left.BorderStyle = BorderStyle.FixedSingle;
			left.Padding = new Padding(12);

			TableLayoutPanel form = new TableLayoutPanel();
			form.Dock = DockStyle.Fill;
			form.BackColor = Back;
			form.ColumnCount = 2;
			form.RowCount = 12;
			form.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 120F));
			form.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));

			for (int i = 0; i < 8; i++)
			{
				form.RowStyles.Add(new RowStyle(SizeType.Absolute, 38F));
			}

			form.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
			form.RowStyles.Add(new RowStyle(SizeType.Absolute, 42F));
			form.RowStyles.Add(new RowStyle(SizeType.Absolute, 38F));
			form.RowStyles.Add(new RowStyle(SizeType.Absolute, 38F));

			AddHeader(form, Brand + " SDK 基础参数", 0);
			txtIp = AddTextRow(form, "IP地址", 1);
			numPort = AddNumberRow(form, "端口", 2, 1, 65535, 3956, 0);
			txtSerial = AddTextRow(form, "序列号", 3);
			cmbTrigger = AddComboRow(form, "触发模式", 4, new string[] { "Off", "Software", "Line0", "Continuous" });
			numExposure = AddNumberRow(form, "曝光(us)", 5, 0, 99999999, 5000, 2);
			numGain = AddNumberRow(form, "增益(dB)", 6, 0, 999, 0, 2);
			cmbPixel = AddComboRow(form, "PixelFormat", 7, GetPixelFormats());

			Panel btnPanel = new Panel();
			btnPanel.Dock = DockStyle.Fill;
			btnPanel.BackColor = Back;

			Button btnConnect = CreateButton("连接", 0, 6, 90);
			Button btnDisconnect = CreateButton("断开", 100, 6, 90);
			Button btnLive = CreateButton("Live", 200, 6, 90);

			btnConnect.Click += delegate { ConnectSdk(); };
			btnDisconnect.Click += delegate { DisconnectSdk(); };
			btnLive.Click += delegate { StartLiveSdk(); };

			btnPanel.Controls.Add(btnConnect);
			btnPanel.Controls.Add(btnDisconnect);
			btnPanel.Controls.Add(btnLive);

			form.Controls.Add(btnPanel, 0, 9);
			form.SetColumnSpan(btnPanel, 2);

			left.Controls.Add(form);

			Panel preview = new Panel();
			preview.Dock = DockStyle.Fill;
			preview.BackColor = Color.FromArgb(1, 8, 16);
			preview.BorderStyle = BorderStyle.FixedSingle;

			Label lbl = new Label();
			lbl.Dock = DockStyle.Fill;
			lbl.TextAlign = ContentAlignment.MiddleCenter;
			lbl.ForeColor = Muted;
			lbl.Font = new Font("Microsoft YaHei UI", 12F, FontStyle.Bold);
			lbl.Text =
				"当前 SDK 模式：" + Brand + Environment.NewLine + Environment.NewLine +
				"这里后续显示 " + Brand + " SDK 图像预览。" + Environment.NewLine +
				"SDK 调用入口：ICameraSdkAdapter / CameraSdkAdapterFactory。" + Environment.NewLine +
				"你后续开发好的品牌 SDK 代码可以放在 Hardware SDK 文件夹中并注册 Adapter。";

			preview.Controls.Add(lbl);

			root.Controls.Add(left, 0, 0);
			root.Controls.Add(preview, 1, 0);
			this.Controls.Add(root);
		}

		protected virtual string[] GetPixelFormats()
		{
			return new string[] { "Mono8", "Mono16", "RGB8", "Coord3D_C16" };
		}

		public virtual void LoadCamera(CameraDeviceConfig camera)
		{
			CurrentCamera = camera;

			if (camera.Sdk == null)
			{
				camera.Sdk = new SdkCameraConfig();
			}

			txtIp.Text = camera.Sdk.IpAddress;
			numPort.Value = Clamp(camera.Sdk.Port, numPort.Minimum, numPort.Maximum);
			txtSerial.Text = camera.Sdk.SerialNumber;
			cmbTrigger.SelectedItem = camera.Sdk.TriggerMode;
			numExposure.Value = Clamp((decimal)camera.Sdk.ExposureUs, numExposure.Minimum, numExposure.Maximum);
			numGain.Value = Clamp((decimal)camera.Sdk.GainDb, numGain.Minimum, numGain.Maximum);
			cmbPixel.SelectedItem = camera.Sdk.PixelFormat;

			if (SdkAdapter != null)
			{
				SdkAdapter.LoadConfig(camera.Sdk);
			}
		}

		public virtual void SaveCamera(CameraDeviceConfig camera)
		{
			if (camera == null)
			{
				return;
			}

			if (camera.Sdk == null)
			{
				camera.Sdk = new SdkCameraConfig();
			}

			camera.Sdk.Brand = Brand;
			camera.Sdk.IpAddress = txtIp.Text.Trim();
			camera.Sdk.Port = (int)numPort.Value;
			camera.Sdk.SerialNumber = txtSerial.Text.Trim();
			camera.Sdk.TriggerMode = cmbTrigger.SelectedItem == null ? "Off" : cmbTrigger.SelectedItem.ToString();
			camera.Sdk.ExposureUs = (double)numExposure.Value;
			camera.Sdk.GainDb = (double)numGain.Value;
			camera.Sdk.PixelFormat = cmbPixel.SelectedItem == null ? "Mono8" : cmbPixel.SelectedItem.ToString();

			if (SdkAdapter != null)
			{
				SdkAdapter.LoadConfig(camera.Sdk);
			}
		}

		protected virtual void ConnectSdk()
		{
			if (SdkAdapter == null)
			{
				MessageBox.Show("SDK Adapter is not registered for " + Brand + ".", "SDK", MessageBoxButtons.OK, MessageBoxIcon.Information);
				return;
			}

			SdkAdapter.Connect();
		}

		protected virtual void DisconnectSdk()
		{
			if (SdkAdapter == null)
			{
				return;
			}

			SdkAdapter.Disconnect();
		}

		protected virtual void StartLiveSdk()
		{
			if (SdkAdapter == null)
			{
				MessageBox.Show("SDK Adapter is not registered for " + Brand + ".", "SDK", MessageBoxButtons.OK, MessageBoxIcon.Information);
				return;
			}

			SdkAdapter.StartLive();
		}

		protected Label CreateLabel(string text)
		{
			Label label = new Label();
			label.Dock = DockStyle.Fill;
			label.Text = text;
			label.TextAlign = ContentAlignment.MiddleLeft;
			label.ForeColor = TextColor;
			return label;
		}

		protected void AddHeader(TableLayoutPanel table, string text, int row)
		{
			Label label = CreateLabel(text);
			label.Font = new Font("Microsoft YaHei UI", 10F, FontStyle.Bold);
			table.Controls.Add(label, 0, row);
			table.SetColumnSpan(label, 2);
		}

		protected TextBox AddTextRow(TableLayoutPanel table, string label, int row)
		{
			TextBox txt = new TextBox();
			txt.Dock = DockStyle.Fill;
			txt.BackColor = Color.FromArgb(1, 8, 16);
			txt.ForeColor = TextColor;
			txt.BorderStyle = BorderStyle.FixedSingle;
			table.Controls.Add(CreateLabel(label), 0, row);
			table.Controls.Add(txt, 1, row);
			return txt;
		}

		protected ComboBox AddComboRow(TableLayoutPanel table, string label, int row, string[] items)
		{
			ComboBox cmb = new ComboBox();
			cmb.Dock = DockStyle.Fill;
			cmb.DropDownStyle = ComboBoxStyle.DropDownList;
			cmb.BackColor = Color.FromArgb(1, 8, 16);
			cmb.ForeColor = TextColor;
			cmb.Items.AddRange(items);

			if (cmb.Items.Count > 0)
			{
				cmb.SelectedIndex = 0;
			}

			table.Controls.Add(CreateLabel(label), 0, row);
			table.Controls.Add(cmb, 1, row);
			return cmb;
		}

		protected NumericUpDown AddNumberRow(TableLayoutPanel table, string label, int row, decimal min, decimal max, decimal value, int decimalPlaces)
		{
			NumericUpDown num = new NumericUpDown();
			num.Dock = DockStyle.Fill;
			num.Minimum = min;
			num.Maximum = max;
			num.Value = Clamp(value, min, max);
			num.DecimalPlaces = decimalPlaces;
			num.BackColor = Color.FromArgb(1, 8, 16);
			num.ForeColor = TextColor;
			num.BorderStyle = BorderStyle.FixedSingle;

			table.Controls.Add(CreateLabel(label), 0, row);
			table.Controls.Add(num, 1, row);
			return num;
		}

		protected Button CreateButton(string text, int x, int y, int width)
		{
			Button btn = new Button();
			btn.Text = text;
			btn.Location = new Point(x, y);
			btn.Size = new Size(width, 30);
			btn.FlatStyle = FlatStyle.Flat;
			btn.FlatAppearance.BorderColor = Accent;
			btn.BackColor = Panel2;
			btn.ForeColor = TextColor;
			btn.Font = new Font("Microsoft YaHei UI", 9F, FontStyle.Bold);
			btn.UseVisualStyleBackColor = false;
			return btn;
		}

		protected decimal Clamp(decimal value, decimal min, decimal max)
		{
			if (value < min) return min;
			if (value > max) return max;
			return value;
		}
	}

	public class LmiSdkCameraConfigPanel : GenericSdkCameraConfigPanel
	{
		public LmiSdkCameraConfigPanel() : base(CameraSdkBrand.LMI) { }

		protected override string[] GetPixelFormats()
		{
			return new string[] { "Profile", "Surface", "Range", "Intensity", "Coord3D_C16" };
		}
	}

	public class KeyenceSdkCameraConfigPanel : GenericSdkCameraConfigPanel
	{
		public KeyenceSdkCameraConfigPanel() : base(CameraSdkBrand.Keyence) { }

		protected override string[] GetPixelFormats()
		{
			return new string[] { "Mono8", "Mono16", "Height", "Luminance" };
		}
	}

	public class HikvisionSdkCameraConfigPanel : GenericSdkCameraConfigPanel
	{
		public HikvisionSdkCameraConfigPanel() : base(CameraSdkBrand.Hikvision) { }

		protected override string[] GetPixelFormats()
		{
			return new string[] { "Mono8", "Mono12", "Mono16", "BayerRG8", "RGB8" };
		}
	}

	public class DahuaSdkCameraConfigPanel : GenericSdkCameraConfigPanel
	{
		public DahuaSdkCameraConfigPanel() : base(CameraSdkBrand.Dahua) { }

		protected override string[] GetPixelFormats()
		{
			return new string[] { "Mono8", "Mono16", "RGB8", "BGR8" };
		}
	}

	public static class VisionProReflectionHelper
	{
		public static object LoadObjectFromFile(string path)
		{
			Type serializerType = FindType(
				"Cognex.VisionPro.CogSerializer",
				new string[]
				{
					"Cognex.VisionPro",
					"Cognex.VisionPro.Core"
				});

			if (serializerType == null)
			{
				throw new Exception(BuildVisionProMissingMessage("CogSerializer not found."));
			}

			MethodInfo method = serializerType.GetMethod(
				"LoadObjectFromFile",
				BindingFlags.Public | BindingFlags.Static,
				null,
				new Type[] { typeof(string) },
				null);

			if (method == null)
			{
				throw new Exception("CogSerializer.LoadObjectFromFile(string) not found.");
			}

			try
			{
				return method.Invoke(null, new object[] { path });
			}
			catch (TargetInvocationException ex)
			{
				throw ex.InnerException == null ? ex : ex.InnerException;
			}
		}

		public static void SaveObjectToFile(object obj, string path)
		{
			Type serializerType = FindType(
				"Cognex.VisionPro.CogSerializer",
				new string[]
				{
					"Cognex.VisionPro",
					"Cognex.VisionPro.Core"
				});

			if (serializerType == null)
			{
				throw new Exception(BuildVisionProMissingMessage("CogSerializer not found."));
			}

			MethodInfo method = serializerType.GetMethod(
				"SaveObjectToFile",
				BindingFlags.Public | BindingFlags.Static,
				null,
				new Type[] { typeof(object), typeof(string) },
				null);

			if (method == null)
			{
				throw new Exception("CogSerializer.SaveObjectToFile(object,string) not found.");
			}

			try
			{
				method.Invoke(null, new object[] { obj, path });
			}
			catch (TargetInvocationException ex)
			{
				throw ex.InnerException == null ? ex : ex.InnerException;
			}
		}

		public static object CreateCogAcqFifoTool()
		{
			Type toolType = FindType(
				"Cognex.VisionPro.CogAcqFifoTool",
				new string[]
				{
					"Cognex.VisionPro",
					"Cognex.VisionPro.AcqFifo"
				});

			if (toolType == null)
			{
				toolType = FindType(
					"Cognex.VisionPro.AcqFifo.CogAcqFifoTool",
					new string[]
					{
						"Cognex.VisionPro.AcqFifo",
						"Cognex.VisionPro"
					});
			}

			if (toolType == null)
			{
				return null;
			}

			return Activator.CreateInstance(toolType);
		}

		public static Control CreateCogAcqFifoEditor(object acqTool)
		{
			if (acqTool == null)
			{
				return null;
			}

			Type editorType = FindType(
				"Cognex.VisionPro.CogAcqFifoEditV2",
				new string[]
				{
					"Cognex.VisionPro",
					"Cognex.VisionPro.Controls",
					"Cognex.VisionPro.AcqFifo",
					"Cognex.VisionPro.AcqFifo.Controls"
				});

			if (editorType == null)
			{
				editorType = FindType(
					"Cognex.VisionPro.AcqFifo.CogAcqFifoEditV2",
					new string[]
					{
						"Cognex.VisionPro.AcqFifo",
						"Cognex.VisionPro.AcqFifo.Controls",
						"Cognex.VisionPro.Controls"
					});
			}

			if (editorType == null)
			{
				editorType = FindType(
					"Cognex.VisionPro.AcqFifo.Controls.CogAcqFifoEditV2",
					new string[]
					{
						"Cognex.VisionPro.AcqFifo.Controls",
						"Cognex.VisionPro.Controls"
					});
			}

			if (editorType == null)
			{
				return null;
			}

			object editorObj = Activator.CreateInstance(editorType);
			Control editor = editorObj as Control;

			if (editor == null)
			{
				return null;
			}

			PropertyInfo subject = editorType.GetProperty("Subject");

			if (subject != null && subject.CanWrite)
			{
				subject.SetValue(editorObj, acqTool, null);
			}

			return editor;
		}

		public static void RunTool(object tool)
		{
			if (tool == null)
			{
				return;
			}

			MethodInfo run = tool.GetType().GetMethod("Run", Type.EmptyTypes);

			if (run == null)
			{
				throw new Exception("Run method not found.");
			}

			try
			{
				run.Invoke(tool, null);
			}
			catch (TargetInvocationException ex)
			{
				throw ex.InnerException == null ? ex : ex.InnerException;
			}
		}

		public static object GetProperty(object obj, string name)
		{
			if (obj == null)
			{
				return null;
			}

			PropertyInfo p = obj.GetType().GetProperty(name);

			if (p == null)
			{
				return null;
			}

			return p.GetValue(obj, null);
		}

		private static Type FindType(string fullName, string[] assemblyNames)
		{
			if (string.IsNullOrWhiteSpace(fullName))
			{
				return null;
			}

			Type t = Type.GetType(fullName, false, true);

			if (t != null)
			{
				return t;
			}

			foreach (Assembly asm in AppDomain.CurrentDomain.GetAssemblies())
			{
				try
				{
					t = asm.GetType(fullName, false, true);

					if (t != null)
					{
						return t;
					}
				}
				catch
				{
				}
			}

			if (assemblyNames != null)
			{
				foreach (string asmName in assemblyNames)
				{
					t = TryLoadTypeFromAssemblyName(fullName, asmName);

					if (t != null)
					{
						return t;
					}
				}
			}

			foreach (string folder in GetVisionProSearchFolders())
			{
				if (string.IsNullOrWhiteSpace(folder) || !Directory.Exists(folder))
				{
					continue;
				}

				try
				{
					foreach (string dll in Directory.GetFiles(folder, "Cognex.VisionPro*.dll", SearchOption.TopDirectoryOnly))
					{
						t = TryLoadTypeFromAssemblyFile(fullName, dll);

						if (t != null)
						{
							return t;
						}
					}
				}
				catch
				{
				}
			}

			return null;
		}

		private static Type TryLoadTypeFromAssemblyName(string fullName, string asmName)
		{
			if (string.IsNullOrWhiteSpace(asmName))
			{
				return null;
			}

			try
			{
				Type t = Type.GetType(fullName + ", " + asmName, false, true);

				if (t != null)
				{
					return t;
				}
			}
			catch
			{
			}

			try
			{
				Assembly asm = Assembly.Load(asmName);
				return asm.GetType(fullName, false, true);
			}
			catch
			{
				return null;
			}
		}

		private static Type TryLoadTypeFromAssemblyFile(string fullName, string dllPath)
		{
			if (string.IsNullOrWhiteSpace(dllPath) || !File.Exists(dllPath))
			{
				return null;
			}

			try
			{
				Assembly asm = Assembly.LoadFrom(dllPath);
				return asm.GetType(fullName, false, true);
			}
			catch
			{
				return null;
			}
		}

		private static List<string> GetVisionProSearchFolders()
		{
			List<string> folders = new List<string>();
			AddFolder(folders, Application.StartupPath);
			AddFolder(folders, AppDomain.CurrentDomain.BaseDirectory);

			string programFiles = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);
			string programFilesX86 = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86);

			AddFolder(folders, Path.Combine(programFiles, "Cognex", "VisionPro", "bin"));
			AddFolder(folders, Path.Combine(programFiles, "Cognex", "VisionPro", "bin", "x64"));
			AddFolder(folders, Path.Combine(programFilesX86, "Cognex", "VisionPro", "bin"));
			AddFolder(folders, Path.Combine(programFilesX86, "Cognex", "VisionPro", "bin", "x64"));

			return folders;
		}

		private static void AddFolder(List<string> folders, string folder)
		{
			if (string.IsNullOrWhiteSpace(folder))
			{
				return;
			}

			if (!folders.Contains(folder))
			{
				folders.Add(folder);
			}
		}

		private static string BuildVisionProMissingMessage(string title)
		{
			return title + Environment.NewLine +
				"The current process cannot find Cognex.VisionPro.CogSerializer." + Environment.NewLine +
				"Please check that Cognex.VisionPro.dll is referenced by this project or copied to the output folder." + Environment.NewLine +
				"Also check x64/x86 platform consistency with the installed VisionPro runtime." + Environment.NewLine +
				"StartupPath: " + Application.StartupPath;
		}
	}

	public class ToolNameDialog : Form
	{
		private TextBox txtToolName;
		private Button btnOk;
		private Button btnCancel;
		private readonly bool _isEnglish;
		public string ToolName { get; private set; }

		public ToolNameDialog(string title, string defaultName, string extensionText)
		{
			_isEnglish = LanguagePreferenceStore.LoadIsEnglish();
			ToolName = defaultName;
			this.Text = title;
			this.StartPosition = FormStartPosition.CenterParent;
			this.FormBorderStyle = FormBorderStyle.FixedDialog;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.ClientSize = new Size(440, 190);
			this.BackColor = Color.FromArgb(2, 10, 20);
			this.Font = new Font("Microsoft YaHei UI", 9F);

			Label lblTitle = CreateLabel(title, 30, 24, 260, 28, 13, true);
			Label lblName = CreateLabel(T("工具名称", "Tool Name"), 40, 80, 90, 24, 9, false);
			txtToolName = CreateTextBox(135, 78, 220);
			txtToolName.Text = Path.GetFileNameWithoutExtension(defaultName);
			Label lblExt = CreateLabel(extensionText, 365, 80, 50, 24, 9, false);
			btnOk = CreateButton(T("确定", "OK"), 220, 135, true);
			btnCancel = CreateButton(T("取消", "Cancel"), 320, 135, false);
			btnOk.Click += btnOk_Click;
			btnCancel.Click += delegate { this.DialogResult = DialogResult.Cancel; this.Close(); };
			this.Controls.Add(lblTitle);
			this.Controls.Add(lblName);
			this.Controls.Add(txtToolName);
			this.Controls.Add(lblExt);
			this.Controls.Add(btnOk);
			this.Controls.Add(btnCancel);
		}

		private void btnOk_Click(object sender, EventArgs e)
		{
			if (string.IsNullOrWhiteSpace(txtToolName.Text))
			{
				MessageBox.Show(T("工具名称不能为空。", "Tool name cannot be empty."), T("工具名称", "Tool Name"), MessageBoxButtons.OK, MessageBoxIcon.Warning);
				return;
			}
			ToolName = HardwareConfigStore.NormalizeFileName(txtToolName.Text.Trim(), "AcqTool");
			this.DialogResult = DialogResult.OK;
			this.Close();
		}

		private Label CreateLabel(string text, int x, int y, int w, int h, int fontSize, bool bold)
		{
			Label label = new Label();
			label.Text = text;
			label.ForeColor = Color.White;
			label.Location = new Point(x, y);
			label.Size = new Size(w, h);
			label.Font = new Font("Microsoft YaHei UI", fontSize, bold ? FontStyle.Bold : FontStyle.Regular);
			return label;
		}

		private TextBox CreateTextBox(int x, int y, int width)
		{
			TextBox txt = new TextBox();
			txt.Location = new Point(x, y);
			txt.Size = new Size(width, 24);
			txt.BackColor = Color.FromArgb(3, 14, 27);
			txt.ForeColor = Color.White;
			txt.BorderStyle = BorderStyle.FixedSingle;
			return txt;
		}

		private Button CreateButton(string text, int x, int y, bool primary)
		{
			Button btn = new Button();
			btn.Text = text;
			btn.Location = new Point(x, y);
			btn.Size = new Size(90, 32);
			btn.FlatStyle = FlatStyle.Flat;
			btn.FlatAppearance.BorderColor = Color.FromArgb(0, 150, 220);
			btn.BackColor = primary ? Color.FromArgb(0, 95, 220) : Color.FromArgb(3, 14, 27);
			btn.ForeColor = Color.White;
			return btn;
		}

		private string T(string chinese, string english)
		{
			return _isEnglish ? english : chinese;
		}
	}

	public class AddCameraDialog : Form
	{
		private TextBox txtName;
		private ComboBox cmbMode;
		private ComboBox cmbBrand;
		private Button btnOk;
		private Button btnCancel;
		private readonly bool _isEnglish;

		public string CameraName { get; private set; }
		public CameraAcquisitionMode AcquisitionMode { get; private set; }
		public CameraSdkBrand SdkBrand { get; private set; }

		public AddCameraDialog(string defaultName)
		{
			_isEnglish = LanguagePreferenceStore.LoadIsEnglish();
			CameraName = defaultName;
			AcquisitionMode = CameraAcquisitionMode.VPro;
			SdkBrand = CameraSdkBrand.Hikvision;

			InitializeUi(defaultName);
		}

		private void InitializeUi(string defaultName)
		{
			this.Text = T("新增相机", "Add Camera");
			this.StartPosition = FormStartPosition.CenterParent;
			this.FormBorderStyle = FormBorderStyle.FixedDialog;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.ClientSize = new Size(420, 250);
			this.BackColor = Color.FromArgb(2, 10, 20);
			this.Font = new Font("Microsoft YaHei UI", 9F);

			Label title = CreateLabel(T("新增相机", "Add Camera"), 30, 22, 200, 28, 14, true);

			Label lblName = CreateLabel(T("相机名称", "Camera Name"), 40, 75, 90, 24, 9, false);
			Label lblMode = CreateLabel(T("采集模式", "Mode"), 40, 115, 90, 24, 9, false);
			Label lblBrand = CreateLabel(T("SDK品牌", "SDK Brand"), 40, 155, 90, 24, 9, false);

			txtName = CreateTextBox(140, 72, 220);
			txtName.Text = defaultName;

			cmbMode = CreateComboBox(140, 112, 220);
			cmbMode.Items.Add(CameraAcquisitionMode.VPro.ToString());
			cmbMode.Items.Add(CameraAcquisitionMode.SDK.ToString());
			cmbMode.SelectedIndex = 0;
			cmbMode.SelectedIndexChanged += delegate
			{
				cmbBrand.Enabled = cmbMode.SelectedItem != null && cmbMode.SelectedItem.ToString() == CameraAcquisitionMode.SDK.ToString();
			};

			cmbBrand = CreateComboBox(140, 152, 220);
			cmbBrand.Items.Add(CameraSdkBrand.LMI.ToString());
			cmbBrand.Items.Add(CameraSdkBrand.Keyence.ToString());
			cmbBrand.Items.Add(CameraSdkBrand.Hikvision.ToString());
			cmbBrand.Items.Add(CameraSdkBrand.Dahua.ToString());
			cmbBrand.SelectedItem = CameraSdkBrand.Hikvision.ToString();
			cmbBrand.Enabled = false;

			btnOk = CreateButton(T("确定", "OK"), 170, 205, true);
			btnCancel = CreateButton(T("取消", "Cancel"), 280, 205, false);

			btnOk.Click += btnOk_Click;
			btnCancel.Click += delegate { this.DialogResult = DialogResult.Cancel; this.Close(); };

			this.Controls.Add(title);
			this.Controls.Add(lblName);
			this.Controls.Add(lblMode);
			this.Controls.Add(lblBrand);
			this.Controls.Add(txtName);
			this.Controls.Add(cmbMode);
			this.Controls.Add(cmbBrand);
			this.Controls.Add(btnOk);
			this.Controls.Add(btnCancel);
		}

		private void btnOk_Click(object sender, EventArgs e)
		{
			if (string.IsNullOrWhiteSpace(txtName.Text))
			{
				MessageBox.Show(T("相机名称不能为空。", "Camera name cannot be empty."), T("新增相机", "Add Camera"), MessageBoxButtons.OK, MessageBoxIcon.Warning);
				return;
			}

			CameraName = txtName.Text.Trim();
			AcquisitionMode = (CameraAcquisitionMode)Enum.Parse(typeof(CameraAcquisitionMode), cmbMode.SelectedItem.ToString());

			if (cmbBrand.SelectedItem != null)
			{
				SdkBrand = (CameraSdkBrand)Enum.Parse(typeof(CameraSdkBrand), cmbBrand.SelectedItem.ToString());
			}

			this.DialogResult = DialogResult.OK;
			this.Close();
		}

		private Label CreateLabel(string text, int x, int y, int w, int h, int fontSize, bool bold)
		{
			Label label = new Label();
			label.Text = text;
			label.ForeColor = Color.White;
			label.Location = new Point(x, y);
			label.Size = new Size(w, h);
			label.Font = new Font("Microsoft YaHei UI", fontSize, bold ? FontStyle.Bold : FontStyle.Regular);
			return label;
		}

		private TextBox CreateTextBox(int x, int y, int width)
		{
			TextBox txt = new TextBox();
			txt.Location = new Point(x, y);
			txt.Size = new Size(width, 24);
			txt.BackColor = Color.FromArgb(3, 14, 27);
			txt.ForeColor = Color.White;
			txt.BorderStyle = BorderStyle.FixedSingle;
			return txt;
		}

		private ComboBox CreateComboBox(int x, int y, int width)
		{
			ComboBox cmb = new ComboBox();
			cmb.Location = new Point(x, y);
			cmb.Size = new Size(width, 24);
			cmb.DropDownStyle = ComboBoxStyle.DropDownList;
			cmb.BackColor = Color.FromArgb(3, 14, 27);
			cmb.ForeColor = Color.White;
			return cmb;
		}

		private Button CreateButton(string text, int x, int y, bool primary)
		{
			Button btn = new Button();
			btn.Text = text;
			btn.Location = new Point(x, y);
			btn.Size = new Size(90, 32);
			btn.FlatStyle = FlatStyle.Flat;
			btn.FlatAppearance.BorderColor = Color.FromArgb(0, 150, 220);
			btn.BackColor = primary ? Color.FromArgb(0, 95, 220) : Color.FromArgb(3, 14, 27);
			btn.ForeColor = Color.White;
			return btn;
		}

		private string T(string chinese, string english)
		{
			return _isEnglish ? english : chinese;
		}
	}
}
