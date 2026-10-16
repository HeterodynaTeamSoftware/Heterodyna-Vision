using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;

namespace HeterodynaVision.Discovery;

public static class NetworkHelper
{
	public static string? GuessLocalSubnetPrefix()
	{
		try
		{
			NetworkInterface[] allNetworkInterfaces = NetworkInterface.GetAllNetworkInterfaces();
			foreach (NetworkInterface ni in allNetworkInterfaces)
			{
				if (ni.OperationalStatus != OperationalStatus.Up)
				{
					continue;
				}
				foreach (UnicastIPAddressInformation addr in ni.GetIPProperties().UnicastAddresses)
				{
					if (addr.Address.AddressFamily == AddressFamily.InterNetwork && !IPAddress.IsLoopback(addr.Address))
					{
						string[] parts = addr.Address.ToString().Split('.');
						if (parts.Length == 4)
						{
							return $"{parts[0]}.{parts[1]}.{parts[2]}.";
						}
					}
				}
			}
		}
		catch
		{
			return null;
		}
		return null;
	}
}
