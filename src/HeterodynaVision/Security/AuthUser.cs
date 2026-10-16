namespace HeterodynaVision.Security;

public sealed class AuthUser
{
	public string Login { get; set; } = "";

	public string PasswordHash { get; set; } = "";

	public string Salt { get; set; } = "";

	public string Role { get; set; } = "Operator";
}
