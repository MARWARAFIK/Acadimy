using Acadimy.Models;
using System.ComponentModel.DataAnnotations;

namespace Acadimy.Models.Teacher
{
    public class TeacherCourse
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(150)]
        public string Title { get; set; } = "";

        [MaxLength(100)]
        public string? Category { get; set; }

        [MaxLength(100)]
        public string? Level { get; set; }

        public string? Description { get; set; }

        [MaxLength(255)]
        public string? ThumbnailPath { get; set; }

        [MaxLength(255)]
        public string? FilePath { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public bool IsArchived { get; set; } = false;

        public string UserId { get; set; } = "";
        public ApplicationUser? User { get; set; }

        public ICollection<TeacherEnrollment> Enrollments { get; set; } = new List<TeacherEnrollment>();
        public ICollection<CourseLesson> Lessons { get; set; } = new List<CourseLesson>();
    
    }
}