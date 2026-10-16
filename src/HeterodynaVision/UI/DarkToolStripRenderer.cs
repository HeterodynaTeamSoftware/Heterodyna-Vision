using System.Drawing;
using System.Windows.Forms;

namespace HeterodynaVision.UI;

public sealed class DarkToolStripRenderer : ToolStripProfessionalRenderer
{
	protected override void OnRenderToolStripBackground(ToolStripRenderEventArgs e)
	{
		using SolidBrush bg = new SolidBrush(ModernTheme.Window);
		e.Graphics.FillRectangle(bg, e.AffectedBounds);
	}

	protected override void OnRenderToolStripBorder(ToolStripRenderEventArgs e)
	{
		using Pen pen = new Pen(ModernTheme.BorderSoft);
		e.Graphics.DrawLine(pen, 0, e.ToolStrip.Height - 1, e.ToolStrip.Width, e.ToolStrip.Height - 1);
	}

	protected override void OnRenderButtonBackground(ToolStripItemRenderEventArgs e)
	{
		Rectangle rect = new Rectangle(Point.Empty, e.Item.Size);
		using SolidBrush bg = new SolidBrush(e.Item.Selected ? Color.FromArgb(25, 49, 78) : ModernTheme.Card);
		using Pen border = new Pen(ModernTheme.BorderSoft);
		e.Graphics.FillRectangle(bg, rect);
		e.Graphics.DrawRectangle(border, 0, 0, rect.Width - 1, rect.Height - 1);
	}

	protected override void OnRenderMenuItemBackground(ToolStripItemRenderEventArgs e)
	{
		Rectangle rect = new Rectangle(Point.Empty, e.Item.Size);
		using SolidBrush bg = new SolidBrush(e.Item.Selected ? Color.FromArgb(25, 49, 78) : ModernTheme.Card);
		e.Graphics.FillRectangle(bg, rect);
	}
}
