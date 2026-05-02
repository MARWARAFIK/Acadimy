using System.ComponentModel.DataAnnotations;

namespace Acadimy.Models.Marketplace
{
    public class ProjectPost
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(200)]
        public string Title { get; set; } = string.Empty;

        [Required]
        public string Description { get; set; } = string.Empty;

        public string? ImagePath { get; set; }
        public string? FilePath { get; set; }

        public string UserId { get; set; } = string.Empty;
        public ApplicationUser? User { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public ICollection<ProjectComment> Comments { get; set; } = new List<ProjectComment>();
        public ICollection<ProjectRating> Ratings { get; set; } = new List<ProjectRating>();
    }
}