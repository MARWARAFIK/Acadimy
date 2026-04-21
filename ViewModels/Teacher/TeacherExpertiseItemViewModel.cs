using System.ComponentModel.DataAnnotations;

namespace Acadimy.ViewModels.Teacher
{
    public class TeacherExpertiseItemViewModel
    {
        public int Id { get; set; }

        // ماشي obligatoire
        public string? Name { get; set; }

        [Range(0, 100, ErrorMessage = "Percentage must be between 0 and 100.")]
        public int Percentage { get; set; }
    }
}