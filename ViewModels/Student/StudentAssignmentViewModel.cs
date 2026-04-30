namespace Acadimy.ViewModels.Student
{
    public class StudentAssignmentViewModel
    {
        public int Id { get; set; }

        public string Title { get; set; } = "";
        public string? Description { get; set; }

        public DateTime Deadline { get; set; }

        public string CourseTitle { get; set; } = "";
        public string? FilePath { get; set; }

        public bool IsSubmitted { get; set; }
        public decimal? Grade { get; set; }
        public string? TeacherFeedback { get; set; }
    }
}