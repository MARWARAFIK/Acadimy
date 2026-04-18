namespace Acadimy.ViewModels.Student
{
    public class ProfileViewModel
    {
        public string? FullName { get; set; }
        public string? Email { get; set; }
        public string? Location { get; set; }
        public string? Phone { get; set; }
        public string? Website { get; set; }
        public string? Bio { get; set; }

        public string? Filiere { get; set; }
        public string? Niveau { get; set; }

        public string? ProfileImagePath { get; set; }
        public string? CoverImagePath { get; set; }

        public string? Skill { get; set; }
        public int SkillPercent { get; set; }
    }
}