using System.Collections.Generic;

namespace Acadimy.ViewModels.Teacher
{
    public class TeacherProfileViewModel
    {
        public string? FullName { get; set; }
        public string? Email { get; set; }
        public string? Bio { get; set; }
        public string? ProfileImagePath { get; set; }
        public string? Specialite { get; set; }
        public string? Grade { get; set; }
        public int Experience { get; set; }
        public List<string> SkillsList { get; set; } = new List<string>();
    }
}