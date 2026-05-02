using System.ComponentModel.DataAnnotations.Schema;
using Acadimy.Models;

namespace Acadimy.Models.Messaging
{
    public class Message
    {
        public int Id { get; set; }

        public int ThreadId { get; set; }

        [ForeignKey(nameof(ThreadId))]
        public MessageThread Thread { get; set; } = null!;

        public string SenderId { get; set; } = "";

        [ForeignKey(nameof(SenderId))]
        public ApplicationUser? Sender { get; set; }

        public string Content { get; set; } = "";

        public string? AttachmentPath { get; set; }
        public string? AttachmentFileName { get; set; }
        public string? AttachmentContentType { get; set; }

        public bool IsVoice { get; set; } = false;

        public DateTime SentAt { get; set; } = DateTime.Now;

        public bool IsRead { get; set; } = false;
        public bool IsDeleted { get; set; } = false;
        public bool IsEdited { get; set; } = false;
        public DateTime? EditedAt { get; set; }
    }
}