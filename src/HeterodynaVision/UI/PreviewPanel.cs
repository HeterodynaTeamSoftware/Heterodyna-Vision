using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using HeterodynaVision.Models;
using HeterodynaVision.Services;
using HeterodynaVision.Storage;

namespace HeterodynaVision.UI;

public sealed class PreviewPanel : Panel
{
	private readonly PictureBox _picture = new PictureBox();

	private readonly Panel _topBar = new Panel();

	private readonly Label _title = new Label();

	private readonly Label _state = new Label();

	private readonly Label _icons = new Label();

	private readonly Button _quickStart = new Button();

	private readonly MotionDetectorService _motionDetector = new MotionDetectorService();

	private readonly MotionRecordingService _motionRecorder = new MotionRecordingService();

	private CancellationTokenSource? _cts;

	private CameraItem? _camera;

	private DateTime _lastMotionEvent = DateTime.MinValue;

	public int SlotIndex { get; }

	public string? AssignedCameraId { get; private set; }

	public bool MotionDetectionEnabled { get; private set; }

	public bool RecordOnMotionEnabled { get; private set; }

	public event Action<int>? SlotClicked;

	public event Action<string>? PanelLog;

	public PreviewPanel(int slotIndex)
	{
		SlotIndex = slotIndex;
		Dock = DockStyle.Fill;
		Margin = new Padding(6);
		Padding = new Padding(1);
		BackColor = ModernTheme.BorderSoft;
		AllowDrop = true;
		_picture.Dock = DockStyle.Fill;
		_picture.SizeMode = PictureBoxSizeMode.StretchImage;
		_picture.BackColor = ModernTheme.CardInner;
		_picture.Click += (object? _, EventArgs _) =>
		{
			SlotClicked?.Invoke(SlotIndex);
		};
		_topBar.Dock = DockStyle.Top;
		_topBar.Height = 40;
		_topBar.BackColor = ModernTheme.CardTop;
		_title.Dock = DockStyle.Left;
		_title.Width = 250;
		_title.TextAlign = ContentAlignment.MiddleLeft;
		_title.Padding = new Padding(14, 0, 0, 0);
		_title.ForeColor = ModernTheme.Text;
		_title.Font = ModernTheme.FontBase(10f, FontStyle.Bold);
		_state.Dock = DockStyle.Left;
		_state.Width = 120;
		_state.TextAlign = ContentAlignment.MiddleLeft;
		_state.ForeColor = ModernTheme.Muted;
		_state.Font = ModernTheme.FontBase(8f);
		_icons.Dock = DockStyle.Right;
		_icons.Width = 112;
		_icons.TextAlign = ContentAlignment.MiddleRight;
		_icons.Padding = new Padding(0, 0, 14, 0);
		_icons.ForeColor = ModernTheme.Text;
		_icons.Font = ModernTheme.FontBase(12f);
		_icons.Text = "⚙  ☆  ⋮";
		_topBar.Controls.Add(_state);
		_topBar.Controls.Add(_title);
		_topBar.Controls.Add(_icons);
		_quickStart.Text = "▷  Start";
		_quickStart.Width = 122;
		_quickStart.Height = 38;
		_quickStart.Visible = true;
		_quickStart.Click += (object? _, EventArgs _) =>
		{
			StartAssigned();
		};
		ModernTheme.StyleButton(_quickStart, accent: true);
		Controls.Add(_picture);
		Controls.Add(_topBar);
		Controls.Add(_quickStart);
		Click += (object? _, EventArgs _) =>
		{
			SlotClicked?.Invoke(SlotIndex);
		};
		DragEnter += PreviewPanel_DragEnter;
		DragDrop += PreviewPanel_DragDrop;
		Resize += (object? _, EventArgs _) =>
		{
			LayoutStartButton();
		};
		SetOfflineState($"Slot {SlotIndex + 1}", "OFFLINE", "Kamera offline", "Kliknij Start, aby rozpocząć podgląd");
	}

	private void LayoutStartButton()
	{
		_quickStart.Left = Math.Max(0, (Width - _quickStart.Width) / 2);
		int preferredTop = Height - 74;
		int minimumTop = _topBar.Bottom + 150;
		_quickStart.Top = Math.Max(minimumTop, preferredTop);
		if (_quickStart.Bottom > Height - 16)
		{
			_quickStart.Top = Math.Max(_topBar.Bottom + 120, Height - _quickStart.Height - 18);
		}
		_quickStart.BringToFront();
	}

	public void SetMotionDetection(bool enabled)
	{
		MotionDetectionEnabled = enabled;
		_motionDetector.Reset();
		_lastMotionEvent = DateTime.MinValue;
	}

	public void SetRecordOnMotion(bool enabled)
	{
		RecordOnMotionEnabled = enabled;
		_motionRecorder.Enabled = enabled;
		if (!enabled)
		{
			_motionRecorder.Stop((string message) =>
			{
				PanelLog?.Invoke(message);
			});
		}
	}

	public void Assign(CameraItem camera)
	{
		AssignedCameraId = camera.Id;
		_camera = camera;
		SetOfflineState(camera.Name, "OFFLINE", "Kamera offline", "Kliknij Start, aby rozpocząć podgląd");
	}

	public void SetPlaceholder(string text)
	{
		Stop();
		SetOfflineState($"Slot {SlotIndex + 1}", "OFFLINE", "Kamera offline", "Kliknij Start, aby rozpocząć podgląd");
	}

	public void StartAssigned()
	{
		if (_camera == null)
		{
			SetOfflineState($"Slot {SlotIndex + 1}", "OFFLINE", "Brak kamery", "Przypisz kamerę do tego slotu");
		}
		else
		{
			Start(_camera);
		}
	}

	public async void Start(CameraItem camera)
	{
		Stop();
		_camera = camera;
		AssignedCameraId = camera.Id;
		_cts = new CancellationTokenSource();
		_motionDetector.Reset();
		_lastMotionEvent = DateTime.MinValue;
		_quickStart.Visible = false;
		SetVisualHeader(camera.Name, "CONNECTING", ModernTheme.Warning);
		SetArt("Łączenie...", camera.Name);
		await LoopAsync(_cts.Token);
	}

	public void Stop()
	{
		if (_cts != null)
		{
			try
			{
				_cts.Cancel();
			}
			catch
			{
			}
			_cts.Dispose();
			_cts = null;
		}
		_motionDetector.Reset();
		_motionRecorder.Stop((string message) =>
		{
			PanelLog?.Invoke(message);
		});
		if (_camera != null)
		{
			SetOfflineState(_camera.Name, "OFFLINE", "Kamera offline", "Kliknij Start, aby rozpocząć podgląd");
		}
	}

	private void SetOfflineState(string name, string state, string title, string subtitle)
	{
		SetVisualHeader(name, state, ModernTheme.Muted);
		_quickStart.Visible = true;
		SetArt(title, subtitle);
		LayoutStartButton();
	}

	private void SetVisualHeader(string name, string state, Color stateColor)
	{
		_title.Text = "●  " + name;
		_title.ForeColor = ModernTheme.Text;
		_state.Text = "●  " + state;
		_state.ForeColor = stateColor;
	}

	private async Task LoopAsync(CancellationToken token)
	{
		if (_camera == null)
		{
			return;
		}
		string cacheDir = Path.Combine(AppContext.BaseDirectory, "cache", "frames");
		Directory.CreateDirectory(cacheDir);
		int frame = 0;
		while (!token.IsCancellationRequested)
		{
			string path = Path.Combine(cacheDir, $"{_camera.Id}_{SlotIndex}_{frame % 2}.jpg");
			FrameCaptureResult result = await FrameCaptureService.CaptureFrameAsync(_camera, path, token);
			if (token.IsCancellationRequested)
			{
				break;
			}
			if (result.Success && File.Exists(path))
			{
				try
				{
					using FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
					using Image temp = Image.FromStream(fs);
					Bitmap bmp = new Bitmap(temp);
					bool motion = false;
					double motionScore = 0.0;
					if (MotionDetectionEnabled)
					{
						motion = _motionDetector.Detect(bmp, out motionScore);
					}
					Image image = _picture.Image;
					_picture.Image = bmp;
					image?.Dispose();
					_quickStart.Visible = false;
					SetVisualHeader(_camera.Name, "ONLINE", ModernTheme.Good);
					if (MotionDetectionEnabled & motion)
					{
						SetVisualHeader(_camera.Name, "MOTION", ModernTheme.Warning);
						AlertService.MotionBeep();
						if ((DateTime.Now - _lastMotionEvent).TotalSeconds >= 10.0)
						{
							_lastMotionEvent = DateTime.Now;
							MotionEventStore.Append($"{_camera.Name} | slot {SlotIndex + 1} | score={motionScore:F1}");
						}
						if (RecordOnMotionEnabled)
						{
							_motionRecorder.OnMotion(_camera, SlotIndex, motionScore, (string message) =>
							{
								PanelLog?.Invoke(message);
							});
						}
					}
					_motionRecorder.Tick((string message) =>
					{
						PanelLog?.Invoke(message);
					});
				}
				catch (Exception ex)
				{
					ShowError("Błąd obrazu", ex.Message);
				}
			}
			else
			{
				ShowError("Kamera offline", "Kliknij Start, aby ponowić podgląd");
				_motionRecorder.Tick((string message) =>
				{
					PanelLog?.Invoke(message);
				});
			}
			frame++;
			try
			{
				await Task.Delay(1200, token);
			}
			catch
			{
				break;
			}
		}
	}

	private void ShowError(string title, string details)
	{
		string name = _camera?.Name ?? $"Slot {SlotIndex + 1}";
		SetVisualHeader(name, "ERROR", ModernTheme.Danger);
		_quickStart.Visible = true;
		SetArt(title, details);
		LayoutStartButton();
	}

	private void SetArt(string title, string subtitle)
	{
		int w = Math.Max(420, Width);
		int h = Math.Max(280, Height - _topBar.Height);
		Image image = _picture.Image;
		_picture.Image = ModernTheme.CreateCameraPlaceholder(w, h, title, subtitle);
		image?.Dispose();
		LayoutStartButton();
	}

	protected override void OnPaint(PaintEventArgs e)
	{
		base.OnPaint(e);
		e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
		using Pen pen = new Pen(ModernTheme.Border, 1f);
		e.Graphics.DrawRoundedRectangle(pen, new Rectangle(0, 0, Width - 1, Height - 1), 14);
	}

	private void PreviewPanel_DragEnter(object? sender, DragEventArgs e)
	{
		if (e.Data != null && e.Data.GetDataPresent(typeof(CameraItem)))
		{
			e.Effect = DragDropEffects.Copy;
		}
		else
		{
			e.Effect = DragDropEffects.None;
		}
	}

	private void PreviewPanel_DragDrop(object? sender, DragEventArgs e)
	{
		if (e.Data != null && e.Data.GetData(typeof(CameraItem)) is CameraItem camera)
		{
			Assign(camera);
		}
	}
}
