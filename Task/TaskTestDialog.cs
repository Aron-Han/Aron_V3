using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace Aron_V3
{
	public class TaskTestImageSource
	{
		public string ImageSourceName { get; set; }
		public string LocalImagePath { get; set; }

		public TaskTestImageSource()
		{
			ImageSourceName = "";
			LocalImagePath = "";
		}
	}

	public class TaskTestOptions
	{
		public bool EnableCommunicationOutput { get; set; }
		public List<TaskTestImageSource> ImageSources { get; set; }

		public TaskTestOptions()
		{
			EnableCommunicationOutput = false;
			ImageSources = new List<TaskTestImageSource>();
		}
	}

	public partial class TaskTestDialog : Form
	{
		private readonly Color _back = Color.FromArgb(2, 10, 20);
		private readonly Color _panel = Color.FromArgb(3, 14, 27);
		private readonly Color _panel2 = Color.FromArgb(5, 18, 34);
		private readonly Color _border = Color.FromArgb(38, 62, 86);
		private readonly Color _accent = Color.FromArgb(0, 150, 220);
		private readonly Color _text = Color.FromArgb(220, 235, 245);
		private readonly Color _muted = Color.FromArgb(135, 160, 180);

		private readonly string _taskName;
		private readonly List<string> _imageSourceNames;

		private DataGridView dgvImages;
		private CheckBox chkEnableCommOutput;
		private Button btnOk;
		private Button btnCancel;
		private Button btnBrowse;
		private Button btnClear;
		private Label lblTitle;
		private Label lblNoImageTip;

		public TaskTestOptions Options { get; private set; }

		public TaskTestDialog(string taskName, IEnumerable<string> imageSourceNames)
		{
			Options = new TaskTestOptions();

			_taskName = string.IsNullOrWhiteSpace(taskName) ? "Task Test" : taskName.Trim();
			_imageSourceNames = NormalizeImageSources(imageSourceNames);

			Text = _taskName;
			StartPosition = FormStartPosition.CenterParent;
			Size = new Size(760, 480);
			BackColor = _back;
			FormBorderStyle = FormBorderStyle.FixedDialog;
			MaximizeBox = false;
			MinimizeBox = false;

			BuildUi();
			LoadImageSources();
			ApplyImageSourceMode();
		}

		// 兼容旧调用：如果还有位置调用 new TaskTestDialog(imageSources)，也不会报错。
		public TaskTestDialog(IEnumerable<string> imageSourceNames)
			: this("Task Test", imageSourceNames)
		{
		}

		private List<string> NormalizeImageSources(IEnumerable<string> imageSourceNames)
		{
			List<string> result = new List<string>();

			if (imageSourceNames == null)
			{
				return result;
			}

			foreach (string source in imageSourceNames)
			{
				if (string.IsNullOrWhiteSpace(source))
				{
					continue;
				}

				string item = source.Trim();

				if (string.Equals(item, "Not Use", StringComparison.OrdinalIgnoreCase) ||
					string.Equals(item, "None", StringComparison.OrdinalIgnoreCase))
				{
					continue;
				}

				if (!result.Exists(x => string.Equals(x, item, StringComparison.OrdinalIgnoreCase)))
				{
					result.Add(item);
				}
			}

			return result;
		}

		private void BuildUi()
		{
			TableLayoutPanel root = new TableLayoutPanel();
			root.Dock = DockStyle.Fill;
			root.BackColor = _back;
			root.Padding = new Padding(16);
			root.RowCount = 4;
			root.ColumnCount = 1;
			root.RowStyles.Add(new RowStyle(SizeType.Absolute, 42F));
			root.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
			root.RowStyles.Add(new RowStyle(SizeType.Absolute, 46F));
			root.RowStyles.Add(new RowStyle(SizeType.Absolute, 52F));

			lblTitle = new Label();
			lblTitle.Dock = DockStyle.Fill;
			lblTitle.ForeColor = _text;
			lblTitle.Font = new Font("Microsoft YaHei UI", 10F, FontStyle.Bold);
			lblTitle.TextAlign = ContentAlignment.MiddleLeft;

			Panel imagePanel = new Panel();
			imagePanel.Dock = DockStyle.Fill;
			imagePanel.BackColor = _back;

			dgvImages = new DataGridView();
			dgvImages.Dock = DockStyle.Fill;
			dgvImages.BackgroundColor = _back;
			dgvImages.BorderStyle = BorderStyle.FixedSingle;
			dgvImages.AllowUserToAddRows = false;
			dgvImages.AllowUserToDeleteRows = false;
			dgvImages.RowHeadersVisible = false;
			dgvImages.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
			dgvImages.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
			dgvImages.MultiSelect = false;
			dgvImages.EnableHeadersVisualStyles = false;
			dgvImages.GridColor = _border;

			dgvImages.ColumnHeadersDefaultCellStyle.BackColor = _panel2;
			dgvImages.ColumnHeadersDefaultCellStyle.ForeColor = _text;
			dgvImages.ColumnHeadersDefaultCellStyle.Font = new Font("Microsoft YaHei UI", 9F, FontStyle.Bold);
			dgvImages.DefaultCellStyle.BackColor = _panel;
			dgvImages.DefaultCellStyle.ForeColor = _text;
			dgvImages.DefaultCellStyle.SelectionBackColor = Color.FromArgb(0, 120, 190);
			dgvImages.DefaultCellStyle.SelectionForeColor = Color.White;

			DataGridViewTextBoxColumn colSource = new DataGridViewTextBoxColumn();
			colSource.Name = "ImageSourceName";
			colSource.HeaderText = "图像源";
			colSource.ReadOnly = true;
			colSource.FillWeight = 30;

			DataGridViewTextBoxColumn colPath = new DataGridViewTextBoxColumn();
			colPath.Name = "LocalImagePath";
			colPath.HeaderText = "本地测试图片";
			colPath.ReadOnly = true;
			colPath.FillWeight = 70;

			dgvImages.Columns.Add(colSource);
			dgvImages.Columns.Add(colPath);

			lblNoImageTip = new Label();
			lblNoImageTip.Dock = DockStyle.Fill;
			lblNoImageTip.TextAlign = ContentAlignment.MiddleCenter;
			lblNoImageTip.ForeColor = _muted;
			lblNoImageTip.Font = new Font("Microsoft YaHei UI", 10F, FontStyle.Bold);
			lblNoImageTip.Text = "当前 Task 未配置图像源。\r\n点击 OK 后将直接按正常流程执行，不需要选择本地图片。";
			lblNoImageTip.Visible = false;

			imagePanel.Controls.Add(dgvImages);
			imagePanel.Controls.Add(lblNoImageTip);

			Panel actionPanel = new Panel();
			actionPanel.Dock = DockStyle.Fill;
			actionPanel.BackColor = _back;

			btnBrowse = CreateButton("选择图片", 0, 6, 110, 32);
			btnClear = CreateButton("清除图片", 120, 6, 110, 32);

			chkEnableCommOutput = new CheckBox();
			chkEnableCommOutput.Text = "允许通讯输出";
			chkEnableCommOutput.Left = 260;
			chkEnableCommOutput.Top = 9;
			chkEnableCommOutput.Width = 160;
			chkEnableCommOutput.Height = 28;
			chkEnableCommOutput.ForeColor = _text;
			chkEnableCommOutput.BackColor = _back;
			chkEnableCommOutput.Checked = false;

			Label commTip = new Label();
			commTip.Text = "默认屏蔽通讯输出，避免测试时误发 PLC / TCP / S7 结果。";
			commTip.Left = 420;
			commTip.Top = 10;
			commTip.Width = 300;
			commTip.Height = 26;
			commTip.ForeColor = _muted;
			commTip.BackColor = _back;
			commTip.TextAlign = ContentAlignment.MiddleLeft;

			btnBrowse.Click += delegate { BrowseImageForSelectedRow(); };
			btnClear.Click += delegate { ClearSelectedImage(); };

			actionPanel.Controls.Add(btnBrowse);
			actionPanel.Controls.Add(btnClear);
			actionPanel.Controls.Add(chkEnableCommOutput);
			actionPanel.Controls.Add(commTip);

			Panel bottom = new Panel();
			bottom.Dock = DockStyle.Fill;
			bottom.BackColor = _back;

			btnOk = CreateButton("OK", 420, 10, 120, 34);
			btnCancel = CreateButton("Cancel", 560, 10, 120, 34);

			btnOk.BackColor = Color.FromArgb(0, 95, 210);
			btnOk.Click += delegate { ConfirmOptions(); };
			btnCancel.Click += delegate { DialogResult = DialogResult.Cancel; Close(); };

			bottom.Controls.Add(btnOk);
			bottom.Controls.Add(btnCancel);

			root.Controls.Add(lblTitle, 0, 0);
			root.Controls.Add(imagePanel, 0, 1);
			root.Controls.Add(actionPanel, 0, 2);
			root.Controls.Add(bottom, 0, 3);

			Controls.Add(root);
		}

		private void LoadImageSources()
		{
			dgvImages.Rows.Clear();

			foreach (string source in _imageSourceNames)
			{
				int rowIndex = dgvImages.Rows.Add();
				dgvImages.Rows[rowIndex].Cells["ImageSourceName"].Value = source;
				dgvImages.Rows[rowIndex].Cells["LocalImagePath"].Value = "";
			}
		}

		private void ApplyImageSourceMode()
		{
			bool hasImageSource = _imageSourceNames != null && _imageSourceNames.Count > 0;

			lblTitle.Text = hasImageSource
				? "Task: " + _taskName + "    请选择本地测试图像，并设置是否允许通讯输出"
				: "Task: " + _taskName + "    当前 Task 没有图像源，只设置是否允许通讯输出";

			dgvImages.Visible = hasImageSource;
			lblNoImageTip.Visible = !hasImageSource;

			btnBrowse.Enabled = hasImageSource;
			btnClear.Enabled = hasImageSource;

			if (!hasImageSource)
			{
				btnBrowse.BackColor = Color.FromArgb(18, 28, 38);
				btnClear.BackColor = Color.FromArgb(18, 28, 38);
			}
		}

		private void BrowseImageForSelectedRow()
		{
			if (dgvImages.CurrentRow == null)
			{
				MessageBox.Show("Please select one image source row first.", _taskName, MessageBoxButtons.OK, MessageBoxIcon.Information);
				return;
			}

			using (OpenFileDialog dialog = new OpenFileDialog())
			{
				dialog.Title = "Select test image";
				dialog.Filter = "Image Files (*.bmp;*.png;*.jpg;*.jpeg;*.tif;*.tiff)|*.bmp;*.png;*.jpg;*.jpeg;*.tif;*.tiff|All Files (*.*)|*.*";
				dialog.Multiselect = false;

				if (dialog.ShowDialog(this) != DialogResult.OK)
				{
					return;
				}

				dgvImages.CurrentRow.Cells["LocalImagePath"].Value = dialog.FileName;
			}
		}

		private void ClearSelectedImage()
		{
			if (dgvImages.CurrentRow == null)
			{
				return;
			}

			dgvImages.CurrentRow.Cells["LocalImagePath"].Value = "";
		}

		private void ConfirmOptions()
		{
			Options.EnableCommunicationOutput = chkEnableCommOutput.Checked;
			Options.ImageSources.Clear();

			if (_imageSourceNames == null || _imageSourceNames.Count <= 0)
			{
				DialogResult = DialogResult.OK;
				Close();
				return;
			}

			foreach (DataGridViewRow row in dgvImages.Rows)
			{
				if (row.IsNewRow)
				{
					continue;
				}

				string sourceName = Convert.ToString(row.Cells["ImageSourceName"].Value);
				string imagePath = Convert.ToString(row.Cells["LocalImagePath"].Value);

				if (string.IsNullOrWhiteSpace(sourceName))
				{
					continue;
				}

				if (!string.IsNullOrWhiteSpace(imagePath) && !File.Exists(imagePath))
				{
					MessageBox.Show("Image file does not exist:\r\n" + imagePath, _taskName, MessageBoxButtons.OK, MessageBoxIcon.Warning);
					return;
				}

				// 有图像源但没有选择本地图片时，允许继续。
				// 后续执行时没有 OverrideImageSources，则走正常图像源取图。
				if (!string.IsNullOrWhiteSpace(imagePath))
				{
					Options.ImageSources.Add(new TaskTestImageSource
					{
						ImageSourceName = sourceName,
						LocalImagePath = imagePath
					});
				}
			}

			DialogResult = DialogResult.OK;
			Close();
		}

		private Button CreateButton(string text, int x, int y, int w, int h)
		{
			Button btn = new Button();
			btn.Text = text;
			btn.Left = x;
			btn.Top = y;
			btn.Width = w;
			btn.Height = h;
			btn.FlatStyle = FlatStyle.Flat;
			btn.FlatAppearance.BorderColor = _accent;
			btn.BackColor = _panel2;
			btn.ForeColor = _text;
			btn.Font = new Font("Microsoft YaHei UI", 9F, FontStyle.Bold);
			return btn;
		}
	}
}
