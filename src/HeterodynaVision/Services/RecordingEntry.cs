using System;

namespace HeterodynaVision.Services;

public sealed class RecordingEntry
{
	public string FilePath { get; set; } = "";

	public string CameraName { get; set; } = "";

	public DateTime CreatedAt { get; set; }

	public long SizeBytes { get; set; }
}
