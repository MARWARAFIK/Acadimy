namespace Acadimy.ViewModels.Teacher
{
    public class TeacherPostViewModel
    {
        public int Id { get; set; }
        public string? UserId { get; set; }

        public string Content { get; set; } = "";
        public string? ImagePath { get; set; }
        public DateTime CreatedAt { get; set; }

        public string FullName { get; set; } = "";
        public string? ProfileImagePath { get; set; }

        public int LikesCount { get; set; }
        public bool IsLikedByCurrentUser { get; set; }
        public bool IsArchived { get; set; }

        public List<TeacherPostCommentViewModel> Comments { get; set; } = new();
    }
}