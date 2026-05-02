using System.ComponentModel.DataAnnotations;

namespace Acadimy.Models.Marketplace
{
    public class ProjectComment
    {
        public int Id { get; set; }

        public int ProjectPostId { get; set; }
        public ProjectPost? ProjectPost { get; set; }

        public string UserId { get; set; } = string.Empty;
        public ApplicationUser? User { get; set; }

        [Required]
        public string Content { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}