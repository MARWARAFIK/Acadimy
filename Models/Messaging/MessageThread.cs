using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Acadimy.Models;

namespace Acadimy.Models.Messaging
{
    public class MessageThread
    {
        public int Id { get; set; }

        [Required]
        public string User1Id { get; set; } = "";

        [Required]
        public string User2Id { get; set; } = "";

        [ForeignKey(nameof(User1Id))]
        public ApplicationUser User1 { get; set; } = null!;

        [ForeignKey(nameof(User2Id))]
        public ApplicationUser User2 { get; set; } = null!;

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public ICollection<Message> Messages { get; set; } = new List<Message>();
    }
}