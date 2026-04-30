using Acadimy.Models;
using System.ComponentModel.DataAnnotations;

namespace Acadimy.Models.Teacher
{
    public class TeacherAssignment
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(150)]
        public string Title { get; set; } = "";

        public string? Description { get; set; }

        public DateTime Deadline { get; set; }

        [MaxLength(255)]
        public string? FilePath { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public bool IsArchived { get; set; } = false;

        [Required]
        public string UserId { get; set; } = "";

        public ApplicationUser? User { get; set; }

        [Required]
        public int TeacherCourseId { get; set; }

        public TeacherCourse? TeacherCourse { get; set; }

        public ICollection<TeacherAssignmentSubmission> Submissions { get; set; } = new List<TeacherAssignmentSubmission>();
    }
}