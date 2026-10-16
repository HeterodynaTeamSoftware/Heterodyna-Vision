namespace HeterodynaVision.Services;

public sealed class CameraStatusResult
{
	public string StatusIcon { get; set; } = "\ud83d\udfe1";

	public string StatusText { get; set; } = "Brak testu";

	public string Details { get; set; } = "";

	public bool Success { get; set; }
}
