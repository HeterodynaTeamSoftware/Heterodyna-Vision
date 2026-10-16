using System;
using System.Drawing;
using System.Windows.Forms;
using HeterodynaVision.Models;
using HeterodynaVision.Services;
using HeterodynaVision.Vendors;

namespace HeterodynaVision.UI;

public sealed class AddCameraForm : Form
{
	private readonly TextBox _location = new TextBox();

	private readonly TextBox _recorder = new TextBox();

	private readonly TextBox _name = new TextBox();

	private readonly TextBox _ip = new TextBox();

	private readonly NumericUpDown _port = new NumericUpDown();

	private readonly TextBox _user = new TextBox();

	private readonly TextBox _pass = new TextBox();

	private readonly TextBox _url = new TextBox();

	private readonly ComboBox _type = new ComboBox();

	private readonly ComboBox _protocol = new ComboBox();

	private readonly ComboBox _sourceFormat = new ComboBox();

	private readonly ComboBox _vendor = new ComboBox();

	public CameraItem? Camera { get; private set; }

	public AddCameraForm(CameraItem? edit = null)
	{
		Text = ((edit == null) ? "Dodaj kamerę / rejestrator" : "Edytuj kamerę / rejestrator");
		Width = 590;
		Height = 530;
		StartPosition = FormStartPosition.CenterParent;
		BackColor = ModernTheme.Background;
		ForeColor = ModernTheme.Text;
		DarkNative.ApplyDarkTitleBar(this);
		FormBorderStyle = FormBorderStyle.FixedDialog;
		MaximizeBox = false;
		MinimizeBox = false;
		BuildUi();
		if (edit != null)
		{
			LoadCamera(edit);
		}
	}

	private void BuildUi()
	{
		TableLayoutPanel root = new TableLayoutPanel
		{
			Dock = DockStyle.Fill,
			Padding = new Padding(14),
			ColumnCount = 2,
			RowCount = 13
		};
		root.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 150f));
		root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
		for (int i = 0; i < 12; i++)
		{
			root.RowStyles.Add(new RowStyle(SizeType.Absolute, 38f));
		}
		root.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));
		Controls.Add(root);
		AddRow(root, 0, "Lokalizacja:", _location);
		AddRow(root, 1, "Rejestrator:", _recorder);
		AddRow(root, 2, "Nazwa kamery:", _name);
		AddRow(root, 3, "IP / host:", _ip);
		_port.Minimum = 1m;
		_port.Maximum = 65535m;
		_port.Value = 554m;
		AddRow(root, 4, "Port:", _port);
		AddRow(root, 5, "Login:", _user);
		_pass.UseSystemPasswordChar = true;
		AddRow(root, 6, "Hasło:", _pass);
		_type.DropDownStyle = ComboBoxStyle.DropDownList;
		_type.Items.AddRange(new object[5] { "RTSP", "HTTP/MJPEG", "ONVIF", "DVR/NVR", "USB" });
		_type.SelectedIndex = 0;
		AddRow(root, 7, "Typ:", _type);
		_protocol.DropDownStyle = ComboBoxStyle.DropDownList;
		_protocol.Items.AddRange(new object[6] { "AUTO", "RTSP", "ONVIF", "HTTP/MJPEG", "USB", "Ręczny URL" });
		_protocol.SelectedIndex = 0;
		AddRow(root, 8, "Protokół:", _protocol);
		_sourceFormat.DropDownStyle = ComboBoxStyle.DropDownList;
		ComboBox.ObjectCollection items = _sourceFormat.Items;
		object[] supportedFormats = StreamFormatHelper.SupportedFormats;
		items.AddRange(supportedFormats);
		_sourceFormat.SelectedIndex = 0;
		AddRow(root, 9, "Format źródła:", _sourceFormat);
		_vendor.DropDownStyle = ComboBoxStyle.DropDownList;
		foreach (VendorProfile profile in VendorProfileStore.Load())
		{
			_vendor.Items.Add(profile.Vendor);
		}
		if (_vendor.Items.Count == 0)
		{
			_vendor.Items.Add("Generic");
		}
		_vendor.SelectedIndex = 0;
		AddRow(root, 10, "Marka:", _vendor);
		AddRow(root, 11, "URL strumienia:", _url);
		_location.Text = "Lokalizacja testowa";
		_recorder.Text = "Ręcznie dodane";
		_name.Text = "Kamera";
		_url.PlaceholderText = "np. rtsp://login:haslo@192.168.1.10:554/Streaming/Channels/101";
		FlowLayoutPanel buttons = new FlowLayoutPanel
		{
			Dock = DockStyle.Fill,
			FlowDirection = FlowDirection.RightToLeft
		};
		Button ok = new Button
		{
			Text = "Zapisz",
			Width = 100,
			Height = 32
		};
		ok.Click += (object? _, EventArgs _) =>
		{
			SaveAndClose();
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
		Button buildUrl = new Button
		{
			Text = "Zbuduj RTSP",
			Width = 120,
			Height = 32
		};
		buildUrl.Click += (object? _, EventArgs _) =>
		{
			BuildRtspUrl();
		};
		buttons.Controls.Add(ok);
		buttons.Controls.Add(cancel);
		buttons.Controls.Add(buildUrl);
		root.Controls.Add(buttons, 0, 12);
		root.SetColumnSpan(buttons, 2);
	}

	private void LoadCamera(CameraItem camera)
	{
		_location.Text = camera.Location;
		_recorder.Text = camera.Recorder;
		_name.Text = camera.Name;
		_ip.Text = camera.Ip;
		_port.Value = Math.Clamp(camera.Port, 1, 65535);
		_user.Text = camera.Username;
		_pass.Text = camera.Password;
		_url.Text = camera.StreamUrl;
		int idx = _type.Items.IndexOf(camera.Type);
		_type.SelectedIndex = ((idx >= 0) ? idx : 0);
		int pidx = _protocol.Items.IndexOf(camera.ProtocolMode);
		_protocol.SelectedIndex = ((pidx >= 0) ? pidx : 0);
		int fidx = _sourceFormat.Items.IndexOf(camera.SourceFormat);
		_sourceFormat.SelectedIndex = ((fidx >= 0) ? fidx : 0);
		int vidx = _vendor.Items.IndexOf(camera.Vendor);
		_vendor.SelectedIndex = ((vidx >= 0) ? vidx : 0);
		Camera = camera;
	}

	private static void AddRow(TableLayoutPanel root, int row, string label, Control control)
	{
		root.Controls.Add(new Label
		{
			Text = label,
			Dock = DockStyle.Fill,
			TextAlign = ContentAlignment.MiddleRight
		}, 0, row);
		control.Dock = DockStyle.Fill;
		root.Controls.Add(control, 1, row);
	}

	private void BuildRtspUrl()
	{
		string ip = _ip.Text.Trim();
		int port = (int)_port.Value;
		string user = _user.Text.Trim();
		string pass = _pass.Text.Trim();
		if (string.IsNullOrWhiteSpace(ip))
		{
			MessageBox.Show("Podaj IP albo host kamery.", "Heterodyna Vision");
			return;
		}
		string auth = "";
		if (!string.IsNullOrWhiteSpace(user))
		{
			auth = Uri.EscapeDataString(user) + ":" + Uri.EscapeDataString(pass) + "@";
		}
		_url.Text = $"rtsp://{auth}{ip}:{port}/Streaming/Channels/101";
	}

	private void SaveAndClose()
	{
		if (string.IsNullOrWhiteSpace(_name.Text))
		{
			MessageBox.Show("Podaj nazwę kamery.", "Heterodyna Vision");
			return;
		}
		if (Camera == null)
		{
			CameraItem cameraItem = (Camera = new CameraItem());
		}
		Camera.Location = (string.IsNullOrWhiteSpace(_location.Text) ? "Lokalizacja" : _location.Text.Trim());
		Camera.Recorder = _recorder.Text.Trim();
		Camera.Name = _name.Text.Trim();
		Camera.Ip = _ip.Text.Trim();
		Camera.Port = (int)_port.Value;
		Camera.Username = _user.Text.Trim();
		Camera.Password = _pass.Text;
		Camera.StreamUrl = _url.Text.Trim();
		Camera.Type = _type.Text;
		Camera.ProtocolMode = _protocol.Text;
		Camera.SourceFormat = _sourceFormat.Text;
		Camera.Vendor = _vendor.Text;
		Camera.RtspTemplate = _url.Text.Trim();
		DialogResult = DialogResult.OK;
	}

	protected override void OnShown(EventArgs e)
	{
		base.OnShown(e);
		DarkNative.ApplyDarkTitleBar(this);
		DarkNative.ApplyDarkExplorerTheme(this);
	}
}
