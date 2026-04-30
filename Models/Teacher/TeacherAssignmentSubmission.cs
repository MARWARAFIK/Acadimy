using Acadimy.Models;
using System.ComponentModel.DataAnnotations;

namespace Acadimy.Models.Teacher
{
    public class TeacherAssignmentSubmission
    {
        public int Id { get; set; }

        [Required]
        public int TeacherAssignmentId { get; set; }

        public TeacherAssignment? TeacherAssignment { get; set; }

        [Required]
        public string StudentId { get; set; } = "";

        public ApplicationUser? Student { get; set; }

        public string? FilePath { get; set; }

        public string? Comment { get; set; }

        public DateTime SubmittedAt { get; set; } = DateTime.Now;

        [Range(0, 20)]
        public decimal? Grade { get; set; }

        public string? TeacherFeedback { get; set; }
    }
}