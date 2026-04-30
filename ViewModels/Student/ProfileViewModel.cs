using System.Collections.Generic;

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

        // ✅ الجديد (مهم بزاف)
        public List<StudentSkillItemViewModel> Skills { get; set; } = new();

        public CreatePostViewModel NewPost { get; set; } = new();
        public List<PostItemViewModel> Posts { get; set; } = new();
    }
}