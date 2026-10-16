namespace HeterodynaVision.Services;

public sealed class RtspProbeResult
{
	public bool Success { get; set; }

	public string Message { get; set; } = "";

	public string Command { get; set; } = "";

	public string Output { get; set; } = "";
}
