using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using HeterodynaVision.Models;

namespace HeterodynaVision.Services;

public static class UserStore
{
	private static readonly string DataDir = Path.Combine(AppContext.BaseDirectory, "data");

	private static readonly string UsersPath = Path.Combine(DataDir, "users.json");

	public static bool HasAnyUser()
	{
		return LoadUsers().Count > 0;
	}

	public static List<UserAccount> LoadUsers()
	{
		try
		{
			Directory.CreateDirectory(DataDir);
			if (!File.Exists(UsersPath))
			{
				return new List<UserAccount>();
			}
			return JsonSerializer.Deserialize<List<UserAccount>>(File.ReadAllText(UsersPath)) ?? new List<UserAccount>();
		}
		catch
		{
			return new List<UserAccount>();
		}
	}

	public static void SaveUsers(List<UserAccount> users)
	{
		Directory.CreateDirectory(DataDir);
		JsonSerializerOptions options = new JsonSerializerOptions
		{
			WriteIndented = true
		};
		File.WriteAllText(UsersPath, JsonSerializer.Serialize(users, options));
	}

	public static (bool ok, string message) Register(string username, string password, string role = "Administrator")
	{
		username = username.Trim();
		if (username.Length < 3)
		{
			return (ok: false, message: "Login musi mieć minimum 3 znaki.");
		}
		if (password.Length < 6)
		{
			return (ok: false, message: "Hasło musi mieć minimum 6 znaków.");
		}
		List<UserAccount> users = LoadUsers();
		if (users.Any((UserAccount u) => string.Equals(u.Username, username, StringComparison.OrdinalIgnoreCase)))
		{
			return (ok: false, message: "Taki użytkownik już istnieje.");
		}
		string salt = CreateSalt();
		users.Add(new UserAccount
		{
			Username = username,
			Salt = salt,
			PasswordHash = HashPassword(password, salt),
			Role = role,
			CreatedAt = DateTime.Now
		});
		SaveUsers(users);
		return (ok: true, message: "Użytkownik został utworzony.");
	}

	public static UserAccount? Login(string username, string password)
	{
		username = username.Trim();
		UserAccount user = LoadUsers().FirstOrDefault((UserAccount u) => string.Equals(u.Username, username, StringComparison.OrdinalIgnoreCase));
		if (user == null)
		{
			return null;
		}
		if (!(HashPassword(password, user.Salt) == user.PasswordHash))
		{
			return null;
		}
		return user;
	}

	private static string CreateSalt()
	{
		return Convert.ToBase64String(RandomNumberGenerator.GetBytes(16));
	}

	private static string HashPassword(string password, string salt)
	{
		byte[] saltBytes = Convert.FromBase64String(salt);
		using Rfc2898DeriveBytes pbkdf2 = new Rfc2898DeriveBytes(Encoding.UTF8.GetBytes(password), saltBytes, 120000, HashAlgorithmName.SHA256);
		return Convert.ToBase64String(pbkdf2.GetBytes(32));
	}
}
