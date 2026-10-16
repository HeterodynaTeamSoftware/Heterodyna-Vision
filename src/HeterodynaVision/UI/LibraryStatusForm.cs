using System;
using System.Drawing;
using System.Windows.Forms;
using HeterodynaVision.Services;

namespace HeterodynaVision.UI;

public sealed class LibraryStatusForm : Form
{
	private readonly TextBox _box = new TextBox();

	public LibraryStatusForm()
	{
		Text = "Biblioteki / FFmpeg";
		Width = 840;
		Height = 520;
		StartPosition = FormStartPosition.CenterParent;
		BackColor = ModernTheme.Background;
		ForeColor = ModernTheme.Text;
		DarkNative.ApplyDarkTitleBar(this);
		BuildUi();
	}

	private void BuildUi()
	{
		TableLayoutPanel root = new TableLayoutPanel
		{
			Dock = DockStyle.Fill,
			Padding = new Padding(12),
			RowCount = 3,
			ColumnCount = 1
		};
		root.RowStyles.Add(new RowStyle(SizeType.Absolute, 34f));
		root.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));
		root.RowStyles.Add(new RowStyle(SizeType.Absolute, 48f));
		Controls.Add(root);
		root.Controls.Add(new Label
		{
			Text = "Wskaż folder, w którym są ffmpeg.exe i ffprobe.exe.",
			Dock = DockStyle.Fill,
			TextAlign = ContentAlignment.MiddleLeft
		}, 0, 0);
		_box.Dock = DockStyle.Fill;
		_box.Multiline = true;
		_box.ScrollBars = ScrollBars.Vertical;
		_box.ReadOnly = true;
		_box.Font = new Font("Consolas", 9f);
		_box.Text = ToolPath.GetDiagnostics();
		root.Controls.Add(_box, 0, 1);
		FlowLayoutPanel buttons = new FlowLayoutPanel
		{
			Dock = DockStyle.Fill,
			FlowDirection = FlowDirection.RightToLeft
		};
		Button close = new Button
		{
			Text = "Zamknij",
			Width = 100,
			Height = 32
		};
		close.Click += (object? _, EventArgs _) =>
		{
			Close();
		};
		Button refresh = new Button
		{
			Text = "Odśwież",
			Width = 100,
			Height = 32
		};
		refresh.Click += (object? _, EventArgs _) =>
		{
			RefreshStatus();
		};
		Button pick = new Button
		{
			Text = "Wskaż folder FFmpeg",
			Width = 170,
			Height = 32
		};
		pick.Click += (object? _, EventArgs _) =>
		{
			PickFolder();
		};
		buttons.Controls.Add(close);
		buttons.Controls.Add(refresh);
		buttons.Controls.Add(pick);
		root.Controls.Add(buttons, 0, 2);
	}

	private void RefreshStatus()
	{
		_box.Text = ToolPath.GetDiagnostics();
	}

	private void PickFolder()
	{
		using FolderBrowserDialog dialog = new FolderBrowserDialog
		{
			Description = "Wybierz folder zawierający ffmpeg.exe i ffprobe.exe",
			UseDescriptionForTitle = true
		};
		if (dialog.ShowDialog(this) == DialogResult.OK)
		{
			MessageBox.Show(ToolPath.SetCustomFfmpegDirectory(dialog.SelectedPath, out string message) ? message : message, "FFmpeg");
			RefreshStatus();
		}
	}

	protected override void OnShown(EventArgs e)
	{
		base.OnShown(e);
		DarkNative.ApplyDarkTitleBar(this);
		DarkNative.ApplyDarkExplorerTheme(this);
	}
}
