namespace Acadimy.ViewModels.Teacher
{
    public class TeacherAssignmentSubmissionViewModel
    {
        public int SubmissionId { get; set; }
        public int AssignmentId { get; set; }

        public string AssignmentTitle { get; set; } = "";
        public string CourseTitle { get; set; } = "";

        public string StudentId { get; set; } = "";
        public string StudentName { get; set; } = "";
        public string? StudentEmail { get; set; }
        public string? ProfileImagePath { get; set; }

        public string? FilePath { get; set; }
        public string? Comment { get; set; }

        public DateTime SubmittedAt { get; set; }

        public decimal? Grade { get; set; }
        public string? TeacherFeedback { get; set; }
    }
}