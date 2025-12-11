using System.ComponentModel.DataAnnotations;

namespace Medicine_ERP.Data.Models.ViewModels
{
	public class LoginModel
	{
		[Required(ErrorMessage = "Email is required.")]
		[EmailAddress(ErrorMessage = "Invalid email format.")]
		public string? Username { get; set; }

		[Required(ErrorMessage = "Password is required.")]
		public string? Password { get; set; }

		// REQUIRED FIX: Added the missing RememberMe property
		public bool RememberMe { get; set; }
	}
}