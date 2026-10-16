namespace HeterodynaVision.Discovery;

public sealed class SubnetScanPlan
{
	public string Prefix { get; set; } = "192.168.1.";

	public int StartHost { get; set; } = 1;

	public int EndHost { get; set; } = 254;

	public string DisplayRange => $"{Prefix}{StartHost}-{EndHost}";
}
