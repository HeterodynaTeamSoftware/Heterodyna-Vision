using System.Drawing;
using System.Windows.Forms;

namespace HeterodynaVision.UI;

public static class DarkListViewRenderer
{
	public static void Apply(ListView list)
	{
		list.OwnerDraw = true;
		list.BackColor = ModernTheme.CardInner;
		list.ForeColor = ModernTheme.Text;
		list.BorderStyle = BorderStyle.None;
		list.FullRowSelect = true;
		list.GridLines = false;
		list.HideSelection = false;
		list.HeaderStyle = ColumnHeaderStyle.Nonclickable;
		list.DrawColumnHeader -= DrawHeader;
		list.DrawItem -= DrawItem;
		list.DrawSubItem -= DrawSubItem;
		list.DrawColumnHeader += DrawHeader;
		list.DrawItem += DrawItem;
		list.DrawSubItem += DrawSubItem;
	}

	private static void DrawHeader(object? sender, DrawListViewColumnHeaderEventArgs e)
	{
		using SolidBrush bg = new SolidBrush(ModernTheme.Card);
		using Pen border = new Pen(ModernTheme.BorderSoft);
		using SolidBrush text = new SolidBrush(ModernTheme.Text);
		e.Graphics.FillRectangle(bg, e.Bounds);
		e.Graphics.DrawRectangle(border, e.Bounds);
		string value = e.Header?.Text ?? "";
		e.Graphics.DrawString(value, ModernTheme.FontBase(8.5f, FontStyle.Bold), text, e.Bounds.Left + 8, e.Bounds.Top + 5);
	}

	private static void DrawItem(object? sender, DrawListViewItemEventArgs e)
	{
	}

	private static void DrawSubItem(object? sender, DrawListViewSubItemEventArgs e)
	{
		if (e.Item == null || e.SubItem == null)
		{
			return;
		}
		using SolidBrush bg = new SolidBrush(e.Item.Selected ? Color.FromArgb(24, 58, 92) : ModernTheme.CardInner);
		using SolidBrush text = new SolidBrush(e.Item.Selected ? Color.White : ModernTheme.Text);
		e.Graphics.FillRectangle(bg, e.Bounds);
		e.Graphics.DrawString(e.SubItem.Text ?? "", ModernTheme.FontBase(), text, e.Bounds.Left + 8, e.Bounds.Top + 5);
	}
}
