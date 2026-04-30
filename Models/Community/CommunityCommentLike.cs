using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Acadimy.Models;

namespace Acadimy.Models.Community
{
    public class CommunityCommentLike
    {
        public int Id { get; set; }

        [Required]
        public int CommunityCommentId { get; set; }

        [ForeignKey(nameof(CommunityCommentId))]
        public CommunityComment Comment { get; set; } = null!;

        [Required]
        public string UserId { get; set; } = string.Empty;

        [ForeignKey(nameof(UserId))]
        public ApplicationUser User { get; set; } = null!;

        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}