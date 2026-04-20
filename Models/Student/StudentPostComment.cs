using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Acadimy.Models;

namespace Acadimy.Models.Student
{
    public class StudentPostComment
    {
        public int Id { get; set; }

        [Required]
        public int StudentPostId { get; set; }

        [ForeignKey(nameof(StudentPostId))]
        public StudentPost? StudentPost { get; set; }

        [Required]
        public string UserId { get; set; } = "";

        [ForeignKey(nameof(UserId))]
        public ApplicationUser? User { get; set; }

        [Required]
        public string Content { get; set; } = "";

        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}