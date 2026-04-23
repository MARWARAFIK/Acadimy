using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace Acadimy.ViewModels.Teacher
{
    public class TeacherCourseViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Title is required.")]
        public string Title { get; set; } = "";

        public string? Category { get; set; }
        public string? Level { get; set; }
        public string? Description { get; set; }

        public string? ExistingThumbnailPath { get; set; }
        public IFormFile? Thumbnail { get; set; }

        public string? ExistingFilePath { get; set; }
        public IFormFile? CourseFile { get; set; }

        public bool IsArchived { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}