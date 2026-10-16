using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace HeterodynaVision.Discovery;

public sealed class NetworkScanner
{
	private readonly Action<string>? _log;

	public NetworkScanner(Action<string>? log = null)
	{
		_log = log;
	}

	public async Task<List<DetectedDevice>> ScanSubnetAsync(string subnetPrefix, int[] ports)
	{
		return await ScanSubnetAsync(new SubnetScanPlan
		{
			Prefix = subnetPrefix,
			StartHost = 1,
			EndHost = 254
		}, ports);
	}

	public async Task<List<DetectedDevice>> ScanSubnetAsync(SubnetScanPlan plan, int[] ports)
	{
		ConcurrentBag<DetectedDevice> results = new ConcurrentBag<DetectedDevice>();
		SemaphoreSlim throttler = new SemaphoreSlim(96);
		try
		{
			await Task.WhenAll(Enumerable.Range(plan.StartHost, plan.EndHost - plan.StartHost + 1).Select((Func<int, Task>)(async (int host) =>
			{
				await throttler.WaitAsync();
				try
				{
					string ip = plan.Prefix + host;
					List<int> openPorts = new List<int>();
					int[] array = ports;
					foreach (int port in array)
					{
						if (await PortScanner.IsPortOpenAsync(ip, port, 450))
						{
							openPorts.Add(port);
						}
					}
					if (openPorts.Count > 0)
					{
						results.Add(new DetectedDevice(ip, openPorts));
						_log?.Invoke("Znaleziono: " + ip + " -> " + string.Join(", ", openPorts));
					}
				}
				finally
				{
					throttler.Release();
				}
			})));
			return results.OrderBy((DetectedDevice x) => IpSortKey(x.Ip)).ToList();
		}
		finally
		{
			if (throttler != null)
			{
				((IDisposable)throttler).Dispose();
			}
		}
	}

	private static int IpSortKey(string ip)
	{
		if (!int.TryParse(ip.Split('.').LastOrDefault(), out var n))
		{
			return 999;
		}
		return n;
	}
}
