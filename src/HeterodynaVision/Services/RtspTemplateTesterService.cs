using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using HeterodynaVision.Models;
using HeterodynaVision.Vendors;

namespace HeterodynaVision.Services;

public static class RtspTemplateTesterService
{
	public static async Task<RtspTemplateTestResult> TestTemplatesAsync(CameraItem camera, Action<string>? log = null)
	{
		VendorProfile profile = VendorProfileStore.FindBest(camera.Vendor);
		List<string> templates = ((profile.RtspTemplates.Count > 0) ? profile.RtspTemplates : VendorProfileStore.FindBest("Generic").RtspTemplates);
		int tested = 0;
		foreach (string template in templates)
		{
			tested++;
			string url = ProtocolDetectorService.BuildTemplate(template, camera);
			log?.Invoke($"RTSP template [{tested}/{templates.Count}]: {url}");
			if ((await RtspProbeService.ProbeAsync(new CameraItem
			{
				Name = camera.Name,
				Ip = camera.Ip,
				Port = camera.Port,
				Username = camera.Username,
				Password = camera.Password,
				StreamUrl = url,
				Vendor = camera.Vendor,
				ProtocolMode = "RTSP",
				Type = "RTSP"
			})).Success)
			{
				return new RtspTemplateTestResult
				{
					Success = true,
					Url = url,
					Vendor = profile.Vendor,
					TestedCount = tested,
					Message = $"Znaleziono działający RTSP dla profilu {profile.Vendor} po {tested} próbach."
				};
			}
		}
		return new RtspTemplateTestResult
		{
			Success = false,
			Vendor = profile.Vendor,
			TestedCount = tested,
			Message = $"Nie znaleziono działającego RTSP. Przetestowano {tested} szablonów."
		};
	}
}
