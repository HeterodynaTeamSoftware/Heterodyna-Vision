using System;

namespace HeterodynaVision.Discovery;

public static class SubnetInputParser
{
	public static SubnetScanPlan Parse(string input)
	{
		input = (input ?? "").Trim();
		if (string.IsNullOrWhiteSpace(input))
		{
			input = NetworkHelper.GuessLocalSubnetPrefix() ?? "192.168.1.";
		}
		if (input.Contains('/'))
		{
			input = input.Split('/')[0].Trim();
		}
		input = input.Replace("*", "").Trim();
		string[] parts = input.Split('.', StringSplitOptions.RemoveEmptyEntries);
		if (parts.Length >= 3 && IsByte(parts[0]) && IsByte(parts[1]) && IsByte(parts[2]))
		{
			return new SubnetScanPlan
			{
				Prefix = $"{parts[0]}.{parts[1]}.{parts[2]}.",
				StartHost = 1,
				EndHost = 254
			};
		}
		throw new ArgumentException("Podaj adres w formacie np. 192.168.2.1, 192.168.2. albo 192.168.2.0/24");
	}

	private static bool IsByte(string value)
	{
		if (int.TryParse(value, out var n) && n >= 0)
		{
			return n <= 255;
		}
		return false;
	}
}
