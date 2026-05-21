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
		private bool _loading = false;
		private bool _isEnglish = false;
		private bool _tcpRuntimeEventBound = false;

		private CheckBox chkEnable;
		private Button btnTcpConnect;
		private Button btnTcpDisconnect;
		private Panel pnlTcpStatusLight;
		private Label lblTcpStatus;
		private Label lblTcpParam1;
		private TextBox txtTcpParam1;
		private Label lblTcpParam2;
		private TextBox txtTcpParam2;

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
			InitializeGridStyle();
			InitializeComboColumns();

			_config = CommunicationConfigStore.LoadOrCreateDefault();
			LoadConfigToUI(_config);
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

			if (_selectedType == CommunicationType.TcpIp)
			{
				_config.TcpIp.Enabled = enabled;
			}
			else if (_selectedType == CommunicationType.Profinet)
			{
				_config.Profinet.Enabled = enabled;
			}
			else
			{
				_config.S7.Enabled = enabled;
			}
		}

		private bool GetCurrentTypeEnabled()
		{
			if (_config == null)
			{
				return false;
			}

			if (_selectedType == CommunicationType.TcpIp)
			{
				return _config.TcpIp.Enabled;
			}

			if (_selectedType == CommunicationType.Profinet)
			{
				return _config.Profinet.Enabled;
			}

			return _config.S7.Enabled;
		}

		private void InitializeGridStyle()
		{
			ApplyGridStyle(dgvInput);
			ApplyGridStyle(dgvOutput);

			dgvInput.DataError -= dgv_DataError;
			dgvOutput.DataError -= dgv_DataError;
			dgvInput.DataError += dgv_DataError;
			dgvOutput.DataError += dgv_DataError;
		}

		private void dgv_DataError(object sender, DataGridViewDataErrorEventArgs e)
		{
			e.ThrowException = false;
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

			cmbMode.Items.Clear();
			cmbMode.Items.Add("Server");
			cmbMode.Items.Add("Client");
			cmbMode.SelectedIndex = 0;
			cmbMode.SelectedIndexChanged += cmbMode_SelectedIndexChanged;
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

		private void InitializeTcpConnectionControls()
		{
			lblTcpParam1 = CreateTcpParamLabel();
			txtTcpParam1 = CreateTcpParamTextBox();

			lblTcpParam2 = CreateTcpParamLabel();
			txtTcpParam2 = CreateTcpParamTextBox();

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
						? (_config == null || _config.TcpIp.LocalPort <= 0 ? "5000" : _config.TcpIp.LocalPort.ToString())
						: txtTcpParam1.Text;
				}
				else
				{
					grpParams.Text = _isEnglish ? "TCP/IP Client Parameters" : "TCP/IP Client 参数";
					lblTcpParam1.Text = _isEnglish ? "Server IP" : "服务器IP";
					lblTcpParam2.Text = _isEnglish ? "Server Port" : "服务器端口";

					if (string.IsNullOrWhiteSpace(txtTcpParam1.Text) && _config != null)
					{
						txtTcpParam1.Text = _config.TcpIp.RemoteIP;
					}

					if (string.IsNullOrWhiteSpace(txtTcpParam2.Text) && _config != null)
					{
						txtTcpParam2.Text = _config.TcpIp.RemotePort <= 0 ? "5000" : _config.TcpIp.RemotePort.ToString();
					}
				}

				// 固定坐标：Server / Client 切换时，模式、连接、断开、状态灯位置保持不变。
				int labelX = 28;
				int inputX = 145;
				int row1Y = 55;
				int row2Y = 100;
				int row3Y = 145;
				int row4Y = 195;
				int row5Y = 236;

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

				btnTcpConnect.SetBounds(labelX, row4Y, buttonW, buttonH);
				btnTcpDisconnect.SetBounds(btnTcpConnect.Right + buttonGap, row4Y, buttonW, buttonH);

				pnlTcpStatusLight.SetBounds(labelX + 6, row5Y + 8, 12, 12);

				int statusWidth = grpParams.ClientSize.Width - pnlTcpStatusLight.Right - 40;
				if (statusWidth < 120)
				{
					statusWidth = 120;
				}

				lblTcpStatus.SetBounds(pnlTcpStatusLight.Right + 12, row5Y, statusWidth, 28);

				lblTcpParam1.BringToFront();
				txtTcpParam1.BringToFront();
				lblTcpParam2.BringToFront();
				txtTcpParam2.BringToFront();

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

			bool isServer = cmbMode.SelectedIndex <= 0;

			if (isServer)
			{
				txtTcpParam1.Text = _config.TcpIp.LocalPort <= 0 ? "5000" : _config.TcpIp.LocalPort.ToString();
				txtTcpParam2.Text = string.Empty;
			}
			else
			{
				txtTcpParam1.Text = _config.TcpIp.RemoteIP;
				txtTcpParam2.Text = _config.TcpIp.RemotePort <= 0 ? "5000" : _config.TcpIp.RemotePort.ToString();
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

			_config.TcpIp.Enabled = chkEnable.Checked;
			_config.SelectedType = _selectedType;

			CommunicationConfigStore.Save(_config);

			if (!_config.TcpIp.Enabled)
			{
				MessageBox.Show("Please enable TCP/IP first.", "TCP/IP", MessageBoxButtons.OK, MessageBoxIcon.Information);
				return;
			}

			CommunicationRuntimeManager.Instance.Start(_config);
			UpdateTcpStatusUi();
		}

		private void btnTcpDisconnect_Click(object sender, EventArgs e)
		{
			ICommunicationRuntime runtime = CommunicationRuntimeManager.Instance.GetRuntime(CommunicationType.TcpIp);

			if (runtime != null)
			{
				runtime.Stop();
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
				"  [TCP Status] " +
				e.State +
				"  " +
				e.Message);

			UpdateTcpStatusUiSafe();
		}

		private void CommunicationRuntime_DataReceived(object sender, CommunicationDataReceivedEventArgs e)
		{
			if (e == null || e.CommunicationType != CommunicationType.TcpIp)
			{
				return;
			}

			string text = DateTime.Now.ToString("HH:mm:ss.fff") +
						  "  [TCP Receive] " +
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

		private void CommunicationRuntime_ErrorOccurred(object sender, Exception e)
		{
			if (e == null)
			{
				return;
			}

			AppendTcpReceiveText(
				DateTime.Now.ToString("HH:mm:ss.fff") +
				"  [TCP Error] " +
				e.Message);

			UpdateTcpStatusUiSafe();
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

			ICommunicationRuntime runtime = CommunicationRuntimeManager.Instance.GetRuntime(CommunicationType.TcpIp);

			CommunicationConnectionState state = CommunicationConnectionState.Stopped;
			bool isRunning = false;
			bool isConnected = false;

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
		}

		private void AppendTcpReceiveText(string text)
		{
			if (txtReceive == null || txtReceive.IsDisposed)
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
			if (_selectedType == CommunicationType.TcpIp)
			{
				return new string[]
				{
					"String"
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

			return defaultValue;
		}


		private bool IsCurrentTypeTcpIp()
		{
			return _selectedType == CommunicationType.TcpIp;
		}

		private CommVariableDataType GetDefaultVariableDataType()
		{
			if (IsCurrentTypeTcpIp())
			{
				return CommVariableDataType.String;
			}

			return CommVariableDataType.Bool;
		}

		private CommVariableDataType NormalizeDataTypeForCurrentCommunication(CommVariableDataType dataType)
		{
			if (IsCurrentTypeTcpIp())
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
					dgvInput.Columns.Insert(1, engineColumn);
				}
				else
				{
					DataGridViewCheckBoxColumn triggerColumn = new DataGridViewCheckBoxColumn();
					triggerColumn.Name = oldName;
					triggerColumn.HeaderText = _isEnglish ? "Use As Trigger" : "作为触发源";
					triggerColumn.Width = oldWidth <= 0 ? 90 : oldWidth;
					dgvInput.Columns.Insert(1, triggerColumn);
				}

				EnsureInputPositionColumn();

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
				return;
			}

			DataGridViewCheckBoxColumn positionColumn = new DataGridViewCheckBoxColumn();
			positionColumn.Name = "colInputUseAsPosition";
			positionColumn.HeaderText = _isEnglish ? "Use As Position" : "作为位置号";
			positionColumn.Width = 90;

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

				ApplySelectedTypeStyle();
				LoadTypeParamsToUI();
				LoadCurrentTypeVariablesToGrid();
				ApplyTcpModeParamVisibility();
				UpdateTcpStatusUi();
			}
			finally
			{
				_loading = false;
			}
		}

		private void SelectCommunicationType(CommunicationType type)
		{
			if (_selectedType == type)
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

				if (_config != null)
				{
					_config.SelectedType = type;
				}

				ApplySelectedTypeStyle();
				LoadTypeParamsToUI();
				LoadCurrentTypeVariablesToGrid();
				ApplyTcpModeParamVisibility();
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
		}

		private void ApplyButtonStyle(Button button, bool selected)
		{
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
					grpParams.Text = _isEnglish ? "TCP/IP Parameters" : "TCP/IP 参数";

					lblP1.Text = _isEnglish ? "Local IP" : "本地IP";
					lblP2.Text = _isEnglish ? "Local Port" : "本地端口";
					lblP3.Text = _isEnglish ? "Server IP" : "服务器IP";
					lblP4.Text = _isEnglish ? "Server Port" : "服务器端口";
					lblP5.Text = _isEnglish ? "Mode" : "模式";
					lblP6.Text = string.Empty;

					txtP1.Text = string.IsNullOrWhiteSpace(_config.TcpIp.LocalIP) ? "0.0.0.0" : _config.TcpIp.LocalIP;
					txtP2.Text = _config.TcpIp.LocalPort <= 0 ? "5000" : _config.TcpIp.LocalPort.ToString();
					txtP3.Text = _config.TcpIp.RemoteIP;
					txtP4.Text = _config.TcpIp.RemotePort <= 0 ? "5000" : _config.TcpIp.RemotePort.ToString();
					txtP5.Text = string.Empty;
					txtP6.Text = string.Empty;

					txtP5.Visible = false;
					txtP6.Visible = false;
					lblP6.Visible = false;
					cmbMode.Visible = true;
					cmbMode.SelectedIndex = _config.TcpIp.IsServer ? 0 : 1;
					SyncTcpDedicatedControlsFromConfig();

					ApplyTcpModeParamVisibility();
					UpdateTcpStatusUi();
				}

				else if (_selectedType == CommunicationType.Profinet)
				{
					grpParams.Text = _isEnglish ? "Profinet Status" : "Profinet 状态";

					lblP1.Text = _isEnglish ? "Device Name" : "设备名称";
					lblP2.Text = _isEnglish ? "Station Name" : "站点名称";
					lblP3.Text = _isEnglish ? "Connection" : "连接状态";
					lblP4.Text = string.Empty;
					lblP5.Text = string.Empty;
					lblP6.Text = string.Empty;

					txtP1.Text = _config.Profinet.DeviceName;
					txtP2.Text = _config.Profinet.StationName;
					txtP3.Text = _config.Profinet.ConnectionStatus;

					txtP3.ReadOnly = true;

					SetParamControlsVisible(true, true, true, false, false, false, false);
				}
				else
				{
					grpParams.Text = _isEnglish ? "S7 Parameters" : "S7 参数";

					lblP1.Text = "PLC IP";
					lblP2.Text = "Rack";
					lblP3.Text = "Slot";
					lblP4.Text = _isEnglish ? "Input DB" : "输入DB";
					lblP5.Text = _isEnglish ? "Output DB" : "输出DB";
					lblP6.Text = _isEnglish ? "Start Byte" : "起始字节";

					txtP1.Text = _config.S7.PlcIP;
					txtP2.Text = _config.S7.Rack.ToString();
					txtP3.Text = _config.S7.Slot.ToString();
					txtP4.Text = _config.S7.InputDB.ToString();
					txtP5.Text = _config.S7.OutputDB.ToString();
					txtP6.Text = _config.S7.InputStartByte.ToString();

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

			if (dgvInput.Columns.Contains("colInputByteOffset"))
			{
				colInputByteOffset.HeaderText = isTcpIp
					? (_isEnglish ? "Char Offset" : "偏移字符")
					: (_isEnglish ? "Byte Offset" : "偏移字节");
			}

			if (dgvOutput.Columns.Contains("colOutputByteOffset"))
			{
				colOutputByteOffset.HeaderText = isTcpIp
					? (_isEnglish ? "Char Offset" : "偏移字符")
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
		}


		private void ForceTcpGridTypeToString()
		{
			if (_selectedType != CommunicationType.TcpIp)
			{
				return;
			}

			foreach (DataGridViewRow row in dgvInput.Rows)
			{
				if (!row.IsNewRow && row.Cells.Count > 2)
				{
					row.Cells[3].Value = "String";
				}
			}

			foreach (DataGridViewRow row in dgvOutput.Rows)
			{
				if (!row.IsNewRow && row.Cells.Count > 1)
				{
					row.Cells[1].Value = "String";
				}
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
					LoadInputRows(_config.TcpIp.InputVariables, false);
					LoadOutputRows(_config.TcpIp.OutputVariables);
					SetVariableGridEditable(true, false);
				}
				else if (_selectedType == CommunicationType.Profinet)
				{
					LoadInputRows(_config.Profinet.InputVariables, true);
					LoadOutputRows(_config.Profinet.OutputVariables);
					SetVariableGridEditable(true, true);
				}
				else
				{
					LoadInputRows(_config.S7.InputVariables, false);
					LoadOutputRows(_config.S7.OutputVariables);
					SetVariableGridEditable(true, false);
				}

				ForceTcpGridTypeToString();

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

					dgvInput.Rows.Add(
						item.Name,
						engine,
						item.UseAsPosition,
						DataTypeToDisplayText(NormalizeDataTypeForCurrentCommunication(item.DataType)),
						item.ByteOffset.ToString(),
						item.BitOffset.ToString(),
						item.Length.ToString(),
						item.Remark);
				}
				else
				{
					dgvInput.Rows.Add(
						item.Name,
						item.UseAsTrigger,
						item.UseAsPosition,
						DataTypeToDisplayText(NormalizeDataTypeForCurrentCommunication(item.DataType)),
						item.ByteOffset.ToString(),
						item.BitOffset.ToString(),
						item.Length.ToString(),
						item.Remark);
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
				dgvOutput.Rows.Add(
					item.Name,
					DataTypeToDisplayText(NormalizeDataTypeForCurrentCommunication(item.DataType)),
					item.ByteOffset.ToString(),
					item.BitOffset.ToString(),
					item.Length.ToString(),
					item.Remark);
			}
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

			btnAddInput.Enabled = editable;
			btnDeleteInput.Enabled = editable;
			btnAddOutput.Enabled = editable;
			btnDeleteOutput.Enabled = editable;

			btnAddInput.Text = _isEnglish ? "+ Add Input" : "+ 新增输入";
			btnDeleteInput.Text = _isEnglish ? "Delete" : "删除选中";
			btnAddOutput.Text = _isEnglish ? "+ Add Output" : "+ 新增输出";
			btnDeleteOutput.Text = _isEnglish ? "Delete" : "删除选中";
		}

		private void SaveCurrentTypeParamsFromUI()
		{
			if (_config == null)
			{
				_config = new CommunicationConfig();
			}

			_config.SelectedType = _selectedType;

			if (_selectedType == CommunicationType.TcpIp)
			{
				bool isServer = cmbMode.SelectedIndex <= 0;

				_config.TcpIp.Enabled = chkEnable.Checked;
				_config.TcpIp.IsServer = isServer;

				if (isServer)
				{
					_config.TcpIp.LocalIP = "0.0.0.0";
					_config.TcpIp.LocalPort = ToInt(txtTcpParam1 == null ? txtP2.Text : txtTcpParam1.Text, 5000);

					_config.TcpIp.RemoteIP = string.Empty;
					_config.TcpIp.RemotePort = 0;
				}
				else
				{
					_config.TcpIp.LocalIP = "0.0.0.0";
					_config.TcpIp.LocalPort = 0;

					_config.TcpIp.RemoteIP = txtTcpParam1 == null ? txtP3.Text.Trim() : txtTcpParam1.Text.Trim();
					_config.TcpIp.RemotePort = ToInt(txtTcpParam2 == null ? txtP4.Text : txtTcpParam2.Text, 5000);
				}
			}

			else if (_selectedType == CommunicationType.Profinet)
			{
				_config.Profinet.Enabled = chkEnable.Checked;
				_config.Profinet.DeviceName = txtP1.Text.Trim();
				_config.Profinet.StationName = txtP2.Text.Trim();
				_config.Profinet.ConnectionStatus = txtP3.Text.Trim();
				_config.Profinet.UseGsdFixedMapping = true;
			}
			else
			{
				_config.S7.Enabled = chkEnable.Checked;
				_config.S7.PlcIP = txtP1.Text.Trim();
				_config.S7.Rack = ToInt(txtP2.Text, 0);
				_config.S7.Slot = ToInt(txtP3.Text, 1);
				_config.S7.InputDB = ToInt(txtP4.Text, 1);
				_config.S7.OutputDB = ToInt(txtP5.Text, 1);
				_config.S7.InputStartByte = ToInt(txtP6.Text, 0);
				_config.S7.OutputStartByte = ToInt(txtP6.Text, 0);
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
				SaveTcpOrS7VariablesFromGrid(_config.TcpIp.InputVariables, _config.TcpIp.OutputVariables);
			}
			else if (_selectedType == CommunicationType.Profinet)
			{
				SaveProfinetVariablesFromGrid();
			}
			else
			{
				SaveTcpOrS7VariablesFromGrid(_config.S7.InputVariables, _config.S7.OutputVariables);
			}
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
				item.Name = name;
				item.UseAsTrigger = GetCellBool(row, 1);
				item.UseAsPosition = GetCellBool(row, 2);
				item.EngineName = string.Empty;
				item.DataType = NormalizeDataTypeForCurrentCommunication(DisplayTextToDataType(GetCellString(row, 3), GetDefaultVariableDataType()));
				item.ByteOffset = GetCellInt(row, 4, 0);
				item.BitOffset = GetCellInt(row, 5, 0);
				item.Length = GetCellInt(row, 6, 1);
				item.Remark = GetCellString(row, 7);

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
				item.Name = name;
				item.DataType = NormalizeDataTypeForCurrentCommunication(DisplayTextToDataType(GetCellString(row, 1), GetDefaultVariableDataType()));
				item.ByteOffset = GetCellInt(row, 2, 0);
				item.BitOffset = GetCellInt(row, 3, 0);
				item.Length = GetCellInt(row, 4, 1);
				item.Remark = GetCellString(row, 5);

				outputList.Add(item);
			}
		}

		private void SaveProfinetVariablesFromGrid()
		{
			_config.Profinet.InputVariables.Clear();
			_config.Profinet.OutputVariables.Clear();

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

				_config.Profinet.InputVariables.Add(item);
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

				_config.Profinet.OutputVariables.Add(item);
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
			if (_selectedType == CommunicationType.Profinet)
			{
				dgvInput.Rows.Add(
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
				dgvInput.Rows.Add(
					"Input_" + (dgvInput.Rows.Count + 1).ToString("00"),
					false,
					false,
					DataTypeToDisplayText(GetDefaultVariableDataType()),
					"0",
					"0",
					"1",
					string.Empty);
			}
		}

		private void btnDeleteInput_Click(object sender, EventArgs e)
		{
			DeleteSelectedRow(dgvInput);
		}

		private void btnAddOutput_Click(object sender, EventArgs e)
		{
			dgvOutput.Rows.Add(
				"Output_" + (dgvOutput.Rows.Count + 1).ToString("00"),
				DataTypeToDisplayText(GetDefaultVariableDataType()),
				"0",
				"0",
				"1",
				string.Empty);
		}

		private void btnDeleteOutput_Click(object sender, EventArgs e)
		{
			DeleteSelectedRow(dgvOutput);
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
		}

		private void btnSave_Click(object sender, EventArgs e)
		{
			SaveCurrentTypeParamsFromUI();
			SaveCurrentTypeVariablesFromGrid();

			_config.SelectedType = _selectedType;
			CommunicationConfigStore.Save(_config);
			CommunicationConfigChangedHub.RaiseConfigChanged();

			MessageBox.Show(
				"Communication configuration saved.",
				"Save",
				MessageBoxButtons.OK,
				MessageBoxIcon.Information);

		}


		private void btnSendTest_Click(object sender, EventArgs e)
		{
			string send = txtSend.Text;

			if (string.IsNullOrEmpty(send))
			{
				MessageBox.Show(
					"Please input send message first.",
					"Test",
					MessageBoxButtons.OK,
					MessageBoxIcon.Warning);
				return;
			}

			if (_selectedType != CommunicationType.TcpIp)
			{
				MessageBox.Show(
					"Current test send is for TCP/IP only.",
					"Test",
					MessageBoxButtons.OK,
					MessageBoxIcon.Information);
				return;
			}

			ICommunicationRuntime runtime = CommunicationRuntimeManager.Instance.GetRuntime(CommunicationType.TcpIp);

			if (runtime == null || !runtime.IsRunning)
			{
				MessageBox.Show(
					"TCP/IP is not connected or listening. Please click Connect first.",
					"TCP/IP",
					MessageBoxButtons.OK,
					MessageBoxIcon.Information);
				return;
			}

			bool ok = CommunicationRuntimeManager.Instance.SendTcpString(send);

			AppendTcpReceiveText(
				DateTime.Now.ToString("HH:mm:ss.fff") +
				"  [TCP Send] " +
				send +
				"  Result=" +
				ok);

			if (!ok)
			{
				MessageBox.Show(
					"TCP/IP send failed. Please check connection state.",
					"TCP/IP",
					MessageBoxButtons.OK,
					MessageBoxIcon.Warning);
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
				if (btnTcpConnect != null) btnTcpConnect.Text = "Connect";
				if (btnTcpDisconnect != null) btnTcpDisconnect.Text = "Disconnect";

				colInputName.HeaderText = "Input Name";
				if (dgvInput.Columns.Contains("colInputUseAsPosition")) dgvInput.Columns["colInputUseAsPosition"].HeaderText = "Use As Position";
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
				if (btnTcpConnect != null) btnTcpConnect.Text = "连接";
				if (btnTcpDisconnect != null) btnTcpDisconnect.Text = "断开";

				colInputName.HeaderText = "输入变量名称";
				if (dgvInput.Columns.Contains("colInputUseAsPosition")) dgvInput.Columns["colInputUseAsPosition"].HeaderText = "作为位置号";
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


		protected override void OnHandleDestroyed(EventArgs e)
		{
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
}
