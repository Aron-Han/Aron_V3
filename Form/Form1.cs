using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace Aron_V3
{
	public partial class Form1 : Form
	{
		private readonly List<CameraViewControl> _cameraViews = new List<CameraViewControl>();

		public Form1()
		{
			InitializeComponent();
			LoadDemoData();
			// 后续改成从配置读取相机数量即可。
			BuildCameraLayout(9);
			//123123
		}

		private void Form1_Load(object sender, EventArgs e)
		{
			// 启动最大化
			this.WindowState = FormWindowState.Maximized;

			// InitCamera();
			// InitPLC();
			// LoadConfig();
			// LoadVisionJobs();
		}

		#region 中间相机区域：根据相机数量动态生成

		private void BuildCameraLayout(int cameraCount)
		{
			tableLayoutPanelCameras.SuspendLayout();

			tableLayoutPanelCameras.Controls.Clear();
			tableLayoutPanelCameras.RowStyles.Clear();
			tableLayoutPanelCameras.ColumnStyles.Clear();
			_cameraViews.Clear();

			int rows;
			int cols;
			GetCameraGridSize(cameraCount, out rows, out cols);

			tableLayoutPanelCameras.RowCount = rows;
			tableLayoutPanelCameras.ColumnCount = cols;

			for (int r = 0; r < rows; r++)
				tableLayoutPanelCameras.RowStyles.Add(new RowStyle(SizeType.Percent, 100f / rows));

			for (int c = 0; c < cols; c++)
				tableLayoutPanelCameras.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f / cols));

			for (int i = 0; i < cameraCount; i++)
			{
				CameraViewControl camView = CreateCameraView(i + 1);
				_cameraViews.Add(camView);

				int row = i / cols;
				int col = i % cols;

				tableLayoutPanelCameras.Controls.Add(camView, col, row);
			}

			tableLayoutPanelCameras.ResumeLayout();
		}

		private void GetCameraGridSize(int cameraCount, out int rows, out int cols)
		{
			if (cameraCount <= 1)
			{
				rows = 1;
				cols = 1;
			}
			else if (cameraCount == 2)
			{
				rows = 1;
				cols = 2;
			}
			else if (cameraCount <= 4)
			{
				rows = 2;
				cols = 2;
			}
			else if (cameraCount <= 6)
			{
				rows = 2;
				cols = 3;
			}
			else if (cameraCount <= 9)
			{
				rows = 3;
				cols = 3;
			}
			else if (cameraCount <= 12)
			{
				rows = 3;
				cols = 4;
			}
			else
			{
				rows = 4;
				cols = 4;
			}
		}

		private CameraViewControl CreateCameraView(int index)
		{
			CameraViewControl view = new CameraViewControl();
			view.Dock = DockStyle.Fill;
			view.Margin = new Padding(0, 0, 8, 8);

			switch (index)
			{
				case 1:
					view.SetTitle("相机01 - 读码");
					view.SetDisplayText("读码");
					view.SetResult(true);
					view.SetStatistics(32, 6);
					view.SetInfo("Job1", "Pos1", "Cam1");
					break;

				case 2:
					view.SetTitle("相机02 - 定位");
					view.SetDisplayText("定位");
					view.SetResult(true);
					view.SetStatistics(22, 22);
					view.SetInfo("Job1", "Pos2", "Cam1");
					break;

				case 3:
					view.SetTitle("相机03 - 表面检测");
					view.SetDisplayText("表面");
					view.SetResult(false);
					view.SetStatistics(25, 5);
					view.SetInfo("Job1", "Pos1", "Cam2");
					break;

				case 4:
					view.SetTitle("相机04 - 表面检测");
					view.SetDisplayText("表面");
					view.SetResult(false);
					view.SetStatistics(40, 4);
					view.SetInfo("Job1", "Pos1", "Cam2");
					break;

				case 5:
					view.SetTitle("相机05 - 备用视图");
					view.SetNoImage();
					view.SetStatistics(0, 0);
					view.SetInfo("Job--", "Pos--", "Cam--");
					break;

				case 6:
					view.SetTitle("相机06 - 读码");
					view.SetDisplayText("读码");
					view.SetResult(true);
					view.SetStatistics(35, 32);
					view.SetInfo("Job1", "Pos2", "Cam2");
					break;

				case 7:
					view.SetTitle("相机07 - 拔针检测");
					view.SetDisplayText("拔针");
					view.SetResult(true);
					view.SetStatistics(30, 29);
					view.SetInfo("Job1", "Pos1", "Cam1");
					break;

				case 8:
					view.SetTitle("相机08 - 定位");
					view.SetDisplayText("定位");
					view.SetResult(true);
					view.SetStatistics(28, 26);
					view.SetInfo("Job1", "Pos2", "Cam1");
					break;

				case 9:
					view.SetTitle("相机09 - 表面检测");
					view.SetDisplayText("表面");
					view.SetResult(true);
					view.SetStatistics(40, 38);
					view.SetInfo("Job1", "Pos1", "Cam2");
					break;

				default:
					view.SetTitle("相机" + index.ToString("00"));
					view.SetDisplayText("检测");
					view.SetResult(true);
					view.SetStatistics(0, 0);
					view.SetInfo("Job1", "Pos1", "Cam" + index);
					break;
			}

			return view;
		}

		private void ReloadCameraLayoutFromConfig()
		{
			// int cameraCount = Global.CurrentConfig.CameraCount;
			// BuildCameraLayout(cameraCount);

			BuildCameraLayout(9);
		}

		#endregion

		#region Demo Data

		private void LoadDemoData()
		{
			dgvResults.Rows.Clear();

			string[,] rows =
			{
				{ "K-001", "OK", "●", "09:31:16" },
				{ "K-002", "OK", "●", "09:31:15" },
				{ "K-003", "OK", "●", "09:31:14" },
				{ "K-004", "OK", "●", "09:31:13" },
				{ "K-005", "OK", "●", "09:31:12" },
				{ "K-006", "OK", "●", "09:31:11" },
				{ "K-007", "NG", "●", "09:31:10" },
				{ "K-008", "OK", "●", "09:31:09" },
				{ "K-009", "OK", "●", "09:31:08" },
				{ "K-010", "OK", "●", "09:31:07" },
				{ "K-011", "OK", "●", "09:31:06" },
				{ "K-012", "OK", "●", "09:31:05" },
				{ "K-013", "NG", "●", "09:31:04" },
				{ "K-014", "OK", "●", "09:31:03" },
				{ "K-015", "OK", "●", "09:31:02" }
			};

			for (int i = 0; i < rows.GetLength(0); i++)
			{
				int rowIndex = dgvResults.Rows.Add(rows[i, 0], rows[i, 1], rows[i, 2], rows[i, 3]);
				bool ok = rows[i, 1] == "OK";

				dgvResults.Rows[rowIndex].Cells[1].Style.ForeColor =
					ok ? Color.WhiteSmoke : Color.FromArgb(235, 54, 65);

				dgvResults.Rows[rowIndex].Cells[2].Style.ForeColor =
					ok ? Color.FromArgb(65, 210, 70) : Color.FromArgb(235, 54, 65);
			}

			if (dgvResults.Rows.Count > 0)
				dgvResults.Rows[0].Selected = true;

			lstLog.Items.Clear();
			lstLog.Items.Add("2025-05-24   09:31:16.121   [INFO]  系统启动完成");
			lstLog.Items.Add("2025-05-24   09:31:16.132   [INFO]  打开项目: D:\\Projects\\DemoProject\\DemoProject.vision");
			lstLog.Items.Add("2025-05-24   09:31:16.256   [INFO]  相机 Cam1 连接成功");
			lstLog.Items.Add("2025-05-24   09:31:16.352   [INFO]  Task 1 图像采集与定位 执行完成 (10.00 ms)");
			lstLog.Items.Add("2025-05-24   09:31:16.421   [INFO]  Task 2 检测分析 执行完成");
			lstLog.Items.Add("2025-05-24   09:31:16.507   [OK]    Blob 分析: OK (数量: 0)");
			lstLog.Items.Add("2025-05-24   09:31:16.612   [INFO]  Hough 直线检测: OK (长度: 64.4 ms)");
			lstLog.Items.Add("2025-05-24   09:31:16.721   [NG]    Task 3 表面检测 发现缺陷 (数量: 3, 面积: 0.86 mm², 占比: 0.72%)");
		}

		#endregion

		#region Toolbar Events

		private void btnLogin_Click(object sender, EventArgs e)
		{
			MessageBox.Show("Login clicked.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
		}

		private void btnAlgorithmConfig_Click(object sender, EventArgs e)
		{
			MessageBox.Show("Algorithm configuration clicked.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
		}

		private void btnDatabase_Click(object sender, EventArgs e)
		{
			MessageBox.Show("Database clicked.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
		}

		private void btnSystemSetting_Click(object sender, EventArgs e)
		{
			MessageBox.Show("System setting clicked.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
		}

		private void btnStop_Click(object sender, EventArgs e)
		{
			MessageBox.Show("Stop clicked.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Warning);
		}

		private void btnExit_Click(object sender, EventArgs e)
		{
			DialogResult result = MessageBox.Show(
				"Are you sure you want to exit the software?",
				"Exit",
				MessageBoxButtons.YesNo,
				MessageBoxIcon.Question);

			if (result == DialogResult.Yes)
				this.Close();
		}

		#endregion
	}
}
