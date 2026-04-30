using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Acadimy.Models.Messaging
{
    public class Message
    {
        public int Id { get; set; }

        public int ThreadId { get; set; }

        [ForeignKey(nameof(ThreadId))]
        public MessageThread Thread { get; set; } = null!;

        public string SenderId { get; set; } = "";

        public string Content { get; set; } = "";

        public DateTime SentAt { get; set; } = DateTime.Now;

        public bool IsRead { get; set; } = false;
    }
}