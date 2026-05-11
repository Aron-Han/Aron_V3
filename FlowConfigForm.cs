using System;
using System.Drawing;
using System.Windows.Forms;

namespace Aron_V3
{
	public partial class FlowConfigForm : Form
	{
		private bool _dragging;
		private Point _dragStartPoint;
		private Point _formStartPoint;
		private bool _isTaskSchedulerMode = false;

		public FlowConfigForm()
		{
			InitializeComponent();

			LoadJobList();
			ShowTriggerManager();

			this.MouseDown += Header_MouseDown;
			this.MouseMove += Header_MouseMove;
			this.MouseUp += Header_MouseUp;

		}

		private void FlowConfigForm_Load(object sender, EventArgs e)
		{
			this.WindowState = FormWindowState.Maximized;
			AdjustContentLayout();
		}

		protected override void OnShown(EventArgs e)
		{
			base.OnShown(e);
			AdjustContentLayout();
		}

		protected override void OnResize(EventArgs e)
		{
			base.OnResize(e);
			AdjustContentLayout();
		}

		#region Window Buttons

		private void btnMinimize_Click(object sender, EventArgs e)
		{
			this.WindowState = FormWindowState.Minimized;
		}

		private void btnMaximize_Click(object sender, EventArgs e)
		{
			this.WindowState = this.WindowState == FormWindowState.Maximized
				? FormWindowState.Normal
				: FormWindowState.Maximized;

			AdjustContentLayout();
		}

		private void btnClose_Click(object sender, EventArgs e)
		{
			this.Close();
		}

		private void Header_MouseDown(object sender, MouseEventArgs e)
		{
			if (e.Button != MouseButtons.Left)
				return;

			_dragging = true;
			_dragStartPoint = Cursor.Position;
			_formStartPoint = this.Location;
		}

		private void Header_MouseMove(object sender, MouseEventArgs e)
		{
			if (!_dragging)
				return;

			if (this.WindowState == FormWindowState.Maximized)
				return;

			Point offset = new Point(Cursor.Position.X - _dragStartPoint.X, Cursor.Position.Y - _dragStartPoint.Y);
			this.Location = new Point(_formStartPoint.X + offset.X, _formStartPoint.Y + offset.Y);
		}

		private void Header_MouseUp(object sender, MouseEventArgs e)
		{
			_dragging = false;
		}

		#endregion

		#region Left Menu

		private void btnTriggerManager_Click(object sender, EventArgs e)
		{
			ShowTriggerManager();
		}

		private void btnTaskScheduler_Click(object sender, EventArgs e)
		{
			ShowTaskScheduler();
		}

		private void SetLeftMenuSelected(Button selectedButton)
		{
			SetMenuButtonStyle(btnTriggerManager, false);
			SetMenuButtonStyle(btnTaskScheduler, false);
			SetMenuButtonStyle(selectedButton, true);
		}

		private void SetMenuButtonStyle(Button button, bool selected)
		{
			if (selected)
			{
				button.BackColor = Color.FromArgb(24, 61, 135);
				button.FlatAppearance.BorderColor = Color.FromArgb(0, 180, 255);
				button.ForeColor = Color.White;
			}
			else
			{
				button.BackColor = Color.FromArgb(8, 22, 39);
				button.FlatAppearance.BorderColor = Color.FromArgb(33, 63, 88);
				button.ForeColor = Color.FromArgb(220, 230, 240);
			}
		}

		#endregion

		#region Page Switch

		private void ShowTriggerManager()
		{
			_isTaskSchedulerMode = false;
			SetLeftMenuSelected(btnTriggerManager);

			lblMainSectionTitle.Text = "触发源设置";

			panelTaskList.Visible = false;

			dgvConfig.Rows.Clear();
			dgvConfig.Columns.Clear();

			dgvConfig.Columns.Add("TaskName", "task名称");
			dgvConfig.Columns.Add("TriggerName", "触发源名称");
			dgvConfig.Columns.Add("InputAddress", "输入地址");
			dgvConfig.Columns.Add("FlagBit", "标志位");
			dgvConfig.Columns.Add("FlagValue", "标志位值");
			dgvConfig.Columns.Add("Remark", "备注");

			ApplyGridColumnStyle();

			btnAdd.Text = "＋  新增 task";
			btnDelete.Text = "▥  删除选中";
			btnMoveUp.Visible = false;
			btnMoveDown.Visible = false;

			AdjustContentLayout();
			UpdateEmptyLabel();
		}

		private void ShowTaskScheduler()
		{
			_isTaskSchedulerMode = true;
			SetLeftMenuSelected(btnTaskScheduler);

			lblMainSectionTitle.Text = "当前 task 中的 step";

			panelTaskList.Visible = true;

			dgvConfig.Rows.Clear();
			dgvConfig.Columns.Clear();

			dgvConfig.Columns.Add("Step", "step");
			dgvConfig.Columns.Add("ImageSource", "图像源");
			dgvConfig.Columns.Add("RunOrder", "执行步序");
			dgvConfig.Columns.Add("Remark", "备注");

			ApplyGridColumnStyle();

			btnAdd.Text = "＋  新增算子";
			btnDelete.Text = "▥  删除选中";
			btnMoveUp.Visible = true;
			btnMoveDown.Visible = true;

			AdjustContentLayout();
			UpdateEmptyLabel();
		}

		private void ApplyGridColumnStyle()
		{
			for (int i = 0; i < dgvConfig.Columns.Count; i++)
			{
				dgvConfig.Columns[i].SortMode = DataGridViewColumnSortMode.NotSortable;
				dgvConfig.Columns[i].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
			}
		}

		#endregion

		#region Layout

		private void AdjustContentLayout()
		{
			if (panelContent == null || panelConfig == null || panelJobList == null)
				return;

			int margin = 22;
			int gap = 14;
			int jobWidth = 250;
			int taskWidth = _isTaskSchedulerMode ? 250 : 0;

			int contentW = Math.Max(300, panelContent.ClientSize.Width);
			int contentH = Math.Max(300, panelContent.ClientSize.Height);

			panelJobList.Left = margin;
			panelJobList.Top = margin;
			panelJobList.Width = jobWidth;
			panelJobList.Height = contentH - margin * 2;

			panelTaskList.Left = panelJobList.Right + gap;
			panelTaskList.Top = margin;
			panelTaskList.Width = taskWidth;
			panelTaskList.Height = contentH - margin * 2;

			int configLeft = _isTaskSchedulerMode
				? panelTaskList.Right + gap
				: panelJobList.Right + gap;

			panelConfig.Left = configLeft;
			panelConfig.Top = margin;
			panelConfig.Width = contentW - configLeft - margin;
			panelConfig.Height = contentH - margin * 2;

			int innerMargin = 18;
			lblMainSectionTitle.Left = innerMargin;
			lblMainSectionTitle.Top = 18;
			lblMainSectionTitle.Width = panelConfig.ClientSize.Width - innerMargin * 2;
			lblMainSectionTitle.Height = 28;

			panelTableBody.Left = innerMargin;
			panelTableBody.Top = 58;
			panelTableBody.Width = panelConfig.ClientSize.Width - innerMargin * 2;
			panelTableBody.Height = panelConfig.ClientSize.Height - 58 - 90;

			panelAction.Left = innerMargin;
			panelAction.Top = panelConfig.ClientSize.Height - 76;
			panelAction.Width = panelConfig.ClientSize.Width - innerMargin * 2;
			panelAction.Height = 60;

			LayoutActionButtons();
			UpdateEmptyLabel();
		}

		private void LayoutActionButtons()
		{
			int x = 0;
			int y = 8;
			int w = 135;
			int h = 40;
			int gap = 14;

			btnAdd.Left = x;
			btnAdd.Top = y;
			btnAdd.Width = w;
			btnAdd.Height = h;

			x += w + gap;
			btnDelete.Left = x;
			btnDelete.Top = y;
			btnDelete.Width = w;
			btnDelete.Height = h;

			if (_isTaskSchedulerMode)
			{
				x += w + gap;
				btnMoveUp.Left = x;
				btnMoveUp.Top = y;
				btnMoveUp.Width = w;
				btnMoveUp.Height = h;

				x += w + gap;
				btnMoveDown.Left = x;
				btnMoveDown.Top = y;
				btnMoveDown.Width = w;
				btnMoveDown.Height = h;
			}

			btnSave.Width = 115;
			btnSave.Height = h;
			btnSave.Left = Math.Max(x + w + gap, panelAction.ClientSize.Width - btnSave.Width);
			btnSave.Top = y;
		}

		private void UpdateEmptyLabel()
		{
			if (lblEmpty == null || panelTableBody == null)
				return;

			lblEmpty.Visible = dgvConfig.Rows.Count == 0;
			lblEmpty.Left = Math.Max(0, panelTableBody.ClientSize.Width / 2 - lblEmpty.Width / 2);
			lblEmpty.Top = Math.Max(0, panelTableBody.ClientSize.Height / 2 - lblEmpty.Height / 2);
			lblEmpty.BringToFront();
		}

		#endregion

		#region Demo Data

		private void LoadJobList()
		{
			listJobs.Items.Clear();
			listJobs.Items.Add("Job_001");
			listJobs.Items.Add("Job_002");
			listJobs.Items.Add("Job_003");
			listJobs.Items.Add("Job_004");
			listJobs.Items.Add("Job_005");
			listJobs.Items.Add("Job_006");
			listJobs.Items.Add("Job_007");
			listJobs.Items.Add("Job_008");
			listJobs.Items.Add("Job_009");
			listJobs.Items.Add("Job_010");

			if (listJobs.Items.Count > 0)
				listJobs.SelectedIndex = 0;

			listTasks.Items.Clear();
			listTasks.Items.Add("Task_Main");
			listTasks.Items.Add("Task_Inspect");
			listTasks.Items.Add("Task_Locate");
			listTasks.Items.Add("Task_Measure");
			listTasks.Items.Add("Task_OCR");
			listTasks.Items.Add("Task_Align");
			listTasks.Items.Add("Task_Classify");

			if (listTasks.Items.Count > 0)
				listTasks.SelectedIndex = 0;
		}

		#endregion

		#region Buttons

		private void btnAdd_Click(object sender, EventArgs e)
		{
			if (_isTaskSchedulerMode)
			{
				dgvConfig.Rows.Add("Step01", "Cam1", "1", "");
			}
			else
			{
				dgvConfig.Rows.Add("Task_Main", "Trigger0", "Input[0]", "0", "1", "");
			}

			UpdateEmptyLabel();
		}

		private void btnDelete_Click(object sender, EventArgs e)
		{
			foreach (DataGridViewRow row in dgvConfig.SelectedRows)
			{
				if (!row.IsNewRow)
					dgvConfig.Rows.Remove(row);
			}

			UpdateEmptyLabel();
		}

		private void btnMoveUp_Click(object sender, EventArgs e)
		{
			MoveSelectedRow(-1);
		}

		private void btnMoveDown_Click(object sender, EventArgs e)
		{
			MoveSelectedRow(1);
		}

		private void MoveSelectedRow(int direction)
		{
			if (dgvConfig.SelectedRows.Count <= 0)
				return;

			DataGridViewRow selectedRow = dgvConfig.SelectedRows[0];
			int oldIndex = selectedRow.Index;
			int newIndex = oldIndex + direction;

			if (newIndex < 0 || newIndex >= dgvConfig.Rows.Count)
				return;

			dgvConfig.Rows.Remove(selectedRow);
			dgvConfig.Rows.Insert(newIndex, selectedRow);
			dgvConfig.ClearSelection();
			selectedRow.Selected = true;
		}

		private void btnSave_Click(object sender, EventArgs e)
		{
			MessageBox.Show("Configuration saved.", "Save", MessageBoxButtons.OK, MessageBoxIcon.Information);
		}

		#endregion
	}
}
