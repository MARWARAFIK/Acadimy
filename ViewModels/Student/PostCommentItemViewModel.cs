namespace Acadimy.ViewModels.Student
{
    public class PostCommentItemViewModel
    {
        public int Id { get; set; }
        public string Content { get; set; } = "";
        public DateTime CreatedAt { get; set; }

        public string FullName { get; set; } = "";
        public string? ProfileImagePath { get; set; }
    }
}