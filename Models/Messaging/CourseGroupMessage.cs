using System.ComponentModel.DataAnnotations.Schema;
using Acadimy.Models;
using Acadimy.Models.Teacher;

namespace Acadimy.Models.Messaging
{
    public class CourseGroupMessage
    {
        public int Id { get; set; }

        public int TeacherCourseId { get; set; }

        [ForeignKey(nameof(TeacherCourseId))]
        public TeacherCourse? TeacherCourse { get; set; }

        public string SenderId { get; set; } = "";

        [ForeignKey(nameof(SenderId))]
        public ApplicationUser? Sender { get; set; }

        public string Content { get; set; } = "";

        public string? AttachmentPath { get; set; }
        public string? AttachmentFileName { get; set; }
        public string? AttachmentContentType { get; set; }

        public bool IsVoice { get; set; } = false;

        public DateTime SentAt { get; set; } = DateTime.Now;
    }
}