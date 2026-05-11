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

		public FlowConfigForm()
		{
			InitializeComponent();
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
				_triggerPage = new TriggerManagerControl();

			ShowPage(_triggerPage);
			SetSideButtonSelected(btnTriggerManager);
		}

		private void ShowTaskScheduler()
		{
			if (_taskPage == null || _taskPage.IsDisposed)
				_taskPage = new TaskSchedulerControl();

			ShowPage(_taskPage);
			SetSideButtonSelected(btnTaskScheduler);
		}

		private void ShowPage(Control page)
		{
			panelContent.SuspendLayout();

			foreach (Control c in panelContent.Controls)
				c.Visible = false;

			if (page.Parent != panelContent)
			{
				page.Dock = DockStyle.Fill;
				panelContent.Controls.Add(page);
				EnableDoubleBuffer(page);
			}

			page.Visible = true;
			page.BringToFront();

			panelContent.ResumeLayout(true);
		}

		private void SetSideButtonSelected(Button selected)
		{
			ResetSideButton(btnTriggerManager);
			ResetSideButton(btnTaskScheduler);

			selected.BackColor = Color.FromArgb(20, 70, 135);
			selected.ForeColor = Color.White;
			selected.FlatAppearance.BorderColor = Color.FromArgb(0, 185, 255);
		}

		private void ResetSideButton(Button btn)
		{
			btn.BackColor = Color.FromArgb(8, 21, 39);
			btn.ForeColor = Color.FromArgb(210, 220, 235);
			btn.FlatAppearance.BorderColor = Color.FromArgb(35, 65, 95);
		}

		private void EnableDoubleBuffer(Control control)
		{
			if (control == null) return;
			try
			{
				PropertyInfo p = typeof(Control).GetProperty("DoubleBuffered", BindingFlags.Instance | BindingFlags.NonPublic);
				if (p != null) p.SetValue(control, true, null);
			}
			catch { }

			foreach (Control child in control.Controls)
				EnableDoubleBuffer(child);
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
	}
}
