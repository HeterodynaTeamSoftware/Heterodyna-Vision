using System;
using System.Windows.Forms;
using HeterodynaVision.UI;

namespace HeterodynaVision;

internal static class Program
{
	[STAThread]
	private static void Main()
	{
		ApplicationConfiguration.Initialize();
		using LoginForm login = new LoginForm();
		if (login.ShowDialog() == DialogResult.OK)
		{
			Application.Run(new MainForm(login.LoggedUser, login.Role));
		}
	}
}
