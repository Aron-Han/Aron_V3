using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using System.Runtime.InteropServices;

namespace Aron_V3
{
	public partial class CommunicationConfigControl : UserControl, ILocalizable
	{
		private CommunicationConfig _config;
		private CommunicationType _selectedType = CommunicationType.TcpIp;
		private string _selectedInstanceName = string.Empty;
		private bool _loading = false;
		private bool _isEnglish = false;
		private bool _tcpRuntimeEventBound = false;
		private bool _validatingRangeCells = false;
		private string _activeTcpRuntimeInstanceName = string.Empty;
		private readonly object _latestInputValuesSyncRoot = new object();
		private readonly Dictionary<string, Dictionary<string, string>> _latestInputValuesByCommunication =
			new Dictionary<string, Dictionary<string, string>>(StringComparer.OrdinalIgnoreCase);

		private CheckBox chkEnable;
		private Button btnTcpConnect;
		private Button btnTcpDisconnect;
		private Button btnChannelSettings;
		private Button btnHeartbeatSettings;
		private Panel pnlTcpStatusLight;
		private Label lblTcpStatus;
		private Label lblTcpParam1;
		private TextBox txtTcpParam1;
		private Label lblTcpParam2;
		private TextBox txtTcpParam2;
		private Label lblTcpPayloadMode;
		private ComboBox cmbTcpPayloadMode;
		private Label lblTcpByteOrder;
		private ComboBox cmbTcpByteOrder;
		private Button btnAddTcpInstance;
		private Button btnAddProfinetInstance;
		private Button btnAddS7Instance;
		private readonly List<Button> _communicationInstanceButtons = new List<Button>();

		private bool? _inputSecondColumnIsProfinet = null;

		[DllImport("user32.dll")]
		private static extern IntPtr SendMessage(IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam);

		private const int WM_SETREDRAW = 0x000B;

		public CommunicationConfigControl()
		{
			InitializeComponent();

			EnableDoubleBufferForPage();

			InitializeEnableCheckBox();
			InitializeTcpConnectionControls();
			InitializeChannelSettingsButton();
			InitializeHeartbeatSettingsButton();
			InitializeCommunicationInstanceTree();
			InitializeGridStyle();
			InitializeComboColumns();
			NormalizeRightPanelMargins();
			InitializeVariableButtonLayout();
			DisableTestSendReceivePanel();

			_config = CommunicationConfigStore.LoadOrCreateDefault();
			LoadConfigToUI(_config);
			GlobalVariableStore.VariablesChanged += GlobalVariableStore_VariablesChanged;
			CommunicationConfigChangedHub.ConfigChanged += CommunicationConfigChangedHub_ConfigChanged;
			RuntimeCommunicationOutputService.OutputValuesChanged += RuntimeCommunicationOutputService_OutputValuesChanged;
			BindTcpRuntimeEvents();
			UpdateTcpStatusUi();
		}

		private void InitializeEnableCheckBox()
		{
			chkEnable = new CheckBox();
			chkEnable.AutoSize = true;
			chkEnable.Text = "启用";
			chkEnable.ForeColor = Color.White;
			chkEnable.BackColor = Color.Transparent;
			chkEnable.Location = new Point(235, 22);
			chkEnable.Name = "chkEnable";
			chkEnable.CheckedChanged += chkEnable_CheckedChanged;

			panelParams.Controls.Add(chkEnable);
			chkEnable.BringToFront();
		}

		private void InitializeChannelSettingsButton()
		{
			btnChannelSettings = CreateTcpSmallButton("通道设置", 18, 390, 150, 34);
			btnChannelSettings.Name = "btnChannelSettings";
			btnChannelSettings.Anchor = AnchorStyles.Left | AnchorStyles.Top;
			btnChannelSettings.Click += btnChannelSettings_Click;
			panelParams.Controls.Add(btnChannelSettings);
			btnChannelSettings.BringToFront();
		}

		private void InitializeHeartbeatSettingsButton()
		{
			btnHeartbeatSettings = CreateTcpSmallButton("心跳设置", 178, 390, 150, 34);
			btnHeartbeatSettings.Name = "btnHeartbeatSettings";
			btnHeartbeatSettings.Anchor = AnchorStyles.Left | AnchorStyles.Top;
			btnHeartbeatSettings.Click += btnHeartbeatSettings_Click;
			panelParams.Controls.Add(btnHeartbeatSettings);
			btnHeartbeatSettings.BringToFront();
		}

		private void InitializeCommunicationInstanceTree()
		{
			btnAddTcpInstance = CreateCommunicationAddButton();
			btnAddTcpInstance.Name = "btnAddTcpInstance";
			btnAddTcpInstance.Click += btnAddTcpInstance_Click;
			panelType.Controls.Add(btnAddTcpInstance);
			btnAddTcpInstance.BringToFront();

			btnAddProfinetInstance = CreateCommunicationAddButton();
			btnAddProfinetInstance.Name = "btnAddProfinetInstance";
			btnAddProfinetInstance.Click += btnAddProfinetInstance_Click;
			panelType.Controls.Add(btnAddProfinetInstance);
			btnAddProfinetInstance.BringToFront();

			btnAddS7Instance = CreateCommunicationAddButton();
			btnAddS7Instance.Name = "btnAddS7Instance";
			btnAddS7Instance.Click += btnAddS7Instance_Click;
			panelType.Controls.Add(btnAddS7Instance);
			btnAddS7Instance.BringToFront();

			panelType.Paint += panelType_Paint;
			panelType.Resize += delegate { LayoutCommunicationInstanceTree(); };
		}

		private Button CreateCommunicationAddButton()
		{
			Button button = new Button();
			button.Text = string.Empty;
			button.Size = new Size(28, 28);
			button.FlatStyle = FlatStyle.Flat;
			button.FlatAppearance.BorderSize = 0;
			button.FlatAppearance.MouseOverBackColor = Color.FromArgb(0, 105, 175);
			button.FlatAppearance.MouseDownBackColor = Color.FromArgb(0, 135, 210);
			button.BackColor = Color.FromArgb(3, 14, 27);
			button.ForeColor = Color.White;
			button.TabStop = false;
			button.Paint += communicationIconAddButton_Paint;
			return button;
		}

		private void DisableTestSendReceivePanel()
		{
			if (grpTest == null)
			{
				return;
			}

			grpTest.Visible = false;
			grpTest.Enabled = false;

			if (grpParams != null)
			{
				grpParams.Height = 326;
			}
		}

		private void RefreshCommunicationInstanceTree()
		{
			if (panelType == null)
			{
				return;
			}

			EnsureUiInstances();

			foreach (Button button in _communicationInstanceButtons)
			{
				if (button != null && !button.IsDisposed)
				{
					panelType.Controls.Remove(button);
					button.Dispose();
				}
			}

			_communicationInstanceButtons.Clear();

			AddInstanceButtons(CommunicationType.TcpIp);
			AddInstanceButtons(CommunicationType.Profinet);
			AddInstanceButtons(CommunicationType.S7);
			LayoutCommunicationInstanceTree();
			panelType.Invalidate();
		}

		private void AddInstanceButtons(CommunicationType type)
		{
			foreach (CommunicationInstanceConfig instance in GetInstances(type))
			{
				if (instance == null)
				{
					continue;
				}

				Button button = CreateCommunicationInstanceButton(instance);
				_communicationInstanceButtons.Add(button);
				panelType.Controls.Add(button);
				button.BringToFront();
			}
		}

		private Button CreateCommunicationInstanceButton(CommunicationInstanceConfig instance)
		{
			Button button = new Button();
			button.Text = instance.InstanceName;
			button.Tag = instance;
			button.Height = 30;
			button.FlatStyle = FlatStyle.Flat;
			button.FlatAppearance.BorderColor = Color.FromArgb(0, 120, 180);
			button.BackColor = Color.FromArgb(3, 14, 27);
			button.ForeColor = Color.White;
			button.Font = new Font("Microsoft YaHei UI", 8.5F, FontStyle.Bold);
			button.TextAlign = ContentAlignment.MiddleLeft;
			button.Padding = new Padding(30, 0, CanDeleteCommunicationInstance(instance) ? 42 : 12, 0);
			button.MouseClick += btnCommunicationInstance_MouseClick;
			button.Paint += communicationInstanceButton_Paint;
			return button;
		}

		private void LayoutCommunicationInstanceTree()
		{
			if (panelType == null || btnTcpIp == null || btnProfinet == null || btnS7 == null)
			{
				return;
			}

			int x = 18;
			int y = 58;
			int mainW = 188;
			int mainH = 44;
			int mainGap = 18;
			int instanceX = x + 34;
			int instanceW = mainW - 34;
			int instanceH = 30;
			int instanceGap = 6;

			SetMainProtocolButtonBounds(btnTcpIp, x, y, mainW, mainH, btnAddTcpInstance);
			y = btnTcpIp.Bottom + 8;
			LayoutInstanceButtons(CommunicationType.TcpIp, instanceX, ref y, instanceW, instanceH, instanceGap);

			y += mainGap;
			SetMainProtocolButtonBounds(btnProfinet, x, y, mainW, mainH, btnAddProfinetInstance);
			UpdateProfinetAddButtonAvailability();
			y = btnProfinet.Bottom + 8;
			LayoutInstanceButtons(CommunicationType.Profinet, instanceX, ref y, instanceW, instanceH, instanceGap);

			y += mainGap;
			SetMainProtocolButtonBounds(btnS7, x, y, mainW, mainH, btnAddS7Instance);
			y = btnS7.Bottom + 8;
			LayoutInstanceButtons(CommunicationType.S7, instanceX, ref y, instanceW, instanceH, instanceGap);

			if (btnAddTcpInstance != null) btnAddTcpInstance.BringToFront();
			if (btnAddProfinetInstance != null) btnAddProfinetInstance.BringToFront();
			if (btnAddS7Instance != null) btnAddS7Instance.BringToFront();

			ApplySelectedTypeStyle();
			panelType.Invalidate();
		}

		private void SetMainProtocolButtonBounds(Button button, int x, int y, int w, int h, Button addButton)
		{
			button.SetBounds(x, y, w, h);

			if (addButton != null)
			{
				addButton.SetBounds(button.Right - addButton.Width - 8, button.Top + 9, addButton.Width, addButton.Height);
				addButton.Visible = true;
			}
		}

		private void UpdateProfinetAddButtonAvailability()
		{
			if (btnAddProfinetInstance == null)
			{
				return;
			}

			bool canAdd = GetInstances(CommunicationType.Profinet).Count <= 0;
			btnAddProfinetInstance.Visible = canAdd;
			btnAddProfinetInstance.Enabled = canAdd;
		}

		private void communicationIconAddButton_Paint(object sender, PaintEventArgs e)
		{
			Button button = sender as Button;
			if (button == null)
			{
				return;
			}

			DrawCenteredCircleIcon(
				e.Graphics,
				new PointF(button.ClientSize.Width / 2F, button.ClientSize.Height / 2F),
				20F,
				true,
				Color.White);
		}

		private void communicationInstanceButton_Paint(object sender, PaintEventArgs e)
		{
			Button button = sender as Button;
			CommunicationInstanceConfig instance = button == null ? null : button.Tag as CommunicationInstanceConfig;
			if (button == null || instance == null)
			{
				return;
			}

			DrawCommunicationInstanceStatusDot(e.Graphics, button, instance);

			if (!CanDeleteCommunicationInstance(instance))
			{
				return;
			}

			bool selected =
				instance.CommunicationType == _selectedType &&
				string.Equals(instance.InstanceName, _selectedInstanceName, StringComparison.OrdinalIgnoreCase);
			Color iconColor = selected ? Color.White : Color.FromArgb(215, 235, 250);
			DrawCenteredCircleIcon(
				e.Graphics,
				new PointF(button.ClientSize.Width - 20F, button.ClientSize.Height / 2F),
				16F,
				false,
				iconColor);
		}

		private void DrawCommunicationInstanceStatusDot(
			Graphics graphics,
			Button button,
			CommunicationInstanceConfig instance)
		{
			if (graphics == null || button == null || instance == null)
			{
				return;
			}

			System.Drawing.Drawing2D.SmoothingMode oldMode = graphics.SmoothingMode;
			System.Drawing.Drawing2D.PixelOffsetMode oldPixelOffsetMode = graphics.PixelOffsetMode;
			graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
			graphics.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.HighQuality;

			try
			{
				Color dotColor = GetCommunicationInstanceStatusColor(instance);
				float diameter = 8F;
				float x = 12F;
				float y = (button.ClientSize.Height - diameter) / 2F;

				using (SolidBrush brush = new SolidBrush(dotColor))
				{
					graphics.FillEllipse(brush, x, y, diameter, diameter);
				}
			}
			finally
			{
				graphics.SmoothingMode = oldMode;
				graphics.PixelOffsetMode = oldPixelOffsetMode;
			}
		}

		private Color GetCommunicationInstanceStatusColor(CommunicationInstanceConfig instance)
		{
			if (instance == null || !IsCommunicationInstanceEnabled(instance))
			{
				return Color.FromArgb(120, 130, 140);
			}

			if (IsCommunicationInstanceConnected(instance))
			{
				return Color.LimeGreen;
			}

			return Color.FromArgb(235, 70, 82);
		}

		private bool IsCommunicationInstanceEnabled(CommunicationInstanceConfig instance)
		{
			if (instance == null)
			{
				return false;
			}

			if (instance.CommunicationType == CommunicationType.TcpIp)
			{
				return instance.TcpIp != null && instance.TcpIp.Enabled;
			}

			if (instance.CommunicationType == CommunicationType.S7)
			{
				return instance.S7 != null && instance.S7.Enabled;
			}

			return instance.Profinet != null && instance.Profinet.Enabled;
		}

		private bool IsCommunicationInstanceConnected(CommunicationInstanceConfig instance)
		{
			if (instance == null)
			{
				return false;
			}

			if (instance.CommunicationType == CommunicationType.TcpIp)
			{
				return CommunicationRuntimeManager.Instance.IsConnected(instance.InstanceName);
			}

			return false;
		}

		private void DrawCenteredCircleIcon(Graphics graphics, PointF center, float diameter, bool drawPlus, Color color)
		{
			if (graphics == null)
			{
				return;
			}

			System.Drawing.Drawing2D.SmoothingMode oldMode = graphics.SmoothingMode;
			System.Drawing.Drawing2D.PixelOffsetMode oldPixelOffsetMode = graphics.PixelOffsetMode;
			System.Drawing.Drawing2D.CompositingQuality oldCompositingQuality = graphics.CompositingQuality;
			graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
			graphics.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.HighQuality;
			graphics.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;

			try
			{
				float radius = diameter / 2F;
				RectangleF bounds = new RectangleF(center.X - radius, center.Y - radius, diameter, diameter);

				using (Pen pen = new Pen(color, 2.2F))
				{
					pen.StartCap = System.Drawing.Drawing2D.LineCap.Round;
					pen.EndCap = System.Drawing.Drawing2D.LineCap.Round;
					graphics.DrawEllipse(pen, bounds);

					float arm = diameter * 0.25F;
					graphics.DrawLine(pen, center.X - arm, center.Y, center.X + arm, center.Y);

					if (drawPlus)
					{
						graphics.DrawLine(pen, center.X, center.Y - arm, center.X, center.Y + arm);
					}
				}
			}
			finally
			{
				graphics.SmoothingMode = oldMode;
				graphics.PixelOffsetMode = oldPixelOffsetMode;
				graphics.CompositingQuality = oldCompositingQuality;
			}
		}

		private void LayoutInstanceButtons(
			CommunicationType type,
			int x,
			ref int y,
			int w,
			int h,
			int gap)
		{
			foreach (Button button in _communicationInstanceButtons)
			{
				CommunicationInstanceConfig instance = button.Tag as CommunicationInstanceConfig;
				if (instance == null || instance.CommunicationType != type)
				{
					continue;
				}

				button.SetBounds(x, y, w, h);
				y = button.Bottom + gap;
			}
		}

		private void panelType_Paint(object sender, PaintEventArgs e)
		{
			if (e == null || _communicationInstanceButtons.Count <= 0)
			{
				return;
			}

			using (Pen pen = new Pen(Color.FromArgb(0, 115, 180), 1F))
			{
				foreach (Button button in _communicationInstanceButtons)
				{
					if (button == null || !button.Visible)
					{
						continue;
					}

					CommunicationInstanceConfig instance = button.Tag as CommunicationInstanceConfig;
					if (instance == null)
					{
						continue;
					}

					Button parent = instance.CommunicationType == CommunicationType.TcpIp
						? btnTcpIp
						: (instance.CommunicationType == CommunicationType.Profinet ? btnProfinet : btnS7);
					if (parent == null)
					{
						continue;
					}

					int lineX = button.Left - 12;
					int parentY = parent.Bottom;
					int childY = button.Top + button.Height / 2;
					e.Graphics.DrawLine(pen, lineX, parentY, lineX, childY);
					e.Graphics.DrawLine(pen, lineX, childY, button.Left - 2, childY);
				}
			}
		}

		private void InvalidateCommunicationInstanceButtons()
		{
			foreach (Button button in _communicationInstanceButtons)
			{
				if (button != null && !button.IsDisposed)
				{
					button.Invalidate();
				}
			}
		}

		private void chkEnable_CheckedChanged(object sender, EventArgs e)
		{
			if (_loading || _config == null)
			{
				return;
			}

			SetCurrentTypeEnabled(chkEnable.Checked);
		}

		private void SetCurrentTypeEnabled(bool enabled)
		{
			if (_config == null)
			{
				return;
			}

			CommunicationInstanceConfig instance = GetSelectedInstance();
			if (instance != null)
			{
				instance.Enabled = enabled;
			}

			if (_selectedType == CommunicationType.TcpIp)
			{
				GetCurrentTcpConfig().Enabled = enabled;
			}
			else if (_selectedType == CommunicationType.Profinet)
			{
				GetCurrentProfinetConfig().Enabled = enabled;
			}
			else
			{
				GetCurrentS7Config().Enabled = enabled;
			}

			InvalidateCommunicationInstanceButtons();
		}

		private bool GetCurrentTypeEnabled()
		{
			if (_config == null)
			{
				return false;
			}

			if (_selectedType == CommunicationType.TcpIp)
			{
				return GetCurrentTcpConfig().Enabled;
			}

			if (_selectedType == CommunicationType.Profinet)
			{
				return GetCurrentProfinetConfig().Enabled;
			}

			return GetCurrentS7Config().Enabled;
		}

		private void EnsureUiInstances()
		{
			if (_config == null)
			{
				_config = new CommunicationConfig();
			}

			if (_config.TcpIp == null) _config.TcpIp = new TcpIpConfig();
			if (_config.Profinet == null) _config.Profinet = new ProfinetConfig();
			if (_config.S7 == null) _config.S7 = new S7Config();
			if (_config.Instances == null) _config.Instances = new List<CommunicationInstanceConfig>();

			bool instanceListWasEmpty = _config.Instances.Count == 0;

			if (instanceListWasEmpty)
			{
				EnsureDefaultUiInstance("TCPIP_01", CommunicationType.TcpIp, _config.TcpIp, null, null);
				EnsureDefaultUiInstance("S7_01", CommunicationType.S7, null, null, _config.S7);
			}

			foreach (CommunicationInstanceConfig instance in _config.Instances)
			{
				NormalizeUiInstance(instance);
			}
		}

		private void EnsureDefaultUiInstance(
			string instanceName,
			CommunicationType type,
			TcpIpConfig tcpIp,
			ProfinetConfig profinet,
			S7Config s7)
		{
			if (_config == null || _config.Instances == null)
			{
				return;
			}

			CommunicationInstanceConfig instance = _config.Instances.FirstOrDefault(x =>
				x != null && string.Equals(x.InstanceName, instanceName, StringComparison.OrdinalIgnoreCase));

			if (instance == null)
			{
				instance = new CommunicationInstanceConfig();
				instance.InstanceName = instanceName;
				_config.Instances.Add(instance);
			}

			instance.CommunicationType = type;

			if (type == CommunicationType.TcpIp && tcpIp != null)
			{
				instance.InstanceKind = tcpIp.IsServer ? CommunicationInstanceKind.TcpIpServer : CommunicationInstanceKind.TcpIpClient;
				instance.Enabled = tcpIp.Enabled;
				instance.TcpIp = tcpIp;
				instance.Channels = tcpIp.Channels;
				instance.Heartbeat = tcpIp.Heartbeat;
			}
			else if (type == CommunicationType.Profinet && profinet != null)
			{
				instance.InstanceKind = CommunicationInstanceKind.Profinet;
				instance.Enabled = profinet.Enabled;
				instance.Profinet = profinet;
				instance.Channels = profinet.Channels;
				instance.Heartbeat = profinet.Heartbeat;
			}
			else if (type == CommunicationType.S7 && s7 != null)
			{
				instance.InstanceKind = CommunicationInstanceKind.S7;
				instance.Enabled = s7.Enabled;
				instance.S7 = s7;
				instance.Channels = s7.Channels;
				instance.Heartbeat = s7.Heartbeat;
			}
		}

		private void NormalizeUiInstance(CommunicationInstanceConfig instance)
		{
			if (instance == null)
			{
				return;
			}

			if (instance.InstanceName == null) instance.InstanceName = string.Empty;
			if (instance.Remark == null) instance.Remark = string.Empty;
			if (instance.TcpIp == null) instance.TcpIp = new TcpIpConfig();
			if (instance.Profinet == null) instance.Profinet = new ProfinetConfig();
			if (instance.S7 == null) instance.S7 = new S7Config();

			if (instance.CommunicationType == CommunicationType.TcpIp)
			{
				if (instance.TcpIp.InputVariables == null) instance.TcpIp.InputVariables = new List<CommInputVariable>();
				if (instance.TcpIp.OutputVariables == null) instance.TcpIp.OutputVariables = new List<CommOutputVariable>();
				if (instance.TcpIp.Channels == null) instance.TcpIp.Channels = new List<CommunicationChannelConfig>();
				if (instance.TcpIp.Heartbeat == null) instance.TcpIp.Heartbeat = new CommunicationHeartbeatConfig();
				instance.Channels = instance.TcpIp.Channels;
				instance.Heartbeat = instance.TcpIp.Heartbeat;
				instance.TcpIp.Enabled = instance.Enabled;
			}
			else if (instance.CommunicationType == CommunicationType.Profinet)
			{
				if (instance.Profinet.InputVariables == null) instance.Profinet.InputVariables = new List<CommInputVariable>();
				if (instance.Profinet.OutputVariables == null) instance.Profinet.OutputVariables = new List<CommOutputVariable>();
				if (instance.Profinet.Channels == null) instance.Profinet.Channels = new List<CommunicationChannelConfig>();
				if (instance.Profinet.Heartbeat == null) instance.Profinet.Heartbeat = new CommunicationHeartbeatConfig();
				instance.Channels = instance.Profinet.Channels;
				instance.Heartbeat = instance.Profinet.Heartbeat;
				instance.Profinet.Enabled = instance.Enabled;
			}
			else
			{
				if (instance.S7.InputVariables == null) instance.S7.InputVariables = new List<CommInputVariable>();
				if (instance.S7.OutputVariables == null) instance.S7.OutputVariables = new List<CommOutputVariable>();
				if (instance.S7.Channels == null) instance.S7.Channels = new List<CommunicationChannelConfig>();
				if (instance.S7.Heartbeat == null) instance.S7.Heartbeat = new CommunicationHeartbeatConfig();
				instance.Channels = instance.S7.Channels;
				instance.Heartbeat = instance.S7.Heartbeat;
				instance.S7.Enabled = instance.Enabled;
			}
		}

		private List<CommunicationInstanceConfig> GetInstances(CommunicationType type)
		{
			EnsureUiInstances();

			if (_config == null || _config.Instances == null)
			{
				return new List<CommunicationInstanceConfig>();
			}

			return _config.Instances
				.Where(x => x != null && x.CommunicationType == type)
				.ToList();
		}

		private CommunicationInstanceConfig GetSelectedInstance()
		{
			EnsureUiInstances();

			if (_config == null || _config.Instances == null)
			{
				return null;
			}

			CommunicationInstanceConfig instance = null;

			if (!string.IsNullOrWhiteSpace(_selectedInstanceName))
			{
				instance = _config.Instances.FirstOrDefault(x =>
					x != null &&
					x.CommunicationType == _selectedType &&
					string.Equals(x.InstanceName, _selectedInstanceName, StringComparison.OrdinalIgnoreCase));
			}

			if (instance == null)
			{
				instance = GetFirstInstance(_selectedType);
			}

			if (instance != null)
			{
				_selectedInstanceName = instance.InstanceName;
			}

			return instance;
		}

		private CommunicationInstanceConfig GetFirstInstance(CommunicationType type)
		{
			EnsureUiInstances();

			if (_config == null || _config.Instances == null)
			{
				return null;
			}

			return _config.Instances.FirstOrDefault(x => x != null && x.CommunicationType == type);
		}

		private string GetFirstInstanceName(CommunicationType type)
		{
			CommunicationInstanceConfig instance = GetFirstInstance(type);
			return instance == null ? string.Empty : instance.InstanceName;
		}

		private void EnsureSelectedInstanceForCurrentType()
		{
			CommunicationInstanceConfig instance = GetSelectedInstance();
			if (instance != null)
			{
				_selectedInstanceName = instance.InstanceName;
			}
		}

		private TcpIpConfig GetCurrentTcpConfig()
		{
			CommunicationInstanceConfig instance = GetSelectedInstance();
			if (instance != null && instance.CommunicationType == CommunicationType.TcpIp && instance.TcpIp != null)
			{
				return instance.TcpIp;
			}

			if (_config == null)
			{
				_config = new CommunicationConfig();
			}

			if (_config.TcpIp == null)
			{
				_config.TcpIp = new TcpIpConfig();
			}

			return _config.TcpIp;
		}

		private ProfinetConfig GetCurrentProfinetConfig()
		{
			CommunicationInstanceConfig instance = GetSelectedInstance();
			if (instance != null && instance.CommunicationType == CommunicationType.Profinet && instance.Profinet != null)
			{
				return instance.Profinet;
			}

			if (_config == null)
			{
				_config = new CommunicationConfig();
			}

			if (_config.Profinet == null)
			{
				_config.Profinet = new ProfinetConfig();
			}

			return _config.Profinet;
		}

		private S7Config GetCurrentS7Config()
		{
			CommunicationInstanceConfig instance = GetSelectedInstance();
			if (instance != null && instance.CommunicationType == CommunicationType.S7 && instance.S7 != null)
			{
				return instance.S7;
			}

			if (_config == null)
			{
				_config = new CommunicationConfig();
			}

			if (_config.S7 == null)
			{
				_config.S7 = new S7Config();
			}

			return _config.S7;
		}

		private void btnAddTcpInstance_Click(object sender, EventArgs e)
		{
			AddCommunicationInstance(CommunicationType.TcpIp);
		}

		private void btnAddProfinetInstance_Click(object sender, EventArgs e)
		{
			AddCommunicationInstance(CommunicationType.Profinet);
		}

		private void btnAddS7Instance_Click(object sender, EventArgs e)
		{
			AddCommunicationInstance(CommunicationType.S7);
		}

		private void btnCommunicationInstance_MouseClick(object sender, MouseEventArgs e)
		{
			Button button = sender as Button;
			CommunicationInstanceConfig instance = button == null ? null : button.Tag as CommunicationInstanceConfig;
			if (instance == null)
			{
				return;
			}

			if (CanDeleteCommunicationInstance(instance) && e.X >= button.Width - 44)
			{
				DeleteCommunicationInstance(instance);
				return;
			}

			SelectCommunicationInstance(instance.CommunicationType, instance.InstanceName);
		}

		private bool CanDeleteCommunicationInstance(CommunicationInstanceConfig instance)
		{
			if (instance == null)
			{
				return false;
			}

			return instance.CommunicationType == CommunicationType.TcpIp ||
				   instance.CommunicationType == CommunicationType.Profinet ||
				   instance.CommunicationType == CommunicationType.S7;
		}

		private void DeleteCommunicationInstance(CommunicationInstanceConfig instance)
		{
			if (!CanDeleteCommunicationInstance(instance) || _config == null || _config.Instances == null)
			{
				return;
			}

			if (!ThemedDialog.ConfirmDeleteCommunication(this, instance.InstanceName, _isEnglish))
			{
				return;
			}

			CommunicationType type = instance.CommunicationType;
			if (type == CommunicationType.TcpIp)
			{
				CommunicationRuntimeManager.Instance.StopInstance(instance.InstanceName);

				if (string.Equals(_activeTcpRuntimeInstanceName, instance.InstanceName, StringComparison.OrdinalIgnoreCase))
				{
					_activeTcpRuntimeInstanceName = string.Empty;
				}
			}

			_config.Instances.Remove(instance);

			bool oldLoading = _loading;
			_loading = true;
			try
			{
				CommunicationConfigStore.Save(_config);
			}
			finally
			{
				_loading = oldLoading;
			}

			string nextInstanceName = GetFirstInstanceName(type);
			_selectedInstanceName = string.Empty;
			SelectCommunicationInstance(type, nextInstanceName);
			RefreshCommunicationInstanceTree();
		}

		private void AddCommunicationInstance(CommunicationType type)
		{
			EnsureUiInstances();

			if (type == CommunicationType.Profinet && GetInstances(CommunicationType.Profinet).Count > 0)
			{
				ThemedDialog.ShowWarning(
					this,
					_isEnglish ? "New Communication" : "新建通讯",
					_isEnglish ? "Only one Profinet communication can be created." : "Profinet 通讯只能创建一个。",
					_isEnglish);
				return;
			}

			if (!_loading)
			{
				SaveCurrentTypeParamsFromUI();
				SaveCurrentTypeVariablesFromGrid();
			}

			string defaultName = CreateNextInstanceName(type);
			string title = _isEnglish ? "New Communication" : "新建通讯";

			using (CommunicationInstanceNameDialog dialog =
				new CommunicationInstanceNameDialog(title, defaultName, _isEnglish))
			{
				if (dialog.ShowDialog(this) != DialogResult.OK)
				{
					return;
				}

				string instanceName = dialog.InstanceName;
				if (IsInstanceNameUsed(instanceName))
				{
					ThemedDialog.ShowWarning(
						this,
						title,
						_isEnglish ? "The communication name already exists." : "通讯名称已存在。",
						_isEnglish);
					return;
				}

				CommunicationInstanceConfig instance = CreateCommunicationInstance(type, instanceName);
				_config.Instances.Add(instance);

				bool oldLoading = _loading;
				_loading = true;
				try
				{
					CommunicationConfigStore.Save(_config);
				}
				finally
				{
					_loading = oldLoading;
				}

				SelectCommunicationInstance(type, instanceName);
				RefreshCommunicationInstanceTree();
			}
		}

		private bool IsInstanceNameUsed(string instanceName)
		{
			if (string.IsNullOrWhiteSpace(instanceName) || _config == null || _config.Instances == null)
			{
				return false;
			}

			return _config.Instances.Any(x =>
				x != null && string.Equals(x.InstanceName, instanceName.Trim(), StringComparison.OrdinalIgnoreCase));
		}

		private string CreateNextInstanceName(CommunicationType type)
		{
			string prefix = type == CommunicationType.TcpIp
				? "TCPIP"
				: (type == CommunicationType.Profinet ? "Profinet" : "S7");
			int index = 1;

			while (IsInstanceNameUsed(prefix + "_" + index.ToString("00")))
			{
				index++;
			}

			return prefix + "_" + index.ToString("00");
		}

		private CommunicationInstanceConfig CreateCommunicationInstance(CommunicationType type, string instanceName)
		{
			CommunicationInstanceConfig instance = new CommunicationInstanceConfig();
			instance.InstanceName = instanceName.Trim();
			instance.CommunicationType = type;
			instance.Enabled = false;

			if (type == CommunicationType.TcpIp)
			{
				TcpIpConfig tcpIp = CloneTcpIpConfig(GetCurrentTcpConfig());
				tcpIp.Enabled = false;
				instance.TcpIp = tcpIp;
				instance.InstanceKind = tcpIp.IsServer ? CommunicationInstanceKind.TcpIpServer : CommunicationInstanceKind.TcpIpClient;
				instance.Channels = tcpIp.Channels;
				instance.Heartbeat = tcpIp.Heartbeat;
			}
			else if (type == CommunicationType.Profinet)
			{
				ProfinetConfig profinet = CloneProfinetConfig(GetCurrentProfinetConfig());
				profinet.Enabled = false;
				instance.Profinet = profinet;
				instance.InstanceKind = CommunicationInstanceKind.Profinet;
				instance.Channels = profinet.Channels;
				instance.Heartbeat = profinet.Heartbeat;
			}
			else
			{
				S7Config s7 = CloneS7Config(GetCurrentS7Config());
				s7.Enabled = false;
				instance.S7 = s7;
				instance.InstanceKind = CommunicationInstanceKind.S7;
				instance.Channels = s7.Channels;
				instance.Heartbeat = s7.Heartbeat;
			}

			return instance;
		}

		private TcpIpConfig CloneTcpIpConfig(TcpIpConfig source)
		{
			TcpIpConfig clone = new TcpIpConfig();
			if (source == null)
			{
				return clone;
			}

			clone.Enabled = source.Enabled;
			clone.IsServer = source.IsServer;
			clone.LocalIP = source.LocalIP;
			clone.LocalPort = source.LocalPort;
			clone.RemoteIP = source.RemoteIP;
			clone.RemotePort = source.RemotePort;
			clone.PayloadMode = source.PayloadMode;
			clone.ByteOrder = source.ByteOrder;
			clone.InputVariables = CloneInputVariables(source.InputVariables);
			clone.OutputVariables = CloneOutputVariables(source.OutputVariables);
			clone.Channels = CloneChannelConfigs(source.Channels);
			clone.Heartbeat = CloneHeartbeatConfig(source.Heartbeat);
			return clone;
		}

		private ProfinetConfig CloneProfinetConfig(ProfinetConfig source)
		{
			ProfinetConfig clone = new ProfinetConfig();
			if (source == null)
			{
				return clone;
			}

			clone.Enabled = source.Enabled;
			clone.DeviceName = source.DeviceName;
			clone.StationName = source.StationName;
			clone.ConnectionStatus = source.ConnectionStatus;
			clone.UseGsdFixedMapping = source.UseGsdFixedMapping;
			clone.InputVariables = CloneInputVariables(source.InputVariables);
			clone.OutputVariables = CloneOutputVariables(source.OutputVariables);
			clone.Channels = CloneChannelConfigs(source.Channels);
			clone.Heartbeat = CloneHeartbeatConfig(source.Heartbeat);
			return clone;
		}

		private S7Config CloneS7Config(S7Config source)
		{
			S7Config clone = new S7Config();
			if (source == null)
			{
				return clone;
			}

			clone.Enabled = source.Enabled;
			clone.PlcIP = source.PlcIP;
			clone.Rack = source.Rack;
			clone.Slot = source.Slot;
			clone.InputDB = source.InputDB;
			clone.OutputDB = source.OutputDB;
			clone.InputStartByte = source.InputStartByte;
			clone.OutputStartByte = source.OutputStartByte;
			clone.InputVariables = CloneInputVariables(source.InputVariables);
			clone.OutputVariables = CloneOutputVariables(source.OutputVariables);
			clone.Channels = CloneChannelConfigs(source.Channels);
			clone.Heartbeat = CloneHeartbeatConfig(source.Heartbeat);
			return clone;
		}

		private List<CommInputVariable> CloneInputVariables(List<CommInputVariable> source)
		{
			List<CommInputVariable> result = new List<CommInputVariable>();
			if (source == null)
			{
				return result;
			}

			foreach (CommInputVariable item in source)
			{
				if (item == null)
				{
					continue;
				}

				result.Add(new CommInputVariable
				{
					Name = item.Name,
					UseAsTrigger = item.UseAsTrigger,
					UseAsPosition = item.UseAsPosition,
					EngineName = item.EngineName,
					DataType = item.DataType,
					ByteOffset = item.ByteOffset,
					BitOffset = item.BitOffset,
					Length = item.Length,
					Remark = item.Remark,
					GlobalVariableName = item.GlobalVariableName
				});
			}

			return result;
		}

		private List<CommOutputVariable> CloneOutputVariables(List<CommOutputVariable> source)
		{
			List<CommOutputVariable> result = new List<CommOutputVariable>();
			if (source == null)
			{
				return result;
			}

			foreach (CommOutputVariable item in source)
			{
				if (item == null)
				{
					continue;
				}

				result.Add(new CommOutputVariable
				{
					Name = item.Name,
					DataType = item.DataType,
					ByteOffset = item.ByteOffset,
					BitOffset = item.BitOffset,
					Length = item.Length,
					Remark = item.Remark,
					GlobalVariableName = item.GlobalVariableName
				});
			}

			return result;
		}

		private CommunicationHeartbeatConfig CloneHeartbeatConfig(CommunicationHeartbeatConfig source)
		{
			CommunicationHeartbeatConfig clone = new CommunicationHeartbeatConfig();
			if (source == null)
			{
				return clone;
			}

			clone.Enabled = source.Enabled;
			clone.OutputName = source.OutputName;
			clone.HeartbeatText = source.HeartbeatText;
			clone.IntervalMs = source.IntervalMs;
			return clone;
		}

		private List<CommunicationChannelConfig> CloneChannelConfigs(List<CommunicationChannelConfig> source)
		{
			List<CommunicationChannelConfig> result = new List<CommunicationChannelConfig>();
			if (source == null)
			{
				return result;
			}

			foreach (CommunicationChannelConfig channel in source)
			{
				if (channel == null)
				{
					continue;
				}

				CommunicationChannelConfig clone = new CommunicationChannelConfig();
				clone.ChannelName = channel.ChannelName;
				clone.Enabled = channel.Enabled;
				clone.TriggerName = channel.TriggerName;
				clone.TriggerExpectedValue = channel.TriggerExpectedValue;
				clone.TriggerGlobalVariableName = channel.TriggerGlobalVariableName;
				clone.CustomTriggerGlobalVariableName = channel.CustomTriggerGlobalVariableName;
				clone.CustomTriggerExpectedValue = channel.CustomTriggerExpectedValue;
				clone.CustomTriggers = CloneCustomTriggerOptions(channel.CustomTriggers);
				clone.PositionSourceName = channel.PositionSourceName;
				clone.PositionGlobalVariableName = channel.PositionGlobalVariableName;
				clone.ProgramNoAddressName = channel.ProgramNoAddressName;
				clone.ProgramSwitchEnableName = channel.ProgramSwitchEnableName;
				clone.ProgramSwitchDoneName = channel.ProgramSwitchDoneName;
				clone.ProgramSwitchFailName = channel.ProgramSwitchFailName;
				clone.ChannelReadyOutputName = channel.ChannelReadyOutputName;
				clone.ChannelReadyBusyValue = channel.ChannelReadyBusyValue;
				clone.ChannelReadyDoneValue = channel.ChannelReadyDoneValue;
				clone.ProgramNoOutputName = channel.ProgramNoOutputName;
				clone.PositionOptions = ClonePositionOptions(channel.PositionOptions);
				clone.ProgramJobMap = CloneProgramJobMap(channel.ProgramJobMap);
				result.Add(clone);
			}

			return result;
		}

		private List<CommunicationCustomTriggerOption> CloneCustomTriggerOptions(List<CommunicationCustomTriggerOption> source)
		{
			List<CommunicationCustomTriggerOption> result = new List<CommunicationCustomTriggerOption>();
			if (source == null)
			{
				return result;
			}

			foreach (CommunicationCustomTriggerOption option in source)
			{
				if (option == null)
				{
					continue;
				}

				result.Add(new CommunicationCustomTriggerOption
				{
					Name = option.Name,
					ExpectedValue = option.ExpectedValue,
					Remark = option.Remark
				});
			}

			return result;
		}

		private List<CommunicationPositionOption> ClonePositionOptions(List<CommunicationPositionOption> source)
		{
			List<CommunicationPositionOption> result = new List<CommunicationPositionOption>();
			if (source == null)
			{
				return result;
			}

			foreach (CommunicationPositionOption option in source)
			{
				if (option == null)
				{
					continue;
				}

				result.Add(new CommunicationPositionOption
				{
					Name = option.Name,
					ExpectedValue = option.ExpectedValue,
					Remark = option.Remark
				});
			}

			return result;
		}

		private List<ProgramJobMapItem> CloneProgramJobMap(List<ProgramJobMapItem> source)
		{
			List<ProgramJobMapItem> result = new List<ProgramJobMapItem>();
			if (source == null)
			{
				return result;
			}

			foreach (ProgramJobMapItem item in source)
			{
				if (item == null)
				{
					continue;
				}

				result.Add(new ProgramJobMapItem
				{
					ProgramNo = item.ProgramNo,
					JobName = item.JobName
				});
			}

			return result;
		}

		private bool ValidateCurrentTcpEndpointUnique(bool showMessage)
		{
			if (_selectedType != CommunicationType.TcpIp)
			{
				return true;
			}

			TcpIpConfig tcpIp = GetCurrentTcpConfig();
			if (tcpIp == null)
			{
				return true;
			}

			CommunicationInstanceConfig currentInstance = GetSelectedInstance();
			string currentName = currentInstance == null ? _selectedInstanceName : currentInstance.InstanceName;
			CommunicationInstanceConfig conflictInstance;

			if (!TryFindTcpEndpointConflict(currentName, tcpIp, out conflictInstance))
			{
				return true;
			}

			if (showMessage && conflictInstance != null)
			{
				string endpointText = GetTcpEndpointDisplayText(tcpIp);
				string message = _isEnglish
					? "TCP/IP communication \"" + currentName + "\" uses the same endpoint as \"" +
					  conflictInstance.InstanceName + "\" (" + endpointText + "). Please change the IP or port before saving/connecting."
					: "TCP/IP 通讯 \"" + currentName + "\" 与 \"" +
					  conflictInstance.InstanceName + "\" 的 IP 和端口完全相同（" + endpointText + "）。请先修改 IP 或端口，再保存/连接。";

				ThemedDialog.ShowWarning(
					this,
					_isEnglish ? "TCP/IP Endpoint Conflict" : "TCP/IP 端口冲突",
					message,
					_isEnglish);
			}

			return false;
		}

		private bool TryFindTcpEndpointConflict(
			string currentName,
			TcpIpConfig currentTcpIp,
			out CommunicationInstanceConfig conflictInstance)
		{
			conflictInstance = null;

			if (currentTcpIp == null)
			{
				return false;
			}

			foreach (CommunicationInstanceConfig instance in GetInstances(CommunicationType.TcpIp))
			{
				if (instance == null ||
					instance.TcpIp == null ||
					string.Equals(instance.InstanceName, currentName, StringComparison.OrdinalIgnoreCase))
				{
					continue;
				}

				if (IsSameTcpEndpoint(currentTcpIp, instance.TcpIp))
				{
					conflictInstance = instance;
					return true;
				}
			}

			return false;
		}

		private bool IsSameTcpEndpoint(TcpIpConfig left, TcpIpConfig right)
		{
			if (left == null || right == null)
			{
				return false;
			}

			if (left.IsServer != right.IsServer)
			{
				return false;
			}

			if (left.IsServer)
			{
				return left.LocalPort > 0 &&
					   right.LocalPort > 0 &&
					   left.LocalPort == right.LocalPort &&
					   string.Equals(
						   NormalizeTcpIpAddress(left.LocalIP),
						   NormalizeTcpIpAddress(right.LocalIP),
						   StringComparison.OrdinalIgnoreCase);
			}

			return left.RemotePort > 0 &&
				   right.RemotePort > 0 &&
				   left.RemotePort == right.RemotePort &&
				   string.Equals(
					   NormalizeTcpIpAddress(left.RemoteIP),
					   NormalizeTcpIpAddress(right.RemoteIP),
					   StringComparison.OrdinalIgnoreCase);
		}

		private string NormalizeTcpIpAddress(string ip)
		{
			return string.IsNullOrWhiteSpace(ip) ? "0.0.0.0" : ip.Trim();
		}

		private string GetTcpEndpointDisplayText(TcpIpConfig tcpIp)
		{
			if (tcpIp == null)
			{
				return string.Empty;
			}

			if (tcpIp.IsServer)
			{
				return (string.IsNullOrWhiteSpace(tcpIp.LocalIP) ? "0.0.0.0" : tcpIp.LocalIP.Trim()) +
					   ":" +
					   tcpIp.LocalPort.ToString();
			}

			return (string.IsNullOrWhiteSpace(tcpIp.RemoteIP) ? "0.0.0.0" : tcpIp.RemoteIP.Trim()) +
				   ":" +
				   tcpIp.RemotePort.ToString();
		}

		private void InitializeGridStyle()
		{
			ApplyGridStyle(dgvInput);
			ApplyGridStyle(dgvOutput);

			dgvInput.DataError -= dgv_DataError;
			dgvOutput.DataError -= dgv_DataError;
			dgvInput.DataError += dgv_DataError;
			dgvOutput.DataError += dgv_DataError;

			dgvInput.CurrentCellDirtyStateChanged -= dgv_CurrentCellDirtyStateChanged;
			dgvOutput.CurrentCellDirtyStateChanged -= dgv_CurrentCellDirtyStateChanged;
			dgvInput.CurrentCellDirtyStateChanged += dgv_CurrentCellDirtyStateChanged;
			dgvOutput.CurrentCellDirtyStateChanged += dgv_CurrentCellDirtyStateChanged;

			dgvInput.CellValueChanged -= dgv_CellValueChanged;
			dgvOutput.CellValueChanged -= dgv_CellValueChanged;
			dgvInput.CellValueChanged += dgv_CellValueChanged;
			dgvOutput.CellValueChanged += dgv_CellValueChanged;
		}

		private void NormalizeRightPanelMargins()
		{
			if (rightLayout != null)
			{
				rightLayout.Padding = Padding.Empty;
			}

			if (panelInput != null)
			{
				panelInput.Margin = new Padding(0, 0, 0, 4);
			}

			if (panelOutput != null)
			{
				panelOutput.Margin = new Padding(0, 4, 0, 0);
			}
		}

		private void InitializeVariableButtonLayout()
		{
			if (panelInputButtons != null)
			{
				panelInputButtons.Resize += delegate { LayoutVariableButtons(); };
			}

			if (panelOutputButtons != null)
			{
				panelOutputButtons.Resize += delegate { LayoutVariableButtons(); };
			}

			if (btnSave != null)
			{
				btnSave.Anchor = AnchorStyles.Top;
			}

			LayoutVariableButtons();
		}

		private void LayoutVariableButtons()
		{
			LayoutButtonRow(panelInputButtons, new Button[]
			{
				btnAddInput,
				btnDeleteInput,
				btnMoveUpInput,
				btnMoveDownInput
			});

			LayoutButtonRow(panelOutputButtons, new Button[]
			{
				btnAddOutput,
				btnDeleteOutput,
				btnMoveUpOutput,
				btnMoveDownOutput,
				btnSave
			});
		}

		private void LayoutButtonRow(Panel panel, Button[] buttons)
		{
			if (panel == null || buttons == null || buttons.Length <= 0)
			{
				return;
			}

			List<Button> visibleButtons = buttons
				.Where(button => button != null && !button.IsDisposed)
				.ToList();

			if (visibleButtons.Count <= 0)
			{
				return;
			}

			int buttonWidth = visibleButtons.Count >= 5 ? 100 : 110;
			int buttonHeight = 30;
			int gap = 16;
			int x = 0;
			int y = Math.Max(0, (panel.ClientSize.Height - buttonHeight) / 2);

			foreach (Button button in visibleButtons)
			{
				button.Anchor = AnchorStyles.Top;
				button.Size = new Size(buttonWidth, buttonHeight);
				button.Location = new Point(x, y);
				x += buttonWidth + gap;
			}
		}

		private void dgv_DataError(object sender, DataGridViewDataErrorEventArgs e)
		{
			e.ThrowException = false;
		}

		private void dgv_CurrentCellDirtyStateChanged(object sender, EventArgs e)
		{
			DataGridView grid = sender as DataGridView;
			if (grid == null || !grid.IsCurrentCellDirty)
			{
				return;
			}

			grid.CommitEdit(DataGridViewDataErrorContexts.Commit);
		}

		private void dgv_CellValueChanged(object sender, DataGridViewCellEventArgs e)
		{
			if (_loading || _validatingRangeCells || e.RowIndex < 0 || e.ColumnIndex < 0)
			{
				return;
			}

			DataGridView grid = sender as DataGridView;
			if (grid == null || e.RowIndex >= grid.Rows.Count)
			{
				return;
			}

			string columnName = grid.Columns[e.ColumnIndex].Name;

			if (columnName == "colInputType")
			{
				if (IsTcpPayloadModeByte())
				{
					ApplyTcpByteLengthRule(grid.Rows[e.RowIndex], true);
				}
			}
			else if (columnName == "colOutputType")
			{
				if (IsTcpPayloadModeByte())
				{
					ApplyTcpByteLengthRule(grid.Rows[e.RowIndex], false);
				}
			}

			if (columnName == "colInputType" ||
				columnName == "colInputByteOffset" ||
				columnName == "colInputLength" ||
				columnName == "colOutputType" ||
				columnName == "colOutputByteOffset" ||
				columnName == "colOutputLength")
			{
				ValidateCommunicationRangeGrid(grid);
			}
		}

		private void ApplyGridStyle(DataGridView dgv)
		{
			dgv.EnableHeadersVisualStyles = false;
			dgv.BackgroundColor = Color.FromArgb(2, 10, 20);
			dgv.GridColor = Color.FromArgb(45, 70, 95);
			dgv.BorderStyle = BorderStyle.None;
			dgv.RowHeadersVisible = false;
			dgv.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
			dgv.MultiSelect = false;
			dgv.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None;

			dgv.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(8, 28, 48);
			dgv.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
			dgv.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
			dgv.ColumnHeadersDefaultCellStyle.Font = new Font("Microsoft YaHei UI", 9F, FontStyle.Bold);

			dgv.DefaultCellStyle.BackColor = Color.FromArgb(2, 10, 20);
			dgv.DefaultCellStyle.ForeColor = Color.White;
			dgv.DefaultCellStyle.SelectionBackColor = Color.FromArgb(0, 120, 200);
			dgv.DefaultCellStyle.SelectionForeColor = Color.White;
			dgv.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
		}

		private void InitializeComboColumns()
		{
			ConfigureInputSecondColumn(false);
			RefreshDataTypeComboItems();
			EnsureGlobalVariableColumns();

			cmbMode.Items.Clear();
			cmbMode.Items.Add("Server");
			cmbMode.Items.Add("Client");
			cmbMode.SelectedIndex = 0;
			cmbMode.SelectedIndexChanged += cmbMode_SelectedIndexChanged;
		}

		private void EnsureGlobalVariableColumns()
		{
			if (!dgvInput.Columns.Contains("colInputGlobalVariable"))
			{
				dgvInput.Columns.Add(GlobalVariableBindingUi.CreateButtonColumn("colInputGlobalVariable", "关联全局变量", 150));
			}

			if (!dgvInput.Columns.Contains("colInputCurrentValue"))
			{
				DataGridViewTextBoxColumn currentColumn = new DataGridViewTextBoxColumn();
				currentColumn.Name = "colInputCurrentValue";
				currentColumn.HeaderText = "当前值";
				currentColumn.Width = 120;
				currentColumn.ReadOnly = true;
				dgvInput.Columns.Add(currentColumn);
			}

			if (!dgvOutput.Columns.Contains("colOutputGlobalVariable"))
			{
				dgvOutput.Columns.Add(GlobalVariableBindingUi.CreateButtonColumn("colOutputGlobalVariable", "关联来源", 150));
			}

			if (!dgvOutput.Columns.Contains("colOutputCurrentValue"))
			{
				DataGridViewTextBoxColumn currentColumn = new DataGridViewTextBoxColumn();
				currentColumn.Name = "colOutputCurrentValue";
				currentColumn.HeaderText = "当前值";
				currentColumn.Width = 120;
				currentColumn.ReadOnly = true;
				dgvOutput.Columns.Add(currentColumn);
			}

			dgvInput.CellContentClick -= VariableBindingCellContentClick;
			dgvOutput.CellContentClick -= VariableBindingCellContentClick;
			dgvInput.CellContentClick += VariableBindingCellContentClick;
			dgvOutput.CellContentClick += VariableBindingCellContentClick;

			ApplyVariableColumnDisplayOrder();
		}

		private void ApplyVariableColumnDisplayOrder()
		{
			SetDisplayIndex(dgvInput, "colInputName", 0);
			SetDisplayIndex(dgvInput, "colInputType", 1);
			SetDisplayIndex(dgvInput, "colInputByteOffset", 2);
			SetDisplayIndex(dgvInput, "colInputLength", 3);
			SetDisplayIndex(dgvInput, "colInputCurrentValue", 4);
			SetDisplayIndex(dgvInput, "colInputGlobalVariable", 5);
			SetDisplayIndex(dgvInput, "colInputRemark", 6);

			SetDisplayIndex(dgvOutput, "colOutputName", 0);
			SetDisplayIndex(dgvOutput, "colOutputType", 1);
			SetDisplayIndex(dgvOutput, "colOutputByteOffset", 2);
			SetDisplayIndex(dgvOutput, "colOutputLength", 3);
			SetDisplayIndex(dgvOutput, "colOutputCurrentValue", 4);
			SetDisplayIndex(dgvOutput, "colOutputGlobalVariable", 5);
			SetDisplayIndex(dgvOutput, "colOutputRemark", 6);
		}

		private void SetDisplayIndex(DataGridView grid, string columnName, int displayIndex)
		{
			if (grid == null || !grid.Columns.Contains(columnName))
			{
				return;
			}

			int maxIndex = Math.Max(0, grid.Columns.Count - 1);
			grid.Columns[columnName].DisplayIndex = Math.Min(displayIndex, maxIndex);
		}

		private void VariableBindingCellContentClick(object sender, DataGridViewCellEventArgs e)
		{
			DataGridView grid = sender as DataGridView;
			if (grid == null || e.RowIndex < 0 || e.ColumnIndex < 0)
			{
				return;
			}

			string columnName = grid.Columns[e.ColumnIndex].Name;
			if (columnName != "colInputGlobalVariable" && columnName != "colOutputGlobalVariable")
			{
				return;
			}

			if (GlobalVariableBindingUi.SelectForCell(this, grid.Rows[e.RowIndex], columnName))
			{
				if (columnName == "colInputGlobalVariable")
				{
					UpdateInputCurrentValueCell(grid.Rows[e.RowIndex]);
				}
				else
				{
					UpdateOutputCurrentValueCell(grid.Rows[e.RowIndex]);
				}
			}
		}

		private void cmbMode_SelectedIndexChanged(object sender, EventArgs e)
		{
			if (_loading || _selectedType != CommunicationType.TcpIp)
			{
				return;
			}

			SaveCurrentTypeParamsFromUI();
			SyncTcpDedicatedControlsFromConfig();
			ApplyTcpModeParamVisibility();
		}

		private void cmbTcpPayloadMode_SelectedIndexChanged(object sender, EventArgs e)
		{
			if (_loading || _selectedType != CommunicationType.TcpIp)
			{
				return;
			}

			SaveCurrentTypeParamsFromUI();
			RefreshDataTypeComboItems();
			ApplyColumnModeByCommunicationType();

			if (!IsTcpPayloadModeByte())
			{
				ForceTcpGridTypeToString();
			}
			else
			{
				ApplyTcpByteLengthRulesToGrid();
			}

			ValidateCommunicationRangeGrids();
			LayoutTcpParameterArea();
		}

		private void cmbTcpByteOrder_SelectedIndexChanged(object sender, EventArgs e)
		{
			if (_loading || _selectedType != CommunicationType.TcpIp)
			{
				return;
			}

			SaveCurrentTypeParamsFromUI();
		}

		private void InitializeTcpConnectionControls()
		{
			lblTcpParam1 = CreateTcpParamLabel();
			txtTcpParam1 = CreateTcpParamTextBox();

			lblTcpParam2 = CreateTcpParamLabel();
			txtTcpParam2 = CreateTcpParamTextBox();

			lblTcpPayloadMode = CreateTcpParamLabel();
			cmbTcpPayloadMode = CreateTcpParamComboBox();
			cmbTcpPayloadMode.Items.Add("String");
			cmbTcpPayloadMode.Items.Add("Byte");
			cmbTcpPayloadMode.SelectedIndex = 0;
			cmbTcpPayloadMode.SelectedIndexChanged += cmbTcpPayloadMode_SelectedIndexChanged;

			lblTcpByteOrder = CreateTcpParamLabel();
			cmbTcpByteOrder = CreateTcpParamComboBox();
			cmbTcpByteOrder.Items.Add("Big Endian");
			cmbTcpByteOrder.Items.Add("Little Endian");
			cmbTcpByteOrder.SelectedIndex = 0;
			cmbTcpByteOrder.SelectedIndexChanged += cmbTcpByteOrder_SelectedIndexChanged;

			btnTcpConnect = CreateTcpSmallButton("连接", 0, 0, 90, 30);
			btnTcpDisconnect = CreateTcpSmallButton("断开", 0, 0, 90, 30);

			pnlTcpStatusLight = new Panel();
			pnlTcpStatusLight.Name = "pnlTcpStatusLight";
			pnlTcpStatusLight.Size = new Size(12, 12);
			pnlTcpStatusLight.BackColor = Color.FromArgb(120, 120, 120);

			lblTcpStatus = new Label();
			lblTcpStatus.Name = "lblTcpStatus";
			lblTcpStatus.Size = new Size(180, 28);
			lblTcpStatus.TextAlign = ContentAlignment.MiddleLeft;
			lblTcpStatus.ForeColor = Color.FromArgb(150, 170, 190);
			lblTcpStatus.BackColor = Color.Transparent;
			lblTcpStatus.Text = "Stopped";

			btnTcpConnect.Click += btnTcpConnect_Click;
			btnTcpDisconnect.Click += btnTcpDisconnect_Click;

			EnsureTcpControlsParent();

			grpParams.Resize += delegate
			{
				LayoutTcpParameterArea();
			};

			LayoutTcpParameterArea();
		}

		private Label CreateTcpParamLabel()
		{
			Label label = new Label();
			label.AutoSize = false;
			label.TextAlign = ContentAlignment.MiddleLeft;
			label.ForeColor = Color.White;
			label.BackColor = Color.Transparent;
			label.Font = new Font("Microsoft YaHei UI", 9F, FontStyle.Bold);
			label.Visible = false;
			return label;
		}

		private TextBox CreateTcpParamTextBox()
		{
			TextBox txt = new TextBox();
			txt.BorderStyle = BorderStyle.FixedSingle;
			txt.BackColor = Color.FromArgb(2, 10, 20);
			txt.ForeColor = Color.White;
			txt.Font = new Font("Microsoft YaHei UI", 9F, FontStyle.Regular);
			txt.Visible = false;
			return txt;
		}

		private ComboBox CreateTcpParamComboBox()
		{
			ComboBox cmb = new ComboBox();
			cmb.DropDownStyle = ComboBoxStyle.DropDownList;
			cmb.FlatStyle = FlatStyle.Flat;
			cmb.BackColor = Color.FromArgb(2, 10, 20);
			cmb.ForeColor = Color.White;
			cmb.Font = new Font("Microsoft YaHei UI", 9F, FontStyle.Regular);
			cmb.Visible = false;
			return cmb;
		}

		private void PositionTcpConnectionControls()
		{
			LayoutTcpParameterArea();
		}

		private void EnsureTcpControlsParent()
		{
			if (grpParams == null)
			{
				return;
			}

			// TCP/IP 页面使用专用参数控件，避免复用 Designer 里的 txtP1~txtP6 时出现父容器和坐标错乱。
			MoveControlToGroup(lblTcpParam1);
			MoveControlToGroup(txtTcpParam1);
			MoveControlToGroup(lblTcpParam2);
			MoveControlToGroup(txtTcpParam2);
			MoveControlToGroup(lblTcpPayloadMode);
			MoveControlToGroup(cmbTcpPayloadMode);
			MoveControlToGroup(lblTcpByteOrder);
			MoveControlToGroup(cmbTcpByteOrder);

			MoveControlToGroup(cmbMode);

			MoveControlToGroup(btnTcpConnect);
			MoveControlToGroup(btnTcpDisconnect);
			MoveControlToGroup(pnlTcpStatusLight);
			MoveControlToGroup(lblTcpStatus);
		}

		private void MoveControlToGroup(Control control)
		{
			if (control == null || grpParams == null)
			{
				return;
			}

			if (control.Parent == grpParams)
			{
				return;
			}

			grpParams.Controls.Add(control);
		}

		private void LayoutTcpParameterArea()
		{
			if (grpParams == null ||
				lblTcpParam1 == null ||
				txtTcpParam1 == null ||
				lblTcpParam2 == null ||
				txtTcpParam2 == null ||
				lblTcpPayloadMode == null ||
				cmbTcpPayloadMode == null ||
				lblTcpByteOrder == null ||
				cmbTcpByteOrder == null ||
				btnTcpConnect == null ||
				btnTcpDisconnect == null ||
				pnlTcpStatusLight == null ||
				lblTcpStatus == null)
			{
				return;
			}

			EnsureTcpControlsParent();

			if (_selectedType != CommunicationType.TcpIp)
			{
				HideTcpDedicatedControls();
				return;
			}

			bool isServer = true;
			if (cmbMode != null)
			{
				isServer = cmbMode.SelectedIndex <= 0;
			}
			TcpIpConfig tcpIp = GetCurrentTcpConfig();

			grpParams.SuspendLayout();

			try
			{
				// TCP/IP 页面不再复用 lblP1~lblP6 / txtP1~txtP6 显示参数。
				// 这些控件在 Designer 中可能属于不同父容器，容易造成坐标错乱或不可见。
				HideCommonParamControls();

				lblTcpParam1.Visible = true;
				txtTcpParam1.Visible = true;

				lblTcpParam2.Visible = !isServer;
				txtTcpParam2.Visible = !isServer;
				lblTcpPayloadMode.Visible = true;
				cmbTcpPayloadMode.Visible = true;

				bool byteMode = IsTcpPayloadModeByte();
				lblTcpByteOrder.Visible = byteMode;
				cmbTcpByteOrder.Visible = byteMode;

				btnTcpConnect.Visible = true;
				btnTcpDisconnect.Visible = true;
				pnlTcpStatusLight.Visible = true;
				lblTcpStatus.Visible = true;

				if (cmbMode != null)
				{
					cmbMode.Visible = true;
				}

				if (isServer)
				{
					grpParams.Text = _isEnglish ? "TCP/IP Server Parameters" : "TCP/IP Server 参数";
					lblTcpParam1.Text = _isEnglish ? "Listen Port" : "监听端口";
					txtTcpParam1.Text = string.IsNullOrWhiteSpace(txtTcpParam1.Text)
						? (tcpIp == null || tcpIp.LocalPort <= 0 ? "5000" : tcpIp.LocalPort.ToString())
						: txtTcpParam1.Text;
				}
				else
				{
					grpParams.Text = _isEnglish ? "TCP/IP Client Parameters" : "TCP/IP Client 参数";
					lblTcpParam1.Text = _isEnglish ? "Server IP" : "服务器IP";
					lblTcpParam2.Text = _isEnglish ? "Server Port" : "服务器端口";

					if (string.IsNullOrWhiteSpace(txtTcpParam1.Text) && tcpIp != null)
					{
						txtTcpParam1.Text = tcpIp.RemoteIP;
					}

					if (string.IsNullOrWhiteSpace(txtTcpParam2.Text) && tcpIp != null)
					{
						txtTcpParam2.Text = tcpIp.RemotePort <= 0 ? "5000" : tcpIp.RemotePort.ToString();
					}
				}

				lblTcpPayloadMode.Text = _isEnglish ? "Data Mode" : "数据模式";
				lblTcpByteOrder.Text = _isEnglish ? "Byte Order" : "字节序";

				// 固定坐标：Server / Client 切换时，模式、连接、断开、状态灯位置保持不变。
				int labelX = 28;
				int inputX = 145;
				int row1Y = 45;
				int row2Y = 80;
				int row3Y = 115;
				int row4Y = 150;
				int row5Y = 185;
				int row6Y = 220;
				int row7Y = 254;

				int labelW = 110;
				int inputH = 26;
				int buttonW = 95;
				int buttonH = 30;
				int buttonGap = 14;

				int inputWidth = grpParams.ClientSize.Width - inputX - 28;
				if (inputWidth < 150)
				{
					inputWidth = 150;
				}
				if (inputWidth > 230)
				{
					inputWidth = 230;
				}

				lblTcpParam1.SetBounds(labelX, row1Y + 3, labelW, 24);
				txtTcpParam1.SetBounds(inputX, row1Y, inputWidth, inputH);

				lblTcpParam2.SetBounds(labelX, row2Y + 3, labelW, 24);
				txtTcpParam2.SetBounds(inputX, row2Y, inputWidth, inputH);

				if (lblP5 != null)
				{
					lblP5.Visible = true;
					lblP5.Text = _isEnglish ? "Mode" : "模式";
					lblP5.SetBounds(labelX, row3Y + 3, labelW, 24);
					lblP5.Parent = grpParams;
				}

				if (cmbMode != null)
				{
					cmbMode.SetBounds(inputX, row3Y, inputWidth, inputH);
				}

				lblTcpPayloadMode.SetBounds(labelX, row4Y + 3, labelW, 24);
				cmbTcpPayloadMode.SetBounds(inputX, row4Y, inputWidth, inputH);

				lblTcpByteOrder.SetBounds(labelX, row5Y + 3, labelW, 24);
				cmbTcpByteOrder.SetBounds(inputX, row5Y, inputWidth, inputH);

				btnTcpConnect.SetBounds(labelX, row6Y, buttonW, buttonH);
				btnTcpDisconnect.SetBounds(btnTcpConnect.Right + buttonGap, row6Y, buttonW, buttonH);

				pnlTcpStatusLight.SetBounds(labelX + 6, row7Y + 8, 12, 12);

				int statusWidth = grpParams.ClientSize.Width - pnlTcpStatusLight.Right - 40;
				if (statusWidth < 120)
				{
					statusWidth = 120;
				}

				lblTcpStatus.SetBounds(pnlTcpStatusLight.Right + 12, row7Y, statusWidth, 28);

				lblTcpParam1.BringToFront();
				txtTcpParam1.BringToFront();
				lblTcpParam2.BringToFront();
				txtTcpParam2.BringToFront();
				lblTcpPayloadMode.BringToFront();
				cmbTcpPayloadMode.BringToFront();
				lblTcpByteOrder.BringToFront();
				cmbTcpByteOrder.BringToFront();

				if (lblP5 != null)
				{
					lblP5.BringToFront();
				}

				if (cmbMode != null)
				{
					cmbMode.BringToFront();
				}

				btnTcpConnect.BringToFront();
				btnTcpDisconnect.BringToFront();
				pnlTcpStatusLight.BringToFront();
				lblTcpStatus.BringToFront();
			}
			finally
			{
				grpParams.ResumeLayout(false);
			}
		}

		private void HideCommonParamControls()
		{
			if (lblP1 != null) lblP1.Visible = false;
			if (txtP1 != null) txtP1.Visible = false;

			if (lblP2 != null) lblP2.Visible = false;
			if (txtP2 != null) txtP2.Visible = false;

			if (lblP3 != null) lblP3.Visible = false;
			if (txtP3 != null) txtP3.Visible = false;

			if (lblP4 != null) lblP4.Visible = false;
			if (txtP4 != null) txtP4.Visible = false;

			if (txtP5 != null) txtP5.Visible = false;

			if (lblP6 != null) lblP6.Visible = false;
			if (txtP6 != null) txtP6.Visible = false;
		}

		private void HideTcpDedicatedControls()
		{
			if (lblTcpParam1 != null) lblTcpParam1.Visible = false;
			if (txtTcpParam1 != null) txtTcpParam1.Visible = false;
			if (lblTcpParam2 != null) lblTcpParam2.Visible = false;
			if (txtTcpParam2 != null) txtTcpParam2.Visible = false;
			if (lblTcpPayloadMode != null) lblTcpPayloadMode.Visible = false;
			if (cmbTcpPayloadMode != null) cmbTcpPayloadMode.Visible = false;
			if (lblTcpByteOrder != null) lblTcpByteOrder.Visible = false;
			if (cmbTcpByteOrder != null) cmbTcpByteOrder.Visible = false;

			if (btnTcpConnect != null) btnTcpConnect.Visible = false;
			if (btnTcpDisconnect != null) btnTcpDisconnect.Visible = false;
			if (pnlTcpStatusLight != null) pnlTcpStatusLight.Visible = false;
			if (lblTcpStatus != null) lblTcpStatus.Visible = false;
		}

		private void SyncTcpDedicatedControlsFromConfig()
		{
			if (_config == null || txtTcpParam1 == null || txtTcpParam2 == null || cmbMode == null)
			{
				return;
			}

			TcpIpConfig tcpIp = GetCurrentTcpConfig();
			bool isServer = cmbMode.SelectedIndex <= 0;

			if (isServer)
			{
				txtTcpParam1.Text = tcpIp.LocalPort <= 0 ? "5000" : tcpIp.LocalPort.ToString();
				txtTcpParam2.Text = string.Empty;
			}
			else
			{
				txtTcpParam1.Text = tcpIp.RemoteIP;
				txtTcpParam2.Text = tcpIp.RemotePort <= 0 ? "5000" : tcpIp.RemotePort.ToString();
			}

			if (cmbTcpPayloadMode != null)
			{
				cmbTcpPayloadMode.SelectedIndex = tcpIp.PayloadMode == TcpIpPayloadMode.Byte ? 1 : 0;
			}

			if (cmbTcpByteOrder != null)
			{
				cmbTcpByteOrder.SelectedIndex = tcpIp.ByteOrder == CommByteOrder.LittleEndian ? 1 : 0;
			}
		}

		private void SetTcpParamVisible(
			bool row1,
			bool row2,
			bool row3,
			bool row4,
			bool row5,
			bool row6)
		{
			// 保留这个方法，避免旧代码调用时报错。
			// TCP/IP 页面实际显示由专用控件 lblTcpParam1/txtTcpParam1/lblTcpParam2/txtTcpParam2 管理。
			if (lblP1 != null) lblP1.Visible = row1;
			if (txtP1 != null) txtP1.Visible = row1;

			if (lblP2 != null) lblP2.Visible = row2;
			if (txtP2 != null) txtP2.Visible = row2;

			if (lblP3 != null) lblP3.Visible = row3;
			if (txtP3 != null) txtP3.Visible = row3;

			if (lblP4 != null) lblP4.Visible = row4;
			if (txtP4 != null) txtP4.Visible = row4;

			if (lblP5 != null) lblP5.Visible = row5;
			if (txtP5 != null) txtP5.Visible = row5;

			if (lblP6 != null) lblP6.Visible = row6;
			if (txtP6 != null) txtP6.Visible = row6;

			if (cmbMode != null) cmbMode.Visible = false;
		}

		private void btnTcpConnect_Click(object sender, EventArgs e)
		{
			PositionTcpConnectionControls();

			if (_selectedType != CommunicationType.TcpIp)
			{
				return;
			}

			SaveCurrentTypeParamsFromUI();
			SaveCurrentTypeVariablesFromGrid();

			if (_config == null)
			{
				_config = new CommunicationConfig();
			}

			CommunicationInstanceConfig selectedInstance = GetSelectedInstance();
			TcpIpConfig tcpIp = GetCurrentTcpConfig();
			tcpIp.Enabled = chkEnable.Checked;
			_config.SelectedType = _selectedType;

			if (!tcpIp.Enabled)
			{
				ThemedDialog.ShowInformation(
					this,
					"TCP/IP",
					_isEnglish ? "Please enable TCP/IP first." : "请先启用 TCP/IP。",
					_isEnglish);
				return;
			}

			if (!ValidateCurrentTcpEndpointUnique(true))
			{
				return;
			}

			CommunicationConfigStore.Save(_config);

			_activeTcpRuntimeInstanceName = selectedInstance == null
				? _selectedInstanceName
				: selectedInstance.InstanceName;

			if (selectedInstance == null)
			{
				selectedInstance = GetSelectedInstance();
			}

			CommunicationRuntimeManager.Instance.StartInstance(selectedInstance);
			UpdateTcpStatusUi();
		}

		private void btnTcpDisconnect_Click(object sender, EventArgs e)
		{
			if (_selectedType == CommunicationType.TcpIp)
			{
				CommunicationRuntimeManager.Instance.StopInstance(_selectedInstanceName);
			}

			if (string.Equals(_activeTcpRuntimeInstanceName, _selectedInstanceName, StringComparison.OrdinalIgnoreCase))
			{
				_activeTcpRuntimeInstanceName = string.Empty;
			}

			UpdateTcpStatusUi();
		}

		private void BindTcpRuntimeEvents()
		{
			if (_tcpRuntimeEventBound)
			{
				return;
			}

			_tcpRuntimeEventBound = true;

			CommunicationRuntimeManager.Instance.StatusChanged += CommunicationRuntime_StatusChanged;
			CommunicationRuntimeManager.Instance.DataReceived += CommunicationRuntime_DataReceived;
			CommunicationRuntimeManager.Instance.ErrorOccurred += CommunicationRuntime_ErrorOccurred;
		}

		private void CommunicationRuntime_StatusChanged(object sender, CommunicationStatusChangedEventArgs e)
		{
			if (e == null || e.CommunicationType != CommunicationType.TcpIp)
			{
				return;
			}

			AppendTcpReceiveText(
				DateTime.Now.ToString("HH:mm:ss.fff") +
				"  [TCP Status][" + e.InstanceName + "] " +
				e.State +
				"  " +
				e.Message);

			UpdateTcpStatusUiSafe();
		}

		private void CommunicationRuntime_DataReceived(object sender, CommunicationDataReceivedEventArgs e)
		{
			if (e == null)
			{
				return;
			}

			CacheLatestInputValues(e);
			RefreshInputCurrentValuesForEventSafe(e);

			if (e.CommunicationType != CommunicationType.TcpIp)
			{
				return;
			}

			string text = DateTime.Now.ToString("HH:mm:ss.fff") +
						  "  [TCP Receive][" + e.InstanceName + "] " +
						  e.RawText;

			if (e.Values != null && e.Values.Count > 0)
			{
				text += Environment.NewLine + "  Parsed: ";

				foreach (KeyValuePair<string, string> pair in e.Values)
				{
					text += pair.Key + "=" + pair.Value + "; ";
				}
			}

			AppendTcpReceiveText(text);
			UpdateTcpStatusUiSafe();
		}

		private void CacheLatestInputValues(CommunicationDataReceivedEventArgs e)
		{
			if (e == null)
			{
				return;
			}

			Dictionary<string, string> values =
				new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

			if (e.Values != null)
			{
				foreach (KeyValuePair<string, string> pair in e.Values)
				{
					values[pair.Key] = pair.Value;
				}
			}

			string key = GetCommunicationCacheKey(e.CommunicationType, e.InstanceName);
			lock (_latestInputValuesSyncRoot)
			{
				_latestInputValuesByCommunication[key] = values;
			}
		}

		private string GetCommunicationCacheKey(CommunicationType communicationType, string instanceName)
		{
			string protocolName = CommunicationRuntimeNaming.GetProtocolName(communicationType);
			string normalizedInstanceName =
				CommunicationRuntimeNaming.NormalizeInstanceName(protocolName, instanceName, _config);
			return CommunicationRuntimeNaming.FormatCommunicationName(protocolName, normalizedInstanceName);
		}

		private string GetSelectedCommunicationCacheKey()
		{
			string protocolName = GetSelectedProtocolName();
			string normalizedInstanceName =
				CommunicationRuntimeNaming.NormalizeInstanceName(protocolName, _selectedInstanceName, _config);
			return CommunicationRuntimeNaming.FormatCommunicationName(protocolName, normalizedInstanceName);
		}

		private void RefreshInputCurrentValuesForEventSafe(CommunicationDataReceivedEventArgs e)
		{
			if (IsDisposed || !IsHandleCreated || e == null)
			{
				return;
			}

			string eventKey = GetCommunicationCacheKey(e.CommunicationType, e.InstanceName);

			if (InvokeRequired)
			{
				BeginInvoke(new Action(delegate
				{
					if (string.Equals(eventKey, GetSelectedCommunicationCacheKey(), StringComparison.OrdinalIgnoreCase))
					{
						RefreshInputCurrentValues();
					}
				}));
				return;
			}

			if (string.Equals(eventKey, GetSelectedCommunicationCacheKey(), StringComparison.OrdinalIgnoreCase))
			{
				RefreshInputCurrentValues();
			}
		}

		private void CommunicationRuntime_ErrorOccurred(object sender, Exception e)
		{
			if (e == null)
			{
				return;
			}

			AppendTcpReceiveText(
				DateTime.Now.ToString("HH:mm:ss.fff") +
				"  [TCP Error][" + GetRuntimeInstanceName(sender) + "] " +
				e.Message);

			UpdateTcpStatusUiSafe();
		}

		private string GetRuntimeInstanceName(object sender)
		{
			ICommunicationRuntime runtime = sender as ICommunicationRuntime;
			if (runtime != null && !string.IsNullOrWhiteSpace(runtime.InstanceName))
			{
				return runtime.InstanceName;
			}

			return string.IsNullOrWhiteSpace(_selectedInstanceName) ? "-" : _selectedInstanceName;
		}

		private void UpdateTcpStatusUiSafe()
		{
			if (this.IsDisposed)
			{
				return;
			}

			if (this.InvokeRequired)
			{
				this.BeginInvoke(new MethodInvoker(delegate
				{
					UpdateTcpStatusUi();
				}));
				return;
			}

			UpdateTcpStatusUi();
		}

		private void UpdateTcpStatusUi()
		{
			PositionTcpConnectionControls();
			if (pnlTcpStatusLight == null || lblTcpStatus == null)
			{
				return;
			}

			CommunicationConnectionState state = CommunicationConnectionState.Stopped;
			bool isRunning = false;
			bool isConnected = false;

			ICommunicationRuntime runtime = _selectedType == CommunicationType.TcpIp
				? CommunicationRuntimeManager.Instance.GetRuntime(_selectedInstanceName)
				: null;

			if (runtime != null)
			{
				state = runtime.State;
				isRunning = runtime.IsRunning;
				isConnected = runtime.IsConnected;
			}

			if (isConnected)
			{
				pnlTcpStatusLight.BackColor = Color.LimeGreen;
				lblTcpStatus.ForeColor = Color.LimeGreen;
				lblTcpStatus.Text = "Connected";
			}
			else if (state == CommunicationConnectionState.Listening)
			{
				pnlTcpStatusLight.BackColor = Color.DeepSkyBlue;
				lblTcpStatus.ForeColor = Color.DeepSkyBlue;
				lblTcpStatus.Text = "Listening";
			}
			else if (state == CommunicationConnectionState.Connecting)
			{
				pnlTcpStatusLight.BackColor = Color.Gold;
				lblTcpStatus.ForeColor = Color.Gold;
				lblTcpStatus.Text = "Connecting";
			}
			else if (state == CommunicationConnectionState.Error)
			{
				pnlTcpStatusLight.BackColor = Color.OrangeRed;
				lblTcpStatus.ForeColor = Color.OrangeRed;
				lblTcpStatus.Text = "Error";
			}
			else if (isRunning)
			{
				pnlTcpStatusLight.BackColor = Color.Gold;
				lblTcpStatus.ForeColor = Color.Gold;
				lblTcpStatus.Text = state.ToString();
			}
			else
			{
				pnlTcpStatusLight.BackColor = Color.FromArgb(120, 120, 120);
				lblTcpStatus.ForeColor = Color.FromArgb(150, 170, 190);
				lblTcpStatus.Text = "Stopped";
			}

			if (btnTcpConnect != null)
			{
				btnTcpConnect.Enabled = _selectedType == CommunicationType.TcpIp && !isRunning;
			}

			if (btnTcpDisconnect != null)
			{
				btnTcpDisconnect.Enabled = _selectedType == CommunicationType.TcpIp && isRunning;
			}

			InvalidateCommunicationInstanceButtons();
		}

		private void AppendTcpReceiveText(string text)
		{
			if (txtReceive == null || txtReceive.IsDisposed)
			{
				return;
			}

			if (grpTest != null && !grpTest.Visible)
			{
				return;
			}

			if (txtReceive.InvokeRequired)
			{
				txtReceive.BeginInvoke(new MethodInvoker(delegate
				{
					AppendTcpReceiveText(text);
				}));
				return;
			}

			txtReceive.AppendText(text + Environment.NewLine);
		}


		private Button CreateTcpSmallButton(string text, int x, int y, int w, int h)
		{
			Button btn = new Button();
			btn.Text = text;
			btn.Location = new Point(x, y);
			btn.Size = new Size(w, h);
			btn.FlatStyle = FlatStyle.Flat;
			btn.FlatAppearance.BorderColor = Color.FromArgb(0, 150, 220);
			btn.BackColor = Color.FromArgb(3, 14, 27);
			btn.ForeColor = Color.White;
			btn.Font = new Font("Microsoft YaHei UI", 9F, FontStyle.Bold);
			return btn;
		}


		private void RefreshDataTypeComboItems()
		{
			colInputType.Items.Clear();
			colOutputType.Items.Clear();

			string[] typeItems = GetDataTypeDisplayItems();

			foreach (string item in typeItems)
			{
				colInputType.Items.Add(item);
				colOutputType.Items.Add(item);
			}
		}

		private string[] GetDataTypeDisplayItems()
		{
			if (_selectedType == CommunicationType.TcpIp && !IsTcpPayloadModeByte())
			{
				return new string[]
				{
					"String"
				};
			}

			if (_selectedType == CommunicationType.TcpIp && IsTcpPayloadModeByte())
			{
				return new string[]
				{
					"Float",
					"Double",
					"Short Int",
					"Long Int",
					"Bool",
					"String",
					"Bytes"
				};
			}

			return new string[]
			{
				"Float",
				"Double",
				"Short Int",
				"Long Int",
				"Bool",
				"String"
			};
		}

		private string DataTypeToDisplayText(CommVariableDataType dataType)
		{
			if (dataType == CommVariableDataType.ShortInt)
			{
				return "Short Int";
			}

			if (dataType == CommVariableDataType.LongInt)
			{
				return "Long Int";
			}

			if (dataType == CommVariableDataType.Bytes)
			{
				return "Bytes";
			}

			return dataType.ToString();
		}

		private CommVariableDataType DisplayTextToDataType(string text, CommVariableDataType defaultValue)
		{
			if (string.IsNullOrWhiteSpace(text))
			{
				return defaultValue;
			}

			string value = text.Trim();

			if (value.Equals("Float", StringComparison.OrdinalIgnoreCase))
			{
				return CommVariableDataType.Float;
			}

			if (value.Equals("Double", StringComparison.OrdinalIgnoreCase))
			{
				return CommVariableDataType.Double;
			}

			if (value.Equals("Short Int", StringComparison.OrdinalIgnoreCase) ||
				value.Equals("ShortInt", StringComparison.OrdinalIgnoreCase) ||
				value.Equals("Int16", StringComparison.OrdinalIgnoreCase))
			{
				return CommVariableDataType.ShortInt;
			}

			if (value.Equals("Long Int", StringComparison.OrdinalIgnoreCase) ||
				value.Equals("LongInt", StringComparison.OrdinalIgnoreCase) ||
				value.Equals("Int32", StringComparison.OrdinalIgnoreCase))
			{
				return CommVariableDataType.LongInt;
			}

			if (value.Equals("Bool", StringComparison.OrdinalIgnoreCase) ||
				value.Equals("Boolean", StringComparison.OrdinalIgnoreCase))
			{
				return CommVariableDataType.Bool;
			}

			if (value.Equals("String", StringComparison.OrdinalIgnoreCase))
			{
				return CommVariableDataType.String;
			}

			if (value.Equals("Bytes", StringComparison.OrdinalIgnoreCase) ||
				value.Equals("Byte Array", StringComparison.OrdinalIgnoreCase) ||
				value.Equals("Hex", StringComparison.OrdinalIgnoreCase))
			{
				return CommVariableDataType.Bytes;
			}

			return defaultValue;
		}


		private bool IsCurrentTypeTcpIp()
		{
			return _selectedType == CommunicationType.TcpIp;
		}

		private bool IsTcpPayloadModeByte()
		{
			if (_selectedType != CommunicationType.TcpIp)
			{
				return false;
			}

			if (cmbTcpPayloadMode != null && cmbTcpPayloadMode.SelectedIndex >= 0)
			{
				return cmbTcpPayloadMode.SelectedIndex == 1;
			}

			return _config != null &&
				   GetCurrentTcpConfig() != null &&
				   GetCurrentTcpConfig().PayloadMode == TcpIpPayloadMode.Byte;
		}

		private CommVariableDataType GetDefaultVariableDataType()
		{
			if (IsCurrentTypeTcpIp() && !IsTcpPayloadModeByte())
			{
				return CommVariableDataType.String;
			}

			return CommVariableDataType.Bool;
		}

		private CommVariableDataType NormalizeDataTypeForCurrentCommunication(CommVariableDataType dataType)
		{
			if (IsCurrentTypeTcpIp() && !IsTcpPayloadModeByte())
			{
				return CommVariableDataType.String;
			}

			return dataType;
		}


		private void ConfigureInputSecondColumn(bool profinetMode)
		{
			if (dgvInput.Columns.Count < 2)
			{
				return;
			}

			BeginUpdateControl(dgvInput);
			dgvInput.SuspendLayout();

			try
			{
				string oldName = dgvInput.Columns[1].Name;
				int oldWidth = dgvInput.Columns[1].Width;

				dgvInput.Columns.RemoveAt(1);

				if (profinetMode)
				{
					DataGridViewComboBoxColumn engineColumn = new DataGridViewComboBoxColumn();
					engineColumn.Name = oldName;
					engineColumn.HeaderText = "Engine";
					engineColumn.Width = oldWidth <= 0 ? 90 : oldWidth;
					engineColumn.FlatStyle = FlatStyle.Flat;
					engineColumn.DisplayStyle = DataGridViewComboBoxDisplayStyle.DropDownButton;
					engineColumn.Items.Add("engine0");
					engineColumn.Items.Add("engine1");
					engineColumn.Items.Add("engine2");
					engineColumn.Items.Add("engine3");
					engineColumn.Visible = true;
					dgvInput.Columns.Insert(1, engineColumn);
				}
				else
				{
					DataGridViewCheckBoxColumn triggerColumn = new DataGridViewCheckBoxColumn();
					triggerColumn.Name = oldName;
					triggerColumn.HeaderText = _isEnglish ? "Use As Trigger" : "作为触发源";
					triggerColumn.Width = oldWidth <= 0 ? 90 : oldWidth;
					triggerColumn.Visible = false;
					dgvInput.Columns.Insert(1, triggerColumn);
				}

				EnsureInputPositionColumn();
				ApplyVariableColumnDisplayOrder();

				_inputSecondColumnIsProfinet = profinetMode;
			}
			finally
			{
				dgvInput.ResumeLayout();
				EndUpdateControl(dgvInput);
			}
		}

		private void EnsureInputPositionColumn()
		{
			if (dgvInput.Columns.Contains("colInputUseAsPosition"))
			{
				dgvInput.Columns["colInputUseAsPosition"].HeaderText = _isEnglish ? "Use As Position" : "作为位置号";
				dgvInput.Columns["colInputUseAsPosition"].Visible = false;
				return;
			}

			DataGridViewCheckBoxColumn positionColumn = new DataGridViewCheckBoxColumn();
			positionColumn.Name = "colInputUseAsPosition";
			positionColumn.HeaderText = _isEnglish ? "Use As Position" : "作为位置号";
			positionColumn.Width = 90;
			positionColumn.Visible = false;

			int insertIndex = dgvInput.Columns.Count > 2 ? 2 : dgvInput.Columns.Count;
			dgvInput.Columns.Insert(insertIndex, positionColumn);
		}


		private void LoadConfigToUI(CommunicationConfig config)
		{
			_loading = true;

			try
			{
				if (config == null)
				{
					config = new CommunicationConfig();
				}

				_config = config;
				_selectedType = config.SelectedType;
				EnsureUiInstances();
				EnsureSelectedInstanceForCurrentType();

				RefreshCommunicationInstanceTree();
				ApplySelectedTypeStyle();
				LoadTypeParamsToUI();
				LoadCurrentTypeVariablesToGrid();
				ApplyTcpModeParamVisibility();
				UpdateHeartbeatButtonVisibility();
				UpdateTcpStatusUi();
			}
			finally
			{
				_loading = false;
			}
		}

		private void SelectCommunicationType(CommunicationType type)
		{
			SelectCommunicationInstance(type, GetFirstInstanceName(type));
		}

		private void SelectCommunicationInstance(CommunicationType type, string instanceName)
		{
			if (_selectedType == type &&
				string.Equals(_selectedInstanceName, instanceName, StringComparison.OrdinalIgnoreCase))
			{
				return;
			}

			BeginPageRefresh();

			try
			{
				if (!_loading)
				{
					SaveCurrentTypeParamsFromUI();
					SaveCurrentTypeVariablesFromGrid();
				}

				_selectedType = type;
				_selectedInstanceName = instanceName;

				if (_config != null)
				{
					_config.SelectedType = type;
					EnsureSelectedInstanceForCurrentType();
				}

				ApplySelectedTypeStyle();
				RefreshCommunicationInstanceTree();
				LoadTypeParamsToUI();
				LoadCurrentTypeVariablesToGrid();
				ApplyTcpModeParamVisibility();
				UpdateHeartbeatButtonVisibility();
				UpdateTcpStatusUi();
			}
			finally
			{
				EndPageRefresh();
			}
		}


		private void ApplySelectedTypeStyle()
		{
			ApplyButtonStyle(btnTcpIp, _selectedType == CommunicationType.TcpIp);
			ApplyButtonStyle(btnProfinet, _selectedType == CommunicationType.Profinet);
			ApplyButtonStyle(btnS7, _selectedType == CommunicationType.S7);
			SyncCommunicationAddButtonStyle();

			foreach (Button button in _communicationInstanceButtons)
			{
				CommunicationInstanceConfig instance = button == null ? null : button.Tag as CommunicationInstanceConfig;
				bool selected = instance != null &&
					instance.CommunicationType == _selectedType &&
					string.Equals(instance.InstanceName, _selectedInstanceName, StringComparison.OrdinalIgnoreCase);
				ApplyInstanceButtonStyle(button, selected);
			}

			UpdateHeartbeatButtonVisibility();
		}

		private void SyncCommunicationAddButtonStyle()
		{
			if (btnAddTcpInstance != null && btnTcpIp != null)
			{
				btnAddTcpInstance.BackColor = btnTcpIp.BackColor;
				btnAddTcpInstance.Invalidate();
			}

			if (btnAddProfinetInstance != null && btnProfinet != null)
			{
				btnAddProfinetInstance.BackColor = btnProfinet.BackColor;
				btnAddProfinetInstance.Invalidate();
			}

			if (btnAddS7Instance != null && btnS7 != null)
			{
				btnAddS7Instance.BackColor = btnS7.BackColor;
				btnAddS7Instance.Invalidate();
			}
		}

		private void UpdateHeartbeatButtonVisibility()
		{
			if (btnHeartbeatSettings != null)
			{
				btnHeartbeatSettings.Visible = _selectedType != CommunicationType.Profinet;
				btnHeartbeatSettings.Enabled = _selectedType != CommunicationType.Profinet;
			}
		}

		private void ApplyButtonStyle(Button button, bool selected)
		{
			if (button == null)
			{
				return;
			}

			if (selected)
			{
				button.BackColor = Color.FromArgb(0, 85, 150);
				button.ForeColor = Color.White;
			}
			else
			{
				button.BackColor = Color.FromArgb(3, 14, 27);
				button.ForeColor = Color.White;
			}
		}

		private void ApplyInstanceButtonStyle(Button button, bool selected)
		{
			if (button == null)
			{
				return;
			}

			if (selected)
			{
				button.BackColor = Color.FromArgb(0, 120, 200);
				button.FlatAppearance.BorderColor = Color.FromArgb(120, 210, 255);
			}
			else
			{
				button.BackColor = Color.FromArgb(2, 10, 20);
				button.FlatAppearance.BorderColor = Color.FromArgb(0, 120, 180);
			}

			button.ForeColor = Color.White;
		}

		private void LoadTypeParamsToUI()
		{
			if (_config == null)
			{
				return;
			}

			_loading = true;

			try
			{
				chkEnable.Checked = GetCurrentTypeEnabled();

				SetParamControlsVisible(true, true, true, true, true, true, true);

				txtP1.ReadOnly = false;
				txtP2.ReadOnly = false;
				txtP3.ReadOnly = false;
				txtP4.ReadOnly = false;
				txtP5.ReadOnly = false;
				txtP6.ReadOnly = false;

				if (_selectedType == CommunicationType.TcpIp)
				{
					TcpIpConfig tcpIp = GetCurrentTcpConfig();
					grpParams.Text = _isEnglish ? "TCP/IP Parameters" : "TCP/IP 参数";

					lblP1.Text = _isEnglish ? "Local IP" : "本地IP";
					lblP2.Text = _isEnglish ? "Local Port" : "本地端口";
					lblP3.Text = _isEnglish ? "Server IP" : "服务器IP";
					lblP4.Text = _isEnglish ? "Server Port" : "服务器端口";
					lblP5.Text = _isEnglish ? "Mode" : "模式";
					lblP6.Text = string.Empty;

					txtP1.Text = string.IsNullOrWhiteSpace(tcpIp.LocalIP) ? "0.0.0.0" : tcpIp.LocalIP;
					txtP2.Text = tcpIp.LocalPort <= 0 ? "5000" : tcpIp.LocalPort.ToString();
					txtP3.Text = tcpIp.RemoteIP;
					txtP4.Text = tcpIp.RemotePort <= 0 ? "5000" : tcpIp.RemotePort.ToString();
					txtP5.Text = string.Empty;
					txtP6.Text = string.Empty;

					txtP5.Visible = false;
					txtP6.Visible = false;
					lblP6.Visible = false;
					cmbMode.Visible = true;
					cmbMode.SelectedIndex = tcpIp.IsServer ? 0 : 1;
					SyncTcpDedicatedControlsFromConfig();

					ApplyTcpModeParamVisibility();
					UpdateTcpStatusUi();
				}

				else if (_selectedType == CommunicationType.Profinet)
				{
					ProfinetConfig profinet = GetCurrentProfinetConfig();
					grpParams.Text = _isEnglish ? "Profinet Status" : "Profinet 状态";

					lblP1.Text = _isEnglish ? "Device Name" : "设备名称";
					lblP2.Text = _isEnglish ? "Station Name" : "站点名称";
					lblP3.Text = _isEnglish ? "Connection" : "连接状态";
					lblP4.Text = string.Empty;
					lblP5.Text = string.Empty;
					lblP6.Text = string.Empty;

					txtP1.Text = profinet.DeviceName;
					txtP2.Text = profinet.StationName;
					txtP3.Text = profinet.ConnectionStatus;

					txtP3.ReadOnly = true;

					SetParamControlsVisible(true, true, true, false, false, false, false);
				}
				else
				{
					S7Config s7 = GetCurrentS7Config();
					grpParams.Text = _isEnglish ? "S7 Parameters" : "S7 参数";

					lblP1.Text = "PLC IP";
					lblP2.Text = "Rack";
					lblP3.Text = "Slot";
					lblP4.Text = _isEnglish ? "Input DB" : "输入DB";
					lblP5.Text = _isEnglish ? "Output DB" : "输出DB";
					lblP6.Text = _isEnglish ? "Start Byte" : "起始字节";

					txtP1.Text = s7.PlcIP;
					txtP2.Text = s7.Rack.ToString();
					txtP3.Text = s7.Slot.ToString();
					txtP4.Text = s7.InputDB.ToString();
					txtP5.Text = s7.OutputDB.ToString();
					txtP6.Text = s7.InputStartByte.ToString();

					cmbMode.Visible = false;
				}
			}
			finally
			{
				_loading = false;
			}
		}

		private void ApplyTcpModeParamVisibility()
		{
			if (_selectedType != CommunicationType.TcpIp)
			{
				if (lblTcpPayloadMode != null) lblTcpPayloadMode.Visible = false;
				if (cmbTcpPayloadMode != null) cmbTcpPayloadMode.Visible = false;
				if (lblTcpByteOrder != null) lblTcpByteOrder.Visible = false;
				if (cmbTcpByteOrder != null) cmbTcpByteOrder.Visible = false;
				if (btnTcpConnect != null) btnTcpConnect.Visible = false;
				if (btnTcpDisconnect != null) btnTcpDisconnect.Visible = false;
				if (pnlTcpStatusLight != null) pnlTcpStatusLight.Visible = false;
				if (lblTcpStatus != null) lblTcpStatus.Visible = false;
				return;
			}

			if (grpParams != null)
			{
				if (grpParams.Height < 300)
				{
					grpParams.Height = 300;
				}
			}

			LayoutTcpParameterArea();
		}



		private void SetParamControlsVisible(
			bool row1,
			bool row2,
			bool row3,
			bool row4,
			bool row5,
			bool row6,
			bool modeVisible)
		{
			lblP1.Visible = row1;
			txtP1.Visible = row1;

			lblP2.Visible = row2;
			txtP2.Visible = row2;

			lblP3.Visible = row3;
			txtP3.Visible = row3;

			lblP4.Visible = row4;
			txtP4.Visible = row4;

			lblP5.Visible = row5;
			txtP5.Visible = row5;

			lblP6.Visible = row6;
			txtP6.Visible = row6;

			cmbMode.Visible = modeVisible;
		}

		private void ApplyColumnModeByCommunicationType()
		{
			// TCP/IP：偏移字节 改成 偏移字符，Bit 列隐藏
			// Profinet / S7：保留偏移字节，Bit 列隐藏
			bool isTcpIp = _selectedType == CommunicationType.TcpIp;
			bool tcpByteMode = isTcpIp && IsTcpPayloadModeByte();

			if (dgvInput.Columns.Contains("colInputByteOffset"))
			{
				colInputByteOffset.HeaderText = isTcpIp
					? (tcpByteMode ? (_isEnglish ? "Byte Offset" : "偏移字节") : (_isEnglish ? "Char Offset" : "偏移字符"))
					: (_isEnglish ? "Byte Offset" : "偏移字节");
			}

			if (dgvOutput.Columns.Contains("colOutputByteOffset"))
			{
				colOutputByteOffset.HeaderText = isTcpIp
					? (tcpByteMode ? (_isEnglish ? "Byte Offset" : "偏移字节") : (_isEnglish ? "Char Offset" : "偏移字符"))
					: (_isEnglish ? "Byte Offset" : "偏移字节");
			}

			// 按你的要求，TCP/IP、Profinet、S7 都不显示 Bit 列
			if (dgvInput.Columns.Contains("colInputBitOffset"))
			{
				colInputBitOffset.Visible = false;
			}

			if (dgvOutput.Columns.Contains("colOutputBitOffset"))
			{
				colOutputBitOffset.Visible = false;
			}

			if (dgvInput.Columns.Contains("colInputUseAsPosition"))
			{
				dgvInput.Columns["colInputUseAsPosition"].Visible = false;
			}

			if (_selectedType != CommunicationType.Profinet && dgvInput.Columns.Count > 1)
			{
				dgvInput.Columns[1].Visible = false;
			}

			ApplyVariableColumnDisplayOrder();
		}


		private void ForceTcpGridTypeToString()
		{
			if (_selectedType != CommunicationType.TcpIp || IsTcpPayloadModeByte())
			{
				return;
			}

			foreach (DataGridViewRow row in dgvInput.Rows)
			{
				if (!row.IsNewRow && row.Cells.Count > 2)
				{
					row.Cells[3].Value = "String";
					if (dgvInput.Columns.Contains("colInputLength"))
					{
						row.Cells["colInputLength"].ReadOnly = false;
					}
				}
			}

			foreach (DataGridViewRow row in dgvOutput.Rows)
			{
				if (!row.IsNewRow && row.Cells.Count > 1)
				{
					row.Cells[1].Value = "String";
					if (dgvOutput.Columns.Contains("colOutputLength"))
					{
						row.Cells["colOutputLength"].ReadOnly = false;
					}
				}
			}
		}

		private void ApplyTcpByteLengthRulesToGrid()
		{
			if (_selectedType != CommunicationType.TcpIp || !IsTcpPayloadModeByte())
			{
				return;
			}

			foreach (DataGridViewRow row in dgvInput.Rows)
			{
				if (!row.IsNewRow)
				{
					ApplyTcpByteLengthRule(row, true);
				}
			}

			foreach (DataGridViewRow row in dgvOutput.Rows)
			{
				if (!row.IsNewRow)
				{
					ApplyTcpByteLengthRule(row, false);
				}
			}
		}

		private void ApplyTcpByteLengthRule(DataGridViewRow row, bool isInput)
		{
			if (_selectedType != CommunicationType.TcpIp || !IsTcpPayloadModeByte())
			{
				return;
			}

			if (row == null || row.IsNewRow || row.DataGridView == null)
			{
				return;
			}

			string typeColumn = isInput ? "colInputType" : "colOutputType";
			string lengthColumn = isInput ? "colInputLength" : "colOutputLength";

			if (!row.DataGridView.Columns.Contains(typeColumn) ||
				!row.DataGridView.Columns.Contains(lengthColumn))
			{
				return;
			}

			CommVariableDataType dataType = DisplayTextToDataType(
				Convert.ToString(row.Cells[typeColumn].Value),
				GetDefaultVariableDataType());

			int fixedLength = GetFixedTcpByteLength(dataType);
			DataGridViewCell lengthCell = row.Cells[lengthColumn];

			if (fixedLength > 0)
			{
				lengthCell.Value = fixedLength.ToString();
				lengthCell.ReadOnly = true;
			}
			else
			{
				lengthCell.ReadOnly = false;

				int length;
				if (!int.TryParse(Convert.ToString(lengthCell.Value), out length) || length <= 0)
				{
					lengthCell.Value = "1";
				}
			}
		}

		private int NormalizeTcpByteLength(CommVariableDataType dataType, int configuredLength)
		{
			int fixedLength = GetFixedTcpByteLength(dataType);
			if (fixedLength > 0)
			{
				return fixedLength;
			}

			return configuredLength <= 0 ? 1 : configuredLength;
		}

		private int GetFixedTcpByteLength(CommVariableDataType dataType)
		{
			switch (dataType)
			{
				case CommVariableDataType.Float:
					return 4;
				case CommVariableDataType.Double:
					return 8;
				case CommVariableDataType.ShortInt:
					return 2;
				case CommVariableDataType.LongInt:
					return 4;
				case CommVariableDataType.Bool:
					return 1;
				default:
					return 0;
			}
		}

		private void ValidateCommunicationRangeGrids()
		{
			ValidateCommunicationRangeGrid(dgvInput);
			ValidateCommunicationRangeGrid(dgvOutput);
		}

		private void ValidateCommunicationRangeGrid(DataGridView grid)
		{
			if (grid == null || grid.IsDisposed)
			{
				return;
			}

			string offsetColumn;
			string lengthColumn;

			if (grid == dgvInput)
			{
				offsetColumn = "colInputByteOffset";
				lengthColumn = "colInputLength";
			}
			else if (grid == dgvOutput)
			{
				offsetColumn = "colOutputByteOffset";
				lengthColumn = "colOutputLength";
			}
			else
			{
				return;
			}

			if (!grid.Columns.Contains(offsetColumn) || !grid.Columns.Contains(lengthColumn))
			{
				return;
			}

			if (_selectedType != CommunicationType.TcpIp || !IsTcpPayloadModeByte())
			{
				_validatingRangeCells = true;

				try
				{
					foreach (DataGridViewRow row in grid.Rows)
					{
						if (!row.IsNewRow)
						{
							ClearRangeIssueStyle(row, offsetColumn, lengthColumn);
						}
					}
				}
				finally
				{
					_validatingRangeCells = false;
				}

				return;
			}

			_validatingRangeCells = true;

			try
			{
				List<CommunicationRangeRowInfo> ranges = new List<CommunicationRangeRowInfo>();

				foreach (DataGridViewRow row in grid.Rows)
				{
					if (row.IsNewRow)
					{
						continue;
					}

					ClearRangeIssueStyle(row, offsetColumn, lengthColumn);

					int offset;
					int length;
					string errorText;
					if (!TryReadRange(row, offsetColumn, lengthColumn, out offset, out length, out errorText))
					{
						MarkRangeIssue(row, offsetColumn, lengthColumn, errorText);
						continue;
					}

					ranges.Add(new CommunicationRangeRowInfo(row, offset, length));
				}

				for (int i = 0; i < ranges.Count; i++)
				{
					for (int j = i + 1; j < ranges.Count; j++)
					{
						if (ranges[i].Offset < ranges[j].End && ranges[i].End > ranges[j].Offset)
						{
							string message = _isEnglish
								? "Byte range overlaps another row."
								: "字节范围与其他行重叠。";
							MarkRangeIssue(ranges[i].Row, offsetColumn, lengthColumn, message);
							MarkRangeIssue(ranges[j].Row, offsetColumn, lengthColumn, message);
						}
					}
				}

			}
			finally
			{
				_validatingRangeCells = false;
			}
		}

		private bool TryReadRange(
			DataGridViewRow row,
			string offsetColumn,
			string lengthColumn,
			out int offset,
			out int length,
			out string errorText)
		{
			offset = 0;
			length = 0;
			errorText = string.Empty;

			string offsetText = GetCellString(row, offsetColumn);
			string lengthText = GetCellString(row, lengthColumn);

			if (!int.TryParse(offsetText, out offset) || offset < 0)
			{
				errorText = _isEnglish ? "Offset must be a non-negative integer." : "偏移字节必须是大于等于 0 的整数。";
				return false;
			}

			if (!int.TryParse(lengthText, out length) || length <= 0)
			{
				errorText = _isEnglish ? "Length must be a positive integer." : "长度必须是大于 0 的整数。";
				return false;
			}

			return true;
		}

		private void ClearRangeIssueStyle(DataGridViewRow row, string offsetColumn, string lengthColumn)
		{
			SetRangeIssueStyle(row, offsetColumn, false, string.Empty);
			SetRangeIssueStyle(row, lengthColumn, false, string.Empty);
		}

		private void MarkRangeIssue(DataGridViewRow row, string offsetColumn, string lengthColumn, string message)
		{
			SetRangeIssueStyle(row, offsetColumn, true, message);
			SetRangeIssueStyle(row, lengthColumn, true, message);
		}

		private void SetRangeIssueStyle(DataGridViewRow row, string columnName, bool hasIssue, string message)
		{
			if (row == null || row.DataGridView == null || !row.DataGridView.Columns.Contains(columnName))
			{
				return;
			}

			DataGridViewCell cell = row.Cells[columnName];

			Color backColor = hasIssue ? Color.FromArgb(255, 210, 210) : Color.Empty;
			Color foreColor = hasIssue ? Color.FromArgb(90, 0, 0) : Color.Empty;
			Color selectionBackColor = hasIssue ? Color.FromArgb(255, 175, 175) : Color.Empty;
			Color selectionForeColor = hasIssue ? Color.Black : Color.Empty;

			if (cell.Style.BackColor != backColor)
			{
				cell.Style.BackColor = backColor;
			}

			if (cell.Style.ForeColor != foreColor)
			{
				cell.Style.ForeColor = foreColor;
			}

			if (cell.Style.SelectionBackColor != selectionBackColor)
			{
				cell.Style.SelectionBackColor = selectionBackColor;
			}

			if (cell.Style.SelectionForeColor != selectionForeColor)
			{
				cell.Style.SelectionForeColor = selectionForeColor;
			}

			if (!string.Equals(cell.ToolTipText, message, StringComparison.Ordinal))
			{
				cell.ToolTipText = message;
			}
		}

		private sealed class CommunicationRangeRowInfo
		{
			public DataGridViewRow Row { get; private set; }
			public int Offset { get; private set; }
			public int Length { get; private set; }

			public int End
			{
				get { return Offset + Length; }
			}

			public CommunicationRangeRowInfo(DataGridViewRow row, int offset, int length)
			{
				Row = row;
				Offset = offset;
				Length = length;
			}
		}


		private void LoadCurrentTypeVariablesToGrid()
		{
			if (_config == null)
			{
				return;
			}

			BeginUpdateControl(dgvInput);
			BeginUpdateControl(dgvOutput);

			dgvInput.SuspendLayout();
			dgvOutput.SuspendLayout();

			try
			{
				dgvInput.Rows.Clear();
				dgvOutput.Rows.Clear();

				bool isProfinet = _selectedType == CommunicationType.Profinet;

				ConfigureInputSecondColumn(isProfinet);
				ApplyColumnModeByCommunicationType();
				RefreshDataTypeComboItems();
				if (_selectedType == CommunicationType.TcpIp)
				{
					TcpIpConfig tcpIp = GetCurrentTcpConfig();
					LoadInputRows(tcpIp.InputVariables, false);
					LoadOutputRows(tcpIp.OutputVariables);
					SetVariableGridEditable(true, false);
				}
				else if (_selectedType == CommunicationType.Profinet)
				{
					ProfinetConfig profinet = GetCurrentProfinetConfig();
					LoadInputRows(profinet.InputVariables, true);
					LoadOutputRows(profinet.OutputVariables);
					SetVariableGridEditable(true, true);
				}
				else
				{
					S7Config s7 = GetCurrentS7Config();
					LoadInputRows(s7.InputVariables, false);
					LoadOutputRows(s7.OutputVariables);
					SetVariableGridEditable(true, false);
				}

				ForceTcpGridTypeToString();
				ApplyTcpByteLengthRulesToGrid();
				ValidateCommunicationRangeGrids();

				dgvInput.ClearSelection();
				dgvOutput.ClearSelection();
			}
			finally
			{
				dgvOutput.ResumeLayout();
				dgvInput.ResumeLayout();

				EndUpdateControl(dgvInput);
				EndUpdateControl(dgvOutput);
			}
		}


		private void LoadInputRows(List<CommInputVariable> list, bool profinetMode)
		{
			if (list == null)
			{
				return;
			}

			foreach (CommInputVariable item in list)
			{
				if (profinetMode)
				{
					string engine = string.IsNullOrEmpty(item.EngineName) ? "engine0" : item.EngineName;

					int rowIndex = dgvInput.Rows.Add(
						item.Name,
						engine,
						item.UseAsPosition,
						DataTypeToDisplayText(NormalizeDataTypeForCurrentCommunication(item.DataType)),
						item.ByteOffset.ToString(),
						item.BitOffset.ToString(),
						item.Length.ToString(),
						item.Remark);
					GlobalVariableBindingUi.SetCellValue(dgvInput.Rows[rowIndex], "colInputGlobalVariable", item.GlobalVariableName);
					UpdateInputCurrentValueCell(dgvInput.Rows[rowIndex]);
				}
				else
				{
					int rowIndex = dgvInput.Rows.Add(
						item.Name,
						item.UseAsTrigger,
						item.UseAsPosition,
						DataTypeToDisplayText(NormalizeDataTypeForCurrentCommunication(item.DataType)),
						item.ByteOffset.ToString(),
						item.BitOffset.ToString(),
						item.Length.ToString(),
						item.Remark);
					GlobalVariableBindingUi.SetCellValue(dgvInput.Rows[rowIndex], "colInputGlobalVariable", item.GlobalVariableName);
					UpdateInputCurrentValueCell(dgvInput.Rows[rowIndex]);
				}
			}
		}

		private void LoadOutputRows(List<CommOutputVariable> list)
		{
			if (list == null)
			{
				return;
			}

			foreach (CommOutputVariable item in list)
			{
				int rowIndex = dgvOutput.Rows.Add(
					item.Name,
					DataTypeToDisplayText(NormalizeDataTypeForCurrentCommunication(item.DataType)),
					item.ByteOffset.ToString(),
					item.BitOffset.ToString(),
					item.Length.ToString(),
					item.Remark);
				GlobalVariableBindingUi.SetCellValue(dgvOutput.Rows[rowIndex], "colOutputGlobalVariable", item.GlobalVariableName);
				dgvOutput.Rows[rowIndex].Tag = item.Name;
				UpdateOutputCurrentValueCell(dgvOutput.Rows[rowIndex]);
			}
		}

		private void UpdateInputCurrentValueCell(DataGridViewRow row)
		{
			if (row == null || row.DataGridView == null)
			{
				return;
			}

			if (!row.DataGridView.Columns.Contains("colInputCurrentValue"))
			{
				return;
			}

			string globalVariableName = GlobalVariableBindingUi.GetCellValue(row, "colInputGlobalVariable");
			if (!string.IsNullOrWhiteSpace(globalVariableName))
			{
				row.Cells["colInputCurrentValue"].Value = GlobalVariableStore.GetValueText(globalVariableName);
				return;
			}

			string inputName = GetInputNameFromRow(row);
			string latestValue;
			row.Cells["colInputCurrentValue"].Value =
				TryGetLatestInputValue(inputName, out latestValue)
					? latestValue
					: string.Empty;
		}

		private string GetInputNameFromRow(DataGridViewRow row)
		{
			if (row == null || row.DataGridView == null || !row.DataGridView.Columns.Contains("colInputName"))
			{
				return string.Empty;
			}

			return Convert.ToString(row.Cells["colInputName"].Value);
		}

		private bool TryGetLatestInputValue(string inputName, out string value)
		{
			value = string.Empty;
			if (string.IsNullOrWhiteSpace(inputName))
			{
				return false;
			}

			string key = GetSelectedCommunicationCacheKey();
			lock (_latestInputValuesSyncRoot)
			{
				Dictionary<string, string> values;
				if (!_latestInputValuesByCommunication.TryGetValue(key, out values) || values == null)
				{
					return false;
				}

				return values.TryGetValue(inputName, out value);
			}
		}

		private void UpdateOutputCurrentValueCell(DataGridViewRow row)
		{
			if (row == null || row.DataGridView == null)
			{
				return;
			}

			if (!row.DataGridView.Columns.Contains("colOutputCurrentValue") ||
				!row.DataGridView.Columns.Contains("colOutputName"))
			{
				return;
			}

			string outputName = Convert.ToString(row.Cells["colOutputName"].Value);
			object value;
			row.Cells["colOutputCurrentValue"].Value =
				RuntimeCommunicationOutputService.TryGetLatestOutputValue(GetSelectedProtocolName(), outputName, out value)
					? Convert.ToString(value)
					: string.Empty;
		}

		private void GlobalVariableStore_VariablesChanged(object sender, EventArgs e)
		{
			if (IsDisposed || !IsHandleCreated)
			{
				return;
			}

			if (InvokeRequired)
			{
				BeginInvoke(new Action(delegate { ReloadCommunicationConfigFromStore(); }));
				return;
			}

			ReloadCommunicationConfigFromStore();
		}

		private void RuntimeCommunicationOutputService_OutputValuesChanged(object sender, RuntimeCommunicationOutputValuesChangedEventArgs e)
		{
			if (IsDisposed || !IsHandleCreated || e == null)
			{
				return;
			}

			if (!string.Equals(e.ProtocolName, GetSelectedProtocolName(), StringComparison.OrdinalIgnoreCase))
			{
				return;
			}

			if (InvokeRequired)
			{
				BeginInvoke(new Action(delegate { RefreshOutputCurrentValues(); }));
				return;
			}

			RefreshOutputCurrentValues();
		}

		private void CommunicationConfigChangedHub_ConfigChanged(object sender, EventArgs e)
		{
			if (IsDisposed || !IsHandleCreated || _loading)
			{
				return;
			}

			if (InvokeRequired)
			{
				BeginInvoke(new Action(delegate { ReloadCommunicationConfigFromStore(); }));
				return;
			}

			ReloadCommunicationConfigFromStore();
		}

		private void ReloadCommunicationConfigFromStore()
		{
			if (IsDisposed || !IsHandleCreated || _loading)
			{
				return;
			}

			CommunicationType selectedType = _selectedType;
			string selectedInstanceName = _selectedInstanceName;
			_config = CommunicationConfigStore.LoadOrCreateDefault();
			_config.SelectedType = selectedType;
			_selectedInstanceName = selectedInstanceName;
			LoadConfigToUI(_config);
		}

		private void RefreshInputCurrentValues()
		{
			foreach (DataGridViewRow row in dgvInput.Rows)
			{
				if (!row.IsNewRow)
				{
					UpdateInputCurrentValueCell(row);
				}
			}
		}

		private void RefreshOutputCurrentValues()
		{
			foreach (DataGridViewRow row in dgvOutput.Rows)
			{
				if (!row.IsNewRow)
				{
					UpdateOutputCurrentValueCell(row);
				}
			}
		}

		private string GetSelectedProtocolName()
		{
			if (_selectedType == CommunicationType.TcpIp)
			{
				return "TCP/IP";
			}

			if (_selectedType == CommunicationType.Profinet)
			{
				return "Profinet";
			}

			return "S7";
		}

		private void SetVariableGridEditable(bool editable, bool profinetMode)
		{
			dgvInput.ReadOnly = false;
			dgvOutput.ReadOnly = false;

			for (int i = 0; i < dgvInput.Columns.Count; i++)
			{
				dgvInput.Columns[i].ReadOnly = false;
			}

			for (int i = 0; i < dgvOutput.Columns.Count; i++)
			{
				dgvOutput.Columns[i].ReadOnly = false;
			}

			SetCurrentValueColumnsReadOnly();

			btnAddInput.Enabled = editable;
			btnDeleteInput.Enabled = editable;
			if (btnMoveUpInput != null) btnMoveUpInput.Enabled = editable;
			if (btnMoveDownInput != null) btnMoveDownInput.Enabled = editable;
			btnAddOutput.Enabled = editable;
			btnDeleteOutput.Enabled = editable;
			if (btnMoveUpOutput != null) btnMoveUpOutput.Enabled = editable;
			if (btnMoveDownOutput != null) btnMoveDownOutput.Enabled = editable;

			btnAddInput.Text = _isEnglish ? "+ Add Input" : "+ 新增输入";
			btnDeleteInput.Text = _isEnglish ? "Delete" : "删除选中";
			if (btnMoveUpInput != null) btnMoveUpInput.Text = _isEnglish ? "Move Up" : "上移选中";
			if (btnMoveDownInput != null) btnMoveDownInput.Text = _isEnglish ? "Move Down" : "下移选中";
			btnAddOutput.Text = _isEnglish ? "+ Add Output" : "+ 新增输出";
			btnDeleteOutput.Text = _isEnglish ? "Delete" : "删除选中";
			if (btnMoveUpOutput != null) btnMoveUpOutput.Text = _isEnglish ? "Move Up" : "上移选中";
			if (btnMoveDownOutput != null) btnMoveDownOutput.Text = _isEnglish ? "Move Down" : "下移选中";
		}

		private void SetCurrentValueColumnsReadOnly()
		{
			if (dgvInput.Columns.Contains("colInputCurrentValue"))
			{
				dgvInput.Columns["colInputCurrentValue"].ReadOnly = true;
			}

			if (dgvOutput.Columns.Contains("colOutputCurrentValue"))
			{
				dgvOutput.Columns["colOutputCurrentValue"].ReadOnly = true;
			}
		}

		private void SaveCurrentTypeParamsFromUI()
		{
			if (_config == null)
			{
				_config = new CommunicationConfig();
			}

			_config.SelectedType = _selectedType;
			EnsureUiInstances();
			CommunicationInstanceConfig instance = GetSelectedInstance();

			if (_selectedType == CommunicationType.TcpIp)
			{
				bool isServer = cmbMode.SelectedIndex <= 0;
				TcpIpConfig tcpIp = GetCurrentTcpConfig();

				tcpIp.Enabled = chkEnable.Checked;
				tcpIp.IsServer = isServer;
				tcpIp.PayloadMode =
					cmbTcpPayloadMode != null && cmbTcpPayloadMode.SelectedIndex == 1
						? TcpIpPayloadMode.Byte
						: TcpIpPayloadMode.String;
				tcpIp.ByteOrder =
					cmbTcpByteOrder != null && cmbTcpByteOrder.SelectedIndex == 1
						? CommByteOrder.LittleEndian
						: CommByteOrder.BigEndian;

				if (isServer)
				{
					tcpIp.LocalIP = "0.0.0.0";
					tcpIp.LocalPort = ToInt(txtTcpParam1 == null ? txtP2.Text : txtTcpParam1.Text, 5000);

					tcpIp.RemoteIP = string.Empty;
					tcpIp.RemotePort = 0;
				}
				else
				{
					tcpIp.LocalIP = "0.0.0.0";
					tcpIp.LocalPort = 0;

					tcpIp.RemoteIP = txtTcpParam1 == null ? txtP3.Text.Trim() : txtTcpParam1.Text.Trim();
					tcpIp.RemotePort = ToInt(txtTcpParam2 == null ? txtP4.Text : txtTcpParam2.Text, 5000);
				}

				if (instance != null)
				{
					instance.Enabled = tcpIp.Enabled;
					instance.InstanceKind = isServer ? CommunicationInstanceKind.TcpIpServer : CommunicationInstanceKind.TcpIpClient;
					instance.TcpIp = tcpIp;
					instance.Channels = tcpIp.Channels;
					instance.Heartbeat = tcpIp.Heartbeat;
				}
			}

			else if (_selectedType == CommunicationType.Profinet)
			{
				ProfinetConfig profinet = GetCurrentProfinetConfig();
				profinet.Enabled = chkEnable.Checked;
				profinet.DeviceName = txtP1.Text.Trim();
				profinet.StationName = txtP2.Text.Trim();
				profinet.ConnectionStatus = txtP3.Text.Trim();
				profinet.UseGsdFixedMapping = true;

				if (instance != null)
				{
					instance.Enabled = profinet.Enabled;
					instance.InstanceKind = CommunicationInstanceKind.Profinet;
					instance.Profinet = profinet;
					instance.Channels = profinet.Channels;
					instance.Heartbeat = profinet.Heartbeat;
				}
			}
			else
			{
				S7Config s7 = GetCurrentS7Config();
				s7.Enabled = chkEnable.Checked;
				s7.PlcIP = txtP1.Text.Trim();
				s7.Rack = ToInt(txtP2.Text, 0);
				s7.Slot = ToInt(txtP3.Text, 1);
				s7.InputDB = ToInt(txtP4.Text, 1);
				s7.OutputDB = ToInt(txtP5.Text, 1);
				s7.InputStartByte = ToInt(txtP6.Text, 0);
				s7.OutputStartByte = ToInt(txtP6.Text, 0);

				if (instance != null)
				{
					instance.Enabled = s7.Enabled;
					instance.InstanceKind = CommunicationInstanceKind.S7;
					instance.S7 = s7;
					instance.Channels = s7.Channels;
					instance.Heartbeat = s7.Heartbeat;
				}
			}
		}

		private void SaveCurrentTypeVariablesFromGrid()
		{
			if (_config == null)
			{
				_config = new CommunicationConfig();
			}

			if (_selectedType == CommunicationType.TcpIp)
			{
				TcpIpConfig tcpIp = GetCurrentTcpConfig();
				OutputVariableNameChanges outputChanges = BuildOutputVariableNameChanges(tcpIp.OutputVariables);
				SaveTcpOrS7VariablesFromGrid(tcpIp.InputVariables, tcpIp.OutputVariables);
				ApplyOutputVariableNameChanges(outputChanges);
			}
			else if (_selectedType == CommunicationType.Profinet)
			{
				OutputVariableNameChanges outputChanges = BuildOutputVariableNameChanges(GetCurrentProfinetConfig().OutputVariables);
				SaveProfinetVariablesFromGrid();
				ApplyOutputVariableNameChanges(outputChanges);
			}
			else
			{
				S7Config s7 = GetCurrentS7Config();
				OutputVariableNameChanges outputChanges = BuildOutputVariableNameChanges(s7.OutputVariables);
				SaveTcpOrS7VariablesFromGrid(s7.InputVariables, s7.OutputVariables);
				ApplyOutputVariableNameChanges(outputChanges);
			}
		}

		private OutputVariableNameChanges BuildOutputVariableNameChanges(List<CommOutputVariable> existingOutputs)
		{
			OutputVariableNameChanges changes = new OutputVariableNameChanges();
			if (existingOutputs == null || existingOutputs.Count <= 0)
			{
				return changes;
			}

			HashSet<string> existingNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
			foreach (CommOutputVariable output in existingOutputs)
			{
				if (output == null || string.IsNullOrWhiteSpace(output.Name))
				{
					continue;
				}

				existingNames.Add(output.Name.Trim());
			}

			HashSet<string> observedOriginalNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
			foreach (DataGridViewRow row in dgvOutput.Rows)
			{
				if (row == null || row.IsNewRow)
				{
					continue;
				}

				string originalName = row.Tag as string;
				string currentName = GetCellString(row, 0);
				if (string.IsNullOrWhiteSpace(originalName))
				{
					continue;
				}

				observedOriginalNames.Add(originalName.Trim());
				if (string.IsNullOrWhiteSpace(currentName))
				{
					changes.DeletedNames.Add(originalName.Trim());
					continue;
				}

				currentName = currentName.Trim();
				if (!string.Equals(originalName.Trim(), currentName, StringComparison.OrdinalIgnoreCase))
				{
					changes.RenamedNames[originalName.Trim()] = currentName;
				}
			}

			foreach (string existingName in existingNames)
			{
				if (!observedOriginalNames.Contains(existingName))
				{
					changes.DeletedNames.Add(existingName);
				}
			}

			return changes;
		}

		private void ApplyOutputVariableNameChanges(OutputVariableNameChanges changes)
		{
			if (changes == null || !changes.HasChanges)
			{
				return;
			}

			UpdateChannelOutputReferences(GetCurrentTypeChannels(), changes);
			UpdateHeartbeatOutputReference(GetCurrentTypeHeartbeat(), changes);
			UpdateFlowSignalOutputReferences(changes);
		}

		private bool UpdateChannelOutputReferences(
			List<CommunicationChannelConfig> channels,
			OutputVariableNameChanges changes)
		{
			bool changed = false;
			if (channels == null || changes == null)
			{
				return false;
			}

			foreach (CommunicationChannelConfig channel in channels)
			{
				if (channel == null)
				{
					continue;
				}

				string readyOutputName = channel.ChannelReadyOutputName;
				if (changes.TryUpdateReference(ref readyOutputName))
				{
					channel.ChannelReadyOutputName = readyOutputName;
					changed = true;
				}

				string programOutputName = channel.ProgramNoOutputName;
				if (changes.TryUpdateReference(ref programOutputName))
				{
					channel.ProgramNoOutputName = programOutputName;
					changed = true;
				}
			}

			return changed;
		}

		private bool UpdateHeartbeatOutputReference(
			CommunicationHeartbeatConfig heartbeat,
			OutputVariableNameChanges changes)
		{
			if (heartbeat == null || changes == null)
			{
				return false;
			}

			string outputName = heartbeat.OutputName;
			if (!changes.TryUpdateReference(ref outputName))
			{
				return false;
			}

			heartbeat.OutputName = outputName;
			return true;
		}

		private void UpdateFlowSignalOutputReferences(OutputVariableNameChanges changes)
		{
			if (changes == null || !changes.HasChanges)
			{
				return;
			}

			ProjectFlowConfig flowConfig = FlowConfigStore.LoadOrCreateDefault();
			if (flowConfig == null || flowConfig.Jobs == null)
			{
				return;
			}

			bool changed = false;
			string selectedProtocolName = CommunicationRuntimeNaming.GetProtocolName(_selectedType);
			string selectedInstanceName = CommunicationRuntimeNaming.NormalizeInstanceName(
				selectedProtocolName,
				_selectedInstanceName,
				_config);

			foreach (JobConfig job in flowConfig.Jobs)
			{
				if (job == null || job.Tasks == null)
				{
					continue;
				}

				foreach (TaskConfig task in job.Tasks)
				{
					if (task == null || task.StepFlow == null)
					{
						continue;
					}

					foreach (StepFlowItem item in task.StepFlow)
					{
						if (item == null || item.SignalOutputs == null ||
							!IsSignalOutputItemForSelectedCommunication(item, task, selectedProtocolName, selectedInstanceName))
						{
							continue;
						}

						foreach (SignalOutputBinding binding in item.SignalOutputs)
						{
							if (binding == null)
							{
								continue;
							}

							string outputName = binding.OutputName;
							bool deleted = changes.IsDeleted(outputName);
							if (changes.TryUpdateReference(ref outputName))
							{
								binding.OutputName = outputName;
								if (deleted)
								{
									binding.AssignedValue = string.Empty;
									binding.ForceValue = false;
									binding.Enabled = false;
								}

								changed = true;
							}
						}
					}
				}
			}

			if (changed)
			{
				FlowConfigStore.Save(flowConfig);
			}
		}

		private bool IsSignalOutputItemForSelectedCommunication(
			StepFlowItem item,
			TaskConfig task,
			string selectedProtocolName,
			string selectedInstanceName)
		{
			string protocolName = string.IsNullOrWhiteSpace(item.SignalProtocol)
				? item.CommunicationOutputProtocol
				: item.SignalProtocol;
			string instanceName = string.IsNullOrWhiteSpace(item.SignalInstanceName)
				? item.CommunicationOutputInstanceName
				: item.SignalInstanceName;

			if (string.IsNullOrWhiteSpace(protocolName) && task != null)
			{
				protocolName = task.CommunicationProtocol;
			}

			if (string.IsNullOrWhiteSpace(instanceName) && task != null)
			{
				instanceName = task.CommunicationInstanceName;
			}

			protocolName = CommunicationRuntimeNaming.NormalizeProtocolName(protocolName);
			instanceName = CommunicationRuntimeNaming.NormalizeInstanceName(protocolName, instanceName, _config);

			return string.Equals(protocolName, selectedProtocolName, StringComparison.OrdinalIgnoreCase) &&
				string.Equals(instanceName, selectedInstanceName, StringComparison.OrdinalIgnoreCase);
		}

		private void SaveTcpOrS7VariablesFromGrid(
			List<CommInputVariable> inputList,
			List<CommOutputVariable> outputList)
		{
			inputList.Clear();
			outputList.Clear();

			foreach (DataGridViewRow row in dgvInput.Rows)
			{
				if (row.IsNewRow)
				{
					continue;
				}

				string name = GetCellString(row, 0);

				if (string.IsNullOrEmpty(name))
				{
					continue;
				}

				CommInputVariable item = new CommInputVariable();
				CommVariableDataType dataType = NormalizeDataTypeForCurrentCommunication(DisplayTextToDataType(GetCellString(row, 3), GetDefaultVariableDataType()));
				item.Name = name;
				item.UseAsTrigger = GetCellBool(row, 1);
				item.UseAsPosition = GetCellBool(row, 2);
				item.EngineName = string.Empty;
				item.DataType = dataType;
				item.ByteOffset = GetCellInt(row, 4, 0);
				item.BitOffset = GetCellInt(row, 5, 0);
				item.Length = _selectedType == CommunicationType.TcpIp && IsTcpPayloadModeByte()
					? NormalizeTcpByteLength(dataType, GetCellInt(row, 6, 1))
					: GetCellInt(row, 6, 1);
				item.Remark = GetCellString(row, 7);
				item.GlobalVariableName = GlobalVariableBindingUi.GetCellValue(row, "colInputGlobalVariable");

				inputList.Add(item);
			}

			foreach (DataGridViewRow row in dgvOutput.Rows)
			{
				if (row.IsNewRow)
				{
					continue;
				}

				string name = GetCellString(row, 0);

				if (string.IsNullOrEmpty(name))
				{
					continue;
				}

				CommOutputVariable item = new CommOutputVariable();
				CommVariableDataType dataType = NormalizeDataTypeForCurrentCommunication(DisplayTextToDataType(GetCellString(row, 1), GetDefaultVariableDataType()));
				item.Name = name;
				item.DataType = dataType;
				item.ByteOffset = GetCellInt(row, 2, 0);
				item.BitOffset = GetCellInt(row, 3, 0);
				item.Length = _selectedType == CommunicationType.TcpIp && IsTcpPayloadModeByte()
					? NormalizeTcpByteLength(dataType, GetCellInt(row, 4, 1))
					: GetCellInt(row, 4, 1);
				item.Remark = GetCellString(row, 5);
				item.GlobalVariableName = GlobalVariableBindingUi.GetCellValue(row, "colOutputGlobalVariable");

				outputList.Add(item);
				row.Tag = item.Name;
			}
		}

		private void SaveProfinetVariablesFromGrid()
		{
			ProfinetConfig profinet = GetCurrentProfinetConfig();
			profinet.InputVariables.Clear();
			profinet.OutputVariables.Clear();

			foreach (DataGridViewRow row in dgvInput.Rows)
			{
				if (row.IsNewRow)
				{
					continue;
				}

				string name = GetCellString(row, 0);

				if (string.IsNullOrEmpty(name))
				{
					continue;
				}

				string engine = GetCellString(row, 1);

				if (string.IsNullOrEmpty(engine))
				{
					engine = "engine0";
				}

				CommInputVariable item = new CommInputVariable();
				item.Name = name;
				item.UseAsTrigger = false;
				item.UseAsPosition = GetCellBool(row, 2);
				item.EngineName = engine;
				item.DataType = NormalizeDataTypeForCurrentCommunication(DisplayTextToDataType(GetCellString(row, 3), GetDefaultVariableDataType()));
				item.ByteOffset = GetCellInt(row, 4, 0);
				item.BitOffset = GetCellInt(row, 5, 0);
				item.Length = GetCellInt(row, 6, 1);
				item.Remark = GetCellString(row, 7);
				item.GlobalVariableName = GlobalVariableBindingUi.GetCellValue(row, "colInputGlobalVariable");

				profinet.InputVariables.Add(item);
			}

			foreach (DataGridViewRow row in dgvOutput.Rows)
			{
				if (row.IsNewRow)
				{
					continue;
				}

				string name = GetCellString(row, 0);

				if (string.IsNullOrEmpty(name))
				{
					continue;
				}

				CommOutputVariable item = new CommOutputVariable();
				item.Name = name;
				item.DataType = NormalizeDataTypeForCurrentCommunication(DisplayTextToDataType(GetCellString(row, 1), GetDefaultVariableDataType()));
				item.ByteOffset = GetCellInt(row, 2, 0);
				item.BitOffset = GetCellInt(row, 3, 0);
				item.Length = GetCellInt(row, 4, 1);
				item.Remark = GetCellString(row, 5);
				item.GlobalVariableName = GlobalVariableBindingUi.GetCellValue(row, "colOutputGlobalVariable");

				profinet.OutputVariables.Add(item);
				row.Tag = item.Name;
			}
		}

		private void btnTcpIp_Click(object sender, EventArgs e)
		{
			SelectCommunicationType(CommunicationType.TcpIp);
		}

		private void btnProfinet_Click(object sender, EventArgs e)
		{
			SelectCommunicationType(CommunicationType.Profinet);
		}

		private void btnS7_Click(object sender, EventArgs e)
		{
			SelectCommunicationType(CommunicationType.S7);
		}

		private void btnAddInput_Click(object sender, EventArgs e)
		{
			int rowIndex;
			if (_selectedType == CommunicationType.Profinet)
			{
				rowIndex = dgvInput.Rows.Add(
					"Input_" + (dgvInput.Rows.Count + 1).ToString("00"),
					"engine0",
					false,
					DataTypeToDisplayText(CommVariableDataType.Bool),
					"0",
					"0",
					"1",
					string.Empty);
			}
			else
			{
				rowIndex = dgvInput.Rows.Add(
					"Input_" + (dgvInput.Rows.Count + 1).ToString("00"),
					false,
					false,
					DataTypeToDisplayText(GetDefaultVariableDataType()),
					"0",
					"0",
					"1",
					string.Empty);
			}
			GlobalVariableBindingUi.SetCellValue(dgvInput.Rows[rowIndex], "colInputGlobalVariable", string.Empty);
			UpdateInputCurrentValueCell(dgvInput.Rows[rowIndex]);
			ApplyTcpByteLengthRule(dgvInput.Rows[rowIndex], true);
			ValidateCommunicationRangeGrid(dgvInput);
		}

		private void btnDeleteInput_Click(object sender, EventArgs e)
		{
			DeleteSelectedRow(dgvInput);
		}

		private void btnMoveUpInput_Click(object sender, EventArgs e)
		{
			MoveSelectedRow(dgvInput, -1);
		}

		private void btnMoveDownInput_Click(object sender, EventArgs e)
		{
			MoveSelectedRow(dgvInput, 1);
		}

		private void btnAddOutput_Click(object sender, EventArgs e)
		{
			int rowIndex = dgvOutput.Rows.Add(
				"Output_" + (dgvOutput.Rows.Count + 1).ToString("00"),
				DataTypeToDisplayText(GetDefaultVariableDataType()),
				"0",
				"0",
				"1",
				string.Empty);
			GlobalVariableBindingUi.SetCellValue(dgvOutput.Rows[rowIndex], "colOutputGlobalVariable", string.Empty);
			dgvOutput.Rows[rowIndex].Tag = string.Empty;
			UpdateOutputCurrentValueCell(dgvOutput.Rows[rowIndex]);
			ApplyTcpByteLengthRule(dgvOutput.Rows[rowIndex], false);
			ValidateCommunicationRangeGrid(dgvOutput);
		}

		private void btnDeleteOutput_Click(object sender, EventArgs e)
		{
			DeleteSelectedRow(dgvOutput);
		}

		private void btnMoveUpOutput_Click(object sender, EventArgs e)
		{
			MoveSelectedRow(dgvOutput, -1);
		}

		private void btnMoveDownOutput_Click(object sender, EventArgs e)
		{
			MoveSelectedRow(dgvOutput, 1);
		}

		private void DeleteSelectedRow(DataGridView dgv)
		{
			if (dgv.SelectedRows.Count <= 0)
			{
				return;
			}

			foreach (DataGridViewRow row in dgv.SelectedRows)
			{
				if (!row.IsNewRow)
				{
					dgv.Rows.Remove(row);
				}
			}

			ValidateCommunicationRangeGrid(dgv);
		}

		private void MoveSelectedRow(DataGridView dgv, int direction)
		{
			if (dgv == null || dgv.SelectedRows.Count <= 0 || direction == 0)
			{
				return;
			}

			dgv.EndEdit();

			DataGridViewRow selectedRow = dgv.SelectedRows[0];
			if (selectedRow == null || selectedRow.IsNewRow)
			{
				return;
			}

			int oldIndex = selectedRow.Index;
			int newIndex = oldIndex + direction;

			if (newIndex < 0 || newIndex >= dgv.Rows.Count)
			{
				return;
			}

			object[] values = new object[selectedRow.Cells.Count];
			for (int i = 0; i < selectedRow.Cells.Count; i++)
			{
				values[i] = selectedRow.Cells[i].Value;
			}

			dgv.Rows.RemoveAt(oldIndex);
			dgv.Rows.Insert(newIndex, values);

			dgv.ClearSelection();
			dgv.Rows[newIndex].Selected = true;

			if (dgv.Columns.Count > 0)
			{
				dgv.CurrentCell = dgv.Rows[newIndex].Cells[0];
			}

			ValidateCommunicationRangeGrid(dgv);
		}

		private void btnSave_Click(object sender, EventArgs e)
		{
			SaveCurrentTypeParamsFromUI();
			SaveCurrentTypeVariablesFromGrid();

			if (!ValidateCurrentTcpEndpointUnique(true))
			{
				return;
			}

			_config.SelectedType = _selectedType;
			CommunicationConfigStore.Save(_config);
			CommunicationConfigChangedHub.RaiseConfigChanged();

			ThemedDialog.ShowInformation(
				this,
				_isEnglish ? "Save" : "保存",
				_isEnglish ? "Communication configuration saved." : "通讯配置已保存。",
				_isEnglish);

		}

		private void btnChannelSettings_Click(object sender, EventArgs e)
		{
			SaveCurrentTypeParamsFromUI();
			SaveCurrentTypeVariablesFromGrid();
			string selectedInstanceName = _selectedInstanceName;
			string selectedProtocolName = CommunicationRuntimeNaming.GetProtocolName(_selectedType);

			using (CommunicationChannelSettingsDialog dialog =
				new CommunicationChannelSettingsDialog(
					GetCurrentTypeChannels(),
					GetCurrentTypeInputs(),
					GetCurrentTypeOutputs(),
					_selectedType,
					_isEnglish))
			{
				if (dialog.ShowDialog(this) != DialogResult.OK)
				{
					return;
				}

				SetCurrentTypeChannels(dialog.Channels);
				CommunicationConfigStore.Save(_config);
				if (dialog.ChannelRenames != null && dialog.ChannelRenames.Count > 0)
				{
					FlowConfigStore.RenameCommunicationChannelReferences(
						selectedProtocolName,
						selectedInstanceName,
						dialog.ChannelRenames);
				}
				LoadConfigToUI(_config);
			}
		}

		private void btnHeartbeatSettings_Click(object sender, EventArgs e)
		{
			if (_selectedType == CommunicationType.Profinet)
			{
				return;
			}

			SaveCurrentTypeParamsFromUI();
			SaveCurrentTypeVariablesFromGrid();

			List<CommOutputVariable> outputs = GetCurrentTypeOutputs();
			using (CommunicationHeartbeatSettingsDialog dialog =
				new CommunicationHeartbeatSettingsDialog(GetCurrentTypeHeartbeat(), outputs, _isEnglish))
			{
				if (dialog.ShowDialog(this) != DialogResult.OK)
				{
					return;
				}

				SetCurrentTypeHeartbeat(dialog.Heartbeat);
				CommunicationConfigStore.Save(_config);
				CommunicationRuntimeManager.Instance.ReloadHeartbeatConfig(_config);
				LoadConfigToUI(_config);
			}
		}

		private List<CommunicationChannelConfig> GetCurrentTypeChannels()
		{
			if (_config == null)
			{
				_config = new CommunicationConfig();
			}

			if (_selectedType == CommunicationType.TcpIp)
			{
				return GetCurrentTcpConfig().Channels;
			}

			if (_selectedType == CommunicationType.Profinet)
			{
				return GetCurrentProfinetConfig().Channels;
			}

			return GetCurrentS7Config().Channels;
		}

		private List<CommInputVariable> GetCurrentTypeInputs()
		{
			if (_config == null)
			{
				_config = new CommunicationConfig();
			}

			if (_selectedType == CommunicationType.TcpIp)
			{
				return GetCurrentTcpConfig().InputVariables;
			}

			if (_selectedType == CommunicationType.Profinet)
			{
				return GetCurrentProfinetConfig().InputVariables;
			}

			return GetCurrentS7Config().InputVariables;
		}

		private List<CommOutputVariable> GetCurrentTypeOutputs()
		{
			if (_config == null)
			{
				_config = new CommunicationConfig();
			}

			if (_selectedType == CommunicationType.TcpIp)
			{
				return GetCurrentTcpConfig().OutputVariables;
			}

			if (_selectedType == CommunicationType.Profinet)
			{
				return GetCurrentProfinetConfig().OutputVariables;
			}

			return GetCurrentS7Config().OutputVariables;
		}

		private CommunicationHeartbeatConfig GetCurrentTypeHeartbeat()
		{
			if (_config == null)
			{
				_config = new CommunicationConfig();
			}

			if (_selectedType == CommunicationType.TcpIp)
			{
				TcpIpConfig tcpIp = GetCurrentTcpConfig();
				if (tcpIp.Heartbeat == null) tcpIp.Heartbeat = new CommunicationHeartbeatConfig();
				return tcpIp.Heartbeat;
			}

			if (_selectedType == CommunicationType.Profinet)
			{
				ProfinetConfig profinet = GetCurrentProfinetConfig();
				if (profinet.Heartbeat == null) profinet.Heartbeat = new CommunicationHeartbeatConfig();
				return profinet.Heartbeat;
			}

			S7Config s7 = GetCurrentS7Config();
			if (s7.Heartbeat == null) s7.Heartbeat = new CommunicationHeartbeatConfig();
			return s7.Heartbeat;
		}

		private void SetCurrentTypeHeartbeat(CommunicationHeartbeatConfig heartbeat)
		{
			if (_config == null)
			{
				_config = new CommunicationConfig();
			}

			if (heartbeat == null)
			{
				heartbeat = new CommunicationHeartbeatConfig();
			}

			if (_selectedType == CommunicationType.TcpIp)
			{
				TcpIpConfig tcpIp = GetCurrentTcpConfig();
				tcpIp.Heartbeat = heartbeat;
				CommunicationInstanceConfig instance = GetSelectedInstance();
				if (instance != null) instance.Heartbeat = heartbeat;
			}
			else if (_selectedType == CommunicationType.Profinet)
			{
				ProfinetConfig profinet = GetCurrentProfinetConfig();
				profinet.Heartbeat = heartbeat;
				CommunicationInstanceConfig instance = GetSelectedInstance();
				if (instance != null) instance.Heartbeat = heartbeat;
			}
			else
			{
				S7Config s7 = GetCurrentS7Config();
				s7.Heartbeat = heartbeat;
				CommunicationInstanceConfig instance = GetSelectedInstance();
				if (instance != null) instance.Heartbeat = heartbeat;
			}
		}

		private void SetCurrentTypeChannels(List<CommunicationChannelConfig> channels)
		{
			if (_config == null)
			{
				_config = new CommunicationConfig();
			}

			if (channels == null)
			{
				channels = new List<CommunicationChannelConfig>();
			}

			if (_selectedType == CommunicationType.TcpIp)
			{
				TcpIpConfig tcpIp = GetCurrentTcpConfig();
				tcpIp.Channels = channels;
				CommunicationInstanceConfig instance = GetSelectedInstance();
				if (instance != null) instance.Channels = channels;
			}
			else if (_selectedType == CommunicationType.Profinet)
			{
				ProfinetConfig profinet = GetCurrentProfinetConfig();
				profinet.Channels = channels;
				CommunicationInstanceConfig instance = GetSelectedInstance();
				if (instance != null) instance.Channels = channels;
			}
			else
			{
				S7Config s7 = GetCurrentS7Config();
				s7.Channels = channels;
				CommunicationInstanceConfig instance = GetSelectedInstance();
				if (instance != null) instance.Channels = channels;
			}
		}


		private void btnSendTest_Click(object sender, EventArgs e)
		{
			string send = txtSend.Text;

			if (string.IsNullOrEmpty(send))
			{
				ThemedDialog.ShowWarning(
					this,
					_isEnglish ? "Test" : "测试",
					_isEnglish ? "Please input send message first." : "请先输入发送报文。",
					_isEnglish);
				return;
			}

			if (_selectedType != CommunicationType.TcpIp)
			{
				ThemedDialog.ShowInformation(
					this,
					_isEnglish ? "Test" : "测试",
					_isEnglish ? "Current test send is for TCP/IP only." : "当前测试发送仅支持 TCP/IP。",
					_isEnglish);
				return;
			}

			ICommunicationRuntime runtime = CommunicationRuntimeManager.Instance.GetRuntime(_selectedInstanceName);

			if (runtime == null || !runtime.IsRunning)
			{
				ThemedDialog.ShowInformation(
					this,
					"TCP/IP",
					_isEnglish ? "TCP/IP is not connected or listening. Please click Connect first." : "TCP/IP 未连接或未监听，请先点击连接。",
					_isEnglish);
				return;
			}

			SaveCurrentTypeParamsFromUI();

			bool ok;
			string displaySend = send;

			if (_config != null &&
				GetCurrentTcpConfig() != null &&
				GetCurrentTcpConfig().PayloadMode == TcpIpPayloadMode.Byte)
			{
				byte[] data;
				string error;
				if (!TcpIpPayloadCodec.TryParseHexText(send, out data, out error))
				{
					ThemedDialog.ShowWarning(
						this,
						"TCP/IP",
						(_isEnglish ? "Byte mode expects hex bytes, for example: 3F 80 00 00." : "Byte 模式需要输入十六进制字节，例如：3F 80 00 00。") +
						Environment.NewLine +
						error,
						_isEnglish);
					return;
				}

				ok = runtime.SendBytes(data);
				displaySend = TcpIpPayloadCodec.ToHexString(data);
			}
			else
			{
				ok = CommunicationRuntimeManager.Instance.SendTcpString(_selectedInstanceName, send);
			}

			AppendTcpReceiveText(
				DateTime.Now.ToString("HH:mm:ss.fff") +
				"  [TCP Send][" + _selectedInstanceName + "] " +
				displaySend +
				"  Result=" +
				ok);

			if (!ok)
			{
				ThemedDialog.ShowWarning(
					this,
					"TCP/IP",
					_isEnglish ? "TCP/IP send failed. Please check connection state." : "TCP/IP 发送失败，请检查连接状态。",
					_isEnglish);
			}

			UpdateTcpStatusUi();

		}


		private void btnClearTest_Click(object sender, EventArgs e)
		{
			txtSend.Clear();
			txtReceive.Clear();
		}

		private string GetCellString(DataGridViewRow row, int columnIndex)
		{
			if (row.Cells[columnIndex].Value == null)
			{
				return string.Empty;
			}

			return row.Cells[columnIndex].Value.ToString().Trim();
		}

		private string GetCellString(DataGridViewRow row, string columnName)
		{
			if (row == null || row.DataGridView == null ||
				string.IsNullOrWhiteSpace(columnName) || !row.DataGridView.Columns.Contains(columnName))
			{
				return string.Empty;
			}

			object value = row.Cells[columnName].Value;
			return value == null ? string.Empty : value.ToString().Trim();
		}

		private bool GetCellBool(DataGridViewRow row, int columnIndex)
		{
			if (row.Cells[columnIndex].Value == null)
			{
				return false;
			}

			bool value;

			if (bool.TryParse(row.Cells[columnIndex].Value.ToString(), out value))
			{
				return value;
			}

			return false;
		}

		private int GetCellInt(DataGridViewRow row, int columnIndex, int defaultValue)
		{
			return ToInt(GetCellString(row, columnIndex), defaultValue);
		}

		private int ToInt(string text, int defaultValue)
		{
			int value;

			if (int.TryParse(text, out value))
			{
				return value;
			}

			return defaultValue;
		}

		public void ApplyLanguage(bool isEnglish)
		{
			_isEnglish = isEnglish;

			if (isEnglish)
			{
				lblTypeTitle.Text = "Communication Type";
				lblParamTitle.Text = "Communication Setting";
				chkEnable.Text = "Enable";
				grpTest.Text = "Test Send / Receive";
				lblSend.Text = "Send Message";
				lblReceive.Text = "Receive Message";
				lblInputTitle.Text = "Input Parameters";
				lblOutputTitle.Text = "Output Parameters";

				btnSave.Text = "Save";
				btnSendTest.Text = "Send Test";
				btnClearTest.Text = "Clear";
				if (btnMoveUpInput != null) btnMoveUpInput.Text = "Move Up";
				if (btnMoveDownInput != null) btnMoveDownInput.Text = "Move Down";
				if (btnMoveUpOutput != null) btnMoveUpOutput.Text = "Move Up";
				if (btnMoveDownOutput != null) btnMoveDownOutput.Text = "Move Down";
				if (btnTcpConnect != null) btnTcpConnect.Text = "Connect";
				if (btnTcpDisconnect != null) btnTcpDisconnect.Text = "Disconnect";
				if (btnChannelSettings != null) btnChannelSettings.Text = "Channel Settings";
				if (btnHeartbeatSettings != null) btnHeartbeatSettings.Text = "Heartbeat";

				colInputName.HeaderText = "Input Name";
				if (dgvInput.Columns.Contains("colInputUseAsPosition")) dgvInput.Columns["colInputUseAsPosition"].HeaderText = "Use As Position";
				if (dgvInput.Columns.Contains("colInputGlobalVariable")) dgvInput.Columns["colInputGlobalVariable"].HeaderText = "Global Variable";
				if (dgvInput.Columns.Contains("colInputCurrentValue")) dgvInput.Columns["colInputCurrentValue"].HeaderText = "Current Value";
				if (dgvOutput.Columns.Contains("colOutputGlobalVariable")) dgvOutput.Columns["colOutputGlobalVariable"].HeaderText = "Source Variable";
				if (dgvOutput.Columns.Contains("colOutputCurrentValue")) dgvOutput.Columns["colOutputCurrentValue"].HeaderText = "Current Value";
				colInputType.HeaderText = "Type";
				colInputByteOffset.HeaderText = "Byte Offset";
				colInputBitOffset.HeaderText = "Bit";
				colInputLength.HeaderText = "Length";
				colInputRemark.HeaderText = "Remark";

				colOutputName.HeaderText = "Output Name";
				colOutputType.HeaderText = "Type";
				colOutputByteOffset.HeaderText = "Byte Offset";
				colOutputBitOffset.HeaderText = "Bit";
				colOutputLength.HeaderText = "Length";
				colOutputRemark.HeaderText = "Remark";
			}
			else
			{
				lblTypeTitle.Text = "通讯类型";
				lblParamTitle.Text = "通讯设置";
				chkEnable.Text = "启用";
				grpTest.Text = "测试收发数据报文";
				lblSend.Text = "发送报文";
				lblReceive.Text = "接收报文";
				lblInputTitle.Text = "输入参数";
				lblOutputTitle.Text = "输出参数";

				btnSave.Text = "保存";
				btnSendTest.Text = "发送测试";
				btnClearTest.Text = "清空";
				if (btnMoveUpInput != null) btnMoveUpInput.Text = "上移选中";
				if (btnMoveDownInput != null) btnMoveDownInput.Text = "下移选中";
				if (btnMoveUpOutput != null) btnMoveUpOutput.Text = "上移选中";
				if (btnMoveDownOutput != null) btnMoveDownOutput.Text = "下移选中";
				if (btnTcpConnect != null) btnTcpConnect.Text = "连接";
				if (btnTcpDisconnect != null) btnTcpDisconnect.Text = "断开";
				if (btnChannelSettings != null) btnChannelSettings.Text = "通道设置";
				if (btnHeartbeatSettings != null) btnHeartbeatSettings.Text = "心跳设置";

				colInputName.HeaderText = "输入变量名称";
				if (dgvInput.Columns.Contains("colInputUseAsPosition")) dgvInput.Columns["colInputUseAsPosition"].HeaderText = "作为位置号";
				if (dgvInput.Columns.Contains("colInputGlobalVariable")) dgvInput.Columns["colInputGlobalVariable"].HeaderText = "关联全局变量";
				if (dgvInput.Columns.Contains("colInputCurrentValue")) dgvInput.Columns["colInputCurrentValue"].HeaderText = "当前值";
				if (dgvOutput.Columns.Contains("colOutputGlobalVariable")) dgvOutput.Columns["colOutputGlobalVariable"].HeaderText = "关联来源";
				if (dgvOutput.Columns.Contains("colOutputCurrentValue")) dgvOutput.Columns["colOutputCurrentValue"].HeaderText = "当前值";
				colInputType.HeaderText = "类型";
				colInputByteOffset.HeaderText = "偏移字节";
				colInputBitOffset.HeaderText = "Bit";
				colInputLength.HeaderText = "长度";
				colInputRemark.HeaderText = "备注";

				colOutputName.HeaderText = "输出变量名称";
				colOutputType.HeaderText = "类型";
				colOutputByteOffset.HeaderText = "偏移字节";
				colOutputBitOffset.HeaderText = "Bit";
				colOutputLength.HeaderText = "长度";
				colOutputRemark.HeaderText = "备注";
			}

			ApplyColumnModeByCommunicationType();
			LoadTypeParamsToUI();
			LoadCurrentTypeVariablesToGrid();
			ApplyTcpModeParamVisibility();
			UpdateTcpStatusUi();
		}

		private void EnableDoubleBufferForPage()
		{
			SetDoubleBuffered(this);
			SetDoubleBuffered(mainLayout);
			SetDoubleBuffered(rightLayout);
			SetDoubleBuffered(panelType);
			SetDoubleBuffered(panelParams);
			SetDoubleBuffered(panelInput);
			SetDoubleBuffered(panelOutput);
			SetDoubleBuffered(dgvInput);
			SetDoubleBuffered(dgvOutput);
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
					System.Reflection.BindingFlags.Instance |
					System.Reflection.BindingFlags.NonPublic);

				if (property != null)
				{
					property.SetValue(control, true, null);
				}
			}
			catch
			{
			}
		}

		private void BeginUpdateControl(Control control)
		{
			if (control == null || control.IsDisposed)
			{
				return;
			}

			if (control.IsHandleCreated)
			{
				SendMessage(control.Handle, WM_SETREDRAW, IntPtr.Zero, IntPtr.Zero);
			}
		}

		private void EndUpdateControl(Control control)
		{
			if (control == null || control.IsDisposed)
			{
				return;
			}

			if (control.IsHandleCreated)
			{
				SendMessage(control.Handle, WM_SETREDRAW, new IntPtr(1), IntPtr.Zero);
			}

			control.Invalidate(true);
		}

		private void BeginPageRefresh()
		{
			this.SuspendLayout();
			mainLayout.SuspendLayout();
			rightLayout.SuspendLayout();
			panelInput.SuspendLayout();
			panelOutput.SuspendLayout();

			BeginUpdateControl(this);
			BeginUpdateControl(mainLayout);
			BeginUpdateControl(rightLayout);
			BeginUpdateControl(panelInput);
			BeginUpdateControl(panelOutput);
			BeginUpdateControl(dgvInput);
			BeginUpdateControl(dgvOutput);

			dgvInput.Visible = false;
			dgvOutput.Visible = false;
		}

		private void EndPageRefresh()
		{
			dgvInput.Visible = true;
			dgvOutput.Visible = true;

			EndUpdateControl(dgvInput);
			EndUpdateControl(dgvOutput);
			EndUpdateControl(panelInput);
			EndUpdateControl(panelOutput);
			EndUpdateControl(rightLayout);
			EndUpdateControl(mainLayout);
			EndUpdateControl(this);

			panelOutput.ResumeLayout();
			panelInput.ResumeLayout();
			rightLayout.ResumeLayout();
			mainLayout.ResumeLayout();
			this.ResumeLayout();

			this.Refresh();
		}

		private sealed class OutputVariableNameChanges
		{
			public Dictionary<string, string> RenamedNames { get; private set; }
			public HashSet<string> DeletedNames { get; private set; }

			public OutputVariableNameChanges()
			{
				RenamedNames = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
				DeletedNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
			}

			public bool HasChanges
			{
				get { return RenamedNames.Count > 0 || DeletedNames.Count > 0; }
			}

			public bool IsDeleted(string name)
			{
				return !string.IsNullOrWhiteSpace(name) && DeletedNames.Contains(name.Trim());
			}

			public bool TryUpdateReference(ref string name)
			{
				if (string.IsNullOrWhiteSpace(name))
				{
					return false;
				}

				string normalizedName = name.Trim();
				string renamedName;
				if (RenamedNames.TryGetValue(normalizedName, out renamedName))
				{
					name = renamedName;
					return true;
				}

				if (DeletedNames.Contains(normalizedName))
				{
					name = string.Empty;
					return true;
				}

				return false;
			}
		}

		protected override void OnHandleDestroyed(EventArgs e)
		{
			GlobalVariableStore.VariablesChanged -= GlobalVariableStore_VariablesChanged;
			CommunicationConfigChangedHub.ConfigChanged -= CommunicationConfigChangedHub_ConfigChanged;
			RuntimeCommunicationOutputService.OutputValuesChanged -= RuntimeCommunicationOutputService_OutputValuesChanged;

			if (_tcpRuntimeEventBound)
			{
				CommunicationRuntimeManager.Instance.StatusChanged -= CommunicationRuntime_StatusChanged;
				CommunicationRuntimeManager.Instance.DataReceived -= CommunicationRuntime_DataReceived;
				CommunicationRuntimeManager.Instance.ErrorOccurred -= CommunicationRuntime_ErrorOccurred;
				_tcpRuntimeEventBound = false;
			}

			base.OnHandleDestroyed(e);
		}

	}

	internal class CommunicationInstanceNameDialog : Form
	{
		private readonly bool _isEnglish;
		private readonly TextBox _txtName;
		private readonly Button _btnOk;
		private readonly Button _btnCancel;

		public string InstanceName { get; private set; }

		public CommunicationInstanceNameDialog(string title, string defaultName, bool isEnglish)
		{
			_isEnglish = isEnglish;
			InstanceName = string.Empty;

			Text = title;
			StartPosition = FormStartPosition.CenterParent;
			Size = new Size(430, 210);
			FormBorderStyle = FormBorderStyle.FixedDialog;
			MinimizeBox = false;
			MaximizeBox = false;
			BackColor = Color.FromArgb(3, 14, 27);
			ForeColor = Color.White;

			Label label = new Label();
			label.Text = _isEnglish ? "Communication Name" : "通讯名称";
			label.ForeColor = Color.White;
			label.AutoSize = true;
			label.Location = new Point(32, 38);
			Controls.Add(label);

			_txtName = new TextBox();
			_txtName.Location = new Point(150, 34);
			_txtName.Size = new Size(220, 28);
			_txtName.Text = defaultName ?? string.Empty;
			Controls.Add(_txtName);

			_btnOk = CreateButton(_isEnglish ? "OK" : "确定", 150, 110);
			_btnCancel = CreateButton(_isEnglish ? "Cancel" : "取消", 270, 110);
			_btnOk.Click += btnOk_Click;
			_btnCancel.Click += delegate { DialogResult = DialogResult.Cancel; Close(); };
			Controls.Add(_btnOk);
			Controls.Add(_btnCancel);

			AcceptButton = _btnOk;
			CancelButton = _btnCancel;
		}

		protected override void OnShown(EventArgs e)
		{
			base.OnShown(e);
			_txtName.Focus();
			_txtName.SelectAll();
		}

		private Button CreateButton(string text, int x, int y)
		{
			Button button = new Button();
			button.Text = text;
			button.Location = new Point(x, y);
			button.Size = new Size(100, 34);
			button.FlatStyle = FlatStyle.Flat;
			button.FlatAppearance.BorderColor = Color.FromArgb(0, 150, 220);
			button.ForeColor = Color.White;
			button.BackColor = Color.FromArgb(2, 10, 20);
			return button;
		}

		private void btnOk_Click(object sender, EventArgs e)
		{
			string name = _txtName.Text.Trim();
			if (string.IsNullOrWhiteSpace(name))
			{
				ThemedDialog.ShowWarning(
					this,
					Text,
					_isEnglish ? "Please input a communication name." : "请输入通讯名称。",
					_isEnglish);
				return;
			}

			InstanceName = name;
			DialogResult = DialogResult.OK;
			Close();
		}
	}

	internal class CommunicationHeartbeatSettingsDialog : Form
	{
		private readonly bool _isEnglish;
		private readonly List<CommOutputVariable> _outputs;
		private readonly CheckBox _chkEnabled;
		private readonly ComboBox _cmbOutput;
		private readonly TextBox _txtHeartbeatText;
		private readonly NumericUpDown _numInterval;
		private readonly Button _btnOk;
		private readonly Button _btnCancel;

		public CommunicationHeartbeatConfig Heartbeat { get; private set; }

		public CommunicationHeartbeatSettingsDialog(
			CommunicationHeartbeatConfig heartbeat,
			List<CommOutputVariable> outputs,
			bool isEnglish)
		{
			_isEnglish = isEnglish;
			_outputs = CloneOutputs(outputs);
			Heartbeat = CloneHeartbeat(heartbeat);

			Text = _isEnglish ? "Heartbeat Settings" : "心跳设置";
			StartPosition = FormStartPosition.CenterParent;
			Size = new Size(520, 330);
			FormBorderStyle = FormBorderStyle.FixedDialog;
			MinimizeBox = false;
			MaximizeBox = false;
			BackColor = Color.FromArgb(3, 14, 27);
			ForeColor = Color.White;

			_chkEnabled = new CheckBox();
			_chkEnabled.Text = _isEnglish ? "Enabled" : "启用";
			_chkEnabled.AutoSize = true;
			_chkEnabled.ForeColor = Color.White;
			_chkEnabled.BackColor = Color.Transparent;
			_chkEnabled.Location = new Point(34, 28);
			Controls.Add(_chkEnabled);

			Label lblOutput = CreateLabel(_isEnglish ? "Output" : "心跳关联输出选项", 34, 72);
			Controls.Add(lblOutput);

			_cmbOutput = new ComboBox();
			_cmbOutput.DropDownStyle = ComboBoxStyle.DropDownList;
			_cmbOutput.Location = new Point(190, 68);
			_cmbOutput.Size = new Size(260, 28);
			Controls.Add(_cmbOutput);

			Label lblText = CreateLabel(_isEnglish ? "Heartbeat Text" : "心跳字符设置", 34, 118);
			Controls.Add(lblText);

			_txtHeartbeatText = new TextBox();
			_txtHeartbeatText.Location = new Point(190, 114);
			_txtHeartbeatText.Size = new Size(260, 28);
			Controls.Add(_txtHeartbeatText);

			Label lblInterval = CreateLabel(_isEnglish ? "Interval (ms)" : "输出频率(ms)", 34, 164);
			Controls.Add(lblInterval);

			_numInterval = new NumericUpDown();
			_numInterval.Minimum = 50;
			_numInterval.Maximum = 600000;
			_numInterval.Increment = 100;
			_numInterval.Location = new Point(190, 160);
			_numInterval.Size = new Size(260, 28);
			Controls.Add(_numInterval);

			_btnOk = CreateButton(_isEnglish ? "OK" : "确定", 230, 230);
			_btnCancel = CreateButton(_isEnglish ? "Cancel" : "取消", 350, 230);
			_btnOk.Click += btnOk_Click;
			_btnCancel.Click += delegate { DialogResult = DialogResult.Cancel; Close(); };
			Controls.Add(_btnOk);
			Controls.Add(_btnCancel);

			LoadOutputs();
			LoadHeartbeat();
		}

		private Label CreateLabel(string text, int x, int y)
		{
			Label label = new Label();
			label.Text = text;
			label.ForeColor = Color.White;
			label.AutoSize = true;
			label.Location = new Point(x, y);
			return label;
		}

		private Button CreateButton(string text, int x, int y)
		{
			Button button = new Button();
			button.Text = text;
			button.Location = new Point(x, y);
			button.Size = new Size(100, 34);
			button.FlatStyle = FlatStyle.Flat;
			button.FlatAppearance.BorderColor = Color.FromArgb(0, 150, 220);
			button.ForeColor = Color.White;
			button.BackColor = Color.FromArgb(2, 10, 20);
			return button;
		}

		private void LoadOutputs()
		{
			_cmbOutput.Items.Clear();

			foreach (CommOutputVariable output in _outputs)
			{
				if (output == null || string.IsNullOrWhiteSpace(output.Name))
				{
					continue;
				}

				_cmbOutput.Items.Add(output.Name);
			}
		}

		private void LoadHeartbeat()
		{
			_chkEnabled.Checked = Heartbeat.Enabled;
			_txtHeartbeatText.Text = Heartbeat.HeartbeatText ?? string.Empty;
			_numInterval.Value = Math.Min(_numInterval.Maximum, Math.Max(_numInterval.Minimum, Heartbeat.IntervalMs));

			if (!string.IsNullOrWhiteSpace(Heartbeat.OutputName) &&
				_cmbOutput.Items.Contains(Heartbeat.OutputName))
			{
				_cmbOutput.SelectedItem = Heartbeat.OutputName;
			}
			else if (_cmbOutput.Items.Count > 0)
			{
				_cmbOutput.SelectedIndex = 0;
			}
		}

		private void btnOk_Click(object sender, EventArgs e)
		{
			if (_chkEnabled.Checked && _cmbOutput.SelectedItem == null)
			{
				ThemedDialog.ShowWarning(
					this,
					Text,
					_isEnglish ? "Please select heartbeat output." : "请选择心跳关联输出项。",
					_isEnglish);
				return;
			}

			string outputName = _cmbOutput.SelectedItem == null ? string.Empty : _cmbOutput.SelectedItem.ToString();
			string heartbeatText = _txtHeartbeatText.Text ?? string.Empty;
			CommOutputVariable output = FindOutput(outputName);

			if (_chkEnabled.Checked && output != null && output.Length > 0 && heartbeatText.Length != output.Length)
			{
				ThemedDialog.ShowWarning(
					this,
					Text,
					_isEnglish
						? "Heartbeat text length does not match the selected output address length. Please modify it."
						: "心跳字符长度和输出地址长度不匹配，请修改。",
					_isEnglish);
				return;
			}

			Heartbeat = new CommunicationHeartbeatConfig();
			Heartbeat.Enabled = _chkEnabled.Checked;
			Heartbeat.OutputName = outputName;
			Heartbeat.HeartbeatText = heartbeatText;
			Heartbeat.IntervalMs = Convert.ToInt32(_numInterval.Value);

			DialogResult = DialogResult.OK;
			Close();
		}

		private CommOutputVariable FindOutput(string outputName)
		{
			if (string.IsNullOrWhiteSpace(outputName))
			{
				return null;
			}

			return _outputs.FirstOrDefault(x =>
				x != null && string.Equals(x.Name, outputName, StringComparison.OrdinalIgnoreCase));
		}

		private static CommunicationHeartbeatConfig CloneHeartbeat(CommunicationHeartbeatConfig heartbeat)
		{
			CommunicationHeartbeatConfig clone = new CommunicationHeartbeatConfig();
			if (heartbeat == null)
			{
				return clone;
			}

			clone.Enabled = heartbeat.Enabled;
			clone.OutputName = heartbeat.OutputName;
			clone.HeartbeatText = heartbeat.HeartbeatText;
			clone.IntervalMs = heartbeat.IntervalMs;
			return clone;
		}

		private static List<CommOutputVariable> CloneOutputs(List<CommOutputVariable> outputs)
		{
			List<CommOutputVariable> result = new List<CommOutputVariable>();
			if (outputs == null)
			{
				return result;
			}

			foreach (CommOutputVariable output in outputs)
			{
				if (output == null)
				{
					continue;
				}

				result.Add(new CommOutputVariable
				{
					Name = output.Name,
					DataType = output.DataType,
					ByteOffset = output.ByteOffset,
					BitOffset = output.BitOffset,
					Length = output.Length,
					Remark = output.Remark,
					GlobalVariableName = output.GlobalVariableName
				});
			}

			return result;
		}

	}

	internal class CommunicationChannelSettingsDialog : Form
	{
		private readonly CommunicationType _communicationType;
		private readonly bool _isEnglish;
		private readonly List<CommInputVariable> _inputVariables;
		private readonly List<CommOutputVariable> _outputVariables;
		private readonly DataGridView _grid;
		private readonly Button _btnAdd;
		private readonly Button _btnDelete;
		private readonly Button _btnOk;
		private readonly Button _btnCancel;

		public List<CommunicationChannelConfig> Channels { get; private set; }
		public Dictionary<string, string> ChannelRenames { get; private set; }

		public CommunicationChannelSettingsDialog(
			List<CommunicationChannelConfig> channels,
			List<CommInputVariable> inputVariables,
			List<CommOutputVariable> outputVariables,
			CommunicationType communicationType,
			bool isEnglish)
		{
			SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint, true);
			DoubleBuffered = true;
			SuspendLayout();

			_communicationType = communicationType;
			_isEnglish = isEnglish;
			_inputVariables = CloneInputVariables(inputVariables);
			_outputVariables = CloneOutputVariables(outputVariables);
			Channels = CloneChannels(channels);
			ChannelRenames = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

			Text = _isEnglish ? "Channel Settings" : "通道设置";
			StartPosition = FormStartPosition.CenterParent;
			Size = new Size(1540, 620);
			MinimizeBox = false;
			MaximizeBox = false;
			BackColor = Color.FromArgb(3, 14, 27);
			ForeColor = Color.White;

			_grid = new BufferedDataGridView();
			_grid.Dock = DockStyle.Top;
			_grid.Height = 430;
			_grid.AllowUserToAddRows = false;
			_grid.AllowUserToDeleteRows = false;
			_grid.RowHeadersVisible = false;
			_grid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
			_grid.MultiSelect = false;
			_grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None;
			_grid.BackgroundColor = Color.FromArgb(2, 10, 20);
			_grid.GridColor = Color.FromArgb(45, 70, 95);
			_grid.BorderStyle = BorderStyle.None;
			_grid.EnableHeadersVisualStyles = false;
			_grid.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(8, 28, 48);
			_grid.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
			_grid.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
			_grid.DefaultCellStyle.BackColor = Color.FromArgb(2, 10, 20);
			_grid.DefaultCellStyle.ForeColor = Color.White;
			_grid.DefaultCellStyle.SelectionBackColor = Color.FromArgb(0, 120, 200);
			_grid.DefaultCellStyle.SelectionForeColor = Color.White;
			_grid.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
			_grid.CellClick += Grid_CellContentClick;
			SetDoubleBuffered(_grid);

			ConfigureGrid();
			LoadChannelsToGrid();
			Controls.Add(_grid);

			Panel buttonPanel = new Panel();
			buttonPanel.Dock = DockStyle.Bottom;
			buttonPanel.Height = 76;
			buttonPanel.BackColor = Color.FromArgb(3, 14, 27);
			SetDoubleBuffered(buttonPanel);
			Controls.Add(buttonPanel);

			_btnAdd = CreateButton(_isEnglish ? "+ Add" : "+ 新增通道", 18, 20, 120);
			_btnDelete = CreateButton(_isEnglish ? "Delete" : "删除选中", 150, 20, 120);
			_btnOk = CreateButton(_isEnglish ? "OK" : "确定", 700, 20, 110);
			_btnCancel = CreateButton(_isEnglish ? "Cancel" : "取消", 825, 20, 110);

			_btnAdd.Enabled = _communicationType != CommunicationType.Profinet;
			_btnDelete.Enabled = _communicationType != CommunicationType.Profinet;

			_btnAdd.Click += btnAdd_Click;
			_btnDelete.Click += btnDelete_Click;
			_btnOk.Click += btnOk_Click;
			_btnCancel.Click += delegate { DialogResult = DialogResult.Cancel; Close(); };

			buttonPanel.Controls.Add(_btnAdd);
			buttonPanel.Controls.Add(_btnDelete);
			buttonPanel.Controls.Add(_btnOk);
			buttonPanel.Controls.Add(_btnCancel);

			ResumeLayout(true);
		}

		private Button CreateButton(string text, int x, int y, int width)
		{
			Button button = new Button();
			button.Text = text;
			button.Location = new Point(x, y);
			button.Size = new Size(width, 34);
			button.FlatStyle = FlatStyle.Flat;
			button.FlatAppearance.BorderColor = Color.FromArgb(0, 150, 220);
			button.ForeColor = Color.White;
			button.BackColor = Color.FromArgb(2, 10, 20);
			return button;
		}

		private void ConfigureGrid()
		{
			_grid.SuspendLayout();
			_grid.Columns.Clear();
			try
			{
				_grid.Columns.Add(CreateTextColumn("colChannel", _isEnglish ? "Channel" : "通道", 120, false));
				DataGridViewTextBoxColumn originalChannelColumn = CreateTextColumn("colOriginalChannel", "OriginalChannel", 10, true);
				originalChannelColumn.Visible = false;
				_grid.Columns.Add(originalChannelColumn);

				DataGridViewCheckBoxColumn enabledColumn = new DataGridViewCheckBoxColumn();
				enabledColumn.Name = "colEnabled";
				enabledColumn.HeaderText = _isEnglish ? "Enabled" : "启用";
				enabledColumn.Width = 70;
				_grid.Columns.Add(enabledColumn);

				_grid.Columns.Add(GlobalVariableBindingUi.CreateButtonColumn(
					"colTriggerGlobal",
					_isEnglish ? "Trigger Global" : "触发源全局变量",
					150));
				_grid.Columns.Add(CreateTextColumn("colTriggerValue", _isEnglish ? "Trigger Value" : "触发期望值", 100, false));
				DataGridViewTextBoxColumn customTriggerColumn = CreateTextColumn(
					"colCustomTriggerGlobal",
					_isEnglish ? "Other Custom Trigger" : "其它自定义触发源",
					220,
					true);
				customTriggerColumn.DefaultCellStyle.WrapMode = DataGridViewTriState.True;
				customTriggerColumn.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
				_grid.Columns.Add(customTriggerColumn);
				_grid.Columns.Add(GlobalVariableBindingUi.CreateButtonColumn(
					"colProgramNo",
					_isEnglish ? "Program Global" : "程序号全局变量",
					150));
				_grid.Columns.Add(GlobalVariableBindingUi.CreateButtonColumn(
					"colProgramSwitch",
					_isEnglish ? "Program Switch Global" : "程序号切换源",
					170));
				_grid.Columns.Add(CreateButtonColumn(
					"colChannelReady",
					_isEnglish ? "Channel Ready" : "通道准备信号",
					260));
				_grid.Columns.Add(CreateButtonColumn(
					"colProgramOutput",
					_isEnglish ? "Program Output" : "输出程序号",
					150));
				DataGridViewTextBoxColumn readyOutputColumn = CreateTextColumn("colChannelReadyOutput", "ChannelReadyOutput", 10, false);
				readyOutputColumn.Visible = false;
				_grid.Columns.Add(readyOutputColumn);
				DataGridViewTextBoxColumn readyBusyColumn = CreateTextColumn("colChannelReadyBusy", "ChannelReadyBusy", 10, false);
				readyBusyColumn.Visible = false;
				_grid.Columns.Add(readyBusyColumn);
				DataGridViewTextBoxColumn readyDoneColumn = CreateTextColumn("colChannelReadyDone", "ChannelReadyDone", 10, false);
				readyDoneColumn.Visible = false;
				_grid.Columns.Add(readyDoneColumn);
				_grid.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCells;
				_grid.DefaultCellStyle.WrapMode = DataGridViewTriState.True;
				if (_grid.Columns.Contains("colCustomTriggerGlobal"))
				{
					_grid.Columns["colCustomTriggerGlobal"].DefaultCellStyle.WrapMode = DataGridViewTriState.True;
				}
			}
			finally
			{
				_grid.ResumeLayout();
			}
		}

		private DataGridViewTextBoxColumn CreateTextColumn(string name, string header, int width, bool readOnly)
		{
			DataGridViewTextBoxColumn column = new DataGridViewTextBoxColumn();
			column.Name = name;
			column.HeaderText = header;
			column.Width = width;
			column.ReadOnly = readOnly;
			return column;
		}

		private DataGridViewButtonColumn CreateButtonColumn(string name, string header, int width)
		{
			DataGridViewButtonColumn column = new DataGridViewButtonColumn();
			column.Name = name;
			column.HeaderText = header;
			column.Width = width;
			column.FlatStyle = FlatStyle.Flat;
			column.UseColumnTextForButtonValue = false;
			return column;
		}

		private void LoadChannelsToGrid()
		{
			_grid.SuspendLayout();
			try
			{
				_grid.Rows.Clear();

				if (Channels == null)
				{
					Channels = new List<CommunicationChannelConfig>();
				}

				foreach (CommunicationChannelConfig channel in Channels)
				{
					if (channel == null)
					{
						continue;
					}

					int rowIndex = _grid.Rows.Add(
						channel.ChannelName,
						channel.ChannelName,
						channel.Enabled,
						string.Empty,
						channel.TriggerExpectedValue,
						FormatCustomTriggers(GetCustomTriggersForChannel(channel)),
						string.Empty,
						string.Empty,
						FormatChannelReadySettings(channel),
						FormatOutputSelection(channel.ProgramNoOutputName),
						channel.ChannelReadyOutputName,
						string.IsNullOrWhiteSpace(channel.ChannelReadyBusyValue) ? "0" : channel.ChannelReadyBusyValue,
						string.IsNullOrWhiteSpace(channel.ChannelReadyDoneValue) ? "1" : channel.ChannelReadyDoneValue);
					_grid.Rows[rowIndex].Tag = CloneCustomTriggers(GetCustomTriggersForChannel(channel));
					GlobalVariableBindingUi.SetCellValue(_grid.Rows[rowIndex], "colTriggerGlobal", channel.TriggerGlobalVariableName);
					GlobalVariableBindingUi.SetCellValue(_grid.Rows[rowIndex], "colProgramNo", channel.ProgramNoAddressName);
					GlobalVariableBindingUi.SetCellValue(_grid.Rows[rowIndex], "colProgramSwitch", channel.ProgramSwitchEnableName);
					SetCellValue(_grid.Rows[rowIndex], "colProgramOutput", FormatOutputSelection(channel.ProgramNoOutputName));
				}
			}
			finally
			{
				_grid.ResumeLayout();
			}
		}

		private void Grid_CellContentClick(object sender, DataGridViewCellEventArgs e)
		{
			if (e.RowIndex < 0 || e.ColumnIndex < 0)
			{
				return;
			}

			string columnName = _grid.Columns[e.ColumnIndex].Name;
			if (columnName == "colCustomTriggerGlobal")
			{
				OpenCustomTriggerDialog(e.RowIndex);
				return;
			}

			if (columnName == "colChannelReady")
			{
				OpenChannelReadyDialog(e.RowIndex);
				return;
			}

			if (columnName == "colProgramOutput")
			{
				OpenOutputSelectDialog(e.RowIndex, columnName);
				return;
			}

			if (columnName != "colTriggerGlobal" &&
				columnName != "colProgramNo" &&
				columnName != "colProgramSwitch")
			{
				return;
			}

			GlobalVariableBindingUi.SelectForCell(this, _grid.Rows[e.RowIndex], columnName);
		}

		private void OpenCustomTriggerDialog(int rowIndex)
		{
			if (rowIndex < 0 || rowIndex >= _grid.Rows.Count)
			{
				return;
			}

			List<CommunicationCustomTriggerOption> triggers = GetRowCustomTriggers(_grid.Rows[rowIndex]);
			using (CommunicationCustomTriggerSettingsDialog dialog =
				new CommunicationCustomTriggerSettingsDialog(triggers, _inputVariables, _isEnglish))
			{
				if (dialog.ShowDialog(this) != DialogResult.OK)
				{
					return;
				}

				_grid.Rows[rowIndex].Tag = CloneCustomTriggers(dialog.Triggers);
				_grid.Rows[rowIndex].Cells["colCustomTriggerGlobal"].Value = FormatCustomTriggers(dialog.Triggers);
			}
		}

		private void OpenChannelReadyDialog(int rowIndex)
		{
			if (rowIndex < 0 || rowIndex >= _grid.Rows.Count)
			{
				return;
			}

			DataGridViewRow row = _grid.Rows[rowIndex];
			string outputName = GetCellString(row, "colChannelReadyOutput");
			string busyValue = GetCellString(row, "colChannelReadyBusy");
			string doneValue = GetCellString(row, "colChannelReadyDone");

			using (CommunicationChannelReadySettingsDialog dialog =
				new CommunicationChannelReadySettingsDialog(outputName, busyValue, doneValue, _outputVariables, _isEnglish))
			{
				if (dialog.ShowDialog(this) != DialogResult.OK)
				{
					return;
				}

				SetCellValue(row, "colChannelReadyOutput", dialog.OutputName);
				SetCellValue(row, "colChannelReadyBusy", dialog.BusyValue);
				SetCellValue(row, "colChannelReadyDone", dialog.DoneValue);
				SetCellValue(row, "colChannelReady", FormatChannelReadySettings(dialog.OutputName, dialog.BusyValue, dialog.DoneValue));
			}
		}

		private void OpenOutputSelectDialog(int rowIndex, string columnName)
		{
			if (rowIndex < 0 || rowIndex >= _grid.Rows.Count)
			{
				return;
			}

			DataGridViewRow row = _grid.Rows[rowIndex];
			string current = columnName == "colProgramOutput"
				? GetCellString(row, "colProgramOutput")
				: GetCellString(row, columnName);
			if (current.Equals(_isEnglish ? "Select..." : "选择...", StringComparison.OrdinalIgnoreCase))
			{
				current = string.Empty;
			}

			using (CommunicationOutputSelectDialog dialog =
				new CommunicationOutputSelectDialog(
					_isEnglish ? "Select Program Output" : "选择输出程序号",
					current,
					_outputVariables,
					_isEnglish))
			{
				if (dialog.ShowDialog(this) != DialogResult.OK)
				{
					return;
				}

				SetCellValue(row, columnName, FormatOutputSelection(dialog.OutputName));
			}
		}

		private void btnAdd_Click(object sender, EventArgs e)
		{
			if (_communicationType == CommunicationType.Profinet)
			{
				return;
			}

			int rowIndex = _grid.Rows.Add(
				"Channel" + (_grid.Rows.Count + 1).ToString("00"),
				string.Empty,
				true,
				string.Empty,
				"1",
				FormatCustomTriggers(null),
				string.Empty,
				string.Empty,
				FormatChannelReadySettings(string.Empty, "0", "1"),
				FormatOutputSelection(string.Empty),
				string.Empty,
				"0",
				"1");
			GlobalVariableBindingUi.SetCellValue(_grid.Rows[rowIndex], "colTriggerGlobal", string.Empty);
			_grid.Rows[rowIndex].Tag = new List<CommunicationCustomTriggerOption>();
			GlobalVariableBindingUi.SetCellValue(_grid.Rows[rowIndex], "colProgramNo", string.Empty);
			GlobalVariableBindingUi.SetCellValue(_grid.Rows[rowIndex], "colProgramSwitch", string.Empty);
		}

		private string FormatChannelReadySettings(CommunicationChannelConfig channel)
		{
			if (channel == null)
			{
				return FormatChannelReadySettings(string.Empty, "0", "1");
			}

			return FormatChannelReadySettings(
				channel.ChannelReadyOutputName,
				string.IsNullOrWhiteSpace(channel.ChannelReadyBusyValue) ? "0" : channel.ChannelReadyBusyValue,
				string.IsNullOrWhiteSpace(channel.ChannelReadyDoneValue) ? "1" : channel.ChannelReadyDoneValue);
		}

		private string FormatChannelReadySettings(string outputName, string busyValue, string doneValue)
		{
			if (string.IsNullOrWhiteSpace(outputName))
			{
				return _isEnglish ? "Select..." : "选择...";
			}

			return FormatOutputVariableName(outputName) + "  " +
				(_isEnglish ? "Ready=" : "就绪=") + (doneValue ?? string.Empty) + "  " +
				(_isEnglish ? "Not Ready=" : "非就绪=") + (busyValue ?? string.Empty);
		}

		private string FormatOutputVariableName(string outputName)
		{
			if (string.IsNullOrWhiteSpace(outputName))
			{
				return string.Empty;
			}

			string normalizedOutputName = outputName.Trim();
			CommOutputVariable output = _outputVariables.FirstOrDefault(x =>
				x != null &&
				!string.IsNullOrWhiteSpace(x.Name) &&
				string.Equals(x.Name.Trim(), normalizedOutputName, StringComparison.OrdinalIgnoreCase));

			if (output == null)
			{
				return normalizedOutputName + (_isEnglish ? " (missing)" : "（未找到）");
			}

			return normalizedOutputName;
		}

		private string FormatOutputSelection(string outputName)
		{
			return string.IsNullOrWhiteSpace(outputName)
				? (_isEnglish ? "Select..." : "选择...")
				: outputName.Trim();
		}

		private void SetCellValue(DataGridViewRow row, string columnName, object value)
		{
			if (row == null || !_grid.Columns.Contains(columnName))
			{
				return;
			}

			row.Cells[columnName].Value = value;
		}

		private void btnDelete_Click(object sender, EventArgs e)
		{
			if (_communicationType == CommunicationType.Profinet || _grid.SelectedRows.Count <= 0)
			{
				return;
			}

			foreach (DataGridViewRow row in _grid.SelectedRows)
			{
				if (!row.IsNewRow)
				{
					_grid.Rows.Remove(row);
				}
			}
		}

		private void btnOk_Click(object sender, EventArgs e)
		{
			_grid.EndEdit();
			Channels = ReadChannelsFromGrid();
			if (!ValidateChannelNames(Channels))
			{
				return;
			}
			if (!ValidateReferencedOutputVariables(Channels))
			{
				return;
			}

			ChannelRenames = ReadChannelRenamesFromGrid();
			DialogResult = DialogResult.OK;
			Close();
		}

		private bool ValidateChannelNames(List<CommunicationChannelConfig> channels)
		{
			HashSet<string> names = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
			foreach (CommunicationChannelConfig channel in channels ?? new List<CommunicationChannelConfig>())
			{
				if (channel == null || string.IsNullOrWhiteSpace(channel.ChannelName))
				{
					continue;
				}

				string name = channel.ChannelName.Trim();
				if (!names.Add(name))
				{
					ThemedDialog.ShowWarning(
						this,
						Text,
						_isEnglish
							? "Channel names must be unique."
							: "通道名称不能重复。",
						_isEnglish);
					return false;
				}
			}

			return true;
		}

		private bool ValidateReferencedOutputVariables(List<CommunicationChannelConfig> channels)
		{
			HashSet<string> outputNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
			foreach (CommOutputVariable output in _outputVariables)
			{
				if (output == null || string.IsNullOrWhiteSpace(output.Name))
				{
					continue;
				}

				outputNames.Add(output.Name.Trim());
			}

			foreach (CommunicationChannelConfig channel in channels ?? new List<CommunicationChannelConfig>())
			{
				if (channel == null)
				{
					continue;
				}

				if (!ValidateOutputReference(
					outputNames,
					channel.ChannelName,
					channel.ChannelReadyOutputName,
					_isEnglish ? "Channel Ready" : "通道准备信号"))
				{
					return false;
				}

				if (!ValidateOutputReference(
					outputNames,
					channel.ChannelName,
					channel.ProgramNoOutputName,
					_isEnglish ? "Program Output" : "输出程序号"))
				{
					return false;
				}
			}

			return true;
		}

		private bool ValidateOutputReference(
			HashSet<string> outputNames,
			string channelName,
			string outputName,
			string fieldName)
		{
			if (string.IsNullOrWhiteSpace(outputName))
			{
				return true;
			}

			if (outputNames != null && outputNames.Contains(outputName.Trim()))
			{
				return true;
			}

			ThemedDialog.ShowWarning(
				this,
				Text,
				_isEnglish
					? fieldName + " of " + (channelName ?? string.Empty) + " references a missing output variable: " + outputName.Trim() + ". Please add it in Output Variables or select an existing output variable."
					: (channelName ?? string.Empty) + " 的" + fieldName + "引用了不存在的输出变量：" + outputName.Trim() + "。请先在输出变量表新增它，或选择已有输出变量。",
				_isEnglish);
			return false;
		}

		private Dictionary<string, string> ReadChannelRenamesFromGrid()
		{
			Dictionary<string, string> renames = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

			foreach (DataGridViewRow row in _grid.Rows)
			{
				if (row.IsNewRow)
				{
					continue;
				}

				string original = GetCellString(row, "colOriginalChannel");
				string current = GetCellString(row, "colChannel");
				if (string.IsNullOrWhiteSpace(original) ||
					string.IsNullOrWhiteSpace(current) ||
					string.Equals(original, current, StringComparison.OrdinalIgnoreCase))
				{
					continue;
				}

				renames[original.Trim()] = current.Trim();
			}

			return renames;
		}

		private List<CommunicationChannelConfig> ReadChannelsFromGrid()
		{
			List<CommunicationChannelConfig> channels = new List<CommunicationChannelConfig>();

			foreach (DataGridViewRow row in _grid.Rows)
			{
				if (row.IsNewRow)
				{
					continue;
				}

				string channelName = GetCellString(row, "colChannel");
				if (string.IsNullOrWhiteSpace(channelName))
				{
					continue;
				}

				CommunicationChannelConfig channel = new CommunicationChannelConfig();
				channel.ChannelName = channelName.Trim();
				channel.Enabled = GetCellBool(row, "colEnabled");
				channel.TriggerExpectedValue = GetCellString(row, "colTriggerValue");
				if (string.IsNullOrWhiteSpace(channel.TriggerExpectedValue))
				{
					channel.TriggerExpectedValue = "1";
				}
				channel.TriggerGlobalVariableName = GlobalVariableBindingUi.GetCellValue(row, "colTriggerGlobal");
				channel.CustomTriggers = GetRowCustomTriggers(row);
				CommunicationCustomTriggerOption firstCustomTrigger = channel.CustomTriggers.FirstOrDefault();
				if (firstCustomTrigger == null)
				{
					channel.CustomTriggerGlobalVariableName = string.Empty;
					channel.CustomTriggerExpectedValue = "1";
				}
				else
				{
					channel.CustomTriggerGlobalVariableName = firstCustomTrigger.Name;
					channel.CustomTriggerExpectedValue = string.IsNullOrWhiteSpace(firstCustomTrigger.ExpectedValue)
						? "1"
						: firstCustomTrigger.ExpectedValue;
				}
				channel.PositionGlobalVariableName = string.Empty;
				channel.ProgramNoAddressName = GlobalVariableBindingUi.GetCellValue(row, "colProgramNo");
				channel.ProgramSwitchEnableName = GlobalVariableBindingUi.GetCellValue(row, "colProgramSwitch");
				channel.ChannelReadyOutputName = GetCellString(row, "colChannelReadyOutput");
				channel.ChannelReadyBusyValue = GetCellString(row, "colChannelReadyBusy");
				channel.ChannelReadyDoneValue = GetCellString(row, "colChannelReadyDone");
				channel.ProgramNoOutputName = ParseOutputSelection(GetCellString(row, "colProgramOutput"));
				if (string.IsNullOrWhiteSpace(channel.ChannelReadyBusyValue)) channel.ChannelReadyBusyValue = "0";
				if (string.IsNullOrWhiteSpace(channel.ChannelReadyDoneValue)) channel.ChannelReadyDoneValue = "1";
				channel.PositionOptions = new List<CommunicationPositionOption>
				{
					new CommunicationPositionOption { Name = "Not Use", ExpectedValue = string.Empty }
				};
				channel.TriggerName = string.IsNullOrWhiteSpace(channel.TriggerGlobalVariableName)
					? "Trigger"
					: channel.TriggerGlobalVariableName;
				channel.PositionSourceName = "Not Use";

				channels.Add(channel);
			}

			return channels;
		}

		private string ParseOutputSelection(string text)
		{
			if (string.IsNullOrWhiteSpace(text))
			{
				return string.Empty;
			}

			text = text.Trim();
			if (text.Equals("Select...", StringComparison.OrdinalIgnoreCase) ||
				text.Equals("选择...", StringComparison.OrdinalIgnoreCase) ||
				text.Equals("Not Use", StringComparison.OrdinalIgnoreCase) ||
				text.Equals("None", StringComparison.OrdinalIgnoreCase))
			{
				return string.Empty;
			}

			return text;
		}

		private string GetCellString(DataGridViewRow row, string columnName)
		{
			if (row == null || !_grid.Columns.Contains(columnName))
			{
				return string.Empty;
			}

			object value = row.Cells[columnName].Value;
			return value == null ? string.Empty : value.ToString().Trim();
		}

		private bool GetCellBool(DataGridViewRow row, string columnName)
		{
			if (row == null || !_grid.Columns.Contains(columnName))
			{
				return false;
			}

			object value = row.Cells[columnName].Value;
			return value is bool && (bool)value;
		}

		private List<CommunicationCustomTriggerOption> GetRowCustomTriggers(DataGridViewRow row)
		{
			List<CommunicationCustomTriggerOption> triggers = row == null
				? null
				: row.Tag as List<CommunicationCustomTriggerOption>;
			return CloneCustomTriggers(triggers);
		}

		private List<CommunicationCustomTriggerOption> GetCustomTriggersForChannel(CommunicationChannelConfig channel)
		{
			List<CommunicationCustomTriggerOption> triggers = channel == null
				? null
				: channel.CustomTriggers;
			List<CommunicationCustomTriggerOption> result = CloneCustomTriggers(triggers);

			if (result.Count <= 0 &&
				channel != null &&
				!string.IsNullOrWhiteSpace(channel.CustomTriggerGlobalVariableName))
			{
				result.Add(new CommunicationCustomTriggerOption
				{
					Name = channel.CustomTriggerGlobalVariableName.Trim(),
					ExpectedValue = string.IsNullOrWhiteSpace(channel.CustomTriggerExpectedValue)
						? "1"
						: channel.CustomTriggerExpectedValue.Trim()
				});
			}

			return result;
		}

		private string FormatCustomTriggers(List<CommunicationCustomTriggerOption> triggers)
		{
			List<CommunicationCustomTriggerOption> list = CloneCustomTriggers(triggers);
			if (list.Count <= 0)
			{
				return _isEnglish ? "Select..." : "选择...";
			}

			List<string> parts = new List<string>();
			foreach (CommunicationCustomTriggerOption trigger in list)
			{
				if (trigger == null || string.IsNullOrWhiteSpace(trigger.Name))
				{
					continue;
				}

				parts.Add(trigger.Name.Trim() + "=" + (trigger.ExpectedValue ?? string.Empty));
			}

			return parts.Count <= 0
				? (_isEnglish ? "Select..." : "选择...")
				: string.Join(Environment.NewLine, parts.ToArray());
		}

		private static List<CommunicationCustomTriggerOption> CloneCustomTriggers(List<CommunicationCustomTriggerOption> source)
		{
			List<CommunicationCustomTriggerOption> result = new List<CommunicationCustomTriggerOption>();
			if (source == null)
			{
				return result;
			}

			foreach (CommunicationCustomTriggerOption trigger in source)
			{
				if (trigger == null || string.IsNullOrWhiteSpace(trigger.Name))
				{
					continue;
				}

				result.Add(new CommunicationCustomTriggerOption
				{
					Name = trigger.Name.Trim(),
					ExpectedValue = trigger.ExpectedValue == null ? string.Empty : trigger.ExpectedValue.Trim(),
					Remark = trigger.Remark
				});
			}

			return result;
		}

		private static List<CommInputVariable> CloneInputVariables(List<CommInputVariable> source)
		{
			List<CommInputVariable> result = new List<CommInputVariable>();
			if (source == null)
			{
				return result;
			}

			foreach (CommInputVariable item in source)
			{
				if (item == null || string.IsNullOrWhiteSpace(item.Name))
				{
					continue;
				}

				result.Add(new CommInputVariable
				{
					Name = item.Name,
					UseAsTrigger = item.UseAsTrigger,
					UseAsPosition = item.UseAsPosition,
					EngineName = item.EngineName,
					DataType = item.DataType,
					ByteOffset = item.ByteOffset,
					BitOffset = item.BitOffset,
					Length = item.Length,
					Remark = item.Remark,
					GlobalVariableName = item.GlobalVariableName
				});
			}

			return result;
		}

		private static List<CommOutputVariable> CloneOutputVariables(List<CommOutputVariable> source)
		{
			List<CommOutputVariable> result = new List<CommOutputVariable>();
			if (source == null)
			{
				return result;
			}

			foreach (CommOutputVariable item in source)
			{
				if (item == null || string.IsNullOrWhiteSpace(item.Name))
				{
					continue;
				}

				result.Add(new CommOutputVariable
				{
					Name = item.Name,
					DataType = item.DataType,
					ByteOffset = item.ByteOffset,
					BitOffset = item.BitOffset,
					Length = item.Length,
					Remark = item.Remark,
					GlobalVariableName = item.GlobalVariableName
				});
			}

			return result;
		}

		private List<CommunicationPositionOption> ParsePositionOptions(string text)
		{
			List<CommunicationPositionOption> result = new List<CommunicationPositionOption>();
			result.Add(new CommunicationPositionOption { Name = "Not Use", ExpectedValue = string.Empty });

			if (string.IsNullOrWhiteSpace(text))
			{
				return result;
			}

			string[] parts = text.Split(new char[] { ';', ',', '|', '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
			foreach (string part in parts)
			{
				string item = part.Trim();
				if (item.Length <= 0)
				{
					continue;
				}

				string name = item;
				string expectedValue = "1";
				int index = item.IndexOf('=');
				if (index >= 0)
				{
					name = item.Substring(0, index).Trim();
					expectedValue = item.Substring(index + 1).Trim();
				}

				if (string.IsNullOrWhiteSpace(name) ||
					result.Any(x => x != null && string.Equals(x.Name, name, StringComparison.OrdinalIgnoreCase)))
				{
					continue;
				}

				result.Add(new CommunicationPositionOption
				{
					Name = name,
					ExpectedValue = expectedValue
				});
			}

			return result;
		}

		private string FormatPositionOptions(List<CommunicationPositionOption> options)
		{
			if (options == null || options.Count <= 0)
			{
				return "Not Use=";
			}

			List<string> parts = new List<string>();
			foreach (CommunicationPositionOption option in options)
			{
				if (option == null || string.IsNullOrWhiteSpace(option.Name))
				{
					continue;
				}

				parts.Add(option.Name + "=" + (option.ExpectedValue ?? string.Empty));
			}

			return parts.Count <= 0 ? "Not Use=" : string.Join(";", parts.ToArray());
		}

		private List<CommunicationChannelConfig> CloneChannels(List<CommunicationChannelConfig> channels)
		{
			List<CommunicationChannelConfig> result = new List<CommunicationChannelConfig>();
			if (channels == null)
			{
				return result;
			}

			foreach (CommunicationChannelConfig channel in channels)
			{
				if (channel == null)
				{
					continue;
				}

				CommunicationChannelConfig clone = new CommunicationChannelConfig();
				clone.ChannelName = channel.ChannelName;
				clone.Enabled = channel.Enabled;
				clone.TriggerName = channel.TriggerName;
				clone.TriggerExpectedValue = channel.TriggerExpectedValue;
				clone.TriggerGlobalVariableName = channel.TriggerGlobalVariableName;
				clone.CustomTriggerGlobalVariableName = channel.CustomTriggerGlobalVariableName;
				clone.CustomTriggerExpectedValue = channel.CustomTriggerExpectedValue;
				clone.CustomTriggers = CloneCustomTriggers(channel.CustomTriggers);
				clone.PositionSourceName = channel.PositionSourceName;
				clone.PositionGlobalVariableName = channel.PositionGlobalVariableName;
				clone.ProgramNoAddressName = channel.ProgramNoAddressName;
				clone.ProgramSwitchEnableName = channel.ProgramSwitchEnableName;
				clone.ProgramSwitchDoneName = channel.ProgramSwitchDoneName;
				clone.ProgramSwitchFailName = channel.ProgramSwitchFailName;
				clone.ChannelReadyOutputName = channel.ChannelReadyOutputName;
				clone.ChannelReadyBusyValue = channel.ChannelReadyBusyValue;
				clone.ChannelReadyDoneValue = channel.ChannelReadyDoneValue;
				clone.ProgramNoOutputName = channel.ProgramNoOutputName;
				clone.PositionOptions = new List<CommunicationPositionOption>();

				if (channel.PositionOptions != null)
				{
					foreach (CommunicationPositionOption option in channel.PositionOptions)
					{
						if (option == null) continue;
						clone.PositionOptions.Add(new CommunicationPositionOption
						{
							Name = option.Name,
							ExpectedValue = option.ExpectedValue,
							Remark = option.Remark
						});
					}
				}

				result.Add(clone);
			}

			return result;
		}

		private static void SetDoubleBuffered(Control control)
		{
			if (control == null)
			{
				return;
			}

			try
			{
				System.Reflection.PropertyInfo property = typeof(Control).GetProperty(
					"DoubleBuffered",
					System.Reflection.BindingFlags.Instance |
					System.Reflection.BindingFlags.NonPublic);

				if (property != null)
				{
					property.SetValue(control, true, null);
				}
			}
			catch
			{
			}
		}

		private class CommunicationOutputSelectDialog : Form
		{
			private readonly bool _isEnglish;
			private readonly ComboBox _cmbOutput;
			private readonly Button _btnOk;
			private readonly Button _btnClear;
			private readonly Button _btnCancel;

			public string OutputName { get; private set; }

			public CommunicationOutputSelectDialog(
				string title,
				string selectedOutput,
				List<CommOutputVariable> outputs,
				bool isEnglish)
			{
				_isEnglish = isEnglish;
				OutputName = selectedOutput ?? string.Empty;

				Text = title;
				StartPosition = FormStartPosition.CenterParent;
				Size = new Size(430, 220);
				FormBorderStyle = FormBorderStyle.FixedDialog;
				MinimizeBox = false;
				MaximizeBox = false;
				BackColor = Color.FromArgb(3, 14, 27);
				ForeColor = Color.White;

				Label label = CreateLabel(_isEnglish ? "Output Pin" : "输出引脚", 32, 42);
				Controls.Add(label);

				_cmbOutput = new ComboBox();
				_cmbOutput.DropDownStyle = ComboBoxStyle.DropDownList;
				_cmbOutput.Location = new Point(130, 38);
				_cmbOutput.Size = new Size(250, 28);
				Controls.Add(_cmbOutput);

				LoadOutputs(outputs, selectedOutput);

				_btnClear = CreateButton(_isEnglish ? "Clear" : "清空", 32, 125);
				_btnOk = CreateButton(_isEnglish ? "OK" : "确定", 190, 125);
				_btnCancel = CreateButton(_isEnglish ? "Cancel" : "取消", 300, 125);
				_btnClear.Click += delegate
				{
					OutputName = string.Empty;
					DialogResult = DialogResult.OK;
					Close();
				};
				_btnOk.Click += btnOk_Click;
				_btnCancel.Click += delegate { DialogResult = DialogResult.Cancel; Close(); };
				Controls.Add(_btnClear);
				Controls.Add(_btnOk);
				Controls.Add(_btnCancel);

				AcceptButton = _btnOk;
				CancelButton = _btnCancel;
			}

			private void LoadOutputs(List<CommOutputVariable> outputs, string selectedOutput)
			{
				_cmbOutput.Items.Clear();
				if (outputs != null)
				{
					foreach (CommOutputVariable output in outputs)
					{
						if (output == null || string.IsNullOrWhiteSpace(output.Name))
						{
							continue;
						}

						_cmbOutput.Items.Add(output.Name);
					}
				}

				if (!string.IsNullOrWhiteSpace(selectedOutput) && _cmbOutput.Items.Contains(selectedOutput))
				{
					_cmbOutput.SelectedItem = selectedOutput;
				}
				else if (_cmbOutput.Items.Count > 0)
				{
					_cmbOutput.SelectedIndex = 0;
				}
			}

			private Label CreateLabel(string text, int x, int y)
			{
				Label label = new Label();
				label.Text = text;
				label.ForeColor = Color.White;
				label.AutoSize = true;
				label.Location = new Point(x, y);
				return label;
			}

			private Button CreateButton(string text, int x, int y)
			{
				Button button = new Button();
				button.Text = text;
				button.Location = new Point(x, y);
				button.Size = new Size(90, 34);
				button.FlatStyle = FlatStyle.Flat;
				button.FlatAppearance.BorderColor = Color.FromArgb(0, 150, 220);
				button.ForeColor = Color.White;
				button.BackColor = Color.FromArgb(2, 10, 20);
				return button;
			}

			private void btnOk_Click(object sender, EventArgs e)
			{
				OutputName = _cmbOutput.SelectedItem == null ? string.Empty : _cmbOutput.SelectedItem.ToString();
				DialogResult = DialogResult.OK;
				Close();
			}
		}

		private class CommunicationChannelReadySettingsDialog : Form
		{
			private readonly bool _isEnglish;
			private readonly List<CommOutputVariable> _outputs;
			private readonly TextBox _txtOutputName;
			private readonly Button _btnSelectOutput;
			private readonly TextBox _txtBusyValue;
			private readonly TextBox _txtDoneValue;
			private readonly Button _btnOk;
			private readonly Button _btnClear;
			private readonly Button _btnCancel;

			public string OutputName { get; private set; }
			public string BusyValue { get; private set; }
			public string DoneValue { get; private set; }

			public CommunicationChannelReadySettingsDialog(
				string outputName,
				string busyValue,
				string doneValue,
				List<CommOutputVariable> outputs,
				bool isEnglish)
			{
				_isEnglish = isEnglish;
				_outputs = CloneOutputVariables(outputs);
				OutputName = outputName ?? string.Empty;
				BusyValue = string.IsNullOrWhiteSpace(busyValue) ? "0" : busyValue;
				DoneValue = string.IsNullOrWhiteSpace(doneValue) ? "1" : doneValue;

				Text = _isEnglish ? "Channel Ready Signal" : "通道准备信号";
				StartPosition = FormStartPosition.CenterParent;
				Size = new Size(620, 330);
				FormBorderStyle = FormBorderStyle.FixedDialog;
				MinimizeBox = false;
				MaximizeBox = false;
				BackColor = Color.FromArgb(3, 14, 27);
				ForeColor = Color.White;

				Controls.Add(CreateLabel(_isEnglish ? "Signal Variable Name" : "信号变量名称", 34, 42));
				_txtOutputName = new TextBox();
				_txtOutputName.Location = new Point(190, 38);
				_txtOutputName.Size = new Size(270, 28);
				_txtOutputName.ReadOnly = true;
				_txtOutputName.Text = OutputName;
				Controls.Add(_txtOutputName);

				_btnSelectOutput = CreateButton(_isEnglish ? "Select" : "选择", 470, 36);
				_btnSelectOutput.Click += btnSelectOutput_Click;
				Controls.Add(_btnSelectOutput);

				Controls.Add(CreateLabel(_isEnglish ? "Ready Value" : "就绪值", 34, 92));
				_txtDoneValue = new TextBox();
				_txtDoneValue.Location = new Point(190, 88);
				_txtDoneValue.Size = new Size(370, 28);
				_txtDoneValue.Text = DoneValue;
				Controls.Add(_txtDoneValue);

				Controls.Add(CreateLabel(_isEnglish ? "Not Ready Value" : "非就绪值", 34, 142));
				_txtBusyValue = new TextBox();
				_txtBusyValue.Location = new Point(190, 138);
				_txtBusyValue.Size = new Size(370, 28);
				_txtBusyValue.Text = BusyValue;
				Controls.Add(_txtBusyValue);

				_btnClear = CreateButton(_isEnglish ? "Clear" : "清空", 34, 230);
				_btnOk = CreateButton(_isEnglish ? "OK" : "确定", 370, 230);
				_btnCancel = CreateButton(_isEnglish ? "Cancel" : "取消", 480, 230);
				_btnClear.Click += delegate
				{
					OutputName = string.Empty;
					BusyValue = "0";
					DoneValue = "1";
					DialogResult = DialogResult.OK;
					Close();
				};
				_btnOk.Click += btnOk_Click;
				_btnCancel.Click += delegate { DialogResult = DialogResult.Cancel; Close(); };
				Controls.Add(_btnClear);
				Controls.Add(_btnOk);
				Controls.Add(_btnCancel);

				AcceptButton = _btnOk;
				CancelButton = _btnCancel;
			}

			private Label CreateLabel(string text, int x, int y)
			{
				Label label = new Label();
				label.Text = text;
				label.ForeColor = Color.White;
				label.AutoSize = true;
				label.Location = new Point(x, y);
				return label;
			}

			private Button CreateButton(string text, int x, int y)
			{
				Button button = new Button();
				button.Text = text;
				button.Location = new Point(x, y);
				button.Size = new Size(90, 34);
				button.FlatStyle = FlatStyle.Flat;
				button.FlatAppearance.BorderColor = Color.FromArgb(0, 150, 220);
				button.ForeColor = Color.White;
				button.BackColor = Color.FromArgb(2, 10, 20);
				return button;
			}

			private void btnSelectOutput_Click(object sender, EventArgs e)
			{
				using (CommunicationOutputVariableSelectDialog dialog =
					new CommunicationOutputVariableSelectDialog(OutputName, _outputs, _isEnglish))
				{
					if (dialog.ShowDialog(this) != DialogResult.OK)
					{
						return;
					}

					OutputName = dialog.SelectedOutputName;
					_txtOutputName.Text = OutputName;
				}
			}

			private void btnOk_Click(object sender, EventArgs e)
			{
				OutputName = _txtOutputName.Text == null ? string.Empty : _txtOutputName.Text.Trim();
				BusyValue = _txtBusyValue.Text == null ? string.Empty : _txtBusyValue.Text.Trim();
				DoneValue = _txtDoneValue.Text == null ? string.Empty : _txtDoneValue.Text.Trim();

				if (!string.IsNullOrWhiteSpace(OutputName) &&
					(string.IsNullOrWhiteSpace(BusyValue) || string.IsNullOrWhiteSpace(DoneValue)))
				{
					ThemedDialog.ShowWarning(
						this,
						Text,
						_isEnglish ? "Please input ready and not-ready values." : "请输入就绪值和非就绪值。",
						_isEnglish);
					return;
				}

				if (string.IsNullOrWhiteSpace(BusyValue)) BusyValue = "0";
				if (string.IsNullOrWhiteSpace(DoneValue)) DoneValue = "1";
				DialogResult = DialogResult.OK;
				Close();
			}

			private static List<CommOutputVariable> CloneOutputVariables(List<CommOutputVariable> outputs)
			{
				List<CommOutputVariable> result = new List<CommOutputVariable>();
				if (outputs == null)
				{
					return result;
				}

				foreach (CommOutputVariable output in outputs)
				{
					if (output == null)
					{
						continue;
					}

					result.Add(new CommOutputVariable
					{
						Name = output.Name,
						DataType = output.DataType,
						ByteOffset = output.ByteOffset,
						BitOffset = output.BitOffset,
						Length = output.Length,
						Remark = output.Remark,
						GlobalVariableName = output.GlobalVariableName
					});
				}

				return result;
			}

			private sealed class CommunicationOutputVariableSelectDialog : Form
			{
				private readonly bool _isEnglish;
				private readonly List<CommOutputVariable> _outputs;
				private readonly string _initialOutputName;
				private readonly TextBox _txtSearch;
				private readonly DataGridView _grid;
				private readonly Button _btnOk;
				private readonly Button _btnClear;
				private readonly Button _btnCancel;

				public string SelectedOutputName { get; private set; }

				public CommunicationOutputVariableSelectDialog(
					string selectedOutputName,
					List<CommOutputVariable> outputs,
					bool isEnglish)
				{
					_isEnglish = isEnglish;
					_outputs = CloneOutputVariables(outputs);
					_initialOutputName = selectedOutputName ?? string.Empty;
					SelectedOutputName = _initialOutputName;

					Text = _isEnglish ? "Select Signal Variable" : "选择信号变量";
					StartPosition = FormStartPosition.CenterParent;
					Size = new Size(760, 430);
					MinimumSize = new Size(620, 360);
					BackColor = Color.FromArgb(2, 10, 20);
					ForeColor = Color.White;
					Font = new Font("Microsoft YaHei UI", 9F);

					TableLayoutPanel root = new TableLayoutPanel();
					root.Dock = DockStyle.Fill;
					root.Padding = new Padding(14);
					root.ColumnCount = 1;
					root.RowCount = 3;
					root.BackColor = BackColor;
					root.RowStyles.Add(new RowStyle(SizeType.Absolute, 44F));
					root.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
					root.RowStyles.Add(new RowStyle(SizeType.Absolute, 50F));

					Panel searchPanel = new Panel();
					searchPanel.Dock = DockStyle.Fill;
					searchPanel.BackColor = BackColor;
					Label lblSearch = new Label();
					lblSearch.Text = _isEnglish ? "Keyword" : "关键字";
					lblSearch.AutoSize = true;
					lblSearch.ForeColor = Color.White;
					lblSearch.Location = new Point(0, 10);
					_txtSearch = new TextBox();
					_txtSearch.Location = new Point(70, 6);
					_txtSearch.Width = 360;
					_txtSearch.BackColor = Color.FromArgb(8, 22, 38);
					_txtSearch.ForeColor = Color.White;
					_txtSearch.BorderStyle = BorderStyle.FixedSingle;
					_txtSearch.TextChanged += delegate { LoadOutputs(); };
					searchPanel.Controls.Add(lblSearch);
					searchPanel.Controls.Add(_txtSearch);

					_grid = new BufferedDataGridView();
					_grid.Dock = DockStyle.Fill;
					_grid.AllowUserToAddRows = false;
					_grid.AllowUserToDeleteRows = false;
					_grid.ReadOnly = true;
					_grid.RowHeadersVisible = false;
					_grid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
					_grid.MultiSelect = false;
					_grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
					_grid.BackgroundColor = BackColor;
					_grid.GridColor = Color.FromArgb(38, 62, 86);
					_grid.BorderStyle = BorderStyle.FixedSingle;
					_grid.EnableHeadersVisualStyles = false;
					_grid.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(8, 28, 48);
					_grid.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
					_grid.ColumnHeadersDefaultCellStyle.Font = new Font("Microsoft YaHei UI", 9F, FontStyle.Bold);
					_grid.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
					_grid.DefaultCellStyle.BackColor = BackColor;
					_grid.DefaultCellStyle.ForeColor = Color.White;
					_grid.DefaultCellStyle.SelectionBackColor = Color.FromArgb(0, 120, 200);
					_grid.DefaultCellStyle.SelectionForeColor = Color.White;
					_grid.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
					_grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Name", HeaderText = _isEnglish ? "Name" : "名称", FillWeight = 120 });
					_grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Type", HeaderText = _isEnglish ? "Type" : "类型", FillWeight = 80 });
					_grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Source", HeaderText = _isEnglish ? "Source" : "关联来源", FillWeight = 130 });
					_grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Remark", HeaderText = _isEnglish ? "Remark" : "备注", FillWeight = 150 });
					_grid.CellDoubleClick += delegate(object sender, DataGridViewCellEventArgs e)
					{
						if (e.RowIndex >= 0)
						{
							AcceptSelection();
						}
					};

					FlowLayoutPanel buttons = new FlowLayoutPanel();
					buttons.Dock = DockStyle.Fill;
					buttons.FlowDirection = FlowDirection.RightToLeft;
					buttons.Padding = new Padding(0, 10, 0, 0);
					buttons.BackColor = BackColor;
					_btnOk = CreateDialogButton(_isEnglish ? "OK" : "确定", true);
					_btnClear = CreateDialogButton(_isEnglish ? "Clear" : "清除关联", false);
					_btnCancel = CreateDialogButton(_isEnglish ? "Cancel" : "取消", false);
					_btnOk.Click += delegate { AcceptSelection(); };
					_btnClear.Click += delegate
					{
						SelectedOutputName = string.Empty;
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

					LoadOutputs();
				}

				private void LoadOutputs()
				{
					string keyword = (_txtSearch.Text ?? string.Empty).Trim();
					_grid.Rows.Clear();

					foreach (CommOutputVariable output in _outputs.Where(output =>
						output != null &&
						!string.IsNullOrWhiteSpace(output.Name) &&
						(string.IsNullOrWhiteSpace(keyword) ||
						 output.Name.IndexOf(keyword, StringComparison.OrdinalIgnoreCase) >= 0 ||
						 output.DataType.ToString().IndexOf(keyword, StringComparison.OrdinalIgnoreCase) >= 0 ||
						 (output.GlobalVariableName ?? string.Empty).IndexOf(keyword, StringComparison.OrdinalIgnoreCase) >= 0 ||
						 (output.Remark ?? string.Empty).IndexOf(keyword, StringComparison.OrdinalIgnoreCase) >= 0)))
					{
						int rowIndex = _grid.Rows.Add(
							output.Name,
							output.DataType.ToString(),
							output.GlobalVariableName,
							output.Remark);

						if (string.Equals(output.Name, _initialOutputName, StringComparison.OrdinalIgnoreCase))
						{
							_grid.Rows[rowIndex].Selected = true;
							_grid.CurrentCell = _grid.Rows[rowIndex].Cells["Name"];
						}
					}
				}

				private Button CreateDialogButton(string text, bool primary)
				{
					Button button = new Button();
					button.Text = text;
					button.Size = new Size(100, 32);
					button.Margin = new Padding(8, 0, 0, 0);
					button.FlatStyle = FlatStyle.Flat;
					button.FlatAppearance.BorderColor = Color.FromArgb(0, 150, 220);
					button.BackColor = primary ? Color.FromArgb(0, 95, 220) : Color.FromArgb(3, 14, 27);
					button.ForeColor = Color.White;
					return button;
				}

				private void AcceptSelection()
				{
					if (_grid.CurrentRow == null)
					{
						return;
					}

					SelectedOutputName = Convert.ToString(_grid.CurrentRow.Cells["Name"].Value);
					DialogResult = DialogResult.OK;
					Close();
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

		private class CommunicationCustomTriggerSettingsDialog : Form
		{
			private readonly bool _isEnglish;
			private readonly List<CommInputVariable> _inputVariables;
			private readonly DataGridView _grid;
			private readonly Button _btnAdd;
			private readonly Button _btnDelete;
			private readonly Button _btnOk;
			private readonly Button _btnCancel;

			public List<CommunicationCustomTriggerOption> Triggers { get; private set; }

			public CommunicationCustomTriggerSettingsDialog(
				List<CommunicationCustomTriggerOption> triggers,
				List<CommInputVariable> inputVariables,
				bool isEnglish)
			{
				SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint, true);
				DoubleBuffered = true;

				_isEnglish = isEnglish;
				_inputVariables = CloneInputVariables(inputVariables);
				Triggers = CloneCustomTriggers(triggers);

				Text = _isEnglish ? "Other Custom Trigger Sources" : "其它自定义触发源";
				StartPosition = FormStartPosition.CenterParent;
				Size = new Size(620, 430);
				MinimizeBox = false;
				MaximizeBox = false;
				BackColor = Color.FromArgb(3, 14, 27);
				ForeColor = Color.White;

				_grid = new BufferedDataGridView();
				_grid.Dock = DockStyle.Top;
				_grid.Height = 300;
				_grid.AllowUserToAddRows = false;
				_grid.AllowUserToDeleteRows = false;
				_grid.RowHeadersVisible = false;
				_grid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
				_grid.MultiSelect = false;
				_grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
				_grid.BackgroundColor = Color.FromArgb(2, 10, 20);
				_grid.GridColor = Color.FromArgb(45, 70, 95);
				_grid.BorderStyle = BorderStyle.None;
				_grid.EnableHeadersVisualStyles = false;
				_grid.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(8, 28, 48);
				_grid.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
				_grid.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
				_grid.DefaultCellStyle.BackColor = Color.FromArgb(2, 10, 20);
				_grid.DefaultCellStyle.ForeColor = Color.White;
				_grid.DefaultCellStyle.SelectionBackColor = Color.FromArgb(0, 120, 200);
				_grid.DefaultCellStyle.SelectionForeColor = Color.White;
				_grid.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
				SetDoubleBuffered(_grid);

				ConfigureGrid();
				LoadTriggers();
				Controls.Add(_grid);

				Panel buttonPanel = new Panel();
				buttonPanel.Dock = DockStyle.Bottom;
				buttonPanel.Height = 78;
				buttonPanel.BackColor = Color.FromArgb(3, 14, 27);
				SetDoubleBuffered(buttonPanel);
				Controls.Add(buttonPanel);

				_btnAdd = CreateButton("+", 24, 22, 58);
				_btnDelete = CreateButton("-", 96, 22, 58);
				_btnOk = CreateButton(_isEnglish ? "OK" : "确定", 350, 22, 92);
				_btnCancel = CreateButton(_isEnglish ? "Cancel" : "取消", 460, 22, 92);

				_btnAdd.Click += delegate { AddTriggerRow(null); };
				_btnDelete.Click += btnDelete_Click;
				_btnOk.Click += btnOk_Click;
				_btnCancel.Click += delegate { DialogResult = DialogResult.Cancel; Close(); };

				buttonPanel.Controls.Add(_btnAdd);
				buttonPanel.Controls.Add(_btnDelete);
				buttonPanel.Controls.Add(_btnOk);
				buttonPanel.Controls.Add(_btnCancel);
			}

			private Button CreateButton(string text, int x, int y, int width)
			{
				Button button = new Button();
				button.Text = text;
				button.Location = new Point(x, y);
				button.Size = new Size(width, 34);
				button.FlatStyle = FlatStyle.Flat;
				button.FlatAppearance.BorderColor = Color.FromArgb(0, 150, 220);
				button.FlatAppearance.MouseOverBackColor = Color.FromArgb(8, 35, 60);
				button.FlatAppearance.MouseDownBackColor = Color.FromArgb(5, 25, 45);
				button.ForeColor = Color.White;
				button.BackColor = Color.FromArgb(2, 10, 20);
				return button;
			}

			private void ConfigureGrid()
			{
				_grid.Columns.Clear();

				DataGridViewComboBoxColumn inputColumn = new DataGridViewComboBoxColumn();
				inputColumn.Name = "colInput";
				inputColumn.HeaderText = _isEnglish ? "Input Object" : "输入对象";
				inputColumn.FlatStyle = FlatStyle.Flat;
				inputColumn.DisplayStyle = DataGridViewComboBoxDisplayStyle.DropDownButton;
				inputColumn.FillWeight = 160;

				foreach (CommInputVariable item in _inputVariables)
				{
					if (item != null &&
						!string.IsNullOrWhiteSpace(item.Name) &&
						!inputColumn.Items.Contains(item.Name.Trim()))
					{
						inputColumn.Items.Add(item.Name.Trim());
					}
				}

				_grid.Columns.Add(inputColumn);

				DataGridViewTextBoxColumn valueColumn = new DataGridViewTextBoxColumn();
				valueColumn.Name = "colExpected";
				valueColumn.HeaderText = _isEnglish ? "Expected Value" : "期望值";
				valueColumn.FillWeight = 90;
				_grid.Columns.Add(valueColumn);
			}

			private void LoadTriggers()
			{
				_grid.Rows.Clear();

				foreach (CommunicationCustomTriggerOption trigger in Triggers)
				{
					AddTriggerRow(trigger);
				}
			}

			private void AddTriggerRow(CommunicationCustomTriggerOption trigger)
			{
				string inputName = trigger == null ? GetDefaultInputName() : trigger.Name;
				string expectedValue = trigger == null
					? "1"
					: (trigger.ExpectedValue ?? string.Empty);

				int rowIndex = _grid.Rows.Add(inputName, expectedValue);
				if (!string.IsNullOrWhiteSpace(inputName))
				{
					DataGridViewComboBoxCell cell = _grid.Rows[rowIndex].Cells["colInput"] as DataGridViewComboBoxCell;
					if (cell != null && !cell.Items.Contains(inputName))
					{
						cell.Items.Add(inputName);
					}
				}
			}

			private string GetDefaultInputName()
			{
				CommInputVariable input = _inputVariables.FirstOrDefault(x =>
					x != null && !string.IsNullOrWhiteSpace(x.Name));
				return input == null ? string.Empty : input.Name.Trim();
			}

			private void btnDelete_Click(object sender, EventArgs e)
			{
				if (_grid.SelectedRows.Count <= 0)
				{
					return;
				}

				foreach (DataGridViewRow row in _grid.SelectedRows)
				{
					if (!row.IsNewRow)
					{
						_grid.Rows.Remove(row);
					}
				}
			}

			private void btnOk_Click(object sender, EventArgs e)
			{
				_grid.EndEdit();
				Triggers = ReadTriggers();
				DialogResult = DialogResult.OK;
				Close();
			}

			private List<CommunicationCustomTriggerOption> ReadTriggers()
			{
				List<CommunicationCustomTriggerOption> result = new List<CommunicationCustomTriggerOption>();

				foreach (DataGridViewRow row in _grid.Rows)
				{
					if (row.IsNewRow)
					{
						continue;
					}

					string name = GetCellString(row, "colInput");
					if (string.IsNullOrWhiteSpace(name))
					{
						continue;
					}

					string expectedValue = GetCellString(row, "colExpected");
					if (string.IsNullOrWhiteSpace(expectedValue))
					{
						expectedValue = "1";
					}

					result.Add(new CommunicationCustomTriggerOption
					{
						Name = name.Trim(),
						ExpectedValue = expectedValue.Trim()
					});
				}

				return result;
			}

			private string GetCellString(DataGridViewRow row, string columnName)
			{
				if (row == null || !_grid.Columns.Contains(columnName))
				{
					return string.Empty;
				}

				object value = row.Cells[columnName].Value;
				return value == null ? string.Empty : value.ToString().Trim();
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
