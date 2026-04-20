using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Acadimy.Models.Student
{
    public class StudentPost
    {
        public int Id { get; set; }

        [Required]
        public string Content { get; set; } = "";

        public string? ImagePath { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        [Required]
        public string UserId { get; set; } = "";

        [ForeignKey("UserId")]
        public ApplicationUser? User { get; set; }

        public List<StudentPostLike> Likes { get; set; } = new();
        public List<StudentPostComment> Comments { get; set; } = new();
    }
}