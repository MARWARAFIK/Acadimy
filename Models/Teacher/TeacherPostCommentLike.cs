using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Acadimy.Models;

namespace Acadimy.Models.Teacher
{
    public class TeacherPostCommentLike
    {
        public int Id { get; set; }

        [Required]
        public int TeacherPostCommentId { get; set; }

        [ForeignKey(nameof(TeacherPostCommentId))]
        public TeacherPostComment? TeacherPostComment { get; set; }

        [Required]
        public string UserId { get; set; } = "";

        [ForeignKey(nameof(UserId))]
        public ApplicationUser? User { get; set; }
    }
}