using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Acadimy.Models;

namespace Acadimy.Models.Teacher
{
    public class TeacherPost
    {
        public int Id { get; set; }

        [Required]
        public string Content { get; set; } = string.Empty;

        public string? ImagePath { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public bool IsArchived { get; set; } = false;

        [Required]
        public string UserId { get; set; } = string.Empty;

        [ForeignKey(nameof(UserId))]
        public ApplicationUser User { get; set; } = null!; // 🔥

        public ICollection<TeacherPostLike> Likes { get; set; } = new List<TeacherPostLike>();
        public ICollection<TeacherPostComment> Comments { get; set; } = new List<TeacherPostComment>();
    }
}