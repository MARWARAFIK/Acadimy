namespace Acadimy.ViewModels.Student
{
    public class PostCommentItemViewModel
    {
        public int Id { get; set; }

        public string Content { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; }

        public string FullName { get; set; } = string.Empty;

        public string? ProfileImagePath { get; set; }

        public int LikesCount { get; set; } = 0;
        public bool IsLikedByCurrentUser { get; set; }
    }
}