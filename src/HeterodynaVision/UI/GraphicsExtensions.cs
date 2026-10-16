using System.Drawing;
using System.Drawing.Drawing2D;

namespace HeterodynaVision.UI;

public static class GraphicsExtensions
{
	public static void FillRoundedRectangle(this Graphics graphics, Brush brush, Rectangle bounds, int radius)
	{
		using GraphicsPath path = RoundedRect(bounds, radius);
		graphics.FillPath(brush, path);
	}

	public static void DrawRoundedRectangle(this Graphics graphics, Pen pen, Rectangle bounds, int radius)
	{
		using GraphicsPath path = RoundedRect(bounds, radius);
		graphics.DrawPath(pen, path);
	}

	private static GraphicsPath RoundedRect(Rectangle bounds, int radius)
	{
		int diameter = radius * 2;
		GraphicsPath graphicsPath = new GraphicsPath();
		graphicsPath.AddArc(bounds.Left, bounds.Top, diameter, diameter, 180f, 90f);
		graphicsPath.AddArc(bounds.Right - diameter, bounds.Top, diameter, diameter, 270f, 90f);
		graphicsPath.AddArc(bounds.Right - diameter, bounds.Bottom - diameter, diameter, diameter, 0f, 90f);
		graphicsPath.AddArc(bounds.Left, bounds.Bottom - diameter, diameter, diameter, 90f, 90f);
		graphicsPath.CloseFigure();
		return graphicsPath;
	}
}
