namespace Acadimy.Models.Teacher
{
    public class TeacherPostLike
    {
        public int Id { get; set; }

        public int TeacherPostId { get; set; }
        public TeacherPost? TeacherPost { get; set; }

        public string UserId { get; set; } = "";
        public ApplicationUser? User { get; set; }
    }
}