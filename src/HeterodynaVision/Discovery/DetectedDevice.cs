using System.Collections.Generic;

namespace HeterodynaVision.Discovery;

public sealed record DetectedDevice(string Ip, List<int> OpenPorts);
