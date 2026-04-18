using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace Acadimy.ViewModels.Student
{
    public class SettingsViewModel
    {
        [Required]
        public string? FirstName { get; set; }

        [Required]
        public string? LastName { get; set; }

        [Required]
        [EmailAddress]
        public string? Email { get; set; }

        public string? PhoneNumberCustom { get; set; }
        public string? Location { get; set; }
        public string? Website { get; set; }
        public string? Bio { get; set; }

        public string? Filiere { get; set; }
        public string? Niveau { get; set; }

        public IFormFile? ProfileImage { get; set; }
        public IFormFile? CoverImage { get; set; }

        public string? ExistingProfileImagePath { get; set; }
        public string? ExistingCoverImagePath { get; set; }

        public bool NotifyNewCourse { get; set; }
        public bool NotifyApplicationStatus { get; set; }
        public bool NotifyAnnouncement { get; set; }
        public bool NotifyMessages { get; set; }

        public string? Skill { get; set; }
        public int SkillPercent { get; set; }

        [DataType(DataType.Password)]
        public string? CurrentPassword { get; set; }

        [DataType(DataType.Password)]
        public string? NewPassword { get; set; }

        [DataType(DataType.Password)]
        [Compare("NewPassword", ErrorMessage = "Confirm password does not match.")]
        public string? ConfirmNewPassword { get; set; }
    }
}