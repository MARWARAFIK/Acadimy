using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Acadimy.Models;

namespace Acadimy.Models.Community
{
    public class CommunityPost
    {
        public int Id { get; set; }

        [Required]
        public string Content { get; set; } = string.Empty;

        public string? ImagePath { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        [Required]
        public string UserId { get; set; } = string.Empty;

        [ForeignKey(nameof(UserId))]
        public ApplicationUser User { get; set; } = null!;

        // 🔥 مهم بزاف
        public string OriginalPostType { get; set; } = string.Empty;
        public int OriginalPostId { get; set; }

        public ICollection<CommunityPostLike> Likes { get; set; } = new List<CommunityPostLike>();
        public ICollection<CommunityComment> Comments { get; set; } = new List<CommunityComment>();
    }
}