using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace HeterodynaVision.Security;

public static class AuthStore
{
	private static readonly string DataDir = Path.Combine(AppContext.BaseDirectory, "data");

	private static readonly string UsersPath = Path.Combine(DataDir, "users.json");

	public static bool HasAdmin()
	{
		return LoadUsers().Any((AuthUser x) => string.Equals(x.Role, "Administrator", StringComparison.OrdinalIgnoreCase));
	}

	public static void CreateUser(string login, string password, string role)
	{
		login = (login ?? "").Trim();
		if (string.IsNullOrWhiteSpace(login))
		{
			throw new ArgumentException("Login nie może być pusty.", "login");
		}
		if (string.IsNullOrWhiteSpace(password))
		{
			throw new ArgumentException("Hasło nie może być puste.", "password");
		}
		List<AuthUser> list = LoadUsers();
		if (list.Any((AuthUser x) => string.Equals(x.Login, login, StringComparison.OrdinalIgnoreCase)))
		{
			throw new InvalidOperationException("Użytkownik już istnieje.");
		}
		string salt = Convert.ToBase64String(RandomNumberGenerator.GetBytes(16));
		list.Add(new AuthUser
		{
			Login = login,
			Salt = salt,
			PasswordHash = HashPassword(password, salt),
			Role = (string.IsNullOrWhiteSpace(role) ? "Operator" : role)
		});
		SaveUsers(list);
	}

	public static bool Validate(string login, string password, out string role)
	{
		role = "";
		login = (login ?? "").Trim();
		AuthUser user = LoadUsers().FirstOrDefault((AuthUser x) => string.Equals(x.Login, login, StringComparison.OrdinalIgnoreCase));
		if (user == null)
		{
			return false;
		}
		string hash = HashPassword(password ?? "", user.Salt);
		if (!CryptographicOperations.FixedTimeEquals(Encoding.UTF8.GetBytes(hash), Encoding.UTF8.GetBytes(user.PasswordHash)))
		{
			return false;
		}
		role = user.Role;
		return true;
	}

	private static List<AuthUser> LoadUsers()
	{
		try
		{
			Directory.CreateDirectory(DataDir);
			if (!File.Exists(UsersPath))
			{
				return new List<AuthUser>();
			}
			return JsonSerializer.Deserialize<List<AuthUser>>(File.ReadAllText(UsersPath)) ?? new List<AuthUser>();
		}
		catch
		{
			return new List<AuthUser>();
		}
	}

	private static void SaveUsers(List<AuthUser> users)
	{
		Directory.CreateDirectory(DataDir);
		string json = JsonSerializer.Serialize(users, new JsonSerializerOptions
		{
			WriteIndented = true
		});
		File.WriteAllText(UsersPath, json);
	}

	private static string HashPassword(string password, string salt)
	{
		using SHA256 sha = SHA256.Create();
		byte[] data = Encoding.UTF8.GetBytes(salt + "|" + password);
		return Convert.ToBase64String(sha.ComputeHash(data));
	}
}
