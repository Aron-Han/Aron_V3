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

		private CheckBox chkEnable;

		private bool? _inputSecondColumnIsProfinet = null;

		[DllImport("user32.dll")]
		private static extern IntPtr SendMessage(IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam);

		private const int WM_SETREDRAW = 0x000B;

		public CommunicationConfigControl()
		{
			InitializeComponent();

			EnableDoubleBufferForPage();

			InitializeEnableCheckBox();
			InitializeGridStyle();
			InitializeComboColumns();

			_config = CommunicationConfigStore.LoadOrCreateDefault();
			LoadConfigToUI(_config);
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

		private void ConfigureInputSecondColumn(bool profinetMode)
		{
			if (dgvInput.Columns.Count < 2)
			{
				return;
			}

			if (_inputSecondColumnIsProfinet.HasValue &&
				_inputSecondColumnIsProfinet.Value == profinetMode)
			{
				dgvInput.Columns[1].HeaderText = profinetMode
					? "Engine"
					: (_isEnglish ? "Use As Trigger" : "作为触发源");

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

				_inputSecondColumnIsProfinet = profinetMode;
			}
			finally
			{
				dgvInput.ResumeLayout();
				EndUpdateControl(dgvInput);
			}
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
					lblP3.Text = _isEnglish ? "Remote IP" : "远程IP";
					lblP4.Text = _isEnglish ? "Remote Port" : "远程端口";
					lblP5.Text = _isEnglish ? "Mode" : "模式";
					lblP6.Text = string.Empty;

					txtP1.Text = _config.TcpIp.LocalIP;
					txtP2.Text = _config.TcpIp.LocalPort.ToString();
					txtP3.Text = _config.TcpIp.RemoteIP;
					txtP4.Text = _config.TcpIp.RemotePort.ToString();
					txtP5.Text = string.Empty;
					txtP6.Text = string.Empty;

					txtP5.Visible = false;
					txtP6.Visible = false;
					lblP6.Visible = false;
					cmbMode.Visible = true;
					cmbMode.SelectedIndex = _config.TcpIp.IsServer ? 0 : 1;
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
						DataTypeToDisplayText(item.DataType),
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
						DataTypeToDisplayText(item.DataType),
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
					DataTypeToDisplayText(item.DataType),
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
				_config.TcpIp.Enabled = chkEnable.Checked;
				_config.TcpIp.LocalIP = txtP1.Text.Trim();
				_config.TcpIp.LocalPort = ToInt(txtP2.Text, 5000);
				_config.TcpIp.RemoteIP = txtP3.Text.Trim();
				_config.TcpIp.RemotePort = ToInt(txtP4.Text, 5000);
				_config.TcpIp.IsServer = cmbMode.SelectedIndex <= 0;
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
				item.EngineName = string.Empty;
				item.DataType = DisplayTextToDataType(GetCellString(row, 2), CommVariableDataType.Bool);
				item.ByteOffset = GetCellInt(row, 3, 0);
				item.BitOffset = GetCellInt(row, 4, 0);
				item.Length = GetCellInt(row, 5, 1);
				item.Remark = GetCellString(row, 6);

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
				item.DataType = DisplayTextToDataType(GetCellString(row, 1), CommVariableDataType.Bool);
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
				item.EngineName = engine;
				item.DataType = DisplayTextToDataType(GetCellString(row, 2), CommVariableDataType.Bool);
				item.ByteOffset = GetCellInt(row, 3, 0);
				item.BitOffset = GetCellInt(row, 4, 0);
				item.Length = GetCellInt(row, 5, 1);
				item.Remark = GetCellString(row, 6);

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
				item.DataType = DisplayTextToDataType(GetCellString(row, 1), CommVariableDataType.Bool);
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
					DataTypeToDisplayText(CommVariableDataType.Bool),
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
				DataTypeToDisplayText(CommVariableDataType.Bool),
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

			MessageBox.Show(
				"Communication configuration saved.",
				"Save",
				MessageBoxButtons.OK,
				MessageBoxIcon.Information);
		}

		private void btnSendTest_Click(object sender, EventArgs e)
		{
			string send = txtSend.Text.Trim();

			if (string.IsNullOrEmpty(send))
			{
				MessageBox.Show(
					"Please input send message first.",
					"Test",
					MessageBoxButtons.OK,
					MessageBoxIcon.Warning);
				return;
			}

			txtReceive.AppendText(
				DateTime.Now.ToString("HH:mm:ss.fff") +
				"  Test send: " +
				send +
				Environment.NewLine);
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

				colInputName.HeaderText = "Input Name";
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

				colInputName.HeaderText = "输入变量名称";
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


	}
}
