using System;
using System.Drawing;
using System.Windows.Forms;
using HeterodynaVision.Models;
using HeterodynaVision.Security;

namespace HeterodynaVision.UI;

public sealed class LoginForm : Form
{
	private readonly TextBox _login = new TextBox();

	private readonly TextBox _password = new TextBox();

	public UserAccount? User { get; private set; }

	public string LoggedUser { get; private set; } = "";

	public string Role { get; private set; } = "";

	public LoginForm()
	{
		Text = "Heterodyna Vision - logowanie";
		Width = 500;
		Height = 390;
		MinimumSize = new Size(500, 390);
		StartPosition = FormStartPosition.CenterScreen;
		FormBorderStyle = FormBorderStyle.FixedDialog;
		MaximizeBox = false;
		MinimizeBox = false;
		BackColor = ModernTheme.Window;
		ForeColor = ModernTheme.Text;
		BuildUi();
		TryDark();
	}

	private void BuildUi()
	{
		Controls.Clear();
		Panel shell = new Panel
		{
			Dock = DockStyle.Fill,
			Padding = new Padding(22),
			BackColor = ModernTheme.Window
		};
		Controls.Add(shell);
		Panel card = new Panel
		{
			Dock = DockStyle.Fill,
			Padding = new Padding(26),
			BackColor = ModernTheme.Card
		};
		shell.Controls.Add(card);
		TableLayoutPanel root = new TableLayoutPanel
		{
			Dock = DockStyle.Fill,
			RowCount = 6,
			ColumnCount = 2,
			BackColor = ModernTheme.Card
		};
		root.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 92f));
		root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
		root.RowStyles.Add(new RowStyle(SizeType.Absolute, 58f));
		root.RowStyles.Add(new RowStyle(SizeType.Absolute, 42f));
		root.RowStyles.Add(new RowStyle(SizeType.Absolute, 42f));
		root.RowStyles.Add(new RowStyle(SizeType.Absolute, 42f));
		root.RowStyles.Add(new RowStyle(SizeType.Absolute, 64f));
		root.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));
		card.Controls.Add(root);
		Label title = new Label
		{
			Text = "\ud83d\udcf9  Heterodyna",
			Dock = DockStyle.Fill,
			TextAlign = ContentAlignment.MiddleLeft,
			ForeColor = ModernTheme.Text,
			Font = ModernTheme.FontBase(19f, FontStyle.Bold)
		};
		root.Controls.Add(title, 0, 0);
		root.SetColumnSpan(title, 2);
		root.Controls.Add(MakeLabel("Login:"), 0, 1);
		_login.Dock = DockStyle.Fill;
		_login.Margin = new Padding(0, 6, 0, 6);
		ModernTheme.StyleLoginInput(_login);
		root.Controls.Add(_login, 1, 1);
		root.Controls.Add(MakeLabel("Hasło:"), 0, 2);
		_password.Dock = DockStyle.Fill;
		_password.Margin = new Padding(0, 6, 0, 6);
		_password.UseSystemPasswordChar = true;
		ModernTheme.StyleLoginInput(_password);
		root.Controls.Add(_password, 1, 2);
		Label info = new Label
		{
			Text = (AuthStore.HasAdmin() ? "Zaloguj się na konto administratora." : "Pierwsze uruchomienie: utwórz konto administratora."),
			Dock = DockStyle.Fill,
			TextAlign = ContentAlignment.MiddleLeft,
			ForeColor = ModernTheme.Muted,
			Font = ModernTheme.FontBase()
		};
		root.Controls.Add(info, 0, 3);
		root.SetColumnSpan(info, 2);
		Panel buttonsHost = new Panel
		{
			Dock = DockStyle.Fill,
			BackColor = ModernTheme.Card
		};
		Button close = new Button
		{
			Text = "Zamknij",
			Width = 92,
			Height = 34,
			Left = 0,
			Top = 12
		};
		ModernTheme.StyleDangerButton(close);
		close.Click += (object? _, EventArgs _) =>
		{
			Close();
		};
		Button createAdmin = new Button
		{
			Text = "Utwórz admina",
			Width = 126,
			Height = 34,
			Left = 102,
			Top = 12
		};
		ModernTheme.StyleSecondaryButton(createAdmin);
		createAdmin.Click += (object? _, EventArgs _) =>
		{
			CreateAdmin();
		};
		Button loginButton = new Button
		{
			Text = "Zaloguj",
			Width = 96,
			Height = 34,
			Left = 238,
			Top = 12
		};
		ModernTheme.StylePrimaryButton(loginButton);
		loginButton.Click += (object? _, EventArgs _) =>
		{
			Login();
		};
		buttonsHost.Controls.Add(close);
		buttonsHost.Controls.Add(createAdmin);
		buttonsHost.Controls.Add(loginButton);
		root.Controls.Add(buttonsHost, 0, 4);
		root.SetColumnSpan(buttonsHost, 2);
		AcceptButton = loginButton;
		CancelButton = close;
	}

	private static Label MakeLabel(string text)
	{
		return new Label
		{
			Text = text,
			Dock = DockStyle.Fill,
			TextAlign = ContentAlignment.MiddleLeft,
			ForeColor = ModernTheme.Text,
			Font = ModernTheme.FontBase(9.5f, FontStyle.Bold),
			Margin = new Padding(0, 6, 0, 6)
		};
	}

	private void CreateAdmin()
	{
		if (string.IsNullOrWhiteSpace(_login.Text) || string.IsNullOrWhiteSpace(_password.Text))
		{
			MessageBox.Show("Podaj login i hasło administratora.", "Heterodyna Vision");
			return;
		}
		if (AuthStore.HasAdmin())
		{
			MessageBox.Show("Administrator już istnieje.", "Heterodyna Vision");
			return;
		}
		AuthStore.CreateUser(_login.Text.Trim(), _password.Text, "Administrator");
		MessageBox.Show("Utworzono konto administratora.", "Heterodyna Vision");
		Login();
	}

	private void Login()
	{
		if (AuthStore.Validate(_login.Text.Trim(), _password.Text, out string role))
		{
			LoggedUser = _login.Text.Trim();
			Role = role;
			User = new UserAccount
			{
				Username = LoggedUser,
				Role = Role
			};
			DialogResult = DialogResult.OK;
			Close();
		}
		else
		{
			MessageBox.Show("Błędny login lub hasło.", "Heterodyna Vision");
		}
	}

	protected override void OnShown(EventArgs e)
	{
		base.OnShown(e);
		_login.Focus();
		TryDark();
	}

	private void TryDark()
	{
		try
		{
			DarkNative.ApplyDarkTitleBar(this);
			DarkNative.ApplyDarkExplorerTheme(this);
		}
		catch
		{
		}
	}
}
