using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Acadimy.Models.Teacher
{
    public class TeacherExpertise
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = "";

        [Range(0, 100)]
        public int Percentage { get; set; }

        [Required]
        public string UserId { get; set; } = "";

        [ForeignKey(nameof(UserId))]
        public ApplicationUser? User { get; set; }
    }
}