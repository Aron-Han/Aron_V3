namespace Aron_V3
{
	partial class FlowConfigForm
	{
		private System.ComponentModel.IContainer components = null;
		private System.Windows.Forms.TableLayoutPanel rootLayout;
		private System.Windows.Forms.Panel panelSide;
		private System.Windows.Forms.Panel panelSideContentGap;
		private System.Windows.Forms.Panel panelContent;
		private System.Windows.Forms.Panel panelButtonGap;
		private System.Windows.Forms.Button btnTriggerManager;
		private System.Windows.Forms.Button btnTaskScheduler;

		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null)) components.Dispose();
			base.Dispose(disposing);
		}

		private void InitializeComponent()
		{
			this.rootLayout = new System.Windows.Forms.TableLayoutPanel();
			this.panelSide = new System.Windows.Forms.Panel();
			this.panelSideContentGap = new System.Windows.Forms.Panel();
			this.panelContent = new System.Windows.Forms.Panel();
			this.panelButtonGap = new System.Windows.Forms.Panel();
			this.btnTriggerManager = new System.Windows.Forms.Button();
			this.btnTaskScheduler = new System.Windows.Forms.Button();
			this.rootLayout.SuspendLayout();
			this.panelSide.SuspendLayout();
			this.SuspendLayout();
			// 
			// rootLayout
			// 
			this.rootLayout.BackColor = System.Drawing.Color.FromArgb(2, 10, 20);
			this.rootLayout.ColumnCount = 3;
			this.rootLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 230F));
			this.rootLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 16F));
			this.rootLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.rootLayout.Controls.Add(this.panelSide, 0, 0);
			this.rootLayout.Controls.Add(this.panelSideContentGap, 1, 0);
			this.rootLayout.Controls.Add(this.panelContent, 2, 0);
			this.rootLayout.Dock = System.Windows.Forms.DockStyle.Fill;
			this.rootLayout.Margin = new System.Windows.Forms.Padding(0);
			this.rootLayout.Padding = new System.Windows.Forms.Padding(8, 10, 10, 10);
			this.rootLayout.RowCount = 1;
			this.rootLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
			// 
			// panelSide
			// 
			this.panelSide.BackColor = System.Drawing.Color.FromArgb(3, 14, 27);
			this.panelSide.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.panelSide.Controls.Add(this.btnTaskScheduler);
			this.panelSide.Controls.Add(this.panelButtonGap);
			this.panelSide.Controls.Add(this.btnTriggerManager);
			this.panelSide.Dock = System.Windows.Forms.DockStyle.Fill;
			this.panelSide.Margin = new System.Windows.Forms.Padding(0);
			this.panelSide.Padding = new System.Windows.Forms.Padding(10, 14, 10, 10);
			// 
			// panelSideContentGap
			// 
			this.panelSideContentGap.BackColor = System.Drawing.Color.FromArgb(2, 10, 20);
			this.panelSideContentGap.Dock = System.Windows.Forms.DockStyle.Fill;
			this.panelSideContentGap.Margin = new System.Windows.Forms.Padding(0);
			// 
			// btnTriggerManager
			// 
			this.btnTriggerManager.BackColor = System.Drawing.Color.FromArgb(8, 21, 39);
			this.btnTriggerManager.Dock = System.Windows.Forms.DockStyle.Top;
			this.btnTriggerManager.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(35, 65, 95);
			this.btnTriggerManager.FlatAppearance.MouseDownBackColor = System.Drawing.Color.FromArgb(20, 70, 135);
			this.btnTriggerManager.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(15, 45, 78);
			this.btnTriggerManager.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.btnTriggerManager.Font = new System.Drawing.Font("Microsoft YaHei UI", 10F, System.Drawing.FontStyle.Bold);
			this.btnTriggerManager.ForeColor = System.Drawing.Color.FromArgb(210, 220, 235);
			this.btnTriggerManager.Height = 58;
			this.btnTriggerManager.Margin = new System.Windows.Forms.Padding(0);
			this.btnTriggerManager.Padding = new System.Windows.Forms.Padding(0);
			this.btnTriggerManager.Text = "⚡  触发管理";
			this.btnTriggerManager.UseVisualStyleBackColor = false;
			this.btnTriggerManager.Click += new System.EventHandler(this.btnTriggerManager_Click);
			// 
			// panelButtonGap
			// 
			this.panelButtonGap.BackColor = System.Drawing.Color.FromArgb(3, 14, 27);
			this.panelButtonGap.Dock = System.Windows.Forms.DockStyle.Top;
			this.panelButtonGap.Height = 12;
			this.panelButtonGap.Margin = new System.Windows.Forms.Padding(0);
			// 
			// btnTaskScheduler
			// 
			this.btnTaskScheduler.BackColor = System.Drawing.Color.FromArgb(8, 21, 39);
			this.btnTaskScheduler.Dock = System.Windows.Forms.DockStyle.Top;
			this.btnTaskScheduler.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(35, 65, 95);
			this.btnTaskScheduler.FlatAppearance.MouseDownBackColor = System.Drawing.Color.FromArgb(20, 70, 135);
			this.btnTaskScheduler.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(15, 45, 78);
			this.btnTaskScheduler.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.btnTaskScheduler.Font = new System.Drawing.Font("Microsoft YaHei UI", 10F, System.Drawing.FontStyle.Bold);
			this.btnTaskScheduler.ForeColor = System.Drawing.Color.FromArgb(210, 220, 235);
			this.btnTaskScheduler.Height = 58;
			this.btnTaskScheduler.Margin = new System.Windows.Forms.Padding(0);
			this.btnTaskScheduler.Padding = new System.Windows.Forms.Padding(0);
			this.btnTaskScheduler.Text = "▣  任务调度";
			this.btnTaskScheduler.UseVisualStyleBackColor = false;
			this.btnTaskScheduler.Click += new System.EventHandler(this.btnTaskScheduler_Click);
			// 
			// panelContent
			// 
			this.panelContent.BackColor = System.Drawing.Color.FromArgb(2, 10, 20);
			this.panelContent.Dock = System.Windows.Forms.DockStyle.Fill;
			this.panelContent.Margin = new System.Windows.Forms.Padding(0);
			this.panelContent.Padding = new System.Windows.Forms.Padding(14, 0, 0, 0);
			// 
			// FlowConfigForm
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.BackColor = System.Drawing.Color.FromArgb(2, 10, 20);
			this.ClientSize = new System.Drawing.Size(1400, 780);
			this.Controls.Add(this.rootLayout);
			this.Font = new System.Drawing.Font("Microsoft YaHei UI", 9F);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
			this.Name = "FlowConfigForm";
			this.Text = "FlowConfigForm";
			this.rootLayout.ResumeLayout(false);
			this.panelSide.ResumeLayout(false);
			this.ResumeLayout(false);
		}
	}
}
