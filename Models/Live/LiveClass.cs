using System.ComponentModel.DataAnnotations;

namespace Acadimy.Models.Live
{
    public class LiveClass
    {
        public int Id { get; set; }

        [Required]
        public string Title { get; set; } = "";

        public int CourseId { get; set; }

        public string TeacherId { get; set; } = "";

        public bool IsActive { get; set; } = false;

        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}