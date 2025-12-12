namespace PharmacyManagementSystem.Services
{
	// Model to hold the result of the login attempt
	public class LoginResult
	{
		public bool Success { get; set; }
		// For success, this holds the Role (e.g., "Admin", "Manager").
		// For failure, this holds the error message.
		public string? ErrorMessage { get; set; }
	}

	// The single interface definition
	public interface IAuthService
	{
		Task<LoginResult> LoginAsync(string Email, string PasswordHash, bool rememberMe);
		Task<List<UserInfo>> GetAllUsersAsync();
		Task UpdateUserRoleAsync(int userId, string newRole, bool isActive);
	}

	// UserInfo DTO for role management
	public class UserInfo
	{
		public int UserId { get; set; }
		public string Email { get; set; } = "";
		public string Role { get; set; } = "";
		public bool IsActive { get; set; }
	}
}