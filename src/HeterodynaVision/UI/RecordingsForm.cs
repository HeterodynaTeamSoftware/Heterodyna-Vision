using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using HeterodynaVision.Services;
using HeterodynaVision.Storage;

namespace HeterodynaVision.UI;

public sealed class RecordingsForm : Form
{
	private readonly ListView _recordings = new ListView();

	private readonly TextBox _motionLog = new TextBox();

	public RecordingsForm()
	{
		Text = "Historia ruchu i nagrań";
		Width = 980;
		Height = 620;
		StartPosition = FormStartPosition.CenterParent;
		BackColor = ModernTheme.Background;
		ForeColor = ModernTheme.Text;
		DarkNative.ApplyDarkTitleBar(this);
		BuildUi();
		RefreshData();
	}

	private void BuildUi()
	{
		TableLayoutPanel root = new TableLayoutPanel
		{
			Dock = DockStyle.Fill,
			Padding = new Padding(10),
			RowCount = 3,
			ColumnCount = 1
		};
		root.RowStyles.Add(new RowStyle(SizeType.Absolute, 42f));
		root.RowStyles.Add(new RowStyle(SizeType.Percent, 60f));
		root.RowStyles.Add(new RowStyle(SizeType.Percent, 40f));
		Controls.Add(root);
		FlowLayoutPanel toolbar = new FlowLayoutPanel
		{
			Dock = DockStyle.Fill,
			FlowDirection = FlowDirection.LeftToRight
		};
		Button refresh = new Button
		{
			Text = "Odśwież",
			Width = 100,
			Height = 30
		};
		refresh.Click += (object? _, EventArgs _) =>
		{
			RefreshData();
		};
		Button openFolder = new Button
		{
			Text = "Otwórz folder nagrań",
			Width = 160,
			Height = 30
		};
		openFolder.Click += (object? _, EventArgs _) =>
		{
			OpenFolder(RecordingIndexService.RecordingsRoot);
		};
		Button openSelected = new Button
		{
			Text = "Otwórz wybrane MP4",
			Width = 150,
			Height = 30
		};
		openSelected.Click += (object? _, EventArgs _) =>
		{
			OpenSelectedRecording();
		};
		Button openLog = new Button
		{
			Text = "Otwórz log ruchu",
			Width = 130,
			Height = 30
		};
		openLog.Click += (object? _, EventArgs _) =>
		{
			OpenFile(MotionEventStore.GetPath());
		};
		Button cleanup7 = new Button
		{
			Text = "Usuń starsze niż 7 dni",
			Width = 170,
			Height = 30
		};
		cleanup7.Click += (object? _, EventArgs _) =>
		{
			CleanupOldRecordings(7);
		};
		Button cleanup30 = new Button
		{
			Text = "Usuń starsze niż 30 dni",
			Width = 180,
			Height = 30
		};
		cleanup30.Click += (object? _, EventArgs _) =>
		{
			CleanupOldRecordings(30);
		};
		toolbar.Controls.Add(refresh);
		toolbar.Controls.Add(openFolder);
		toolbar.Controls.Add(openSelected);
		toolbar.Controls.Add(openLog);
		toolbar.Controls.Add(cleanup7);
		toolbar.Controls.Add(cleanup30);
		root.Controls.Add(toolbar, 0, 0);
		_recordings.Dock = DockStyle.Fill;
		_recordings.View = View.Details;
		_recordings.FullRowSelect = true;
		_recordings.Columns.Add("Data", 150);
		_recordings.Columns.Add("Kamera", 170);
		_recordings.Columns.Add("Rozmiar", 90);
		_recordings.Columns.Add("Plik", 520);
		_recordings.DoubleClick += (object? _, EventArgs _) =>
		{
			OpenSelectedRecording();
		};
		root.Controls.Add(_recordings, 0, 1);
		_motionLog.Dock = DockStyle.Fill;
		_motionLog.Multiline = true;
		_motionLog.ScrollBars = ScrollBars.Vertical;
		_motionLog.ReadOnly = true;
		_motionLog.Font = new Font("Consolas", 9f);
		root.Controls.Add(_motionLog, 0, 2);
	}

	private void RefreshData()
	{
		_recordings.Items.Clear();
		foreach (RecordingEntry rec in RecordingIndexService.GetRecordings())
		{
			ListViewItem item = new ListViewItem(rec.CreatedAt.ToString("yyyy-MM-dd HH:mm:ss"));
			item.SubItems.Add(rec.CameraName);
			item.SubItems.Add(RecordingIndexService.FormatSize(rec.SizeBytes));
			item.SubItems.Add(rec.FilePath);
			item.Tag = rec.FilePath;
			_recordings.Items.Add(item);
		}
		string logPath = MotionEventStore.GetPath();
		if (File.Exists(logPath))
		{
			_motionLog.Text = File.ReadAllText(logPath);
		}
		else
		{
			_motionLog.Text = "Brak logu ruchu: " + logPath;
		}
	}

	private void CleanupOldRecordings(int days)
	{
		if (MessageBox.Show($"Usunąć nagrania starsze niż {days} dni?", "Czyszczenie nagrań", MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation) == DialogResult.Yes)
		{
			MessageBox.Show(RecordingCleanupService.DeleteOlderThanDays(days).Message, "Czyszczenie nagrań");
			RefreshData();
		}
	}

	private void OpenSelectedRecording()
	{
		if (_recordings.SelectedItems.Count == 0)
		{
			MessageBox.Show("Zaznacz nagranie MP4.", "Heterodyna Vision");
			return;
		}
		string file = _recordings.SelectedItems[0].Tag as string;
		if (!string.IsNullOrWhiteSpace(file))
		{
			OpenFile(file);
		}
	}

	private static void OpenFolder(string path)
	{
		try
		{
			Directory.CreateDirectory(path);
			Process.Start(new ProcessStartInfo
			{
				FileName = path,
				UseShellExecute = true
			});
		}
		catch (Exception ex)
		{
			MessageBox.Show("Nie udało się otworzyć folderu: " + ex.Message);
		}
	}

	private static void OpenFile(string path)
	{
		try
		{
			if (!File.Exists(path))
			{
				MessageBox.Show("Plik nie istnieje: " + path);
				return;
			}
			Process.Start(new ProcessStartInfo
			{
				FileName = path,
				UseShellExecute = true
			});
		}
		catch (Exception ex)
		{
			MessageBox.Show("Nie udało się otworzyć pliku: " + ex.Message);
		}
	}

	protected override void OnShown(EventArgs e)
	{
		base.OnShown(e);
		DarkNative.ApplyDarkTitleBar(this);
		DarkNative.ApplyDarkExplorerTheme(this);
	}
}
