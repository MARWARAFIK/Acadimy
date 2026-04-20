using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace Acadimy.ViewModels.Student
{
    public class CreatePostViewModel
    {
        [Required]
        public string Content { get; set; } = "";

        public IFormFile? Image { get; set; }
    }
}