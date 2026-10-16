using System.Drawing;
using System.Windows.Forms;

namespace HeterodynaVision.UI;

public sealed class DarkListView : ListView
{
	public DarkListView()
	{
		SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer, value: true);
		OwnerDraw = true;
		View = View.Details;
		FullRowSelect = true;
		BorderStyle = BorderStyle.None;
		BackColor = ModernTheme.CardInner;
		ForeColor = ModernTheme.Text;
		Font = ModernTheme.FontBase();
		HeaderStyle = ColumnHeaderStyle.Nonclickable;
		DrawColumnHeader += (object? _, DrawListViewColumnHeaderEventArgs e) =>
		{
			using SolidBrush brush = new SolidBrush(ModernTheme.Card);
			using SolidBrush brush2 = new SolidBrush(ModernTheme.Text);
			using Pen pen = new Pen(ModernTheme.BorderSoft);
			e.Graphics.FillRectangle(brush, e.Bounds);
			e.Graphics.DrawRectangle(pen, e.Bounds);
			string s = e.Header?.Text ?? "";
			e.Graphics.DrawString(s, ModernTheme.FontBase(8.5f, FontStyle.Bold), brush2, e.Bounds.Left + 8, e.Bounds.Top + 6);
		};
		DrawItem += (object? _, DrawListViewItemEventArgs e) =>
		{
			e.DrawDefault = false;
		};
		DrawSubItem += (object? _, DrawListViewSubItemEventArgs e) =>
		{
			if (e.Item == null || e.SubItem == null)
			{
				return;
			}
			using SolidBrush brush = new SolidBrush(e.Item.Selected ? Color.FromArgb(23, 55, 88) : ModernTheme.CardInner);
			using SolidBrush brush2 = new SolidBrush(e.Item.Selected ? Color.White : ModernTheme.Text);
			e.Graphics.FillRectangle(brush, e.Bounds);
			e.Graphics.DrawString(e.SubItem?.Text ?? "", ModernTheme.FontBase(), brush2, e.Bounds.Left + 8, e.Bounds.Top + 5);
		};
	}
}
