namespace Acadimy.ViewModels.Student
{
    public class PostItemViewModel
    {
        public int Id { get; set; }

        public string Content { get; set; } = "";

        public string? ImagePath { get; set; }

        public DateTime CreatedAt { get; set; }

        public string FullName { get; set; } = "";
        public string? ProfileImagePath { get; set; }
        public string? Filiere { get; set; }
        public string? Niveau { get; set; }

        public int LikesCount { get; set; }
        public bool IsLikedByCurrentUser { get; set; }

        public List<PostCommentItemViewModel> Comments { get; set; } = new();
    }
}