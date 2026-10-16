using System;
using System.Drawing;
using System.Windows.Forms;

namespace HeterodynaVision.UI;

public sealed class DarkSeparator : Panel
{
	public DarkSeparator()
	{
		Height = 16;
		Dock = DockStyle.Bottom;
		BackColor = ModernTheme.Sidebar;
		DoubleBuffered = true;
	}

	protected override void OnPaint(PaintEventArgs e)
	{
		base.OnPaint(e);
		using SolidBrush bg = new SolidBrush(ModernTheme.Sidebar);
		e.Graphics.FillRectangle(bg, ClientRectangle);
		int y = Height / 2;
		int left = 18;
		int width = Math.Min(132, Math.Max(40, Width - 36));
		using SolidBrush track = new SolidBrush(Color.FromArgb(22, 32, 49));
		using SolidBrush thumb = new SolidBrush(Color.FromArgb(66, 78, 96));
		e.Graphics.FillRoundedRectangle(track, new Rectangle(14, y - 3, Math.Max(40, Width - 28), 6), 3);
		e.Graphics.FillRoundedRectangle(thumb, new Rectangle(left, y - 2, width, 4), 2);
	}
}
