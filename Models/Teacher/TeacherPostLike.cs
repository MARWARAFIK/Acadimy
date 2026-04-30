using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Acadimy.Models;

namespace Acadimy.Models.Teacher
{
    public class TeacherPostLike
    {
        public int Id { get; set; }

        [Required]
        public int TeacherPostId { get; set; }

        [ForeignKey(nameof(TeacherPostId))]
        public TeacherPost? TeacherPost { get; set; }

        [Required]
        public string UserId { get; set; } = "";

        [ForeignKey(nameof(UserId))]
        public ApplicationUser? User { get; set; }
    }
}