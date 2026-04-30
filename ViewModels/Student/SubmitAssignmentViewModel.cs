using Microsoft.AspNetCore.Http;

namespace Acadimy.ViewModels.Student
{
    public class SubmitAssignmentViewModel
    {
        public int AssignmentId { get; set; }

        public string AssignmentTitle { get; set; } = "";
        public string CourseTitle { get; set; } = "";

        public string? Description { get; set; }
        public DateTime Deadline { get; set; }

        public string? AssignmentFilePath { get; set; }

        public IFormFile? File { get; set; }
        public string? Comment { get; set; }

        public string? ExistingSubmissionFile { get; set; }

        public decimal? Grade { get; set; }
        public string? TeacherFeedback { get; set; }
    }
}