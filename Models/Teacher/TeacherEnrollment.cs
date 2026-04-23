using Acadimy.Models;
using System.ComponentModel.DataAnnotations;

namespace Acadimy.Models.Teacher
{
    public class TeacherEnrollment
    {
        public int Id { get; set; }

        [Required]
        public string StudentId { get; set; } = "";

        public ApplicationUser? Student { get; set; }

        [Required]
        public int TeacherCourseId { get; set; }

        public TeacherCourse? TeacherCourse { get; set; }

        public DateTime EnrolledAt { get; set; } = DateTime.Now;

        [Range(0, 100)]
        public int ProgressPercent { get; set; } = 0;

        public bool IsCompleted { get; set; } = false;

        [MaxLength(50)]
        public string Status { get; set; } = "Active";
    }
}