namespace Acadimy.ViewModels.Teacher
{
    public class TeacherSettingsViewModel
    {
        public string? Grade { get; set; }
        public string? Specialite { get; set; }
        public int Experience { get; set; }
        public string? Bio { get; set; }
        public IFormFile? ProfileImage { get; set; }
        public string? ExistingProfileImagePath { get; set; }

        // Mot de passe
        public string? CurrentPassword { get; set; }
        public string? NewPassword { get; set; }
        public string? ConfirmNewPassword { get; set; }
    }
}