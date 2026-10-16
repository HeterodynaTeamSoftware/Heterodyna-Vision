using System;

namespace HeterodynaVision.Models;

public sealed class UserAccount
{
	public string Username { get; set; } = "";

	public string PasswordHash { get; set; } = "";

	public string Salt { get; set; } = "";

	public string Role { get; set; } = "Administrator";

	public DateTime CreatedAt { get; set; } = DateTime.Now;
}
