using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using HeterodynaVision.Discovery;
using HeterodynaVision.Models;
using HeterodynaVision.Services;
using HeterodynaVision.Storage;

namespace HeterodynaVision.UI;

public sealed class MainForm : Form
{
	private readonly TreeView _tree = new TreeView();

	private readonly ListView _devices = new ListView();

	private readonly TextBox _subnet = new TextBox();

	private readonly TextBox _log = new TextBox();

	private readonly Button _scanButton = new Button();

	private readonly ComboBox _gridMode = new ComboBox();

	private readonly Panel _sidebar = new Panel();

	private readonly TableLayoutPanel _cameraGrid = new TableLayoutPanel();

	private readonly List<CameraItem> _cameras = new List<CameraItem>();

	private readonly List<PreviewPanel> _previewPanels = new List<PreviewPanel>();

	private int _selectedSlotIndex;

	private readonly Dictionary<int, string> _slotAssignments = new Dictionary<int, string>();

	private readonly RecordingService _recorder = new RecordingService();

	private readonly string _currentUser;

	private readonly string _currentRole;

	private bool _motionDetectionEnabled;

	private bool _recordOnMotionEnabled;

	private bool _soundAlertEnabled;

	public MainForm(string currentUser, string currentRole)
	{
		_currentUser = currentUser;
		_currentRole = currentRole;
		Text = $"Heterodyna Vision v01.001.0059 - {_currentUser} ({_currentRole})";
		Width = 1280;
		Height = 760;
		MinimumSize = new Size(980, 620);
		StartPosition = FormStartPosition.CenterScreen;
		_cameras = ConfigStore.Load();
		LoadSlotAssignments();
		BuildUi();
		RebuildTree();
		ApplyUiHotfixes();
		SidebarVisualFix.Apply(this);
		TopBarVisualFix.Apply(this);
		TopBarVisualFix.Apply(this);
		BuildCameraGrid(4);
		Log("Zalogowany: " + _currentUser + " / " + _currentRole);
		Log("Konfiguracja: " + ConfigStore.GetConfigPath());
		Log($"Wczytano kamer: {_cameras.Count}");
		Log("Mapowanie slotów: " + SlotStore.GetSlotsPath());
	}

	protected override void OnFormClosing(FormClosingEventArgs e)
	{
		foreach (PreviewPanel previewPanel in _previewPanels)
		{
			previewPanel.Stop();
		}
		base.OnFormClosing(e);
	}

	private void BuildUi()
	{
		TableLayoutPanel root = new TableLayoutPanel
		{
			Dock = DockStyle.Fill,
			ColumnCount = 2,
			RowCount = 1
		};
		root.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 310f));
		root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
		Controls.Add(root);
		_sidebar.Dock = DockStyle.Fill;
		_sidebar.Padding = new Padding(10);
		_sidebar.BackColor = Color.FromArgb(31, 35, 45);
		root.Controls.Add(_sidebar, 0, 0);
		TableLayoutPanel sideLayout = new TableLayoutPanel
		{
			Dock = DockStyle.Fill,
			RowCount = 5,
			ColumnCount = 1
		};
		sideLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 38f));
		sideLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 38f));
		sideLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 46f));
		sideLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 34f));
		sideLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 20f));
		_sidebar.Controls.Add(sideLayout);
		sideLayout.Controls.Add(new Label
		{
			Text = "\ud83d\udcf9 Heterodyna Vision",
			ForeColor = Color.White,
			Dock = DockStyle.Fill,
			Font = new Font("Segoe UI", 12f, FontStyle.Bold),
			TextAlign = ContentAlignment.MiddleLeft
		}, 0, 0);
		TableLayoutPanel scanPanel = new TableLayoutPanel
		{
			Dock = DockStyle.Fill,
			ColumnCount = 2
		};
		scanPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 60f));
		scanPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 40f));
		sideLayout.Controls.Add(scanPanel, 0, 1);
		_subnet.Text = NetworkHelper.GuessLocalSubnetPrefix() ?? "192.168.1.";
		_subnet.PlaceholderText = "np. 192.168.2.1 albo 192.168.2.0/24";
		_subnet.Dock = DockStyle.Fill;
		scanPanel.Controls.Add(_subnet, 0, 0);
		_scanButton.Text = "Skanuj";
		_scanButton.Dock = DockStyle.Fill;
		_scanButton.Click += async (object? _, EventArgs _) =>
		{
			await ScanAsync();
		};
		scanPanel.Controls.Add(_scanButton, 1, 0);
		_tree.Dock = DockStyle.Fill;
		_tree.BackColor = Color.FromArgb(39, 44, 57);
		_tree.ForeColor = Color.White;
		_tree.BorderStyle = BorderStyle.None;
		_tree.NodeMouseDoubleClick += (object? _, TreeNodeMouseClickEventArgs e) =>
		{
			EditCameraFromNode(e.Node);
		};
		_tree.ItemDrag += (object? _, ItemDragEventArgs e) =>
		{
			if (e.Item is TreeNode { Tag: var tag })
			{
				string id = tag as string;
				if (id != null)
				{
					CameraItem cameraItem = _cameras.FirstOrDefault((CameraItem c) => c.Id == id);
					if (cameraItem != null)
					{
						DoDragDrop(cameraItem, DragDropEffects.Copy);
					}
				}
			}
		};
		sideLayout.Controls.Add(_tree, 0, 2);
		_devices.Dock = DockStyle.Fill;
		_devices.View = View.Details;
		_devices.FullRowSelect = true;
		_devices.Columns.Add("IP", 110);
		_devices.Columns.Add("Typ", 160);
		_devices.Columns.Add("Porty", 160);
		_devices.DoubleClick += (object? _, EventArgs _) =>
		{
			AddSelectedDeviceToTree();
		};
		sideLayout.Controls.Add(_devices, 0, 3);
		_log.Dock = DockStyle.Fill;
		_log.Multiline = true;
		_log.ReadOnly = true;
		_log.ScrollBars = ScrollBars.Vertical;
		_log.BackColor = Color.FromArgb(20, 22, 28);
		_log.ForeColor = Color.LightGreen;
		sideLayout.Controls.Add(_log, 0, 4);
		TableLayoutPanel main = new TableLayoutPanel
		{
			Dock = DockStyle.Fill,
			RowCount = 2,
			ColumnCount = 1
		};
		main.RowStyles.Add(new RowStyle(SizeType.Absolute, 50f));
		main.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));
		root.Controls.Add(main, 1, 0);
		FlowLayoutPanel toolbar = new FlowLayoutPanel
		{
			Dock = DockStyle.Fill,
			Padding = new Padding(8),
			BackColor = Color.FromArgb(245, 246, 250)
		};
		main.Controls.Add(toolbar, 0, 0);
		Button menuButton = new Button
		{
			Text = "☰ Menu",
			Width = 100,
			Height = 32
		};
		menuButton.Click += (object? _, EventArgs _) =>
		{
			root.ColumnStyles[0].Width = ((!(root.ColumnStyles[0].Width > 0f)) ? 310 : 0);
		};
		toolbar.Controls.Add(menuButton);
		toolbar.Controls.Add(new Label
		{
			Text = "Widok:",
			Width = 50,
			Height = 32,
			TextAlign = ContentAlignment.MiddleRight
		});
		_gridMode.DropDownStyle = ComboBoxStyle.DropDownList;
		_gridMode.Items.AddRange(new object[6] { "1 kamera", "4 kamery", "8 kamer", "16 kamer", "32 kamery", "64 kamery" });
		_gridMode.SelectedIndex = 1;
		_gridMode.Width = 120;
		_gridMode.SelectedIndexChanged += (object? _, EventArgs _) =>
		{
			BuildCameraGrid(SelectedGridCount());
		};
		toolbar.Controls.Add(_gridMode);
		AddToolbarButton(toolbar, "▶ Start", 80, () =>
		{
			StartSelectedCamera();
		});
		AddToolbarButton(toolbar, "▶▶ Start all", 100, () =>
		{
			StartAllAssigned();
		});
		AddToolbarButton(toolbar, "■ Stop", 80, () =>
		{
			StopAllStreams();
		});
		AddToolbarButton(toolbar, "\ud83d\udea8 Ruch", 90, () =>
		{
			ToggleMotionDetection();
		});
		AddToolbarButton(toolbar, "\ud83c\udf9e REC ruch", 110, () =>
		{
			ToggleRecordOnMotion();
		});
		AddToolbarButton(toolbar, "\ud83d\udcc1 Historia", 110, () =>
		{
			ShowRecordingsHistory();
		});
		AddToolbarButton(toolbar, "\ud83d\udd14 Alarm", 95, () =>
		{
			ToggleSoundAlert();
		});
		AddToolbarButton(toolbar, "\ud83c\udfaf Przypisz", 95, () =>
		{
			AssignSelectedCameraToSlot();
		});
		AddToolbarButton(toolbar, "\ud83d\udcbe Układ", 90, () =>
		{
			SaveSlotAssignments();
		});
		AddToolbarButton(toolbar, "➕ Dodaj", 100, () =>
		{
			AddManualCamera();
		});
		AddToolbarButton(toolbar, "✏\ufe0f Edytuj", 100, () =>
		{
			EditSelectedCamera();
		});
		AddToolbarButton(toolbar, "\ud83d\uddd1 Usuń", 90, () =>
		{
			DeleteSelectedCamera();
		});
		AddToolbarButton(toolbar, "\ud83d\udcbe Zapisz", 90, () =>
		{
			SaveConfig();
		});
		AddToolbarButton(toolbar, "⬇ Import", 95, () =>
		{
			ImportConfig();
		});
		AddToolbarButton(toolbar, "⬆ Export", 95, () =>
		{
			ExportConfig();
		});
		AddToolbarButton(toolbar, "\ud83e\uddea Test", 80, async () =>
		{
			await TestSelectedRtspAsync();
		});
		AddToolbarButton(toolbar, "\ud83e\uddf0 Biblioteki", 115, () =>
		{
			ShowLibraryStatus();
		});
		AddToolbarButton(toolbar, "\ud83d\udce1 Formaty", 100, () =>
		{
			ShowSupportedFormats();
		});
		AddToolbarButton(toolbar, "\ud83d\udcca Status", 95, () =>
		{
			ShowCameraStatus();
		});
		AddToolbarButton(toolbar, "\ud83d\udcf7 USB", 85, async () =>
		{
			await AddUsbCameraAsync();
		});
		AddToolbarButton(toolbar, "\ud83e\udde0 Auto prot.", 110, async () =>
		{
			await DetectSelectedProtocolAsync();
		});
		AddToolbarButton(toolbar, "\ud83c\udfaf RTSP auto", 110, async () =>
		{
			await TestRtspTemplatesAsync();
		});
		AddToolbarButton(toolbar, "⏺ REC", 80, () =>
		{
			StartRecording();
		});
		AddToolbarButton(toolbar, "⏹ STOP REC", 110, () =>
		{
			StopRecording();
		});
		AddToolbarButton(toolbar, "\ud83d\udd0e ONVIF", 90, async () =>
		{
			await OnvifDiscoverAsync();
		});
		AddToolbarButton(toolbar, "\ud83d\udd17 ONVIF URI", 115, async () =>
		{
			await GetOnvifStreamUriAsync();
		});
		AddToolbarButton(toolbar, "\ud83c\udf9a ONVIF prof.", 120, async () =>
		{
			await SelectOnvifProfileAsync();
		});
		_cameraGrid.Dock = DockStyle.Fill;
		_cameraGrid.BackColor = Color.FromArgb(14, 16, 22);
		_cameraGrid.Padding = new Padding(8);
		main.Controls.Add(_cameraGrid, 0, 1);
	}

	private static void AddToolbarButton(FlowLayoutPanel toolbar, string text, int width, Action action)
	{
		Button btn = new Button
		{
			Text = text,
			Width = width,
			Height = 32
		};
		btn.Click += (object? _, EventArgs _) =>
		{
			action();
		};
		toolbar.Controls.Add(btn);
	}

	private static void AddToolbarButton(FlowLayoutPanel toolbar, string text, int width, Func<Task> action)
	{
		Button btn = new Button
		{
			Text = text,
			Width = width,
			Height = 32
		};
		btn.Click += async (object? _, EventArgs _) =>
		{
			await action();
		};
		toolbar.Controls.Add(btn);
	}

	private int SelectedGridCount()
	{
		return _gridMode.SelectedIndex switch
		{
			0 => 1, 
			1 => 4, 
			2 => 8, 
			3 => 16, 
			4 => 32, 
			5 => 64, 
			_ => 4, 
		};
	}

	private void RebuildTree()
	{
		_tree.Nodes.Clear();
		foreach (IGrouping<string, CameraItem> group in from c in _cameras
			group c by c.Location)
		{
			TreeNode locationNode = new TreeNode("\ud83d\udccd " + group.Key);
			foreach (IGrouping<string, CameraItem> recGroup in from c in @group
				group c by (!string.IsNullOrWhiteSpace(c.Recorder)) ? c.Recorder : "Kamery bez rejestratora")
			{
				TreeNode recNode = new TreeNode("\ud83e\udde0 " + recGroup.Key);
				foreach (CameraItem cam in recGroup)
				{
					recNode.Nodes.Add(new TreeNode($"\ud83c\udfa5 {cam.Name} [{cam.Type}] {cam.Ip} | {cam.LastStatus}")
					{
						Tag = cam.Id
					});
				}
				locationNode.Nodes.Add(recNode);
			}
			_tree.Nodes.Add(locationNode);
		}
		if (_tree.Nodes.Count == 0)
		{
			TreeNode location = new TreeNode("\ud83d\udccd Brak skonfigurowanych kamer");
			TreeNode info = new TreeNode("➕ Kliknij Dodaj albo dwuklik na znalezionym urządzeniu");
			location.Nodes.Add(info);
			_tree.Nodes.Add(location);
		}
		_tree.ExpandAll();
	}

	private void BuildCameraGrid(int count)
	{
		foreach (PreviewPanel previewPanel in _previewPanels)
		{
			previewPanel.Stop();
		}
		_previewPanels.Clear();
		_cameraGrid.Controls.Clear();
		_cameraGrid.ColumnStyles.Clear();
		_cameraGrid.RowStyles.Clear();
		int columns = count switch
		{
			1 => 1, 
			4 => 2, 
			8 => 4, 
			16 => 4, 
			32 => 8, 
			64 => 8, 
			_ => 2, 
		};
		int rows = (int)Math.Ceiling((double)count / (double)columns);
		_cameraGrid.ColumnCount = columns;
		_cameraGrid.RowCount = rows;
		for (int c = 0; c < columns; c++)
		{
			_cameraGrid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f / (float)columns));
		}
		for (int r = 0; r < rows; r++)
		{
			_cameraGrid.RowStyles.Add(new RowStyle(SizeType.Percent, 100f / (float)rows));
		}
		for (int i = 0; i < count; i++)
		{
			PreviewPanel panel = new PreviewPanel(i);
			panel.SlotClicked += (int slot) =>
			{
				_selectedSlotIndex = slot;
				Log("Wybrano slot podglądu: " + (slot + 1));
			};
			panel.PanelLog += (string message) =>
			{
				Invoke(() =>
				{
					Log(message);
				});
			};
			if (_slotAssignments.TryGetValue(i, out string cameraId))
			{
				CameraItem assigned = _cameras.FirstOrDefault((CameraItem cameraItem) => cameraItem.Id == cameraId);
				if (assigned != null)
				{
					panel.Assign(assigned);
				}
				else
				{
					panel.SetPlaceholder($"Slot {i + 1}\nPrzypisana kamera nie istnieje");
				}
			}
			else if (i < _cameras.Count)
			{
				panel.Assign(_cameras[i]);
				_slotAssignments[i] = _cameras[i].Id;
			}
			else
			{
				panel.SetPlaceholder($"Slot {i + 1}\nBrak strumienia");
			}
			_previewPanels.Add(panel);
			_cameraGrid.Controls.Add(panel, i % columns, i / columns);
		}
	}

	private void StartSelectedCamera()
	{
		CameraItem camera = GetSelectedCamera();
		if (camera == null)
		{
			if (_cameras.Count == 0)
			{
				MessageBox.Show("Nie masz jeszcze dodanej realnej kamery. Dodaj ją teraz, a potem kliknij Start.", "Heterodyna Vision");
				AddManualCamera();
				return;
			}
			camera = _cameras.FirstOrDefault();
			if (camera == null)
			{
				MessageBox.Show("Dodaj kamerę albo wybierz ją w drzewie po lewej.", "Heterodyna Vision");
				return;
			}
			Log("Nie zaznaczono kamery, uruchamiam pierwszą z listy: " + camera.Name);
		}
		if (_previewPanels.Count == 0)
		{
			BuildCameraGrid(SelectedGridCount());
		}
		int slot = Math.Clamp(_selectedSlotIndex, 0, _previewPanels.Count - 1);
		_previewPanels[slot].Start(camera);
		camera.LastStatus = "Pseudo-live uruchomiony w slocie " + (slot + 1);
		SaveConfig();
		RebuildTree();
		ApplyUiHotfixes();
		SidebarVisualFix.Apply(this);
		TopBarVisualFix.Apply(this);
		Log($"Start pseudo-live: {camera.Name} -> slot {slot + 1}");
	}

	private void LoadSlotAssignments()
	{
		_slotAssignments.Clear();
		foreach (SlotAssignment assignment in SlotStore.Load())
		{
			if (assignment.SlotIndex >= 0 && !string.IsNullOrWhiteSpace(assignment.CameraId))
			{
				_slotAssignments[assignment.SlotIndex] = assignment.CameraId;
			}
		}
	}

	private void SaveSlotAssignments()
	{
		SlotStore.Save(_slotAssignments.Select((KeyValuePair<int, string> kv) => new SlotAssignment
		{
			SlotIndex = kv.Key,
			CameraId = kv.Value
		}).ToList());
		Log("Zapisano układ slotów.");
	}

	private void AssignSelectedCameraToSlot()
	{
		CameraItem camera = GetSelectedCamera();
		if (camera == null)
		{
			MessageBox.Show("Zaznacz kamerę w drzewie po lewej.", "Heterodyna Vision");
			return;
		}
		if (_previewPanels.Count == 0)
		{
			BuildCameraGrid(SelectedGridCount());
		}
		int slot = Math.Clamp(_selectedSlotIndex, 0, _previewPanels.Count - 1);
		_previewPanels[slot].Assign(camera);
		_slotAssignments[slot] = camera.Id;
		SaveSlotAssignments();
		Log($"Przypisano kamerę {camera.Name} do slotu {slot + 1}");
	}

	private void StartAllAssigned()
	{
		if (_previewPanels.Count == 0)
		{
			BuildCameraGrid(SelectedGridCount());
		}
		foreach (PreviewPanel previewPanel in _previewPanels)
		{
			previewPanel.StartAssigned();
		}
		Log("Uruchomiono wszystkie przypisane sloty.");
	}

	protected override void OnShown(EventArgs e)
	{
		base.OnShown(e);
		ApplyModernTheme();
		ApplyUiHotfixes();
		DarkNative.ApplyDarkTitleBar(this);
		DarkNative.ApplyDarkExplorerTheme(this);
		FixWhiteSidebarBars();
		SidebarVisualFix.Apply(this);
		TopBarVisualFix.Apply(this);
	}

	private void ApplyModernTheme()
	{
		DarkNative.ApplyDarkTitleBar(this);
		BackColor = ModernTheme.Window;
		ForeColor = ModernTheme.Text;
		Font = ModernTheme.FontBase();
		foreach (Control control in GetAllControls(this))
		{
			if (control is Button button)
			{
				bool danger = button.Text.Contains("Usuń", StringComparison.OrdinalIgnoreCase);
				bool accent = button.Text.Contains("Start", StringComparison.OrdinalIgnoreCase) || button.Text.Contains("Skanuj", StringComparison.OrdinalIgnoreCase) || button.Text.Contains("Dodaj", StringComparison.OrdinalIgnoreCase);
				ModernTheme.StyleButton(button, accent, danger);
			}
			else if (control is TextBox textBox)
			{
				ModernTheme.StyleTextBox(textBox);
			}
			else if (control is ComboBox combo)
			{
				ModernTheme.StyleCombo(combo);
			}
			else if (control is ListView list)
			{
				ModernTheme.StyleListView(list);
			}
			else if (control is RichTextBox richTextBox)
			{
				ModernTheme.StyleRichTextBox(richTextBox);
			}
			else if (control is DataGridView grid)
			{
				ModernTheme.StyleDataGrid(grid);
			}
			else if (control is ListBox listBox)
			{
				ModernTheme.StyleListBox(listBox);
			}
			else if (control is TreeView tree)
			{
				tree.BackColor = ModernTheme.Sidebar;
				tree.ForeColor = ModernTheme.Text;
				tree.LineColor = ModernTheme.Border;
				tree.BorderStyle = BorderStyle.None;
				tree.Font = ModernTheme.FontBase();
			}
			else if (control is Panel panel)
			{
				if (panel.BackColor == SystemColors.Control || panel.BackColor == Color.Empty || panel.BackColor.ToArgb() == Color.White.ToArgb())
				{
					panel.BackColor = ModernTheme.Background;
				}
			}
			else if (control is Label label)
			{
				label.ForeColor = ModernTheme.Text;
				label.Font = ModernTheme.FontBase(label.Font.Size, label.Font.Style);
			}
		}
		DarkNative.ApplyDarkExplorerTheme(this);
	}

	private void FixWhiteSidebarBars()
	{
		foreach (Control control in GetAllControls(this))
		{
			if (control is HScrollBar hbar)
			{
				hbar.Visible = false;
				hbar.Enabled = false;
				hbar.Height = 0;
				continue;
			}
			if (control is TreeView tree)
			{
				tree.BackColor = ModernTheme.Sidebar;
				tree.ForeColor = ModernTheme.Text;
				tree.BorderStyle = BorderStyle.None;
				tree.HideSelection = false;
				AddDarkSeparatorIfMissing(tree.Parent);
			}
			if (control is ListView list)
			{
				list.BackColor = ModernTheme.CardInner;
				list.ForeColor = ModernTheme.Text;
				list.BorderStyle = BorderStyle.None;
				list.Scrollable = true;
				AddDarkSeparatorIfMissing(list.Parent);
			}
			if (control is TextBox { Multiline: not false } text)
			{
				text.BackColor = ModernTheme.CardInner;
				text.ForeColor = ModernTheme.Text;
				text.BorderStyle = BorderStyle.None;
				AddDarkSeparatorIfMissing(text.Parent);
			}
			if (control.GetType().Name.Contains("Grid", StringComparison.OrdinalIgnoreCase))
			{
				control.BackColor = ModernTheme.CardInner;
				AddDarkSeparatorIfMissing(control.Parent);
			}
		}
	}

	private static void AddDarkSeparatorIfMissing(Control? parent)
	{
		if (parent != null && !parent.Controls.OfType<DarkSeparator>().Any() && parent.Width <= 340)
		{
			DarkSeparator sep = new DarkSeparator();
			parent.Controls.Add(sep);
			sep.BringToFront();
		}
	}

	private void ApplyUiHotfixes()
	{
		try
		{
			SidebarVisualFix.Apply(this);
			TopBarVisualFix.Apply(this);
			DarkComboBoxFix.Apply(this);
			LogScrollbarFix.Apply(this);
		}
		catch
		{
		}
	}

	private static IEnumerable<Control> GetAllControls(Control root)
	{
		foreach (Control child in root.Controls)
		{
			yield return child;
			foreach (Control allControl in GetAllControls(child))
			{
				yield return allControl;
			}
		}
	}

	private void ShowCameraStatus()
	{
		using CameraStatusForm dialog = new CameraStatusForm(_cameras);
		dialog.ShowDialog(this);
		SaveConfig();
		RebuildTree();
		ApplyUiHotfixes();
		SidebarVisualFix.Apply(this);
		TopBarVisualFix.Apply(this);
	}

	private void ShowSupportedFormats()
	{
		MessageBox.Show("Obsługiwane formaty wejścia:\n\n" + StreamFormatHelper.Explain() + "\n\nNajpewniejsze produkcyjnie: RTSP + ONVIF. MJPEG/HLS/RTMP działają przez FFmpeg i zależą od poprawnego URL.", "Formaty źródeł");
	}

	private void ShowLibraryStatus()
	{
		using LibraryStatusForm dialog = new LibraryStatusForm();
		dialog.ShowDialog(this);
	}

	private void ToggleMotionDetection()
	{
		_motionDetectionEnabled = !_motionDetectionEnabled;
		foreach (PreviewPanel previewPanel in _previewPanels)
		{
			previewPanel.SetMotionDetection(_motionDetectionEnabled);
		}
		Log("Detekcja ruchu: " + (_motionDetectionEnabled ? "ON" : "OFF"));
		Log("Log ruchu: " + MotionEventStore.GetPath());
		MessageBox.Show("Detekcja ruchu: " + (_motionDetectionEnabled ? "WŁĄCZONA" : "WYŁĄCZONA") + "\n\nDziała na aktywnych panelach LIVE. Zdarzenia są zapisywane w data\\motion-events.log.", "Heterodyna Vision");
	}

	private void ToggleRecordOnMotion()
	{
		_recordOnMotionEnabled = !_recordOnMotionEnabled;
		foreach (PreviewPanel previewPanel in _previewPanels)
		{
			previewPanel.SetRecordOnMotion(_recordOnMotionEnabled);
		}
		Log("Nagrywanie po ruchu: " + (_recordOnMotionEnabled ? "ON" : "OFF"));
		MessageBox.Show("Nagrywanie po ruchu: " + (_recordOnMotionEnabled ? "WŁĄCZONE" : "WYŁĄCZONE") + "\n\nEtap 1: zapis zdarzeń ruchu do logu. W następnym kroku podepniemy automatyczne segmenty MP4.", "Heterodyna Vision");
	}

	private void ToggleSoundAlert()
	{
		_soundAlertEnabled = !_soundAlertEnabled;
		AlertService.SoundEnabled = _soundAlertEnabled;
		Log("Alarm dźwiękowy: " + (_soundAlertEnabled ? "ON" : "OFF"));
		MessageBox.Show("Alarm dźwiękowy: " + (_soundAlertEnabled ? "WŁĄCZONY" : "WYŁĄCZONY") + "\n\nPrzy wykryciu ruchu program odtworzy krótki sygnał systemowy maksymalnie raz na 5 sekund.", "Heterodyna Vision");
	}

	private void ShowRecordingsHistory()
	{
		using RecordingsForm dialog = new RecordingsForm();
		dialog.ShowDialog(this);
	}

	private void StopAllStreams()
	{
		foreach (PreviewPanel previewPanel in _previewPanels)
		{
			previewPanel.Stop();
		}
		Log("Zatrzymano wszystkie podglądy.");
	}

	private async Task OnvifDiscoverAsync()
	{
		_devices.Items.Clear();
		Log("Start ONVIF discovery UDP 3702...");
		try
		{
			List<OnvifDevice> found = await OnvifDiscoveryService.DiscoverAsync(TimeSpan.FromSeconds(5.0), (string message) =>
			{
				Invoke(() =>
				{
					Log(message);
				});
			});
			foreach (OnvifDevice device in found)
			{
				ListViewItem item = new ListViewItem(device.Address);
				item.SubItems.Add("ONVIF kamera / NVR");
				item.SubItems.Add("3702 / " + Shorten(device.XAddrs, 120));
				_devices.Items.Add(item);
			}
			Log("Koniec ONVIF discovery. Znaleziono: " + found.Count);
			if (found.Count == 0)
			{
				MessageBox.Show("Nie znaleziono urządzeń ONVIF. Sprawdź, czy kamery mają włączony ONVIF i są w tej samej sieci.", "Heterodyna Vision");
			}
		}
		catch (Exception ex)
		{
			Log("Błąd ONVIF: " + ex.Message);
			MessageBox.Show("Błąd ONVIF: " + ex.Message, "Heterodyna Vision");
		}
	}

	private static string Shorten(string value, int max)
	{
		if (string.IsNullOrWhiteSpace(value))
		{
			return "";
		}
		if (value.Length > max)
		{
			return value.Substring(0, max) + "...";
		}
		return value;
	}

	private async Task ScanAsync()
	{
		_scanButton.Enabled = false;
		_devices.Items.Clear();
		Log("Start skanowania...");
		SubnetScanPlan plan;
		try
		{
			plan = SubnetInputParser.Parse(_subnet.Text);
		}
		catch (Exception ex)
		{
			MessageBox.Show(ex.Message, "Błędny zakres skanowania");
			_scanButton.Enabled = true;
			return;
		}
		Log("Zakres skanowania: " + plan.DisplayRange);
		int[] ports = new int[11]
		{
			80, 81, 88, 554, 8000, 8080, 8081, 8899, 5000, 37777,
			34567
		};
		try
		{
			List<DetectedDevice> found = await new NetworkScanner((string message) =>
			{
				Invoke(() =>
				{
					Log(message);
				});
			}).ScanSubnetAsync(plan, ports);
			foreach (DetectedDevice device in found)
			{
				string type = DeviceClassifier.Classify(device.OpenPorts);
				ListViewItem item = new ListViewItem(device.Ip);
				item.SubItems.Add(type);
				item.SubItems.Add(string.Join(", ", device.OpenPorts));
				_devices.Items.Add(item);
			}
			Log($"Koniec skanowania. Znaleziono: {found.Count}");
		}
		catch (Exception ex2)
		{
			Log("Błąd: " + ex2.Message);
		}
		finally
		{
			_scanButton.Enabled = true;
		}
	}

	private void AddSelectedDeviceToTree()
	{
		if (_devices.SelectedItems.Count != 0)
		{
			ListViewItem listViewItem = _devices.SelectedItems[0];
			string ip = listViewItem.Text;
			string type = listViewItem.SubItems[1].Text;
			int port = ParseFirstPort(listViewItem.SubItems[2].Text);
			CameraItem camera = new CameraItem
			{
				Location = "Lokalizacja testowa",
				Recorder = ((type.Contains("DVR") || type.Contains("NVR") || type.Contains("XVR")) ? ip : "Skan sieci"),
				Name = "Kamera / urządzenie " + ip,
				Ip = ip,
				Port = port,
				Type = type,
				ProtocolMode = (type.Contains("ONVIF") ? "ONVIF" : "AUTO"),
				Vendor = "Generic",
				LastStatus = (type.Contains("ONVIF") ? "Dodano z ONVIF" : "Dodano ze skanu"),
				StreamUrl = ((port == 554) ? ("rtsp://" + ip + ":554/Streaming/Channels/101") : "")
			};
			_cameras.Add(camera);
			SaveConfig();
			RebuildTree();
			ApplyUiHotfixes();
			SidebarVisualFix.Apply(this);
			TopBarVisualFix.Apply(this);
			BuildCameraGrid(SelectedGridCount());
			Log("Dodano do konfiguracji: " + ip);
		}
	}

	private static int ParseFirstPort(string ports)
	{
		if (!int.TryParse(ports.Split(',').FirstOrDefault()?.Trim(), out var port))
		{
			return 554;
		}
		return port;
	}

	private async Task AddUsbCameraAsync()
	{
		using UsbDevicesForm dialog = new UsbDevicesForm();
		if (dialog.ShowDialog(this) == DialogResult.OK && dialog.SelectedCamera != null)
		{
			_cameras.Add(dialog.SelectedCamera);
			SaveConfig();
			RebuildTree();
			ApplyUiHotfixes();
			SidebarVisualFix.Apply(this);
			TopBarVisualFix.Apply(this);
			Log("Dodano kamerę USB/DirectShow: " + dialog.SelectedCamera.Name + " | " + dialog.SelectedCamera.UsbResolution + " @" + dialog.SelectedCamera.UsbFps + "fps");
			MessageBox.Show("Dodano kamerę USB: " + dialog.SelectedCamera.Name, "Heterodyna Vision");
		}
		await Task.CompletedTask;
	}

	private void AddManualCamera()
	{
		using AddCameraForm dialog = new AddCameraForm();
		if (dialog.ShowDialog(this) == DialogResult.OK && dialog.Camera != null)
		{
			dialog.Camera.LastStatus = "Dodano ręcznie";
			_cameras.Add(dialog.Camera);
			SaveConfig();
			RebuildTree();
			ApplyUiHotfixes();
			SidebarVisualFix.Apply(this);
			TopBarVisualFix.Apply(this);
			BuildCameraGrid(SelectedGridCount());
			Log("Dodano ręcznie: " + dialog.Camera.Name);
		}
	}

	private CameraItem? GetSelectedCamera()
	{
		object obj = _tree.SelectedNode?.Tag;
		string id = obj as string;
		if (id == null)
		{
			return null;
		}
		return _cameras.FirstOrDefault((CameraItem c) => c.Id == id);
	}

	private void EditSelectedCamera()
	{
		CameraItem camera = GetSelectedCamera();
		if (camera == null)
		{
			MessageBox.Show("Zaznacz kamerę w drzewie po lewej.", "Heterodyna Vision");
			return;
		}
		using AddCameraForm dialog = new AddCameraForm(camera);
		if (dialog.ShowDialog(this) == DialogResult.OK)
		{
			SaveConfig();
			RebuildTree();
			ApplyUiHotfixes();
			SidebarVisualFix.Apply(this);
			TopBarVisualFix.Apply(this);
			BuildCameraGrid(SelectedGridCount());
			Log("Zaktualizowano kamerę: " + camera.Name);
		}
	}

	private void EditCameraFromNode(TreeNode node)
	{
		if (node.Tag is string)
		{
			EditSelectedCamera();
		}
	}

	private void DeleteSelectedCamera()
	{
		CameraItem camera = GetSelectedCamera();
		if (camera == null)
		{
			MessageBox.Show("Zaznacz kamerę w drzewie po lewej.", "Heterodyna Vision");
		}
		else
		{
			if (MessageBox.Show("Usunąć kamerę: " + camera.Name + "?", "Potwierdź", MessageBoxButtons.YesNo) != DialogResult.Yes)
			{
				return;
			}
			_cameras.Remove(camera);
			foreach (int key in (from x in _slotAssignments
				where x.Value == camera.Id
				select x.Key).ToList())
			{
				_slotAssignments.Remove(key);
			}
			SaveSlotAssignments();
			SaveConfig();
			RebuildTree();
			ApplyUiHotfixes();
			SidebarVisualFix.Apply(this);
			TopBarVisualFix.Apply(this);
			BuildCameraGrid(SelectedGridCount());
			Log("Usunięto kamerę: " + camera.Name);
		}
	}

	private async Task DetectSelectedProtocolAsync()
	{
		CameraItem camera = GetSelectedCamera();
		if (camera == null)
		{
			MessageBox.Show("Zaznacz kamerę w drzewie po lewej.", "Heterodyna Vision");
			return;
		}
		Log("Auto-wykrywanie protokołu: " + camera.Name);
		ProtocolDetectionResult result = await ProtocolDetectorService.DetectAsync(camera);
		camera.ProtocolMode = result.Protocol;
		camera.SourceFormat = result.Protocol;
		if (!string.IsNullOrWhiteSpace(result.SuggestedUrl))
		{
			camera.StreamUrl = result.SuggestedUrl;
		}
		camera.LastStatus = "Protokół: " + result.Protocol;
		SaveConfig();
		RebuildTree();
		ApplyUiHotfixes();
		SidebarVisualFix.Apply(this);
		TopBarVisualFix.Apply(this);
		Log(result.Message);
		MessageBox.Show(result.Message + "\n\nProtokół: " + result.Protocol + "\nURL: " + result.SuggestedUrl, "Auto protokół");
	}

	private async Task TestRtspTemplatesAsync()
	{
		CameraItem camera = GetSelectedCamera();
		if (camera == null)
		{
			MessageBox.Show("Zaznacz kamerę w drzewie po lewej.", "Heterodyna Vision");
			return;
		}
		if (string.IsNullOrWhiteSpace(camera.Ip))
		{
			MessageBox.Show("Kamera nie ma ustawionego IP/hosta.", "Heterodyna Vision");
			return;
		}
		Log("Start auto-testu szablonów RTSP dla: " + camera.Name + " / " + camera.Vendor);
		RtspTemplateTestResult result = await RtspTemplateTesterService.TestTemplatesAsync(camera, (string message) =>
		{
			Invoke(() =>
			{
				Log(message);
			});
		});
		if (result.Success)
		{
			camera.StreamUrl = result.Url;
			camera.ProtocolMode = "RTSP";
			camera.Type = "RTSP";
			camera.LastStatus = "RTSP auto OK";
			SaveConfig();
			RebuildTree();
			ApplyUiHotfixes();
			SidebarVisualFix.Apply(this);
			TopBarVisualFix.Apply(this);
			MessageBox.Show(result.Message + "\n\nURL:\n" + result.Url, "RTSP auto OK");
		}
		else
		{
			camera.LastStatus = "RTSP auto błąd";
			SaveConfig();
			RebuildTree();
			ApplyUiHotfixes();
			SidebarVisualFix.Apply(this);
			TopBarVisualFix.Apply(this);
			MessageBox.Show(result.Message + "\n\nSpróbuj ręcznie wybrać markę/protokół albo wpisz URL.", "RTSP auto błąd");
		}
		Log(result.Message);
	}

	private void StartRecording()
	{
		CameraItem camera = GetSelectedCamera();
		if (camera == null)
		{
			MessageBox.Show("Zaznacz kamerę do nagrywania.", "Heterodyna Vision");
			return;
		}
		string text = Path.Combine(AppContext.BaseDirectory, "recordings");
		Directory.CreateDirectory(text);
		string safeName = string.Join("_", camera.Name.Split(Path.GetInvalidFileNameChars(), StringSplitOptions.RemoveEmptyEntries));
		if (string.IsNullOrWhiteSpace(safeName))
		{
			safeName = "kamera";
		}
		string file = Path.Combine(text, safeName + "_" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".mp4");
		try
		{
			_recorder.Start(camera, file);
			camera.LastStatus = "REC";
			SaveConfig();
			RebuildTree();
			ApplyUiHotfixes();
			SidebarVisualFix.Apply(this);
			TopBarVisualFix.Apply(this);
			Log("Nagrywanie start: " + file);
		}
		catch (Exception ex)
		{
			MessageBox.Show("Nie udało się rozpocząć nagrywania: " + ex.Message, "Heterodyna Vision");
			Log("Błąd REC: " + ex.Message);
		}
	}

	private void StopRecording()
	{
		try
		{
			_recorder.Stop();
			Log("Nagrywanie zatrzymane.");
		}
		catch (Exception ex)
		{
			Log("Błąd STOP REC: " + ex.Message);
		}
	}

	private async Task GetOnvifStreamUriAsync()
	{
		CameraItem camera = GetSelectedCamera();
		if (camera == null)
		{
			MessageBox.Show("Zaznacz kamerę ONVIF w drzewie po lewej.", "Heterodyna Vision");
			return;
		}
		Log("ONVIF Media: pobieranie RTSP URI dla " + camera.Name);
		OnvifMediaResult result = await OnvifMediaService.TryGetStreamUriAsync(camera);
		if (result.Success)
		{
			camera.StreamUrl = result.StreamUri;
			camera.ProtocolMode = "RTSP";
			camera.Type = "RTSP";
			camera.OnvifProfileToken = result.ProfileToken;
			camera.OnvifProfileName = result.ProfileName;
			camera.LastStatus = "ONVIF URI OK";
			SaveConfig();
			RebuildTree();
			ApplyUiHotfixes();
			SidebarVisualFix.Apply(this);
			TopBarVisualFix.Apply(this);
			MessageBox.Show(result.Message + "\n\nAuth:\n" + result.AuthMode + "\n\nMedia URL:\n" + result.MediaServiceUrl + "\n\nProfile:\n" + result.ProfileName + " / " + result.ProfileToken + "\n\nRTSP:\n" + result.StreamUri, "ONVIF URI OK");
		}
		else
		{
			camera.LastStatus = "ONVIF URI błąd";
			SaveConfig();
			RebuildTree();
			ApplyUiHotfixes();
			SidebarVisualFix.Apply(this);
			TopBarVisualFix.Apply(this);
			MessageBox.Show(result.Message + "\n\nProgram próbował PasswordDigest i PasswordText. Sprawdź użytkownika ONVIF, czas kamery oraz włączony ONVIF.", "ONVIF URI błąd");
		}
		Log(result.Message);
	}

	private async Task SelectOnvifProfileAsync()
	{
		CameraItem camera = GetSelectedCamera();
		if (camera == null)
		{
			MessageBox.Show("Zaznacz kamerę ONVIF w drzewie po lewej.", "Heterodyna Vision");
			return;
		}
		Log("ONVIF: pobieranie listy profili dla " + camera.Name);
		List<OnvifProfileInfo> profiles = await OnvifMediaService.TryGetProfilesAsync(camera);
		if (profiles.Count == 0)
		{
			MessageBox.Show("Nie udało się pobrać profili ONVIF. Sprawdź login/hasło ONVIF, czas kamery i uprawnienia.", "ONVIF profile");
			return;
		}
		using OnvifProfileSelectForm dialog = new OnvifProfileSelectForm(profiles);
		if (dialog.ShowDialog(this) == DialogResult.OK && dialog.SelectedProfile != null)
		{
			camera.OnvifProfileToken = dialog.SelectedProfile.Token;
			camera.OnvifProfileName = dialog.SelectedProfile.Name;
			camera.LastStatus = "ONVIF profil: " + dialog.SelectedProfile.Name;
			SaveConfig();
			RebuildTree();
			ApplyUiHotfixes();
			SidebarVisualFix.Apply(this);
			TopBarVisualFix.Apply(this);
			Log("Wybrano ONVIF profile: " + dialog.SelectedProfile.Name + " / " + dialog.SelectedProfile.Token);
		}
	}

	private async Task TestSelectedRtspAsync()
	{
		CameraItem camera = GetSelectedCamera();
		if (camera == null)
		{
			using AddCameraForm dialog = new AddCameraForm();
			if (dialog.ShowDialog(this) != DialogResult.OK || dialog.Camera == null)
			{
				return;
			}
			camera = dialog.Camera;
		}
		Log("Test RTSP: " + camera.Name);
		RtspProbeResult result = await RtspProbeService.ProbeAsync(camera);
		camera.LastStatus = (result.Success ? "RTSP OK" : "RTSP błąd");
		SaveConfig();
		RebuildTree();
		ApplyUiHotfixes();
		SidebarVisualFix.Apply(this);
		TopBarVisualFix.Apply(this);
		Log(result.Message);
		Log(result.Command);
		MessageBox.Show(result.Message + "\n\nKomenda:\n" + result.Command + "\n\nWyjście:\n" + TrimOutput(result.Output), result.Success ? "RTSP OK" : "RTSP błąd");
	}

	private static string TrimOutput(string text)
	{
		if (string.IsNullOrWhiteSpace(text))
		{
			return "(brak)";
		}
		if (text.Length <= 1800)
		{
			return text;
		}
		return text.Substring(0, 1800) + "\n...";
	}

	private void SaveConfig()
	{
		ConfigStore.Save(_cameras);
		Log("Zapisano konfigurację kamer.");
	}

	private void ExportConfig()
	{
		using SaveFileDialog dialog = new SaveFileDialog
		{
			Filter = "JSON konfiguracji|*.json",
			FileName = $"heterodyna-vision-cameras-{DateTime.Now:yyyyMMdd-HHmm}.json"
		};
		if (dialog.ShowDialog(this) == DialogResult.OK)
		{
			ConfigStore.ExportTo(dialog.FileName, _cameras);
			Log("Wyeksportowano konfigurację: " + dialog.FileName);
		}
	}

	private void ImportConfig()
	{
		using OpenFileDialog dialog = new OpenFileDialog
		{
			Filter = "JSON konfiguracji|*.json"
		};
		if (dialog.ShowDialog(this) == DialogResult.OK)
		{
			List<CameraItem> imported = ConfigStore.ImportFrom(dialog.FileName);
			if (MessageBox.Show($"Zaimportować {imported.Count} kamer? Obecna lista zostanie zastąpiona.", "Import", MessageBoxButtons.YesNo) == DialogResult.Yes)
			{
				_cameras.Clear();
				_cameras.AddRange(imported);
				_slotAssignments.Clear();
				SaveSlotAssignments();
				SaveConfig();
				RebuildTree();
				ApplyUiHotfixes();
				SidebarVisualFix.Apply(this);
				TopBarVisualFix.Apply(this);
				BuildCameraGrid(SelectedGridCount());
				Log("Zaimportowano konfigurację: " + dialog.FileName);
			}
		}
	}

	private void Log(string message)
	{
		_log.AppendText($"[{DateTime.Now:HH:mm:ss}] {message}{Environment.NewLine}");
	}
}
