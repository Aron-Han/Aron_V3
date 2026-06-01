using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace Aron_V3
{
	public class DiagnosticManagementControl : UserControl, ILocalizable
	{
		private readonly Color _back = Color.FromArgb(2, 10, 20);
		private readonly Color _panel = Color.FromArgb(5, 18, 34);
		private readonly Color _border = Color.FromArgb(38, 62, 86);
		private readonly Color _accent = Color.FromArgb(0, 150, 220);
		private readonly Color _text = Color.FromArgb(220, 235, 245);
		private readonly Color _muted = Color.FromArgb(150, 175, 195);

		private bool _isEnglish;
		private Label _titleLabel;
		private Label _descriptionLabel;
		private Label _diagnosticFolderLabel;
		private Label _packageFolderLabel;
		private Label _statusLabel;
		private Button _exportButton;
		private Button _openLogFolderButton;
		private Button _openPackageFolderButton;

		public DiagnosticManagementControl()
		{
			Dock = DockStyle.Fill;
			BackColor = _back;
			BuildUi();
			RefreshPaths();
		}

		public void ApplyLanguage(bool isEnglish)
		{
			_isEnglish = isEnglish;
			if (_titleLabel != null)
			{
				_titleLabel.Text = T("现场诊断日志", "Field Diagnostics");
			}

			if (_descriptionLabel != null)
			{
				_descriptionLabel.Text = T(
					"诊断日志会实时保存运行事件、异常、环境信息和最近事件缓冲，用于分析现场无法复现的问题。",
					"Diagnostics continuously saves runtime events, exceptions, environment data, and recent event buffers for field issue analysis.");
			}

			if (_exportButton != null)
			{
				_exportButton.Text = T("导出诊断包", "Export Package");
			}

			if (_openLogFolderButton != null)
			{
				_openLogFolderButton.Text = T("打开诊断日志", "Open Logs");
			}

			if (_openPackageFolderButton != null)
			{
				_openPackageFolderButton.Text = T("打开诊断包目录", "Open Packages");
			}

			RefreshPaths();
		}

		private void BuildUi()
		{
			Controls.Clear();

			Panel content = new Panel();
			content.Dock = DockStyle.Fill;
			content.BackColor = _back;
			content.Padding = new Padding(22);

			_titleLabel = new Label();
			_titleLabel.Text = T("现场诊断日志", "Field Diagnostics");
			_titleLabel.Location = new Point(22, 18);
			_titleLabel.Size = new Size(520, 34);
			_titleLabel.ForeColor = _text;
			_titleLabel.Font = new Font("Microsoft YaHei UI", 13F, FontStyle.Bold);

			_descriptionLabel = new Label();
			_descriptionLabel.Location = new Point(22, 58);
			_descriptionLabel.Size = new Size(820, 48);
			_descriptionLabel.ForeColor = _muted;
			_descriptionLabel.Font = new Font("Microsoft YaHei UI", 9.5F, FontStyle.Regular);
			_descriptionLabel.TextAlign = ContentAlignment.MiddleLeft;

			Panel infoPanel = new Panel();
			infoPanel.Location = new Point(22, 120);
			infoPanel.Size = new Size(860, 150);
			infoPanel.Anchor = AnchorStyles.Left | AnchorStyles.Top | AnchorStyles.Right;
			infoPanel.BackColor = _panel;
			infoPanel.Paint += delegate(object sender, PaintEventArgs e)
			{
				using (Pen pen = new Pen(_border))
				{
					Rectangle rect = infoPanel.ClientRectangle;
					rect.Width -= 1;
					rect.Height -= 1;
					e.Graphics.DrawRectangle(pen, rect);
				}
			};

			_diagnosticFolderLabel = CreateInfoLabel(18, 18, 810);
			_packageFolderLabel = CreateInfoLabel(18, 58, 810);
			_statusLabel = CreateInfoLabel(18, 98, 810);
			infoPanel.Controls.Add(_diagnosticFolderLabel);
			infoPanel.Controls.Add(_packageFolderLabel);
			infoPanel.Controls.Add(_statusLabel);

			_exportButton = CreateButton(T("导出诊断包", "Export Package"), 22, 294, 150, true);
			_openLogFolderButton = CreateButton(T("打开诊断日志", "Open Logs"), 190, 294, 150, false);
			_openPackageFolderButton = CreateButton(T("打开诊断包目录", "Open Packages"), 358, 294, 170, false);

			_exportButton.Click += ExportButton_Click;
			_openLogFolderButton.Click += delegate { OpenFolder(DiagnosticLogStore.DiagnosticRoot); };
			_openPackageFolderButton.Click += delegate { OpenFolder(DiagnosticLogStore.PackageFolder); };

			content.Controls.Add(_titleLabel);
			content.Controls.Add(_descriptionLabel);
			content.Controls.Add(infoPanel);
			content.Controls.Add(_exportButton);
			content.Controls.Add(_openLogFolderButton);
			content.Controls.Add(_openPackageFolderButton);

			Controls.Add(content);
			ApplyLanguage(_isEnglish);
		}

		private Label CreateInfoLabel(int x, int y, int width)
		{
			Label label = new Label();
			label.Location = new Point(x, y);
			label.Size = new Size(width, 26);
			label.Anchor = AnchorStyles.Left | AnchorStyles.Top | AnchorStyles.Right;
			label.ForeColor = _text;
			label.BackColor = Color.Transparent;
			label.Font = new Font("Microsoft YaHei UI", 9F, FontStyle.Regular);
			label.AutoEllipsis = true;
			label.TextAlign = ContentAlignment.MiddleLeft;
			return label;
		}

		private Button CreateButton(string text, int x, int y, int width, bool primary)
		{
			Button button = new Button();
			button.Text = text;
			button.Location = new Point(x, y);
			button.Size = new Size(width, 36);
			button.FlatStyle = FlatStyle.Flat;
			button.FlatAppearance.BorderColor = _accent;
			button.FlatAppearance.MouseDownBackColor = Color.FromArgb(20, 70, 135);
			button.FlatAppearance.MouseOverBackColor = Color.FromArgb(15, 45, 78);
			button.BackColor = primary ? Color.FromArgb(0, 95, 210) : Color.FromArgb(3, 14, 27);
			button.ForeColor = Color.White;
			button.Font = new Font("Microsoft YaHei UI", 9F, FontStyle.Bold);
			button.UseVisualStyleBackColor = false;
			button.Cursor = Cursors.Hand;
			return button;
		}

		private void ExportButton_Click(object sender, EventArgs e)
		{
			try
			{
				_exportButton.Enabled = false;
				_statusLabel.Text = T("正在导出诊断包...", "Exporting diagnostic package...");
				Refresh();

				string packagePath = DiagnosticPackageExporter.ExportPackage();
				_statusLabel.Text = T("最近导出: ", "Last export: ") + packagePath;
				ThemedDialog.ShowInformation(
					this,
					T("诊断包", "Diagnostic Package"),
					T("诊断包已导出:\r\n", "Diagnostic package exported:\r\n") + packagePath,
					_isEnglish);
			}
			catch (Exception ex)
			{
				_statusLabel.Text = T("导出失败: ", "Export failed: ") + ex.Message;
				DiagnosticLogStore.WriteCrashReport(ex, "DiagnosticPackageExport", false);
				ThemedDialog.ShowError(
					this,
					T("导出失败", "Export Failed"),
					ex.Message,
					_isEnglish);
			}
			finally
			{
				_exportButton.Enabled = true;
			}
		}

		private void OpenFolder(string folder)
		{
			try
			{
				if (string.IsNullOrWhiteSpace(folder))
				{
					return;
				}

				Directory.CreateDirectory(folder);
				ProcessStartInfo info = new ProcessStartInfo();
				info.FileName = "explorer.exe";
				info.Arguments = "\"" + folder + "\"";
				info.UseShellExecute = true;
				Process.Start(info);
			}
			catch (Exception ex)
			{
				DiagnosticLogStore.WriteCrashReport(ex, "OpenDiagnosticFolder", false);
				ThemedDialog.ShowError(
					this,
					T("打开目录失败", "Open Folder Failed"),
					ex.Message,
					_isEnglish);
			}
		}

		private void RefreshPaths()
		{
			if (_diagnosticFolderLabel != null)
			{
				_diagnosticFolderLabel.Text = T("诊断日志目录: ", "Diagnostic log folder: ") + DiagnosticLogStore.DiagnosticRoot;
			}

			if (_packageFolderLabel != null)
			{
				_packageFolderLabel.Text = T("诊断包目录: ", "Diagnostic package folder: ") + DiagnosticLogStore.PackageFolder;
			}

			if (_statusLabel != null && string.IsNullOrWhiteSpace(_statusLabel.Text))
			{
				_statusLabel.Text = T("状态: 正在实时记录。", "Status: Recording in real time.");
			}
		}

		private string T(string chinese, string english)
		{
			return _isEnglish ? english : chinese;
		}
	}
}
