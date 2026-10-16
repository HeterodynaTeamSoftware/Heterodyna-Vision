using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace HeterodynaVision.Discovery;

public static class OnvifDiscoveryService
{
	private const string MulticastAddress = "239.255.255.250";

	private const int MulticastPort = 3702;

	public static async Task<List<OnvifDevice>> DiscoverAsync(TimeSpan timeout, Action<string>? log = null)
	{
		Dictionary<string, OnvifDevice> devices = new Dictionary<string, OnvifDevice>(StringComparer.OrdinalIgnoreCase);
		using UdpClient udp = new UdpClient(AddressFamily.InterNetwork);
		udp.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, optionValue: true);
		udp.Client.ReceiveTimeout = 1000;
		IPEndPoint endpoint = new IPEndPoint(IPAddress.Parse("239.255.255.250"), 3702);
		string probe = BuildProbeMessage();
		byte[] data = Encoding.UTF8.GetBytes(probe);
		log?.Invoke("ONVIF: wysyłam WS-Discovery probe UDP 3702...");
		await udp.SendAsync(data, data.Length, endpoint);
		using CancellationTokenSource cts = new CancellationTokenSource(timeout);
		while (!cts.IsCancellationRequested)
		{
			try
			{
				Task<UdpReceiveResult> receiveTask = udp.ReceiveAsync();
				Task delayTask = Task.Delay(500, cts.Token);
				if (await Task.WhenAny(receiveTask, delayTask) == receiveTask)
				{
					UdpReceiveResult result = receiveTask.Result;
					string xml = Encoding.UTF8.GetString(result.Buffer);
					string xaddrs = ExtractTag(xml, "XAddrs");
					string types = ExtractTag(xml, "Types");
					string scopes = ExtractTag(xml, "Scopes");
					string key = ((!string.IsNullOrWhiteSpace(xaddrs)) ? xaddrs : result.RemoteEndPoint.Address.ToString());
					if (!devices.ContainsKey(key))
					{
						OnvifDevice device = (devices[key] = new OnvifDevice
						{
							Address = result.RemoteEndPoint.Address.ToString(),
							XAddrs = xaddrs,
							Types = types,
							Scopes = scopes
						});
						log?.Invoke("ONVIF znaleziono: " + device.Address + " " + device.XAddrs);
					}
				}
			}
			catch (OperationCanceledException)
			{
				break;
			}
			catch
			{
			}
		}
		return devices.Values.OrderBy((OnvifDevice x) => x.Address).ToList();
	}

	private static string BuildProbeMessage()
	{
		string uuid = Guid.NewGuid().ToString();
		return "<?xml version=\"1.0\" encoding=\"UTF-8\"?>\n<e:Envelope xmlns:e=\"http://www.w3.org/2003/05/soap-envelope\"\n            xmlns:w=\"http://schemas.xmlsoap.org/ws/2004/08/addressing\"\n            xmlns:d=\"http://schemas.xmlsoap.org/ws/2005/04/discovery\"\n            xmlns:dn=\"http://www.onvif.org/ver10/network/wsdl\">\n  <e:Header>\n    <w:MessageID>uuid:" + uuid + "</w:MessageID>\n    <w:To>urn:schemas-xmlsoap-org:ws:2005:04:discovery</w:To>\n    <w:Action>http://schemas.xmlsoap.org/ws/2005/04/discovery/Probe</w:Action>\n  </e:Header>\n  <e:Body>\n    <d:Probe>\n      <d:Types>dn:NetworkVideoTransmitter</d:Types>\n    </d:Probe>\n  </e:Body>\n</e:Envelope>";
	}

	private static string ExtractTag(string xml, string localName)
	{
		Match match = Regex.Match(xml, $"<(?:[a-zA-Z0-9]+:)?{Regex.Escape(localName)}[^>]*>(.*?)</(?:[a-zA-Z0-9]+:)?{Regex.Escape(localName)}>", RegexOptions.IgnoreCase | RegexOptions.Singleline);
		if (!match.Success)
		{
			return "";
		}
		return WebUtility.HtmlDecode(match.Groups[1].Value.Trim());
	}
}
