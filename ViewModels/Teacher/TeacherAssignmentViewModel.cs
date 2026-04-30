using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace Acadimy.ViewModels.Teacher
{
    public class TeacherAssignmentViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Title is required.")]
        public string Title { get; set; } = "";

        public string? Description { get; set; }

        [Required(ErrorMessage = "Deadline is required.")]
        public DateTime Deadline { get; set; } = DateTime.Now.AddDays(7);

        [Required(ErrorMessage = "Course is required.")]
        public int TeacherCourseId { get; set; }

        public string? CourseTitle { get; set; }

        public string? ExistingFilePath { get; set; }

        public IFormFile? AssignmentFile { get; set; }

        public DateTime CreatedAt { get; set; }

        public int SubmissionsCount { get; set; }
    }
}