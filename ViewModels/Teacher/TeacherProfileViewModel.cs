namespace Acadimy.ViewModels.Teacher
{
    public class TeacherProfileViewModel
    {
        public string FullName { get; set; } = "";
        public string Email { get; set; } = "";
        public string Bio { get; set; } = "";
        public string Specialite { get; set; } = "";
        public string Grade { get; set; } = "";
        public int Experience { get; set; }

        public string? ProfileImagePath { get; set; }
        public string? CoverImagePath { get; set; }

        public List<TeacherExpertiseItemViewModel> Expertises { get; set; } = new();
        public List<TeacherPostViewModel> Posts { get; set; } = new();
    }
}