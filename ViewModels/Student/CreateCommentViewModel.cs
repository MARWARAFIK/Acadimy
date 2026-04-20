using System.ComponentModel.DataAnnotations;

namespace Acadimy.ViewModels.Student
{
    public class CreateCommentViewModel
    {
        public int PostId { get; set; }

        [Required]
        public string Content { get; set; } = "";
    }
}