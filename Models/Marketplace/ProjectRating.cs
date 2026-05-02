using System.ComponentModel.DataAnnotations;

namespace Acadimy.Models.Marketplace
{
    public class ProjectRating
    {
        public int Id { get; set; }

        public int ProjectPostId { get; set; }
        public ProjectPost? ProjectPost { get; set; }

        public string UserId { get; set; } = string.Empty;
        public ApplicationUser? User { get; set; }

        [Range(1, 5)]
        public int Value { get; set; }
    }
}