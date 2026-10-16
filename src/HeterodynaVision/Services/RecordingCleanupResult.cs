namespace HeterodynaVision.Services;

public sealed class RecordingCleanupResult
{
	public int DeletedFiles { get; set; }

	public long FreedBytes { get; set; }

	public string Message { get; set; } = "";
}
