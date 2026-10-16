using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Windows.Forms;

namespace HeterodynaVision.UI;

public static class ModernTheme
{
	public static readonly Color Window = Color.FromArgb(6, 11, 20);

	public static readonly Color Toolbar = Color.FromArgb(8, 14, 25);

	public static readonly Color Background = Color.FromArgb(7, 13, 24);

	public static readonly Color Sidebar = Color.FromArgb(11, 19, 32);

	public static readonly Color SidebarCard = Color.FromArgb(15, 26, 43);

	public static readonly Color Card = Color.FromArgb(14, 25, 42);

	public static readonly Color CardTop = Color.FromArgb(18, 31, 50);

	public static readonly Color CardInner = Color.FromArgb(10, 18, 32);

	public static readonly Color Border = Color.FromArgb(39, 59, 88);

	public static readonly Color BorderSoft = Color.FromArgb(28, 45, 68);

	public static readonly Color Accent = Color.FromArgb(45, 145, 255);

	public static readonly Color AccentHot = Color.FromArgb(31, 169, 255);

	public static readonly Color Good = Color.FromArgb(92, 235, 151);

	public static readonly Color Warning = Color.FromArgb(255, 191, 73);

	public static readonly Color Danger = Color.FromArgb(255, 83, 105);

	public static readonly Color Text = Color.FromArgb(238, 244, 255);

	public static readonly Color Muted = Color.FromArgb(143, 160, 187);

	public static readonly Color Muted2 = Color.FromArgb(99, 118, 148);

	public static readonly Color ScrollTrack = Color.FromArgb(22, 32, 49);

	public static readonly Color ScrollThumb = Color.FromArgb(92, 105, 124);

	public static Font FontBase(float size = 9f, FontStyle style = FontStyle.Regular)
	{
		return new Font("Segoe UI", size, style);
	}

	public static void StyleButton(Button button, bool accent = false, bool danger = false)
	{
		button.FlatStyle = FlatStyle.Flat;
		button.FlatAppearance.BorderSize = 1;
		FlatButtonAppearance flatAppearance = button.FlatAppearance;
		Color borderColor;
		if (danger)
		{
			borderColor = Color.FromArgb(92, 43, 58);
		}
		else
		{
			borderColor = (accent ? Color.FromArgb(43, 108, 177) : BorderSoft);
		}
		flatAppearance.BorderColor = borderColor;
		FlatButtonAppearance flatAppearance2 = button.FlatAppearance;
		Color mouseOverBackColor;
		if (danger)
		{
			mouseOverBackColor = Color.FromArgb(76, 31, 43);
		}
		else
		{
			mouseOverBackColor = (accent ? Color.FromArgb(24, 66, 108) : Color.FromArgb(25, 40, 63));
		}
		flatAppearance2.MouseOverBackColor = mouseOverBackColor;
		FlatButtonAppearance flatAppearance3 = button.FlatAppearance;
		Color mouseDownBackColor;
		if (danger)
		{
			mouseDownBackColor = Color.FromArgb(95, 35, 51);
		}
		else
		{
			mouseDownBackColor = (accent ? Color.FromArgb(31, 82, 132) : Color.FromArgb(31, 49, 75));
		}
		flatAppearance3.MouseDownBackColor = mouseDownBackColor;
		Button button2 = button;
		Color backColor;
		if (danger)
		{
			backColor = Color.FromArgb(40, 20, 30);
		}
		else
		{
			backColor = (accent ? Color.FromArgb(15, 44, 76) : Color.FromArgb(18, 30, 49));
		}
		button2.BackColor = backColor;
		button.ForeColor = (danger ? Color.FromArgb(255, 132, 147) : Text);
		button.Font = FontBase();
		button.Height = Math.Max(button.Height, 32);
		button.Cursor = Cursors.Hand;
	}

	public static void StyleTextBox(TextBox box)
	{
		box.BorderStyle = BorderStyle.FixedSingle;
		box.BackColor = Color.FromArgb(8, 16, 29);
		box.ForeColor = Text;
		box.Font = FontBase();
	}

	public static void StyleCombo(ComboBox combo)
	{
		combo.FlatStyle = FlatStyle.Flat;
		combo.BackColor = Color.FromArgb(13, 24, 41);
		combo.ForeColor = Text;
		combo.Font = FontBase();
		DarkComboBoxFix.ApplyOne(combo);
	}

	public static void StyleRichTextBox(RichTextBox box)
	{
		box.BorderStyle = BorderStyle.None;
		box.BackColor = Color.FromArgb(8, 16, 29);
		box.ForeColor = Text;
		box.Font = FontBase();
	}

	public static void StyleDataGrid(DataGridView grid)
	{
		grid.BackgroundColor = CardInner;
		grid.GridColor = BorderSoft;
		grid.BorderStyle = BorderStyle.None;
		grid.EnableHeadersVisualStyles = false;
		grid.ColumnHeadersDefaultCellStyle.BackColor = Card;
		grid.ColumnHeadersDefaultCellStyle.ForeColor = Text;
		grid.ColumnHeadersDefaultCellStyle.SelectionBackColor = Card;
		grid.ColumnHeadersDefaultCellStyle.SelectionForeColor = Text;
		grid.DefaultCellStyle.BackColor = CardInner;
		grid.DefaultCellStyle.ForeColor = Text;
		grid.DefaultCellStyle.SelectionBackColor = Color.FromArgb(23, 55, 88);
		grid.DefaultCellStyle.SelectionForeColor = Color.White;
		grid.RowHeadersVisible = false;
	}

	public static void StyleListBox(ListBox list)
	{
		list.BackColor = CardInner;
		list.ForeColor = Text;
		list.BorderStyle = BorderStyle.None;
		list.Font = FontBase();
	}

	public static void StyleListView(ListView list)
	{
		list.BackColor = Color.FromArgb(10, 18, 31);
		list.ForeColor = Text;
		list.BorderStyle = BorderStyle.None;
		list.Font = FontBase();
		list.FullRowSelect = true;
		list.GridLines = false;
		DarkListViewRenderer.Apply(list);
	}

	public static void StylePrimaryButton(Button button)
	{
		button.FlatStyle = FlatStyle.Flat;
		button.FlatAppearance.BorderSize = 1;
		button.FlatAppearance.BorderColor = Color.FromArgb(56, 126, 205);
		button.FlatAppearance.MouseOverBackColor = Color.FromArgb(29, 83, 132);
		button.FlatAppearance.MouseDownBackColor = Color.FromArgb(34, 97, 153);
		button.BackColor = Color.FromArgb(18, 61, 102);
		button.ForeColor = Color.White;
		button.Font = FontBase(9f, FontStyle.Bold);
		button.Cursor = Cursors.Hand;
	}

	public static void StyleSecondaryButton(Button button)
	{
		button.FlatStyle = FlatStyle.Flat;
		button.FlatAppearance.BorderSize = 1;
		button.FlatAppearance.BorderColor = Color.FromArgb(55, 73, 101);
		button.FlatAppearance.MouseOverBackColor = Color.FromArgb(31, 49, 77);
		button.FlatAppearance.MouseDownBackColor = Color.FromArgb(39, 59, 88);
		button.BackColor = Color.FromArgb(22, 34, 54);
		button.ForeColor = Color.White;
		button.Font = FontBase();
		button.Cursor = Cursors.Hand;
	}

	public static void StyleDangerButton(Button button)
	{
		button.FlatStyle = FlatStyle.Flat;
		button.FlatAppearance.BorderSize = 1;
		button.FlatAppearance.BorderColor = Color.FromArgb(120, 54, 70);
		button.FlatAppearance.MouseOverBackColor = Color.FromArgb(72, 27, 39);
		button.FlatAppearance.MouseDownBackColor = Color.FromArgb(95, 35, 51);
		button.BackColor = Color.FromArgb(45, 21, 31);
		button.ForeColor = Color.FromArgb(255, 220, 225);
		button.Font = FontBase(9f, FontStyle.Bold);
		button.Cursor = Cursors.Hand;
	}

	public static void StyleLoginInput(TextBox box)
	{
		box.BorderStyle = BorderStyle.FixedSingle;
		box.BackColor = Color.FromArgb(10, 18, 32);
		box.ForeColor = Color.White;
		box.Font = FontBase(9.5f);
	}

	public static Bitmap CreateCameraPlaceholder(int width, int height, string title, string subtitle)
	{
		width = Math.Max(320, width);
		height = Math.Max(230, height);
		Bitmap bmp = new Bitmap(width, height);
		using Graphics g = Graphics.FromImage(bmp);
		g.SmoothingMode = SmoothingMode.AntiAlias;
		g.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;
		using LinearGradientBrush bg = new LinearGradientBrush(new Rectangle(0, 0, width, height), Color.FromArgb(17, 30, 49), Color.FromArgb(8, 14, 25), 90f);
		g.FillRectangle(bg, 0, 0, width, height);
		int pad = 18;
		Rectangle inner = new Rectangle(pad, pad + 10, width - pad * 2, height - pad * 2 - 22);
		using SolidBrush innerBrush = new SolidBrush(Color.FromArgb(32, 12, 22, 38));
		using Pen innerPen = new Pen(Color.FromArgb(44, 62, 90), 1f);
		g.FillRoundedRectangle(innerBrush, inner, 16);
		g.DrawRoundedRectangle(innerPen, inner, 16);
		int iconCenterY = height / 2 - 78;
		using SolidBrush glow = new SolidBrush(Color.FromArgb(28, 42, 120, 205));
		g.FillEllipse(glow, width / 2 - 72, iconCenterY - 66, 144, 132);
		using SolidBrush particle = new SolidBrush(Color.FromArgb(75, 142, 164, 202));
		g.FillEllipse(particle, width / 2 - 150, iconCenterY - 16, 6, 6);
		g.FillEllipse(particle, width / 2 + 132, iconCenterY - 52, 5, 5);
		g.DrawString("+", FontBase(13f, FontStyle.Bold), new SolidBrush(Color.FromArgb(80, 150, 170, 210)), width / 2 - 112, iconCenterY - 48);
		g.DrawString("+", FontBase(9f, FontStyle.Bold), new SolidBrush(Color.FromArgb(70, 150, 170, 210)), width / 2 + 86, iconCenterY - 15);
		DrawCameraIcon(g, width / 2, iconCenterY - 8, 0.52f);
		using Font titleFont = FontBase(13.5f, FontStyle.Bold);
		using Font subFont = FontBase(8.5f);
		using SolidBrush titleBrush = new SolidBrush(Text);
		using SolidBrush subBrush = new SolidBrush(Muted);
		float titleY = height / 2 + 2;
		float subY = titleY + 28f;
		string safeTitle = TrimText(g, title, titleFont, width - 70);
		string safeSubtitle = TrimText(g, subtitle, subFont, width - 90);
		SizeF titleSize = g.MeasureString(safeTitle, titleFont);
		SizeF subSize = g.MeasureString(safeSubtitle, subFont);
		g.DrawString(safeTitle, titleFont, titleBrush, ((float)width - titleSize.Width) / 2f, titleY);
		g.DrawString(safeSubtitle, subFont, subBrush, ((float)width - subSize.Width) / 2f, subY);
		return bmp;
	}

	private static string TrimText(Graphics g, string text, Font font, int maxWidth)
	{
		if (string.IsNullOrWhiteSpace(text))
		{
			return "";
		}
		if (g.MeasureString(text, font).Width <= (float)maxWidth)
		{
			return text;
		}
		string ellipsis = "...";
		for (int len = text.Length - 1; len > 4; len--)
		{
			string cut = text.Substring(0, len) + ellipsis;
			if (g.MeasureString(cut, font).Width <= (float)maxWidth)
			{
				return cut;
			}
		}
		return ellipsis;
	}

	private static void DrawCameraIcon(Graphics g, int cx, int cy, float scale)
	{
		using (SolidBrush white = new SolidBrush(Color.FromArgb(226, 236, 249)))
		{
			using SolidBrush shade = new SolidBrush(Color.FromArgb(160, 180, 205));
			using SolidBrush shade2 = new SolidBrush(Color.FromArgb(121, 142, 170));
			using SolidBrush lens = new SolidBrush(Color.FromArgb(13, 23, 40));
			using SolidBrush lensRing = new SolidBrush(Color.FromArgb(38, 58, 88));
			using SolidBrush blue = new SolidBrush(Color.FromArgb(64, 139, 255));
			using SolidBrush dark = new SolidBrush(Color.FromArgb(8, 15, 28));
			using SolidBrush shine = new SolidBrush(Color.FromArgb(190, 255, 255, 255));
			using Pen pen = new Pen(Color.FromArgb(86, 111, 145), Math.Max(1, S(2)));
			g.FillPie(white, cx - S(86), cy - S(64), S(172), S(108), 180, 180);
			g.DrawArc(pen, cx - S(86), cy - S(64), S(172), S(108), 180, 180);
			g.FillRoundedRectangle(shade2, new Rectangle(cx - S(70), cy - S(30), S(140), S(82)), S(20));
			g.FillRoundedRectangle(shade, new Rectangle(cx - S(63), cy - S(38), S(126), S(72)), S(18));
			g.FillRoundedRectangle(white, new Rectangle(cx - S(58), cy - S(48), S(116), S(66)), S(18));
			g.FillEllipse(lens, cx - S(42), cy - S(24), S(84), S(84));
			g.FillEllipse(lensRing, cx - S(32), cy - S(14), S(64), S(64));
			g.FillEllipse(blue, cx - S(18), cy, S(36), S(36));
			g.FillEllipse(dark, cx - S(11), cy + S(7), S(22), S(22));
			g.FillEllipse(shine, cx - S(7), cy + S(2), S(9), S(9));
		}
		int S(int v)
		{
			return Math.Max(1, (int)((float)v * scale));
		}
	}
}
