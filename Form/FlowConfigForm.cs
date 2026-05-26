using System;
using System.Drawing;
using System.Reflection;
using System.Windows.Forms;

namespace Aron_V3
{
	public partial class FlowConfigForm : Form, ILocalizable
	{
		private TriggerManagerControl _triggerPage;
		private TaskSchedulerControl _taskPage;
		private Func<string, string, TaskRunOptions, bool> _taskTestExecutor;

		public Func<string, string, TaskRunOptions, bool> TaskTestExecutor
		{
			get { return _taskTestExecutor; }
			set
			{
				_taskTestExecutor = value;

				if (_triggerPage != null && !_triggerPage.IsDisposed)
				{
					_triggerPage.TaskTestExecutor = value;
				}
			}
		}

		public FlowConfigForm()
		{
			InitializeComponent();

			ApplyFlowConfigLayoutSpacing();
			EnableDoubleBuffer(this);

			this.Load += FlowConfigForm_Load;
		}

		private void FlowConfigForm_Load(object sender, EventArgs e)
		{
			ShowTriggerManager();
		}

		private void btnTriggerManager_Click(object sender, EventArgs e)
		{
			ShowTriggerManager();
		}

		private void btnTaskScheduler_Click(object sender, EventArgs e)
		{
			ShowTaskScheduler();
		}

		private void ShowTriggerManager()
		{
			if (_triggerPage == null || _triggerPage.IsDisposed)
			{
				_triggerPage = new TriggerManagerControl();
				_triggerPage.TaskTestExecutor = _taskTestExecutor;
			}

			ShowPage(_triggerPage);
			SetSideButtonSelected(btnTriggerManager);
		}

		private void ShowTaskScheduler()
		{
			if (_taskPage == null || _taskPage.IsDisposed)
			{
				_taskPage = new TaskSchedulerControl();
			}

			ShowPage(_taskPage);
			SetSideButtonSelected(btnTaskScheduler);
		}

		private void ShowPage(Control page)
		{
			if (page == null)
			{
				return;
			}

			panelContent.SuspendLayout();

			try
			{
				foreach (Control c in panelContent.Controls)
				{
					c.Visible = false;
				}

				if (page.Parent != panelContent)
				{
					page.Dock = DockStyle.Fill;
					page.Margin = new Padding(0);
					panelContent.Controls.Add(page);
					EnableDoubleBuffer(page);
				}

				page.Visible = true;
				page.BringToFront();
			}
			finally
			{
				panelContent.ResumeLayout(true);
			}
		}

		private void SetSideButtonSelected(Button selected)
		{
			ResetSideButton(btnTriggerManager);
			ResetSideButton(btnTaskScheduler);

			if (selected == null)
			{
				return;
			}

			selected.BackColor = Color.FromArgb(20, 70, 135);
			selected.ForeColor = Color.White;
			selected.FlatAppearance.BorderColor = Color.FromArgb(0, 185, 255);
		}

		private void ResetSideButton(Button btn)
		{
			if (btn == null)
			{
				return;
			}

			btn.BackColor = Color.FromArgb(8, 21, 39);
			btn.ForeColor = Color.FromArgb(210, 220, 235);
			btn.FlatAppearance.BorderColor = Color.FromArgb(35, 65, 95);
		}

		private void EnableDoubleBuffer(Control control)
		{
			if (control == null)
			{
				return;
			}

			try
			{
				PropertyInfo p = typeof(Control).GetProperty(
					"DoubleBuffered",
					BindingFlags.Instance | BindingFlags.NonPublic);

				if (p != null)
				{
					p.SetValue(control, true, null);
				}
			}
			catch
			{
			}

			foreach (Control child in control.Controls)
			{
				EnableDoubleBuffer(child);
			}
		}

		public void ApplyLanguage(bool isEnglish)
		{
			if (isEnglish)
			{
				btnTriggerManager.Text = "⚡  Trigger";
				btnTaskScheduler.Text = "▣  Scheduler";
			}
			else
			{
				btnTriggerManager.Text = "⚡  触发管理";
				btnTaskScheduler.Text = "▣  任务调度";
			}

			foreach (Control ctrl in panelContent.Controls)
			{
				ILocalizable localizable = ctrl as ILocalizable;

				if (localizable != null)
				{
					localizable.ApplyLanguage(isEnglish);
				}
			}
		}

		private void ApplyFlowConfigLayoutSpacing()
		{
			// 这里不再动态重建 ColumnStyles，避免覆盖 Designer.cs 中的三列布局。
			// Designer.cs 已经改为：
			// 左侧菜单 230px + 中间间隔 16px + 右侧内容 Percent 100%。

			this.BackColor = Color.FromArgb(2, 10, 20);

			rootLayout.BackColor = Color.FromArgb(2, 10, 20);
			rootLayout.Padding = new Padding(8, 10, 10, 10);
			rootLayout.CellBorderStyle = TableLayoutPanelCellBorderStyle.None;

			panelSide.BackColor = Color.FromArgb(3, 14, 27);
			panelSide.Padding = new Padding(10, 14, 10, 10);
			panelSide.Margin = new Padding(0);
			panelSide.BorderStyle = BorderStyle.FixedSingle;

			panelButtonGap.BackColor = Color.FromArgb(3, 14, 27);
			panelButtonGap.Height = 12;
			panelButtonGap.Dock = DockStyle.Top;
			panelButtonGap.Margin = new Padding(0);

			panelContent.BackColor = Color.FromArgb(2, 10, 20);
			panelContent.Padding = new Padding(14, 0, 0, 0);
			panelContent.Margin = new Padding(0);
			panelContent.BorderStyle = BorderStyle.None;

			ApplySideButtonBaseStyle(btnTriggerManager);
			ApplySideButtonBaseStyle(btnTaskScheduler);
		}

		private void ApplySideButtonBaseStyle(Button btn)
		{
			if (btn == null)
			{
				return;
			}

			btn.Dock = DockStyle.Top;
			btn.Height = 58;
			btn.Margin = new Padding(0);
			btn.Padding = new Padding(0);
			btn.FlatStyle = FlatStyle.Flat;
			btn.FlatAppearance.BorderSize = 1;
			btn.FlatAppearance.BorderColor = Color.FromArgb(35, 65, 95);
			btn.FlatAppearance.MouseDownBackColor = Color.FromArgb(20, 70, 135);
			btn.FlatAppearance.MouseOverBackColor = Color.FromArgb(15, 45, 78);
			btn.BackColor = Color.FromArgb(8, 21, 39);
			btn.ForeColor = Color.FromArgb(210, 220, 235);
			btn.Font = new Font("Microsoft YaHei UI", 10F, FontStyle.Bold);
			btn.TextAlign = ContentAlignment.MiddleCenter;
			btn.UseVisualStyleBackColor = false;
		}
	}
}
