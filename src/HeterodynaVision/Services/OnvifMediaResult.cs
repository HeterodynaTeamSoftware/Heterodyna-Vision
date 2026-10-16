namespace HeterodynaVision.Services;

public sealed class OnvifMediaResult
{
	public bool Success { get; set; }

	public string Message { get; set; } = "";

	public string MediaServiceUrl { get; set; } = "";

	public string ProfileToken { get; set; } = "";

	public string ProfileName { get; set; } = "";

	public string StreamUri { get; set; } = "";

	public string AuthMode { get; set; } = "";

	public string Raw { get; set; } = "";
}
