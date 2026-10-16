using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace HeterodynaVision.UI;

public static class DarkComboBoxFix
{
	public static void Apply(Control root)
	{
		foreach (Control item in Enumerate(root))
		{
			if (item is ComboBox combo)
			{
				ApplyOne(combo);
			}
		}
	}

	public static void ApplyOne(ComboBox combo)
	{
		combo.FlatStyle = FlatStyle.Flat;
		combo.BackColor = ModernTheme.Card;
		combo.ForeColor = ModernTheme.Text;
		combo.DropDownStyle = ComboBoxStyle.DropDownList;
		combo.DrawMode = DrawMode.OwnerDrawFixed;
		combo.ItemHeight = Math.Max(20, combo.ItemHeight);
		combo.DrawItem -= DrawItem;
		combo.DrawItem += DrawItem;
	}

	private static void DrawItem(object? sender, DrawItemEventArgs e)
	{
		if (!(sender is ComboBox combo))
		{
			return;
		}
		using SolidBrush bg = new SolidBrush(((e.State & DrawItemState.Selected) == DrawItemState.Selected) ? Color.FromArgb(29, 83, 132) : ModernTheme.Card);
		using SolidBrush fg = new SolidBrush(ModernTheme.Text);
		e.Graphics.FillRectangle(bg, e.Bounds);
		string text = "";
		text = ((e.Index < 0 || e.Index >= combo.Items.Count) ? (combo.Text ?? "") : (combo.Items[e.Index]?.ToString() ?? ""));
		e.Graphics.DrawString(text, ModernTheme.FontBase(), fg, e.Bounds.Left + 6, e.Bounds.Top + 3);
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
