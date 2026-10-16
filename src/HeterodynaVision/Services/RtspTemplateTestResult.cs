namespace HeterodynaVision.Services;

public sealed class RtspTemplateTestResult
{
	public bool Success { get; set; }

	public string Url { get; set; } = "";

	public string Message { get; set; } = "";

	public string Vendor { get; set; } = "";

	public int TestedCount { get; set; }
}
