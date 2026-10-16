using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;
using HeterodynaVision.Models;
using HeterodynaVision.Services;

namespace HeterodynaVision.UI;

public sealed class CameraStatusForm : Form
{
	private readonly ListView _list = new ListView();

	private readonly TextBox _details = new TextBox();

	private readonly List<CameraItem> _cameras;

	public CameraStatusForm(List<CameraItem> cameras)
	{
		_cameras = cameras;
		Text = "Status kamer";
		Width = 1050;
		Height = 650;
		StartPosition = FormStartPosition.CenterParent;
		BackColor = ModernTheme.Background;
		ForeColor = ModernTheme.Text;
		DarkNative.ApplyDarkTitleBar(this);
		BuildUi();
		FillInitial();
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
		root.RowStyles.Add(new RowStyle(SizeType.Percent, 62f));
		root.RowStyles.Add(new RowStyle(SizeType.Percent, 38f));
		Controls.Add(root);
		FlowLayoutPanel toolbar = new FlowLayoutPanel
		{
			Dock = DockStyle.Fill,
			FlowDirection = FlowDirection.LeftToRight
		};
		Button testSelected = new Button
		{
			Text = "Test wybranej",
			Width = 130,
			Height = 30
		};
		testSelected.Click += async (object? _, EventArgs _) =>
		{
			await TestSelectedAsync();
		};
		Button testAll = new Button
		{
			Text = "Test wszystkich",
			Width = 130,
			Height = 30
		};
		testAll.Click += async (object? _, EventArgs _) =>
		{
			await TestAllAsync();
		};
		Button close = new Button
		{
			Text = "Zamknij",
			Width = 100,
			Height = 30
		};
		close.Click += (object? _, EventArgs _) =>
		{
			Close();
		};
		toolbar.Controls.Add(testSelected);
		toolbar.Controls.Add(testAll);
		toolbar.Controls.Add(close);
		root.Controls.Add(toolbar, 0, 0);
		_list.Dock = DockStyle.Fill;
		_list.View = View.Details;
		_list.FullRowSelect = true;
		_list.Columns.Add("Status", 80);
		_list.Columns.Add("Nazwa", 190);
		_list.Columns.Add("Format", 130);
		_list.Columns.Add("IP / Źródło", 230);
		_list.Columns.Add("Wynik", 180);
		_list.Columns.Add("URL", 320);
		_list.SelectedIndexChanged += (object? _, EventArgs _) =>
		{
			ShowSelectedDetails();
		};
		root.Controls.Add(_list, 0, 1);
		_details.Dock = DockStyle.Fill;
		_details.Multiline = true;
		_details.ReadOnly = true;
		_details.ScrollBars = ScrollBars.Vertical;
		_details.Font = new Font("Consolas", 9f);
		root.Controls.Add(_details, 0, 2);
	}

	private void FillInitial()
	{
		_list.Items.Clear();
		foreach (CameraItem camera in _cameras)
		{
			ListViewItem item = new ListViewItem("\ud83d\udfe1");
			item.SubItems.Add(camera.Name);
			item.SubItems.Add(camera.SourceFormat);
			item.SubItems.Add(string.IsNullOrWhiteSpace(camera.Ip) ? camera.StreamUrl : camera.Ip);
			item.SubItems.Add(camera.LastStatus);
			item.SubItems.Add(camera.StreamUrl);
			item.Tag = camera;
			_list.Items.Add(item);
		}
	}

	private async Task TestSelectedAsync()
	{
		if (_list.SelectedItems.Count == 0)
		{
			MessageBox.Show("Zaznacz kamerę.", "Heterodyna Vision");
		}
		else
		{
			await TestItemAsync(_list.SelectedItems[0]);
		}
	}

	private async Task TestAllAsync()
	{
		foreach (ListViewItem item in _list.Items)
		{
			await TestItemAsync(item);
		}
	}

	private async Task TestItemAsync(ListViewItem item)
	{
		object tag = item.Tag;
		if (tag is CameraItem camera)
		{
			item.SubItems[0].Text = "\ud83d\udfe1";
			item.SubItems[4].Text = "Testuję...";
			_details.Text = "Testuję: " + camera.Name;
			CameraStatusResult result = await CameraStatusService.TestAsync(camera);
			item.SubItems[0].Text = result.StatusIcon;
			item.SubItems[4].Text = result.StatusText;
			item.Tag = camera;
			camera.LastStatus = result.StatusIcon + " " + result.StatusText;
			_details.Text = result.Details;
		}
	}

	private void ShowSelectedDetails()
	{
		if (_list.SelectedItems.Count != 0 && _list.SelectedItems[0].Tag is CameraItem camera)
		{
			_details.Text = "Nazwa: " + camera.Name + Environment.NewLine + "Format: " + camera.SourceFormat + Environment.NewLine + "Protokół: " + camera.ProtocolMode + Environment.NewLine + "IP/Źródło: " + camera.Ip + Environment.NewLine + "Port: " + camera.Port + Environment.NewLine + "URL: " + camera.StreamUrl + Environment.NewLine + "Status: " + camera.LastStatus;
		}
	}

	protected override void OnShown(EventArgs e)
	{
		base.OnShown(e);
		DarkNative.ApplyDarkTitleBar(this);
		DarkNative.ApplyDarkExplorerTheme(this);
	}
}
