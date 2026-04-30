using System.ComponentModel.DataAnnotations;

namespace Acadimy.Models
{
    public class Notification
    {
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; } = "";

        public ApplicationUser? User { get; set; }

        [Required]
        [MaxLength(150)]
        public string Title { get; set; } = "";

        public string Message { get; set; } = "";

        [MaxLength(50)]
        public string Type { get; set; } = "Info";

        public string? Link { get; set; }

        public bool IsRead { get; set; } = false;

        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}