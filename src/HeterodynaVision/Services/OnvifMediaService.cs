using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using HeterodynaVision.Models;

namespace HeterodynaVision.Services;

public static class OnvifMediaService
{
	public static async Task<List<OnvifProfileInfo>> TryGetProfilesAsync(CameraItem camera)
	{
		List<OnvifProfileInfo> digest = await TryGetProfilesInternalAsync(camera, useDigest: true);
		if (digest.Count > 0)
		{
			return digest;
		}
		return await TryGetProfilesInternalAsync(camera, useDigest: false);
	}

	public static async Task<OnvifMediaResult> TryGetStreamUriAsync(CameraItem camera)
	{
		OnvifMediaResult digest = await TryGetStreamUriInternalAsync(camera, useDigest: true);
		if (digest.Success)
		{
			return digest;
		}
		OnvifMediaResult plain = await TryGetStreamUriInternalAsync(camera, useDigest: false);
		if (plain.Success)
		{
			return plain;
		}
		return new OnvifMediaResult
		{
			Success = false,
			Message = "ONVIF Media nie pobrał RTSP. Digest: " + digest.Message + " | Plain: " + plain.Message,
			Raw = digest.Raw + Environment.NewLine + plain.Raw
		};
	}

	private static async Task<List<OnvifProfileInfo>> TryGetProfilesInternalAsync(CameraItem camera, bool useDigest)
	{
		_ = 1;
		try
		{
			string mediaUrl = ExtractMediaUrl(await PostSoapAsync(BuildDeviceServiceUrl(camera), BuildGetCapabilitiesEnvelope(camera.Username, camera.Password, useDigest)));
			if (string.IsNullOrWhiteSpace(mediaUrl))
			{
				mediaUrl = "http://" + camera.Ip + "/onvif/Media";
			}
			return ExtractProfiles(await PostSoapAsync(mediaUrl, BuildGetProfilesEnvelope(camera.Username, camera.Password, useDigest)));
		}
		catch
		{
			return new List<OnvifProfileInfo>();
		}
	}

	private static async Task<OnvifMediaResult> TryGetStreamUriInternalAsync(CameraItem camera, bool useDigest)
	{
		OnvifMediaResult result = new OnvifMediaResult
		{
			AuthMode = (useDigest ? "PasswordDigest" : "PasswordText")
		};
		if (string.IsNullOrWhiteSpace(camera.Ip))
		{
			result.Message = "Brak IP kamery.";
			return result;
		}
		string deviceUrl = BuildDeviceServiceUrl(camera);
		try
		{
			string capabilitiesXml = (result.Raw = await PostSoapAsync(deviceUrl, BuildGetCapabilitiesEnvelope(camera.Username, camera.Password, useDigest)));
			string mediaUrl = ExtractMediaUrl(capabilitiesXml);
			if (string.IsNullOrWhiteSpace(mediaUrl))
			{
				mediaUrl = "http://" + camera.Ip + "/onvif/Media";
			}
			result.MediaServiceUrl = mediaUrl;
			string profilesXml = await PostSoapAsync(mediaUrl, BuildGetProfilesEnvelope(camera.Username, camera.Password, useDigest));
			List<OnvifProfileInfo> profiles = ExtractProfiles(profilesXml);
			string token = camera.OnvifProfileToken;
			string profileName = camera.OnvifProfileName;
			if (string.IsNullOrWhiteSpace(token))
			{
				OnvifProfileInfo onvifProfileInfo = ChooseBestProfile(profiles);
				token = onvifProfileInfo.Token;
				profileName = onvifProfileInfo.Name;
			}
			if (string.IsNullOrWhiteSpace(token))
			{
				result.Message = "ONVIF działa częściowo, ale nie udało się pobrać profilu Media. Auth=" + result.AuthMode;
				result.Raw = result.Raw + Environment.NewLine + profilesXml;
				return result;
			}
			result.ProfileToken = token;
			result.ProfileName = profileName;
			string uriXml = await PostSoapAsync(mediaUrl, BuildGetStreamUriEnvelope(camera.Username, camera.Password, token, useDigest));
			string streamUri = ExtractStreamUri(uriXml);
			OnvifMediaResult onvifMediaResult = result;
			onvifMediaResult.Raw = onvifMediaResult.Raw + Environment.NewLine + profilesXml + Environment.NewLine + uriXml;
			if (string.IsNullOrWhiteSpace(streamUri))
			{
				result.Message = "Nie udało się pobrać StreamUri z ONVIF. Auth=" + result.AuthMode;
				return result;
			}
			result.Success = true;
			result.StreamUri = streamUri;
			result.Message = "Pobrano RTSP z ONVIF Media Service. Auth=" + result.AuthMode;
			return result;
		}
		catch (Exception ex)
		{
			result.Success = false;
			result.Message = "Błąd ONVIF Media Auth=" + result.AuthMode + ": " + ex.Message;
			return result;
		}
	}

	private static OnvifProfileInfo ChooseBestProfile(List<OnvifProfileInfo> profiles)
	{
		if (profiles.Count == 0)
		{
			return new OnvifProfileInfo();
		}
		return profiles.FirstOrDefault((OnvifProfileInfo p) => p.Name.Contains("main", StringComparison.OrdinalIgnoreCase) || p.Name.Contains("high", StringComparison.OrdinalIgnoreCase) || p.Token.Contains("main", StringComparison.OrdinalIgnoreCase)) ?? profiles[0];
	}

	private static string BuildDeviceServiceUrl(CameraItem camera)
	{
		string host = camera.Ip.Trim();
		if (host.StartsWith("http://", StringComparison.OrdinalIgnoreCase) || host.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
		{
			return host.TrimEnd('/') + "/onvif/device_service";
		}
		return "http://" + host + "/onvif/device_service";
	}

	private static async Task<string> PostSoapAsync(string url, string body)
	{
		using HttpClient client = new HttpClient();
		client.Timeout = TimeSpan.FromSeconds(8.0);
		using StringContent content = new StringContent(body, Encoding.UTF8, "application/soap+xml");
		content.Headers.ContentType.CharSet = "utf-8";
		HttpResponseMessage response = await client.PostAsync(url, content);
		string text = await response.Content.ReadAsStringAsync();
		if (!response.IsSuccessStatusCode)
		{
			throw new InvalidOperationException($"HTTP {(int)response.StatusCode}: {response.ReasonPhrase} {text}");
		}
		return text;
	}

	private static string BuildGetCapabilitiesEnvelope(string user, string pass, bool useDigest)
	{
		return WithSecurity(user, pass, useDigest, "<tds:GetCapabilities xmlns:tds=\"http://www.onvif.org/ver10/device/wsdl\">\n  <tds:Category>All</tds:Category>\n</tds:GetCapabilities>");
	}

	private static string BuildGetProfilesEnvelope(string user, string pass, bool useDigest)
	{
		return WithSecurity(user, pass, useDigest, "<trt:GetProfiles xmlns:trt=\"http://www.onvif.org/ver10/media/wsdl\" />");
	}

	private static string BuildGetStreamUriEnvelope(string user, string pass, string token, bool useDigest)
	{
		return WithSecurity(user, pass, useDigest, "<trt:GetStreamUri xmlns:trt=\"http://www.onvif.org/ver10/media/wsdl\">\n  <trt:StreamSetup>\n    <tt:Stream xmlns:tt=\"http://www.onvif.org/ver10/schema\">RTP-Unicast</tt:Stream>\n    <tt:Transport xmlns:tt=\"http://www.onvif.org/ver10/schema\">\n      <tt:Protocol>RTSP</tt:Protocol>\n    </tt:Transport>\n  </trt:StreamSetup>\n  <trt:ProfileToken>" + SecurityElementEscape(token) + "</trt:ProfileToken>\n</trt:GetStreamUri>");
	}

	private static string WithSecurity(string user, string pass, bool useDigest, string body)
	{
		string security = "";
		if (!string.IsNullOrWhiteSpace(user))
		{
			security = (useDigest ? BuildDigestSecurity(user, pass) : BuildPlainSecurity(user, pass));
		}
		return $"<?xml version=\"1.0\" encoding=\"UTF-8\"?>\n<s:Envelope xmlns:s=\"http://www.w3.org/2003/05/soap-envelope\">\n  <s:Header>\n    {security}\n  </s:Header>\n  <s:Body>\n    {body}\n  </s:Body>\n</s:Envelope>";
	}

	private static string BuildPlainSecurity(string user, string pass)
	{
		return $"<wsse:Security s:mustUnderstand=\"1\"\n xmlns:wsse=\"http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-secext-1.0.xsd\">\n  <wsse:UsernameToken>\n    <wsse:Username>{SecurityElementEscape(user)}</wsse:Username>\n    <wsse:Password Type=\"http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-username-token-profile-1.0#PasswordText\">{SecurityElementEscape(pass)}</wsse:Password>\n  </wsse:UsernameToken>\n</wsse:Security>";
	}

	private static string BuildDigestSecurity(string user, string pass)
	{
		byte[] nonceBytes = RandomNumberGenerator.GetBytes(16);
		string nonceB64 = Convert.ToBase64String(nonceBytes);
		string created = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss.fffZ");
		byte[] createdBytes = Encoding.UTF8.GetBytes(created);
		byte[] passwordBytes = Encoding.UTF8.GetBytes(pass ?? "");
		byte[] combined = new byte[nonceBytes.Length + createdBytes.Length + passwordBytes.Length];
		Buffer.BlockCopy(nonceBytes, 0, combined, 0, nonceBytes.Length);
		Buffer.BlockCopy(createdBytes, 0, combined, nonceBytes.Length, createdBytes.Length);
		Buffer.BlockCopy(passwordBytes, 0, combined, nonceBytes.Length + createdBytes.Length, passwordBytes.Length);
		string digest = Convert.ToBase64String(SHA1.HashData(combined));
		return $"<wsse:Security s:mustUnderstand=\"1\"\n xmlns:wsse=\"http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-secext-1.0.xsd\"\n xmlns:wsu=\"http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-utility-1.0.xsd\">\n  <wsse:UsernameToken>\n    <wsse:Username>{SecurityElementEscape(user)}</wsse:Username>\n    <wsse:Password Type=\"http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-username-token-profile-1.0#PasswordDigest\">{digest}</wsse:Password>\n    <wsse:Nonce EncodingType=\"http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-soap-message-security-1.0#Base64Binary\">{nonceB64}</wsse:Nonce>\n    <wsu:Created>{created}</wsu:Created>\n  </wsse:UsernameToken>\n</wsse:Security>";
	}

	private static string ExtractMediaUrl(string xml)
	{
		Match mediaBlock = Regex.Match(xml, "<[^>]*Media[^>]*>.*?<[^>]*XAddr[^>]*>(.*?)</[^>]*XAddr>.*?</[^>]*Media>", RegexOptions.IgnoreCase | RegexOptions.Singleline);
		if (mediaBlock.Success)
		{
			return WebUtility.HtmlDecode(mediaBlock.Groups[1].Value.Trim());
		}
		Match match = Regex.Match(xml, "<[^>]*XAddr[^>]*>(.*?)</[^>]*XAddr>", RegexOptions.IgnoreCase | RegexOptions.Singleline);
		if (!match.Success)
		{
			return "";
		}
		return WebUtility.HtmlDecode(match.Groups[1].Value.Trim());
	}

	private static List<OnvifProfileInfo> ExtractProfiles(string xml)
	{
		List<OnvifProfileInfo> profiles = new List<OnvifProfileInfo>();
		foreach (Match item in Regex.Matches(xml, "<[^>]*Profiles[^>]*token\\s*=\\s*[\"']([^\"']+)[\"'][^>]*>(.*?)</[^>]*Profiles>", RegexOptions.IgnoreCase | RegexOptions.Singleline))
		{
			string token = WebUtility.HtmlDecode(item.Groups[1].Value.Trim());
			string name = ExtractFirstTag(item.Groups[2].Value, "Name");
			profiles.Add(new OnvifProfileInfo
			{
				Token = token,
				Name = (string.IsNullOrWhiteSpace(name) ? token : name)
			});
		}
		if (profiles.Count == 0)
		{
			foreach (Match match2 in Regex.Matches(xml, "token\\s*=\\s*[\"']([^\"']+)[\"']", RegexOptions.IgnoreCase))
			{
				string token2 = WebUtility.HtmlDecode(match2.Groups[1].Value.Trim());
				if (!profiles.Any((OnvifProfileInfo p) => p.Token == token2))
				{
					profiles.Add(new OnvifProfileInfo
					{
						Token = token2,
						Name = token2
					});
				}
			}
		}
		return profiles;
	}

	private static string ExtractFirstTag(string xml, string tag)
	{
		Match match = Regex.Match(xml, $"<[^>]*{tag}[^>]*>(.*?)</[^>]*{tag}>", RegexOptions.IgnoreCase | RegexOptions.Singleline);
		if (!match.Success)
		{
			return "";
		}
		return WebUtility.HtmlDecode(match.Groups[1].Value.Trim());
	}

	private static string ExtractStreamUri(string xml)
	{
		Match match = Regex.Match(xml, "<[^>]*Uri[^>]*>(.*?)</[^>]*Uri>", RegexOptions.IgnoreCase | RegexOptions.Singleline);
		if (!match.Success)
		{
			return "";
		}
		return WebUtility.HtmlDecode(match.Groups[1].Value.Trim());
	}

	private static string SecurityElementEscape(string value)
	{
		return (value ?? "").Replace("&", "&amp;").Replace("<", "&lt;").Replace(">", "&gt;")
			.Replace("\"", "&quot;")
			.Replace("'", "&apos;");
	}
}
