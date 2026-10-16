using System;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;
using HeterodynaVision.Models;
using HeterodynaVision.Services;

namespace HeterodynaVision.UI;

public sealed class UsbDevicesForm : Form
{
	private readonly ListBox _devices = new ListBox();

	private readonly ComboBox _options = new ComboBox();

	private readonly Label _status = new Label();

	public CameraItem? SelectedCamera { get; private set; }

	public UsbDevicesForm()
	{
		Text = "Kamery USB / DirectShow";
		Width = 720;
		Height = 500;
		StartPosition = FormStartPosition.CenterParent;
		BackColor = ModernTheme.Background;
		ForeColor = ModernTheme.Text;
		DarkNative.ApplyDarkTitleBar(this);
		BuildUi();
		LoadDevicesAsync();
	}

	private void BuildUi()
	{
		TableLayoutPanel root = new TableLayoutPanel
		{
			Dock = DockStyle.Fill,
			Padding = new Padding(12),
			RowCount = 6,
			ColumnCount = 1
		};
		root.RowStyles.Add(new RowStyle(SizeType.Absolute, 34f));
		root.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));
		root.RowStyles.Add(new RowStyle(SizeType.Absolute, 34f));
		root.RowStyles.Add(new RowStyle(SizeType.Absolute, 40f));
		root.RowStyles.Add(new RowStyle(SizeType.Absolute, 34f));
		root.RowStyles.Add(new RowStyle(SizeType.Absolute, 46f));
		Controls.Add(root);
		root.Controls.Add(new Label
		{
			Text = "Wybierz kamerę USB wykrytą przez FFmpeg/DirectShow.",
			Dock = DockStyle.Fill,
			TextAlign = ContentAlignment.MiddleLeft
		}, 0, 0);
		_devices.Dock = DockStyle.Fill;
		_devices.SelectedIndexChanged += async (object? _, EventArgs _) =>
		{
			await LoadOptionsForSelectedAsync();
		};
		_devices.DoubleClick += (object? _, EventArgs _) =>
		{
			AddSelected();
		};
		root.Controls.Add(_devices, 0, 1);
		root.Controls.Add(new Label
		{
			Text = "Rozdzielczość / FPS:",
			Dock = DockStyle.Fill,
			TextAlign = ContentAlignment.MiddleLeft
		}, 0, 2);
		_options.Dock = DockStyle.Fill;
		_options.DropDownStyle = ComboBoxStyle.DropDownList;
		root.Controls.Add(_options, 0, 3);
		_status.Dock = DockStyle.Fill;
		_status.TextAlign = ContentAlignment.MiddleLeft;
		_status.Text = "Wczytywanie urządzeń...";
		root.Controls.Add(_status, 0, 4);
		FlowLayoutPanel buttons = new FlowLayoutPanel
		{
			Dock = DockStyle.Fill,
			FlowDirection = FlowDirection.RightToLeft
		};
		Button cancel = new Button
		{
			Text = "Zamknij",
			Width = 100,
			Height = 30
		};
		cancel.Click += (object? _, EventArgs _) =>
		{
			DialogResult = DialogResult.Cancel;
		};
		Button add = new Button
		{
			Text = "Dodaj kamerę",
			Width = 130,
			Height = 30
		};
		add.Click += (object? _, EventArgs _) =>
		{
			AddSelected();
		};
		Button refresh = new Button
		{
			Text = "Odśwież",
			Width = 100,
			Height = 30
		};
		refresh.Click += async (object? _, EventArgs _) =>
		{
			await LoadDevicesAsync();
		};
		Button test = new Button
		{
			Text = "Test opcji",
			Width = 100,
			Height = 30
		};
		test.Click += (object? _, EventArgs _) =>
		{
			ShowTestInfo();
		};
		buttons.Controls.Add(cancel);
		buttons.Controls.Add(add);
		buttons.Controls.Add(test);
		buttons.Controls.Add(refresh);
		root.Controls.Add(buttons, 0, 5);
	}

	private async Task LoadDevicesAsync()
	{
		_devices.Items.Clear();
		_options.Items.Clear();
		_status.Text = "Szukam kamer USB przez FFmpeg...";
		foreach (DirectShowDevice device in await DirectShowDeviceService.GetVideoDevicesAsync())
		{
			_devices.Items.Add(device.Name);
		}
		if (_devices.Items.Count > 0)
		{
			_devices.SelectedIndex = 0;
			_status.Text = "Znaleziono kamer USB: " + _devices.Items.Count;
		}
		else
		{
			_status.Text = "Nie znaleziono kamer USB. Sprawdź FFmpeg i uprawnienia urządzenia.";
		}
	}

	private async Task LoadOptionsForSelectedAsync()
	{
		_options.Items.Clear();
		if (_devices.SelectedItem == null)
		{
			return;
		}
		string name = _devices.SelectedItem.ToString() ?? "";
		_status.Text = "Wczytuję opcje dla: " + name;
		foreach (DirectShowVideoOption option in await DirectShowDeviceService.GetVideoOptionsAsync(name))
		{
			_options.Items.Add(option);
		}
		if (_options.Items.Count > 0)
		{
			_options.SelectedIndex = 0;
		}
		_status.Text = $"Opcje dla {name}: {_options.Items.Count}";
	}

	private async void ShowTestInfo()
	{
		CameraItem camera = BuildSelectedCamera();
		if (camera == null)
		{
			MessageBox.Show("Zaznacz kamerę USB.", "Heterodyna Vision");
			return;
		}
		_status.Text = "Testuję kamerę USB: " + camera.Name;
		DirectShowTestResult result = await DirectShowTestService.CaptureTestFrameAsync(camera);
		_status.Text = result.Message;
		if (result.Success)
		{
			using (UsbTestFrameForm preview = new UsbTestFrameForm(result.FramePath, result.Message, result.Output))
			{
				preview.ShowDialog(this);
				return;
			}
		}
		MessageBox.Show(result.Message + "\n\n" + result.Output, "Test USB");
	}

	private CameraItem? BuildSelectedCamera()
	{
		if (_devices.SelectedItem == null)
		{
			return null;
		}
		string name = _devices.SelectedItem.ToString() ?? "USB Camera";
		string resolution = "";
		int fps = 0;
		if (_options.SelectedItem is DirectShowVideoOption option)
		{
			resolution = option.Resolution;
			fps = option.Fps;
		}
		return new CameraItem
		{
			Location = "Lokalne urządzenia",
			Recorder = "DirectShow / USB",
			Name = name,
			Ip = name,
			Port = 0,
			Type = "USB",
			ProtocolMode = "USB/DirectShow",
			SourceFormat = "USB/DirectShow",
			StreamUrl = name,
			UsbResolution = resolution,
			UsbFps = fps,
			Vendor = "USB/DirectShow",
			LastStatus = (string.IsNullOrWhiteSpace(resolution) ? "Dodano kamerę USB AUTO" : $"Dodano kamerę USB {resolution} @{fps}fps")
		};
	}

	private void AddSelected()
	{
		CameraItem camera = BuildSelectedCamera();
		if (camera == null)
		{
			MessageBox.Show("Zaznacz kamerę USB.", "Heterodyna Vision");
			return;
		}
		SelectedCamera = camera;
		DialogResult = DialogResult.OK;
	}

	protected override void OnShown(EventArgs e)
	{
		base.OnShown(e);
		DarkNative.ApplyDarkTitleBar(this);
		DarkNative.ApplyDarkExplorerTheme(this);
	}
}
