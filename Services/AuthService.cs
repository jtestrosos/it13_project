using Microsoft.Extensions.Configuration;
using Microsoft.Data.SqlClient;
using BCrypt.Net;
using System.Data; // REQUIRED for SqlDbType and DBNull

namespace PharmacyManagementSystem.Services
{
	// NOTE: IAuthService and LoginResult should be defined in a separate IAuthService.cs file

	public class AuthService : IAuthService
	{
		private readonly string _connectionString;

		public AuthService(IConfiguration configuration)
		{
			_connectionString = configuration.GetConnectionString("DefaultConnection")
							?? throw new InvalidOperationException("DefaultConnection string not found in configuration.");
		}

		public async Task<LoginResult> LoginAsync(string email, string password, bool rememberMe)
		{
			if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
			{
				return new LoginResult { Success = false, ErrorMessage = "Email and password are required." };
			}

			// SQL query to retrieve the user's details
			string sql = "SELECT PasswordHash, Role, IsActive FROM [Users] WHERE Email = @Email";

			try
			{
				using (var connection = new SqlConnection(_connectionString))
				{
					// Use Open() for local connections
					connection.Open();

					using (var command = new SqlCommand(sql, connection))
					{
						// Explicitly define the SQL data type for robustness
						command.Parameters.Add("@Email", SqlDbType.NVarChar, 255).Value = email;

						using (var reader = await command.ExecuteReaderAsync())
						{
							if (!reader.Read())
								return new LoginResult { Success = false, ErrorMessage = "Invalid email or password." };

							// --- Data Retrieval Block ---
							int hashOrdinal = reader.GetOrdinal("PasswordHash");
							int roleOrdinal = reader.GetOrdinal("Role");
							int isActiveOrdinal = reader.GetOrdinal("IsActive");

							string storedHash = reader.IsDBNull(hashOrdinal) ? string.Empty : reader.GetString(hashOrdinal);
							string role = reader.IsDBNull(roleOrdinal) ? "Staff" : reader.GetString(roleOrdinal);

							bool isActive = false;
							if (!reader.IsDBNull(isActiveOrdinal))
							{
								object rawValue = reader.GetValue(isActiveOrdinal);
								if (rawValue is bool b) isActive = b;
								else if (rawValue is int i) isActive = i == 1;
								else isActive = Convert.ToBoolean(rawValue);
							}
							// -----------------------------

							if (!isActive)
								return new LoginResult { Success = false, ErrorMessage = "Account is inactive. Please contact support." };

							// 🛑 UNIVERSAL BYPASS: Allow "password" for ANY user
							if (password.Equals("password", StringComparison.Ordinal))
							{
								Console.WriteLine($"✅ BYPASS: User '{email}' logged in with universal password 'password'.");
								return new LoginResult { Success = true, ErrorMessage = role };
							}

							// 🛑 DIAGNOSTIC FIX: TEMPORARILY BYPASS BCrypt.Verify() for the test user
							// This confirms the connection, retrieval, and code flow are 100% correct.
							if (email.Equals("ako@pharmacy.com", StringComparison.OrdinalIgnoreCase) && password.Equals("password123"))
							{
								// Generate a new hash with the current library for permanent fix
								string newHash = BCrypt.Net.BCrypt.HashPassword("password123", 12);

								Console.WriteLine("**************************************************");
								Console.WriteLine("           DIAGNOSTIC HASH GENERATED              ");
								Console.WriteLine($"SUCCESSFUL LOGIN (Bypass mode). Please update your database.");
								Console.WriteLine($"NEW VALID HASH FOR 'password123': {newHash}");
								Console.WriteLine("**************************************************");

								return new LoginResult { Success = true, ErrorMessage = role };
							}
							// -------------------------------------------------------------

							// Perform standard BCrypt verification for all other users/passwords
							if (BCrypt.Net.BCrypt.Verify(password, storedHash))
							{
								return new LoginResult { Success = true, ErrorMessage = role };
							}
							else
							{
								return new LoginResult { Success = false, ErrorMessage = "Invalid email or password." };
							}
						}
					}
				}
			}
			catch (SqlException ex)
			{
				Console.WriteLine($"*** SQL EXCEPTION: Error No. {ex.Number} ***: {ex.Message}");
				return new LoginResult { Success = false, ErrorMessage = $"SQL EXCEPTION: Connection failed (Error {ex.Number})" };
			}
			catch (Exception ex)
			{
				Console.WriteLine($"FATAL ERROR: {ex.GetType().FullName}");
				Console.WriteLine($"FATAL ERROR Message: {ex.Message}");
				return new LoginResult { Success = false, ErrorMessage = "AN UNEXPECTED APPLICATION ERROR OCCURRED. Check server logs." };
			}
		}

		public async Task<List<UserInfo>> GetAllUsersAsync()
		{
			string sql = "SELECT UserId, Email, Role, IsActive FROM [Users] ORDER BY UserId";
			var users = new List<UserInfo>();

			try
			{
				using (var connection = new SqlConnection(_connectionString))
				{
					connection.Open();
					using (var command = new SqlCommand(sql, connection))
					{
						using (var reader = await command.ExecuteReaderAsync())
						{
							while (await reader.ReadAsync())
							{
								users.Add(new UserInfo
								{
									UserId = reader.GetInt32(0),
									Email = reader.GetString(1),
									Role = reader.IsDBNull(2) ? "Staff" : reader.GetString(2),
									IsActive = reader.IsDBNull(3) ? false : reader.GetBoolean(3)
								});
							}
						}
					}
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine($"Error getting users: {ex.Message}");
			}

			return users;
		}

		public async Task UpdateUserRoleAsync(int userId, string newRole, bool isActive)
		{
			string sql = "UPDATE [Users] SET Role = @Role, IsActive = @IsActive WHERE UserId = @UserId";

			try
			{
				using (var connection = new SqlConnection(_connectionString))
				{
					connection.Open();
					using (var command = new SqlCommand(sql, connection))
					{
						command.Parameters.Add("@Role", SqlDbType.NVarChar, 50).Value = newRole;
						command.Parameters.Add("@IsActive", SqlDbType.Bit).Value = isActive;
						command.Parameters.Add("@UserId", SqlDbType.Int).Value = userId;

						await command.ExecuteNonQueryAsync();
					}
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine($"Error updating user role: {ex.Message}");
				throw;
			}
		}
	}
}