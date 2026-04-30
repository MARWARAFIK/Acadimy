using Acadimy.Models;
using System.ComponentModel.DataAnnotations;

namespace Acadimy.Models.Teacher
{
    public class StudentLessonProgress
    {
        public int Id { get; set; }

        [Required]
        public string StudentId { get; set; } = "";
        public ApplicationUser? Student { get; set; }

        [Required]
        public int CourseLessonId { get; set; }
        public CourseLesson? CourseLesson { get; set; }

        public DateTime CompletedAt { get; set; } = DateTime.Now;
    }
}