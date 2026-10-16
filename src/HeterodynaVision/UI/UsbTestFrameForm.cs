using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace HeterodynaVision.UI;

public sealed class UsbTestFrameForm : Form
{
	private readonly PictureBox _picture = new PictureBox();

	private readonly TextBox _info = new TextBox();

	public UsbTestFrameForm(string imagePath, string message, string output)
	{
		Text = "Test klatki USB";
		Width = 860;
		Height = 620;
		StartPosition = FormStartPosition.CenterParent;
		BackColor = ModernTheme.Background;
		ForeColor = ModernTheme.Text;
		DarkNative.ApplyDarkTitleBar(this);
		BuildUi(imagePath, message, output);
	}

	private void BuildUi(string imagePath, string message, string output)
	{
		TableLayoutPanel root = new TableLayoutPanel
		{
			Dock = DockStyle.Fill,
			Padding = new Padding(10),
			RowCount = 3,
			ColumnCount = 1
		};
		root.RowStyles.Add(new RowStyle(SizeType.Percent, 70f));
		root.RowStyles.Add(new RowStyle(SizeType.Percent, 30f));
		root.RowStyles.Add(new RowStyle(SizeType.Absolute, 44f));
		Controls.Add(root);
		_picture.Dock = DockStyle.Fill;
		_picture.SizeMode = PictureBoxSizeMode.Zoom;
		_picture.BackColor = Color.Black;
		if (File.Exists(imagePath))
		{
			using FileStream fs = new FileStream(imagePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
			using Image temp = Image.FromStream(fs);
			_picture.Image = new Bitmap(temp);
		}
		root.Controls.Add(_picture, 0, 0);
		_info.Dock = DockStyle.Fill;
		_info.Multiline = true;
		_info.ReadOnly = true;
		_info.ScrollBars = ScrollBars.Vertical;
		_info.Font = new Font("Consolas", 9f);
		_info.Text = message + Environment.NewLine + Environment.NewLine + output;
		root.Controls.Add(_info, 0, 1);
		FlowLayoutPanel buttons = new FlowLayoutPanel
		{
			Dock = DockStyle.Fill,
			FlowDirection = FlowDirection.RightToLeft
		};
		Button close = new Button
		{
			Text = "Zamknij",
			Width = 100,
			Height = 30
		};
		close.Click += (object? _, EventArgs _) =>
		{
			Close();
		};
		buttons.Controls.Add(close);
		root.Controls.Add(buttons, 0, 2);
	}

	protected override void Dispose(bool disposing)
	{
		if (disposing)
		{
			_picture.Image?.Dispose();
		}
		base.Dispose(disposing);
	}

	protected override void OnShown(EventArgs e)
	{
		base.OnShown(e);
		DarkNative.ApplyDarkTitleBar(this);
		DarkNative.ApplyDarkExplorerTheme(this);
	}
}
