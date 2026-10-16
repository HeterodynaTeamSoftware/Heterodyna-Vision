using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using HeterodynaVision.Services;

namespace HeterodynaVision.UI;

public sealed class OnvifProfileSelectForm : Form
{
	private readonly ListBox _list = new ListBox();

	private readonly List<OnvifProfileInfo> _profiles;

	public OnvifProfileInfo? SelectedProfile { get; private set; }

	public OnvifProfileSelectForm(List<OnvifProfileInfo> profiles)
	{
		_profiles = profiles;
		Text = "Wybierz profil ONVIF";
		Width = 520;
		Height = 360;
		StartPosition = FormStartPosition.CenterParent;
		BackColor = ModernTheme.Background;
		ForeColor = ModernTheme.Text;
		DarkNative.ApplyDarkTitleBar(this);
		FormBorderStyle = FormBorderStyle.FixedDialog;
		MaximizeBox = false;
		MinimizeBox = false;
		BuildUi();
	}

	private void BuildUi()
	{
		TableLayoutPanel root = new TableLayoutPanel
		{
			Dock = DockStyle.Fill,
			Padding = new Padding(12),
			RowCount = 3,
			ColumnCount = 1
		};
		root.RowStyles.Add(new RowStyle(SizeType.Absolute, 38f));
		root.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));
		root.RowStyles.Add(new RowStyle(SizeType.Absolute, 46f));
		Controls.Add(root);
		root.Controls.Add(new Label
		{
			Text = "Wybierz profil: main = jakość, sub = mniejsze obciążenie.",
			Dock = DockStyle.Fill,
			TextAlign = ContentAlignment.MiddleLeft
		}, 0, 0);
		_list.Dock = DockStyle.Fill;
		foreach (OnvifProfileInfo p in _profiles)
		{
			_list.Items.Add(p.Name + "  | token: " + p.Token);
		}
		if (_list.Items.Count > 0)
		{
			_list.SelectedIndex = 0;
		}
		root.Controls.Add(_list, 0, 1);
		FlowLayoutPanel buttons = new FlowLayoutPanel
		{
			Dock = DockStyle.Fill,
			FlowDirection = FlowDirection.RightToLeft
		};
		Button ok = new Button
		{
			Text = "Wybierz",
			Width = 100,
			Height = 32
		};
		ok.Click += (object? _, EventArgs _) =>
		{
			SelectAndClose();
		};
		Button cancel = new Button
		{
			Text = "Anuluj",
			Width = 100,
			Height = 32
		};
		cancel.Click += (object? _, EventArgs _) =>
		{
			DialogResult = DialogResult.Cancel;
		};
		buttons.Controls.Add(ok);
		buttons.Controls.Add(cancel);
		root.Controls.Add(buttons, 0, 2);
	}

	private void SelectAndClose()
	{
		if (_list.SelectedIndex < 0 || _list.SelectedIndex >= _profiles.Count)
		{
			MessageBox.Show("Wybierz profil ONVIF.", "Heterodyna Vision");
			return;
		}
		SelectedProfile = _profiles[_list.SelectedIndex];
		DialogResult = DialogResult.OK;
	}

	protected override void OnShown(EventArgs e)
	{
		base.OnShown(e);
		DarkNative.ApplyDarkTitleBar(this);
		DarkNative.ApplyDarkExplorerTheme(this);
	}
}
