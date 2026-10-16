namespace HeterodynaVision.Services;

public sealed class ProtocolDetectionResult
{
	public string Protocol { get; set; } = "UNKNOWN";

	public string Message { get; set; } = "";

	public string SuggestedUrl { get; set; } = "";
}
