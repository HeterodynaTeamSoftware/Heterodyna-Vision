using System.Collections.Generic;

namespace HeterodynaVision.Vendors;

public sealed class VendorProfile
{
	public string Vendor { get; set; } = "Generic";

	public string Family { get; set; } = "";

	public List<int> Ports { get; set; } = new List<int>();

	public List<string> Protocols { get; set; } = new List<string>();

	public List<string> RtspTemplates { get; set; } = new List<string>();

	public string Notes { get; set; } = "";
}
