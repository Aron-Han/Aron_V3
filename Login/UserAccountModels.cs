using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Windows.Forms;
using System.Xml.Serialization;

namespace Aron_V3
{
	public class UserPermission
	{
		public bool CanRun { get; set; }
		public bool CanHardwareConfig { get; set; }
		public bool CanAlgorithmConfig { get; set; }
		public bool CanFlowConfig { get; set; }
		public bool CanCommunicationConfig { get; set; }
		public bool CanDatabaseConfig { get; set; }
		public bool CanSystemConfig { get; set; }
		public bool CanUserManagement { get; set; }

		public UserPermission()
		{
			CanRun = true;
			CanHardwareConfig = false;
			CanAlgorithmConfig = false;
			CanFlowConfig = false;
			CanCommunicationConfig = false;
			CanDatabaseConfig = false;
			CanSystemConfig = false;
			CanUserManagement = false;
		}

		public static UserPermission CreateAdminPermission()
		{
			return new UserPermission
			{
				CanRun = true,
				CanHardwareConfig = true,
				CanAlgorithmConfig = true,
				CanFlowConfig = true,
				CanCommunicationConfig = true,
				CanDatabaseConfig = true,
				CanSystemConfig = true,
				CanUserManagement = true
			};
		}

		public static UserPermission CreateEngineerPermission()
		{
			return new UserPermission
			{
				CanRun = true,
				CanHardwareConfig = true,
				CanAlgorithmConfig = true,
				CanFlowConfig = true,
				CanCommunicationConfig = true,
				CanDatabaseConfig = true,
				CanSystemConfig = false,
				CanUserManagement = false
			};
		}

		public static UserPermission CreateOperatorPermission()
		{
			return new UserPermission
			{
				CanRun = true,
				CanHardwareConfig = false,
				CanAlgorithmConfig = false,
				CanFlowConfig = false,
				CanCommunicationConfig = false,
				CanDatabaseConfig = false,
				CanSystemConfig = false,
				CanUserManagement = false
			};
		}
	}

	public class UserAccount
	{
		[XmlAttribute]
		public string UserName { get; set; }

		[XmlAttribute]
		public string DisplayName { get; set; }

		[XmlAttribute]
		public string Role { get; set; }

		[XmlAttribute]
		public bool Enabled { get; set; }

		public string PasswordSalt { get; set; }
		public string PasswordHash { get; set; }
		public UserPermission Permission { get; set; }

		public UserAccount()
		{
			UserName = string.Empty;
			DisplayName = string.Empty;
			Role = "Operator";
			Enabled = true;
			PasswordSalt = string.Empty;
			PasswordHash = string.Empty;
			Permission = UserPermission.CreateOperatorPermission();
		}
	}

	[XmlRoot("UserAccountConfig")]
	public class UserAccountConfig
	{
		[XmlAttribute]
		public int AutoLogoutMinutes { get; set; }

		[XmlArray("Users")]
		[XmlArrayItem("User")]
		public List<UserAccount> Users { get; set; }

		public UserAccountConfig()
		{
			AutoLogoutMinutes = 30;
			Users = new List<UserAccount>();
		}
	}

	public static class LoginSession
	{
		public static UserAccount CurrentUser { get; private set; }
		public static DateTime LoginTime { get; private set; }
		public static DateTime LastActiveTime { get; private set; }

		public static bool IsLoggedIn
		{
			get { return CurrentUser != null; }
		}

		public static string CurrentUserName
		{
			get { return CurrentUser == null ? "Guest" : CurrentUser.UserName; }
		}

		public static UserPermission Permission
		{
			get
			{
				if (CurrentUser == null || CurrentUser.Permission == null)
				{
					return UserPermission.CreateOperatorPermission();
				}

				return CurrentUser.Permission;
			}
		}

		public static void Login(UserAccount user)
		{
			CurrentUser = user;
			LoginTime = DateTime.Now;
			LastActiveTime = DateTime.Now;
		}

		public static void Logout()
		{
			CurrentUser = null;
			LoginTime = DateTime.MinValue;
			LastActiveTime = DateTime.MinValue;
		}

		public static void Touch()
		{
			if (IsLoggedIn)
			{
				LastActiveTime = DateTime.Now;
			}
		}
	}

	public static class UserAccountStore
	{
		public static string ConfigFolder
		{
			get { return ProjectPathStore.SystemConfigRoot; }
		}

		public static string ConfigFile
		{
			get { return Path.Combine(ConfigFolder, "UserAccounts.xml"); }
		}

		public static UserAccountConfig LoadOrCreateDefault()
		{
			EnsureFolder();

			if (!File.Exists(ConfigFile))
			{
				UserAccountConfig defaultConfig = CreateDefaultConfig();
				Save(defaultConfig);
				return defaultConfig;
			}

			try
			{
				XmlSerializer serializer = new XmlSerializer(typeof(UserAccountConfig));

				using (FileStream fs = new FileStream(ConfigFile, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
				{
					UserAccountConfig config = serializer.Deserialize(fs) as UserAccountConfig;

					if (config == null)
					{
						config = CreateDefaultConfig();
					}

					Normalize(config);
					return config;
				}
			}
			catch
			{
				UserAccountConfig config = CreateDefaultConfig();
				Save(config);
				return config;
			}
		}

		public static void Save(UserAccountConfig config)
		{
			EnsureFolder();

			if (config == null)
			{
				config = CreateDefaultConfig();
			}

			Normalize(config);

			XmlSerializer serializer = new XmlSerializer(typeof(UserAccountConfig));

			using (FileStream fs = new FileStream(ConfigFile, FileMode.Create, FileAccess.Write, FileShare.Read))
			{
				serializer.Serialize(fs, config);
			}
		}

		private static void EnsureFolder()
		{
			if (!Directory.Exists(ConfigFolder))
			{
				Directory.CreateDirectory(ConfigFolder);
			}
		}

		private static void Normalize(UserAccountConfig config)
		{
			if (config.Users == null)
			{
				config.Users = new List<UserAccount>();
			}

			if (config.AutoLogoutMinutes <= 0)
			{
				config.AutoLogoutMinutes = 30;
			}

			foreach (UserAccount user in config.Users)
			{
				if (user.Permission == null)
				{
					if (string.Equals(user.Role, "Admin", StringComparison.OrdinalIgnoreCase))
					{
						user.Permission = UserPermission.CreateAdminPermission();
					}
					else if (string.Equals(user.Role, "Engineer", StringComparison.OrdinalIgnoreCase))
					{
						user.Permission = UserPermission.CreateEngineerPermission();
					}
					else
					{
						user.Permission = UserPermission.CreateOperatorPermission();
					}
				}

				if (string.IsNullOrEmpty(user.DisplayName))
				{
					user.DisplayName = user.UserName;
				}

				if (string.IsNullOrEmpty(user.Role))
				{
					user.Role = "Operator";
				}
			}

			if (!config.Users.Any(u => string.Equals(u.UserName, "admin", StringComparison.OrdinalIgnoreCase)))
			{
				UserAccount admin = new UserAccount();
				admin.UserName = "admin";
				admin.DisplayName = "Administrator";
				admin.Role = "Admin";
				admin.Enabled = true;
				admin.Permission = UserPermission.CreateAdminPermission();
				SetPassword(admin, "admin");
				config.Users.Add(admin);
			}
		}

		private static UserAccountConfig CreateDefaultConfig()
		{
			UserAccountConfig config = new UserAccountConfig();
			config.AutoLogoutMinutes = 30;

			UserAccount admin = new UserAccount();
			admin.UserName = "admin";
			admin.DisplayName = "Administrator";
			admin.Role = "Admin";
			admin.Enabled = true;
			admin.Permission = UserPermission.CreateAdminPermission();
			SetPassword(admin, "admin");

			UserAccount operatorUser = new UserAccount();
			operatorUser.UserName = "operator";
			operatorUser.DisplayName = "Operator";
			operatorUser.Role = "Operator";
			operatorUser.Enabled = true;
			operatorUser.Permission = UserPermission.CreateOperatorPermission();
			SetPassword(operatorUser, "123456");

			config.Users.Add(admin);
			config.Users.Add(operatorUser);

			return config;
		}

		public static UserAccount Authenticate(string userName, string password)
		{
			UserAccountConfig config = LoadOrCreateDefault();

			UserAccount user = config.Users.FirstOrDefault(u =>
				string.Equals(u.UserName, userName, StringComparison.OrdinalIgnoreCase));

			if (user == null || !user.Enabled)
			{
				return null;
			}

			string hash = ComputeHash(password, user.PasswordSalt);

			if (string.Equals(hash, user.PasswordHash, StringComparison.OrdinalIgnoreCase))
			{
				return user;
			}

			return null;
		}

		public static bool AddUser(string userName, string displayName, string password, string role, UserPermission permission, out string error)
		{
			error = string.Empty;

			if (string.IsNullOrWhiteSpace(userName))
			{
				error = "User name cannot be empty.";
				return false;
			}

			if (string.IsNullOrWhiteSpace(password))
			{
				error = "Password cannot be empty.";
				return false;
			}

			UserAccountConfig config = LoadOrCreateDefault();

			if (config.Users.Any(u => string.Equals(u.UserName, userName, StringComparison.OrdinalIgnoreCase)))
			{
				error = "User already exists.";
				return false;
			}

			UserAccount user = new UserAccount();
			user.UserName = userName.Trim();
			user.DisplayName = string.IsNullOrWhiteSpace(displayName) ? user.UserName : displayName.Trim();
			user.Role = string.IsNullOrWhiteSpace(role) ? "Operator" : role.Trim();
			user.Enabled = true;
			user.Permission = permission ?? UserPermission.CreateOperatorPermission();
			SetPassword(user, password);

			config.Users.Add(user);
			Save(config);

			return true;
		}

		public static bool ChangePassword(string userName, string oldPassword, string newPassword, out string error)
		{
			error = string.Empty;

			if (string.IsNullOrWhiteSpace(newPassword))
			{
				error = "New password cannot be empty.";
				return false;
			}

			UserAccountConfig config = LoadOrCreateDefault();

			UserAccount user = config.Users.FirstOrDefault(u =>
				string.Equals(u.UserName, userName, StringComparison.OrdinalIgnoreCase));

			if (user == null)
			{
				error = "User does not exist.";
				return false;
			}

			if (!string.Equals(user.PasswordHash, ComputeHash(oldPassword, user.PasswordSalt), StringComparison.OrdinalIgnoreCase))
			{
				error = "Old password is incorrect.";
				return false;
			}

			SetPassword(user, newPassword);
			Save(config);
			return true;
		}

		public static bool ResetPasswordByAdmin(string userName, string newPassword, out string error)
		{
			error = string.Empty;

			if (string.IsNullOrWhiteSpace(newPassword))
			{
				error = "New password cannot be empty.";
				return false;
			}

			UserAccountConfig config = LoadOrCreateDefault();

			UserAccount user = config.Users.FirstOrDefault(u =>
				string.Equals(u.UserName, userName, StringComparison.OrdinalIgnoreCase));

			if (user == null)
			{
				error = "User does not exist.";
				return false;
			}

			SetPassword(user, newPassword);
			Save(config);
			return true;
		}

		public static void SetPassword(UserAccount user, string password)
		{
			user.PasswordSalt = CreateSalt();
			user.PasswordHash = ComputeHash(password, user.PasswordSalt);
		}

		private static string CreateSalt()
		{
			byte[] bytes = new byte[16];

			using (RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider())
			{
				rng.GetBytes(bytes);
			}

			return Convert.ToBase64String(bytes);
		}

		private static string ComputeHash(string password, string salt)
		{
			if (password == null)
			{
				password = string.Empty;
			}

			if (salt == null)
			{
				salt = string.Empty;
			}

			using (SHA256 sha = SHA256.Create())
			{
				byte[] bytes = Encoding.UTF8.GetBytes(salt + "|" + password);
				byte[] hash = sha.ComputeHash(bytes);
				return Convert.ToBase64String(hash);
			}
		}
	}
}
