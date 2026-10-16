using System.Collections.Generic;
using System.Windows.Forms;

namespace HeterodynaVision.UI;

public static class LogScrollbarFix
{
	public static void Apply(Control root)
	{
		foreach (Control item in Enumerate(root))
		{
			if (item is TextBox { Multiline: not false } textBox)
			{
				textBox.WordWrap = true;
				textBox.ScrollBars = ScrollBars.Vertical;
				textBox.BackColor = ModernTheme.CardInner;
				textBox.ForeColor = ModernTheme.Text;
				textBox.BorderStyle = BorderStyle.None;
			}
			if (item is RichTextBox richTextBox)
			{
				richTextBox.WordWrap = true;
				richTextBox.ScrollBars = RichTextBoxScrollBars.Vertical;
				richTextBox.BackColor = ModernTheme.CardInner;
				richTextBox.ForeColor = ModernTheme.Text;
				richTextBox.BorderStyle = BorderStyle.None;
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
