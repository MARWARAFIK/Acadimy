using System.ComponentModel.DataAnnotations;

namespace Acadimy.Models.Teacher
{
    public class TeacherPostComment
    {
        public int Id { get; set; }

        public int TeacherPostId { get; set; }
        public TeacherPost? TeacherPost { get; set; }

        public string UserId { get; set; } = "";
        public ApplicationUser? User { get; set; }

        [Required]
        public string Content { get; set; } = "";

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // Reply system
        public int? ParentCommentId { get; set; }
        public TeacherPostComment? ParentComment { get; set; }

        public ICollection<TeacherPostComment> Replies { get; set; } = new List<TeacherPostComment>();
        public ICollection<TeacherPostCommentLike> Likes { get; set; } = new List<TeacherPostCommentLike>();
    }
}