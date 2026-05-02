using System.ComponentModel.DataAnnotations;

namespace Acadimy.ViewModels.Admin
{
    public class AdminSettingsViewModel
    {
        [Required]
        public string FirstName { get; set; } = "";

        [Required]
        public string LastName { get; set; } = "";

        [EmailAddress]
        public string Email { get; set; } = "";

        [DataType(DataType.Password)]
        public string? CurrentPassword { get; set; }

        [DataType(DataType.Password)]
        [MinLength(6)]
        public string? NewPassword { get; set; }

        [DataType(DataType.Password)]
        [Compare("NewPassword", ErrorMessage = "Les mots de passe ne correspondent pas.")]
        public string? ConfirmPassword { get; set; }
    }
}