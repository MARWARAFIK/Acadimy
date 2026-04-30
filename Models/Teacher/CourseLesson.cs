using System.ComponentModel.DataAnnotations;

namespace Acadimy.Models.Teacher
{
    public class CourseLesson
    {
        public int Id { get; set; }

        [Required]
        public int TeacherCourseId { get; set; }

        public TeacherCourse? TeacherCourse { get; set; }

        [Required]
        [MaxLength(150)]
        public string Title { get; set; } = "";

        public string? Description { get; set; }

        public int Order { get; set; } = 1;

        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}