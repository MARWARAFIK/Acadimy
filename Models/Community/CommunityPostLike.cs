using System.ComponentModel.DataAnnotations.Schema;

namespace Acadimy.Models.Community
{
    public class CommunityPostLike
    {
        public int Id { get; set; }

        public int CommunityPostId { get; set; }

        [ForeignKey(nameof(CommunityPostId))]
        public CommunityPost Post { get; set; } = null!;

        public string UserId { get; set; } = string.Empty;

        [ForeignKey(nameof(UserId))]
        public ApplicationUser User { get; set; } = null!;
    }
}