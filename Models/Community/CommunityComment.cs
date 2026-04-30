using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Acadimy.Models;

namespace Acadimy.Models.Community
{
    public class CommunityComment
    {
        public int Id { get; set; }

        [Required]
        public int CommunityPostId { get; set; }

        [ForeignKey(nameof(CommunityPostId))]
        public CommunityPost Post { get; set; } = null!;

        [Required]
        public string UserId { get; set; } = string.Empty;

        [ForeignKey(nameof(UserId))]
        public ApplicationUser User { get; set; } = null!;

        [Required]
        public string Content { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public int? ParentCommentId { get; set; }

        [ForeignKey(nameof(ParentCommentId))]
        public CommunityComment? ParentComment { get; set; }

        public ICollection<CommunityComment> Replies { get; set; } = new List<CommunityComment>();

        public ICollection<CommunityCommentLike> Likes { get; set; } = new List<CommunityCommentLike>();
    }
}