using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace Acadimy.Models
{
    public class ApplicationUser : IdentityUser
    {
        [MaxLength(100)]
        public string? FirstName { get; set; }

        [MaxLength(100)]
        public string? LastName { get; set; }

        [MaxLength(150)]
        public string? Location { get; set; }

        [MaxLength(50)]
        public string? PhoneNumberCustom { get; set; }

        [MaxLength(150)]
        public string? Website { get; set; }

        public string? Bio { get; set; }

        [MaxLength(100)]
        public string? Filiere { get; set; }

        [MaxLength(50)]
        public string? Niveau { get; set; }

        public string? ProfileImagePath { get; set; }
        public string? CoverImagePath { get; set; }

        public bool NotifyNewCourse { get; set; } = true;
        public bool NotifyApplicationStatus { get; set; } = true;
        public bool NotifyAnnouncement { get; set; } = true;
        public bool NotifyMessages { get; set; } = true;

        [MaxLength(100)]
        public string? Skill { get; set; }

        public int SkillPercent { get; set; } = 0;

        public string FullName => $"{FirstName} {LastName}".Trim();
    }
}