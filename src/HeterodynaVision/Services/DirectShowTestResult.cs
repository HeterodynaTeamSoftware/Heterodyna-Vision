namespace HeterodynaVision.Services;

public sealed class DirectShowTestResult
{
	public bool Success { get; set; }

	public string Message { get; set; } = "";

	public string FramePath { get; set; } = "";

	public string Output { get; set; } = "";
}
