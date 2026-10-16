using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace HeterodynaVision.UI;

public static class SidebarVisualFix
{
	public static void Apply(Control root)
	{
		foreach (Control control in Enumerate(root))
		{
			if (control is ListView list)
			{
				DarkListViewRenderer.Apply(list);
				foreach (ColumnHeader column in list.Columns)
				{
					if (column.Width > 150)
					{
						column.Width = 150;
					}
				}
			}
			if (control is TreeView tree)
			{
				tree.BackColor = ModernTheme.Sidebar;
				tree.ForeColor = ModernTheme.Text;
				tree.BorderStyle = BorderStyle.None;
				tree.LineColor = ModernTheme.Border;
			}
			if (control is TextBox { Multiline: not false } textBox)
			{
				textBox.BackColor = ModernTheme.CardInner;
				textBox.ForeColor = ModernTheme.Text;
				textBox.BorderStyle = BorderStyle.None;
			}
			if (control is Panel { Width: <=340 } panel)
			{
				if (panel.BackColor == SystemColors.Control || panel.BackColor == Color.White || panel.BackColor == Color.Empty)
				{
					panel.BackColor = ModernTheme.Sidebar;
				}
				if ((panel.Controls.OfType<ListView>().Any() || panel.Controls.OfType<TreeView>().Any() || panel.Controls.OfType<TextBox>().Any((TextBox x) => x.Multiline)) && !panel.Controls.OfType<DarkSeparator>().Any())
				{
					DarkSeparator sep = new DarkSeparator();
					panel.Controls.Add(sep);
					sep.BringToFront();
				}
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
