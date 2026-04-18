using Microsoft.AspNetCore.Identity;

namespace Acadimy.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? Location { get; set; }
        public string? PhoneNumberCustom { get; set; }
        public string? Website { get; set; }
        public string? Bio { get; set; }

        public string? Filiere { get; set; }
        public string? Niveau { get; set; }

        public string? ProfileImagePath { get; set; }
        public string? CoverImagePath { get; set; }

        public bool NotifyNewCourse { get; set; }
        public bool NotifyApplicationStatus { get; set; }
        public bool NotifyAnnouncement { get; set; }
        public bool NotifyMessages { get; set; }

        public string? Skill { get; set; }
        public int SkillPercent { get; set; }
    }
}