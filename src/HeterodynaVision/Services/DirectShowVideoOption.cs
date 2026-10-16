namespace HeterodynaVision.Services;

public sealed class DirectShowVideoOption
{
	public string Resolution { get; set; } = "";

	public int Fps { get; set; }

	public string Raw { get; set; } = "";

	public override string ToString()
	{
		if (!string.IsNullOrWhiteSpace(Resolution) && Fps > 0)
		{
			return $"{Resolution} @ {Fps} FPS";
		}
		if (!string.IsNullOrWhiteSpace(Raw))
		{
			return Raw;
		}
		return "AUTO";
	}
}
