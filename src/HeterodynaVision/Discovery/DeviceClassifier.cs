using System.Collections.Generic;

namespace HeterodynaVision.Discovery;

public static class DeviceClassifier
{
	public static string Classify(List<int> openPorts)
	{
		if (openPorts.Contains(554) && (openPorts.Contains(80) || openPorts.Contains(8080)))
		{
			return "kamera IP / rejestrator";
		}
		if (openPorts.Contains(554))
		{
			return "RTSP kamera / NVR / DVR";
		}
		if (openPorts.Contains(37777))
		{
			return "Dahua / BCS / XVR";
		}
		if (openPorts.Contains(8000))
		{
			return "Hikvision / HiWatch";
		}
		if (openPorts.Contains(34567))
		{
			return "XM / tani DVR";
		}
		if (openPorts.Contains(80) || openPorts.Contains(8080))
		{
			return "panel WWW / możliwa kamera";
		}
		return "nieznane urządzenie";
	}
}
