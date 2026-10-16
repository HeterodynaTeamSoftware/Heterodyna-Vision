using System;
using System.Linq;
using System.Threading.Tasks;
using HeterodynaVision.Discovery;
using HeterodynaVision.Models;
using HeterodynaVision.Vendors;

namespace HeterodynaVision.Services;

public static class ProtocolDetectorService
{
	public static async Task<ProtocolDetectionResult> DetectAsync(CameraItem camera)
	{
		if (!string.IsNullOrWhiteSpace(camera.StreamUrl))
		{
			string detectedFromUrl = StreamFormatHelper.DetectFromUrl(camera.StreamUrl);
			if (!string.Equals(detectedFromUrl, "AUTO", StringComparison.OrdinalIgnoreCase))
			{
				return new ProtocolDetectionResult
				{
					Protocol = detectedFromUrl,
					Message = "Wykryto protokół po adresie URL: " + detectedFromUrl,
					SuggestedUrl = camera.StreamUrl
				};
			}
		}
		if (!string.Equals(camera.ProtocolMode, "AUTO", StringComparison.OrdinalIgnoreCase))
		{
			return new ProtocolDetectionResult
			{
				Protocol = camera.ProtocolMode,
				Message = "Wybrano ręcznie: " + camera.ProtocolMode,
				SuggestedUrl = RtspProbeService.BuildUrl(camera)
			};
		}
		VendorProfile profile = VendorProfileStore.FindBest(camera.Vendor);
		bool flag = await PortScanner.IsPortOpenAsync(camera.Ip, 554, 700);
		if (!flag)
		{
			flag = await PortScanner.IsPortOpenAsync(camera.Ip, camera.Port, 700);
		}
		if (flag)
		{
			ProtocolDetectionResult protocolDetectionResult = new ProtocolDetectionResult
			{
				Protocol = "RTSP",
				Message = "Wykryto otwarty port RTSP. Użyj RTSP auto, aby znaleźć konkretną ścieżkę."
			};
			string t = profile.RtspTemplates.FirstOrDefault();
			protocolDetectionResult.SuggestedUrl = ((t != null) ? BuildTemplate(t, camera) : RtspProbeService.BuildUrl(camera));
			return protocolDetectionResult;
		}
		flag = await PortScanner.IsPortOpenAsync(camera.Ip, 80, 700);
		if (!flag)
		{
			flag = await PortScanner.IsPortOpenAsync(camera.Ip, 8080, 700);
		}
		if (flag)
		{
			return new ProtocolDetectionResult
			{
				Protocol = "HTTP/MJPEG",
				Message = "Wykryto port HTTP. Kamera może wymagać HTTP Snapshot, MJPEG albo panelu WWW.",
				SuggestedUrl = "http://" + camera.Ip + "/"
			};
		}
		return new ProtocolDetectionResult
		{
			Protocol = "UNKNOWN",
			Message = "Nie wykryto RTSP/HTTP. Sprawdź IP, port, firewall, VLAN albo włącz ONVIF/RTSP."
		};
	}

	public static string BuildTemplate(string template, CameraItem camera)
	{
		string auth = "";
		if (!string.IsNullOrWhiteSpace(camera.Username))
		{
			auth = Uri.EscapeDataString(camera.Username) + ":" + Uri.EscapeDataString(camera.Password) + "@";
		}
		return template.Replace("{auth}", auth).Replace("{ip}", camera.Ip.Trim()).Replace("{port}", camera.Port.ToString())
			.Replace("{user}", Uri.EscapeDataString(camera.Username ?? ""))
			.Replace("{pass}", Uri.EscapeDataString(camera.Password ?? ""));
	}
}
