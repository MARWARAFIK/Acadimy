using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Acadimy.Models;

namespace Acadimy.Models.Student
{
    public class StudentPost
    {
        public int Id { get; set; }

        [Required]
        public string Content { get; set; } = string.Empty;

        public string? ImagePath { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        [Required]
        public string UserId { get; set; } = string.Empty;

        [ForeignKey(nameof(UserId))]
        public ApplicationUser User { get; set; } = null!; // 🔥 مهم

        public ICollection<StudentPostLike> Likes { get; set; } = new List<StudentPostLike>();
        public ICollection<StudentPostComment> Comments { get; set; } = new List<StudentPostComment>();


        public bool IsArchived { get; set; } = false;
    
    }
}