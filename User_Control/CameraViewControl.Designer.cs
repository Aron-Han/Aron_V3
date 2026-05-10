namespace Aron_V3
{
	partial class CameraViewControl
	{
		private System.ComponentModel.IContainer components = null;

		private System.Windows.Forms.TableLayoutPanel rootLayout;
		private System.Windows.Forms.Panel panelHeader;
		private System.Windows.Forms.Label lblTitle;
		private System.Windows.Forms.Label lblStatus;
		private System.Windows.Forms.Panel panelImage;
		private System.Windows.Forms.Label lblImageText;
		private System.Windows.Forms.TableLayoutPanel footerLayout;
		private System.Windows.Forms.TableLayoutPanel statsLayout;
		private System.Windows.Forms.Label lblTotal;
		private System.Windows.Forms.Label lblPass;
		private System.Windows.Forms.Label lblPassRate;
		private System.Windows.Forms.TableLayoutPanel buttonLayout;
		private System.Windows.Forms.Button btnReset;
		private System.Windows.Forms.Button btnTrigger;
		private System.Windows.Forms.Button btnReplay;
		private System.Windows.Forms.TableLayoutPanel infoLayout;
		private System.Windows.Forms.Label lblJob;
		private System.Windows.Forms.Label lblPos;
		private System.Windows.Forms.Label lblCam;

		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
				components.Dispose();

			base.Dispose(disposing);
		}

		private void InitializeComponent()
		{
			this.rootLayout = new System.Windows.Forms.TableLayoutPanel();
			this.panelHeader = new System.Windows.Forms.Panel();
			this.lblTitle = new System.Windows.Forms.Label();
			this.lblStatus = new System.Windows.Forms.Label();
			this.panelImage = new System.Windows.Forms.Panel();
			this.lblImageText = new System.Windows.Forms.Label();
			this.footerLayout = new System.Windows.Forms.TableLayoutPanel();
			this.statsLayout = new System.Windows.Forms.TableLayoutPanel();
			this.lblTotal = new System.Windows.Forms.Label();
			this.lblPass = new System.Windows.Forms.Label();
			this.lblPassRate = new System.Windows.Forms.Label();
			this.buttonLayout = new System.Windows.Forms.TableLayoutPanel();
			this.btnReset = new System.Windows.Forms.Button();
			this.btnTrigger = new System.Windows.Forms.Button();
			this.btnReplay = new System.Windows.Forms.Button();
			this.infoLayout = new System.Windows.Forms.TableLayoutPanel();
			this.lblJob = new System.Windows.Forms.Label();
			this.lblPos = new System.Windows.Forms.Label();
			this.lblCam = new System.Windows.Forms.Label();
			this.SuspendLayout();

			// CameraViewControl
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.BackColor = System.Drawing.Color.FromArgb(10, 24, 42);
			this.Margin = new System.Windows.Forms.Padding(0, 0, 8, 8);
			this.Name = "CameraViewControl";
			this.Size = new System.Drawing.Size(460, 220);

			// rootLayout
			this.rootLayout.BackColor = System.Drawing.Color.FromArgb(10, 24, 42);
			this.rootLayout.ColumnCount = 1;
			this.rootLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.rootLayout.Dock = System.Windows.Forms.DockStyle.Fill;
			this.rootLayout.Margin = new System.Windows.Forms.Padding(0);
			this.rootLayout.Name = "rootLayout";
			this.rootLayout.RowCount = 3;
			this.rootLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
			this.rootLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));

			// 关键修复：
			// 原来 footer 是 78，但 footerLayout 自身 Padding(上4+下6) + 子行高度会超过可用高度，导致按钮文字被挤压，看起来不居中。
			// 这里把 footer 高度加到 84，给按钮和文字足够垂直空间。
			this.rootLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 84F));

			// panelHeader
			this.panelHeader.BackColor = System.Drawing.Color.FromArgb(12, 29, 50);
			this.panelHeader.Dock = System.Windows.Forms.DockStyle.Fill;
			this.panelHeader.Margin = new System.Windows.Forms.Padding(0);
			this.panelHeader.Name = "panelHeader";

			// lblTitle
			this.lblTitle.Dock = System.Windows.Forms.DockStyle.Fill;
			this.lblTitle.Font = new System.Drawing.Font("Microsoft YaHei UI", 9.5F, System.Drawing.FontStyle.Bold);
			this.lblTitle.ForeColor = System.Drawing.Color.WhiteSmoke;
			this.lblTitle.Padding = new System.Windows.Forms.Padding(10, 0, 0, 0);
			this.lblTitle.Text = "相机01 - 读码";
			this.lblTitle.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;

			// lblStatus
			this.lblStatus.Dock = System.Windows.Forms.DockStyle.Right;
			this.lblStatus.Font = new System.Drawing.Font("Microsoft YaHei UI", 11.5F, System.Drawing.FontStyle.Bold);
			this.lblStatus.ForeColor = System.Drawing.Color.FromArgb(65, 210, 70);
			this.lblStatus.Size = new System.Drawing.Size(72, 30);
			this.lblStatus.Text = "OK";
			this.lblStatus.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;

			this.panelHeader.Controls.Add(this.lblTitle);
			this.panelHeader.Controls.Add(this.lblStatus);

			// panelImage
			this.panelImage.BackColor = System.Drawing.Color.Black;
			this.panelImage.Dock = System.Windows.Forms.DockStyle.Fill;
			this.panelImage.Margin = new System.Windows.Forms.Padding(0);
			this.panelImage.Name = "panelImage";
			this.panelImage.Padding = new System.Windows.Forms.Padding(10);

			// lblImageText
			this.lblImageText.Dock = System.Windows.Forms.DockStyle.Fill;
			this.lblImageText.Font = new System.Drawing.Font("Microsoft YaHei UI", 11F, System.Drawing.FontStyle.Bold);
			this.lblImageText.ForeColor = System.Drawing.Color.FromArgb(95, 110, 125);
			this.lblImageText.Text = "";
			this.lblImageText.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			this.lblImageText.Visible = false;

			this.panelImage.Controls.Add(this.lblImageText);

			// footerLayout
			this.footerLayout.BackColor = System.Drawing.Color.FromArgb(7, 18, 32);
			this.footerLayout.ColumnCount = 1;
			this.footerLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.footerLayout.Dock = System.Windows.Forms.DockStyle.Fill;
			this.footerLayout.Margin = new System.Windows.Forms.Padding(0);
			this.footerLayout.Name = "footerLayout";

			// 关键修复：
			// Padding 不要太大，否则按钮区域垂直空间不足。
			this.footerLayout.Padding = new System.Windows.Forms.Padding(8, 3, 8, 6);

			this.footerLayout.RowCount = 3;
			this.footerLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 23F));
			this.footerLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 28F));
			this.footerLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 24F));

			// statsLayout
			this.statsLayout.ColumnCount = 3;
			this.statsLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 28F));
			this.statsLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 28F));
			this.statsLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 44F));
			this.statsLayout.Dock = System.Windows.Forms.DockStyle.Fill;
			this.statsLayout.Margin = new System.Windows.Forms.Padding(0);
			this.statsLayout.Padding = new System.Windows.Forms.Padding(0);
			this.statsLayout.Name = "statsLayout";
			this.statsLayout.RowCount = 1;
			this.statsLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));

			this.lblTotal.Dock = System.Windows.Forms.DockStyle.Fill;
			this.lblTotal.Font = new System.Drawing.Font("Consolas", 8.5F);
			this.lblTotal.ForeColor = System.Drawing.Color.FromArgb(220, 230, 240);
			this.lblTotal.Margin = new System.Windows.Forms.Padding(0);
			this.lblTotal.Padding = new System.Windows.Forms.Padding(0);
			this.lblTotal.Text = "Total: 0";
			this.lblTotal.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;

			this.lblPass.Dock = System.Windows.Forms.DockStyle.Fill;
			this.lblPass.Font = new System.Drawing.Font("Consolas", 8.5F);
			this.lblPass.ForeColor = System.Drawing.Color.FromArgb(220, 230, 240);
			this.lblPass.Margin = new System.Windows.Forms.Padding(0);
			this.lblPass.Padding = new System.Windows.Forms.Padding(0);
			this.lblPass.Text = "Pass: 0";
			this.lblPass.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;

			this.lblPassRate.Dock = System.Windows.Forms.DockStyle.Fill;
			this.lblPassRate.Font = new System.Drawing.Font("Consolas", 8.5F);
			this.lblPassRate.ForeColor = System.Drawing.Color.FromArgb(220, 230, 240);
			this.lblPassRate.Margin = new System.Windows.Forms.Padding(0);
			this.lblPassRate.Padding = new System.Windows.Forms.Padding(0);
			this.lblPassRate.Text = "PassRate: 0.00%";
			this.lblPassRate.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;

			this.statsLayout.Controls.Add(this.lblTotal, 0, 0);
			this.statsLayout.Controls.Add(this.lblPass, 1, 0);
			this.statsLayout.Controls.Add(this.lblPassRate, 2, 0);

			// buttonLayout
			this.buttonLayout.ColumnCount = 3;
			this.buttonLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33.333F));
			this.buttonLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33.333F));
			this.buttonLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33.333F));
			this.buttonLayout.Dock = System.Windows.Forms.DockStyle.Fill;
			this.buttonLayout.Margin = new System.Windows.Forms.Padding(0);
			this.buttonLayout.Padding = new System.Windows.Forms.Padding(0);
			this.buttonLayout.Name = "buttonLayout";
			this.buttonLayout.RowCount = 1;
			this.buttonLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));

			// btnReset
			this.btnReset.AutoSize = false;
			this.btnReset.BackColor = System.Drawing.Color.FromArgb(8, 24, 38);
			this.btnReset.Dock = System.Windows.Forms.DockStyle.Fill;
			this.btnReset.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.btnReset.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(45, 75, 100);
			this.btnReset.Font = new System.Drawing.Font("Microsoft YaHei UI", 8.5F, System.Drawing.FontStyle.Regular);
			this.btnReset.ForeColor = System.Drawing.Color.WhiteSmoke;
			this.btnReset.Margin = new System.Windows.Forms.Padding(0, 1, 4, 1);
			this.btnReset.Padding = new System.Windows.Forms.Padding(0);
			this.btnReset.Text = "Reset Count";
			this.btnReset.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			this.btnReset.TextImageRelation = System.Windows.Forms.TextImageRelation.Overlay;
			this.btnReset.UseVisualStyleBackColor = false;
			this.btnReset.Click += new System.EventHandler(this.btnReset_Click);

			// btnTrigger
			this.btnTrigger.AutoSize = false;
			this.btnTrigger.BackColor = System.Drawing.Color.FromArgb(8, 24, 38);
			this.btnTrigger.Dock = System.Windows.Forms.DockStyle.Fill;
			this.btnTrigger.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.btnTrigger.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(45, 75, 100);
			this.btnTrigger.Font = new System.Drawing.Font("Microsoft YaHei UI", 8.5F, System.Drawing.FontStyle.Regular);
			this.btnTrigger.ForeColor = System.Drawing.Color.WhiteSmoke;
			this.btnTrigger.Margin = new System.Windows.Forms.Padding(0, 1, 4, 1);
			this.btnTrigger.Padding = new System.Windows.Forms.Padding(0);
			this.btnTrigger.Text = "Trigger Manual";
			this.btnTrigger.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			this.btnTrigger.TextImageRelation = System.Windows.Forms.TextImageRelation.Overlay;
			this.btnTrigger.UseVisualStyleBackColor = false;
			this.btnTrigger.Click += new System.EventHandler(this.btnTrigger_Click);

			// btnReplay
			this.btnReplay.AutoSize = false;
			this.btnReplay.BackColor = System.Drawing.Color.FromArgb(8, 24, 38);
			this.btnReplay.Dock = System.Windows.Forms.DockStyle.Fill;
			this.btnReplay.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.btnReplay.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(45, 75, 100);
			this.btnReplay.Font = new System.Drawing.Font("Microsoft YaHei UI", 8.5F, System.Drawing.FontStyle.Regular);
			this.btnReplay.ForeColor = System.Drawing.Color.WhiteSmoke;
			this.btnReplay.Margin = new System.Windows.Forms.Padding(0, 1, 0, 1);
			this.btnReplay.Padding = new System.Windows.Forms.Padding(0);
			this.btnReplay.Text = "Replay";
			this.btnReplay.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			this.btnReplay.TextImageRelation = System.Windows.Forms.TextImageRelation.Overlay;
			this.btnReplay.UseVisualStyleBackColor = false;
			this.btnReplay.Click += new System.EventHandler(this.btnReplay_Click);

			this.buttonLayout.Controls.Add(this.btnReset, 0, 0);
			this.buttonLayout.Controls.Add(this.btnTrigger, 1, 0);
			this.buttonLayout.Controls.Add(this.btnReplay, 2, 0);

			// infoLayout
			this.infoLayout.ColumnCount = 3;
			this.infoLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33.333F));
			this.infoLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33.333F));
			this.infoLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33.333F));
			this.infoLayout.Dock = System.Windows.Forms.DockStyle.Fill;
			this.infoLayout.Margin = new System.Windows.Forms.Padding(0);
			this.infoLayout.Padding = new System.Windows.Forms.Padding(0);
			this.infoLayout.Name = "infoLayout";
			this.infoLayout.RowCount = 1;
			this.infoLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));

			this.lblJob.BackColor = System.Drawing.Color.FromArgb(8, 38, 25);
			this.lblJob.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.lblJob.Dock = System.Windows.Forms.DockStyle.Fill;
			this.lblJob.Font = new System.Drawing.Font("Microsoft YaHei UI", 8F);
			this.lblJob.ForeColor = System.Drawing.Color.FromArgb(80, 220, 180);
			this.lblJob.Margin = new System.Windows.Forms.Padding(0, 2, 4, 0);
			this.lblJob.Text = "Job1";
			this.lblJob.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;

			this.lblPos.BackColor = System.Drawing.Color.FromArgb(8, 38, 25);
			this.lblPos.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.lblPos.Dock = System.Windows.Forms.DockStyle.Fill;
			this.lblPos.Font = new System.Drawing.Font("Microsoft YaHei UI", 8F);
			this.lblPos.ForeColor = System.Drawing.Color.FromArgb(80, 220, 180);
			this.lblPos.Margin = new System.Windows.Forms.Padding(0, 2, 4, 0);
			this.lblPos.Text = "Pos1";
			this.lblPos.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;

			this.lblCam.BackColor = System.Drawing.Color.FromArgb(8, 38, 25);
			this.lblCam.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.lblCam.Dock = System.Windows.Forms.DockStyle.Fill;
			this.lblCam.Font = new System.Drawing.Font("Microsoft YaHei UI", 8F);
			this.lblCam.ForeColor = System.Drawing.Color.FromArgb(80, 220, 180);
			this.lblCam.Margin = new System.Windows.Forms.Padding(0, 2, 0, 0);
			this.lblCam.Text = "Cam1";
			this.lblCam.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;

			this.infoLayout.Controls.Add(this.lblJob, 0, 0);
			this.infoLayout.Controls.Add(this.lblPos, 1, 0);
			this.infoLayout.Controls.Add(this.lblCam, 2, 0);

			this.footerLayout.Controls.Add(this.statsLayout, 0, 0);
			this.footerLayout.Controls.Add(this.buttonLayout, 0, 1);
			this.footerLayout.Controls.Add(this.infoLayout, 0, 2);

			this.rootLayout.Controls.Add(this.panelHeader, 0, 0);
			this.rootLayout.Controls.Add(this.panelImage, 0, 1);
			this.rootLayout.Controls.Add(this.footerLayout, 0, 2);

			this.Controls.Add(this.rootLayout);
			this.ResumeLayout(false);
		}
	}
}
