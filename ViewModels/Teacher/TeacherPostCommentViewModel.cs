namespace Acadimy.ViewModels.Teacher
{
    public class TeacherPostCommentViewModel
    {
        public int Id { get; set; }
        public string? UserId { get; set; }
        public string FullName { get; set; } = "";
        public string? ProfileImagePath { get; set; }
        public string Content { get; set; } = "";
        public DateTime CreatedAt { get; set; }
        public int LikesCount { get; set; }
        public bool IsLikedByCurrentUser { get; set; }
        public List<TeacherPostCommentViewModel> Replies { get; set; } = new();
    }
}