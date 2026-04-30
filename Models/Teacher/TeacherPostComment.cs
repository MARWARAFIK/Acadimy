using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Acadimy.Models;

namespace Acadimy.Models.Teacher
{
    public class TeacherPostComment
    {
        public int Id { get; set; }

        [Required]
        public int TeacherPostId { get; set; }

        [ForeignKey(nameof(TeacherPostId))]
        public TeacherPost? TeacherPost { get; set; }

        [Required]
        public string UserId { get; set; } = "";

        [ForeignKey(nameof(UserId))]
        public ApplicationUser? User { get; set; }

        [Required]
        public string Content { get; set; } = "";

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public int? ParentCommentId { get; set; }

        [ForeignKey(nameof(ParentCommentId))]
        public TeacherPostComment? ParentComment { get; set; }

        public ICollection<TeacherPostComment> Replies { get; set; } = new List<TeacherPostComment>();
        public ICollection<TeacherPostCommentLike> Likes { get; set; } = new List<TeacherPostCommentLike>();
    }
}