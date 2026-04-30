using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace Acadimy.ViewModels.Teacher
{
    public class TeacherSettingsViewModel
    {
        [Required(ErrorMessage = "First name is required.")]
        public string FirstName { get; set; } = "";

        [Required(ErrorMessage = "Last name is required.")]
        public string LastName { get; set; } = "";

        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Invalid email address.")]
        public string Email { get; set; } = "";

        public string? Grade { get; set; }
        public string? Specialite { get; set; }

        [Range(0, 50, ErrorMessage = "Experience must be between 0 and 50.")]
        public int Experience { get; set; }

        public string? Bio { get; set; }

        public IFormFile? ProfileImage { get; set; }
        public IFormFile? CoverImage { get; set; }

        public string? ExistingProfileImagePath { get; set; }
        public string? ExistingCoverImagePath { get; set; }

        public bool ChangePassword { get; set; }

        [DataType(DataType.Password)]
        public string? CurrentPassword { get; set; }

        [DataType(DataType.Password)]
        public string? NewPassword { get; set; }

        [DataType(DataType.Password)]
        public string? ConfirmNewPassword { get; set; }

        public bool NotifyStudentSubmission { get; set; }
        public bool NotifyCourseActivity { get; set; }
        public bool NotifyAnnouncement { get; set; }
        public bool NotifyMessages { get; set; }

        public List<TeacherExpertiseItemViewModel> Expertises { get; set; } = new();
    }
}