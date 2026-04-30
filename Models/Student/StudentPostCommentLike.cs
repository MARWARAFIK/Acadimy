using System.ComponentModel.DataAnnotations.Schema;
using Acadimy.Models;

namespace Acadimy.Models.Student
{
    public class StudentPostCommentLike
    {
        public int Id { get; set; }

        public int StudentPostCommentId { get; set; }

        [ForeignKey(nameof(StudentPostCommentId))]
        public StudentPostComment StudentPostComment { get; set; } = null!;

        public string UserId { get; set; } = string.Empty;

        [ForeignKey(nameof(UserId))]
        public ApplicationUser User { get; set; } = null!;

        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}