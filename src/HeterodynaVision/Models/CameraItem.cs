using System;

namespace HeterodynaVision.Models;

public sealed class CameraItem
{
	public string Id { get; set; } = Guid.NewGuid().ToString("N");

	public string Location { get; set; } = "Lokalizacja";

	public string Recorder { get; set; } = "";

	public string Name { get; set; } = "Kamera";

	public string Ip { get; set; } = "";

	public int Port { get; set; } = 554;

	public string Username { get; set; } = "";

	public string Password { get; set; } = "";

	public string StreamUrl { get; set; } = "";

	public string Type { get; set; } = "RTSP";

	public string ProtocolMode { get; set; } = "AUTO";

	public string SourceFormat { get; set; } = "AUTO";

	public string UsbResolution { get; set; } = "";

	public int UsbFps { get; set; }

	public string Vendor { get; set; } = "Generic";

	public string RtspTemplate { get; set; } = "";

	public string OnvifProfileToken { get; set; } = "";

	public string OnvifProfileName { get; set; } = "";

	public string LastStatus { get; set; } = "Nie testowano";
}
