using System;
using System.Drawing;
using System.Windows.Forms;

namespace Aron_V3
{
	public partial class CameraViewControl : UserControl
	{
		private readonly Color _okColor = Color.FromArgb(65, 210, 70);
		private readonly Color _ngColor = Color.FromArgb(235, 54, 65);
		private bool _noImage = false;

		public CameraViewControl()
		{
			InitializeComponent();
			SetResult(true);
			SetStatistics(0, 0);
		}

		public void SetTitle(string title)
		{
			lblTitle.Text = title;
		}

		public void SetDisplayText(string text)
		{
			_noImage = false;

			panelImage.BackColor = Color.Black;
			lblImageText.Text = string.Empty;
			lblImageText.Visible = false;

			btnReset.Enabled = true;
			btnTrigger.Enabled = true;
			btnReplay.Enabled = true;
		}

		public void SetNoImage()
		{
			_noImage = true;

			panelImage.BackColor = Color.FromArgb(4, 10, 18);
			lblImageText.Visible = true;
			lblImageText.Font = new Font("Microsoft YaHei UI", 11F, FontStyle.Bold);
			lblImageText.ForeColor = Color.FromArgb(95, 110, 125);
			lblImageText.TextAlign = ContentAlignment.MiddleCenter;
			lblImageText.Text = "▣\r\n暂无图像";

			lblStatus.Text = "OK";
			lblStatus.ForeColor = _okColor;

			btnReset.Enabled = false;
			btnTrigger.Enabled = false;
			btnReplay.Enabled = false;
		}

		public void SetResult(bool ok)
		{
			lblStatus.Text = ok ? "OK" : "NG";
			lblStatus.ForeColor = ok ? _okColor : _ngColor;
		}

		public void SetStatistics(int total, int pass)
		{
			double passRate = total <= 0 ? 0 : pass * 100.0 / total;

			lblTotal.Text = "Total: " + total;
			lblPass.Text = "Pass: " + pass;
			lblPassRate.Text = "PassRate: " + passRate.ToString("0.00") + "%";
		}

		public void SetInfo(string job, string pos, string cam)
		{
			lblJob.Text = job;
			lblPos.Text = pos;
			lblCam.Text = cam;
		}

		public Control ImageHost
		{
			get { return panelImage; }
		}

		private void btnReset_Click(object sender, EventArgs e)
		{
			SetStatistics(0, 0);
		}

		private void btnTrigger_Click(object sender, EventArgs e)
		{
			// 后续接单次触发逻辑
		}

		private void btnReplay_Click(object sender, EventArgs e)
		{
			// 后续接图像回放逻辑
		}
	}
}
