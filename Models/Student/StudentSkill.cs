using System.ComponentModel.DataAnnotations;

namespace Acadimy.Models.Student
{
    public class StudentSkill
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = "";

        [Range(0, 100)]
        public int Percentage { get; set; }

        public string UserId { get; set; } = "";
        public ApplicationUser? User { get; set; }
    }
}