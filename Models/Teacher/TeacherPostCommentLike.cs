namespace Acadimy.Models.Teacher
{
    public class TeacherPostCommentLike
    {
        public int Id { get; set; }

        public int TeacherPostCommentId { get; set; }
        public TeacherPostComment? TeacherPostComment { get; set; }

        public string UserId { get; set; } = "";
        public ApplicationUser? User { get; set; }
    }
}