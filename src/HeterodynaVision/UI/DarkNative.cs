using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace HeterodynaVision.UI;

public static class DarkNative
{
	private const int DWMWA_USE_IMMERSIVE_DARK_MODE_BEFORE_20H1 = 19;

	private const int DWMWA_USE_IMMERSIVE_DARK_MODE = 20;

	[DllImport("dwmapi.dll")]
	private static extern int DwmSetWindowAttribute(nint hwnd, int attr, ref int attrValue, int attrSize);

	[DllImport("uxtheme.dll", CharSet = CharSet.Unicode, SetLastError = true)]
	private static extern int SetWindowTheme(nint hWnd, string? pszSubAppName, string? pszSubIdList);

	public static void ApplyDarkTitleBar(Form form)
	{
		if (!OperatingSystem.IsWindows())
		{
			return;
		}
		try
		{
			int useDark = 1;
			DwmSetWindowAttribute(form.Handle, 20, ref useDark, 4);
			DwmSetWindowAttribute(form.Handle, 19, ref useDark, 4);
		}
		catch
		{
		}
	}

	public static void ApplyDarkExplorerTheme(Control root)
	{
		if (!OperatingSystem.IsWindows())
		{
			return;
		}
		foreach (Control control in Enumerate(root))
		{
			try
			{
				if (control.IsHandleCreated)
				{
					SetWindowTheme(control.Handle, "DarkMode_Explorer", null);
				}
			}
			catch
			{
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
