using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace HeterodynaVision.UI;

public static class TopBarVisualFix
{
	public static void Apply(Control root)
	{
		foreach (Control control in Enumerate(root))
		{
			if (control is ToolStrip toolStrip)
			{
				toolStrip.BackColor = ModernTheme.Window;
				toolStrip.ForeColor = ModernTheme.Text;
				toolStrip.RenderMode = ToolStripRenderMode.Professional;
				toolStrip.Renderer = new DarkToolStripRenderer();
				toolStrip.GripStyle = ToolStripGripStyle.Hidden;
				foreach (ToolStripItem item in toolStrip.Items)
				{
					item.BackColor = ModernTheme.Card;
					item.ForeColor = ModernTheme.Text;
				}
			}
			if (control is MenuStrip menuStrip)
			{
				menuStrip.BackColor = ModernTheme.Window;
				menuStrip.ForeColor = ModernTheme.Text;
				menuStrip.RenderMode = ToolStripRenderMode.Professional;
				menuStrip.Renderer = new DarkToolStripRenderer();
			}
			if (control is Panel panel && ((panel.Top <= 80 && panel.Height <= 70 && panel.Width > 400) || panel.BackColor == SystemColors.Control || panel.BackColor == Color.White || panel.BackColor == Color.Empty))
			{
				panel.BackColor = ModernTheme.Window;
				panel.ForeColor = ModernTheme.Text;
			}
			if (control is FlowLayoutPanel { Top: <=90, Height: <=80 } flow)
			{
				flow.BackColor = ModernTheme.Window;
				flow.ForeColor = ModernTheme.Text;
			}
			if (control is TableLayoutPanel { Top: <=90, Height: <=90 } table)
			{
				table.BackColor = ModernTheme.Window;
				table.ForeColor = ModernTheme.Text;
			}
		}
	}

	private static IEnumerable<Control> Enumerate(Control root)
	{
		foreach (Control child in root.Controls)
		{
			yield return child;
			foreach (Control item in Enumerate(child))
			{
				yield return item;
			}
		}
	}
}
