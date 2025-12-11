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
	}
}