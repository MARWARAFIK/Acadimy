namespace Acadimy.ViewModels.Teacher
{
    public class TeacherStudentsPageViewModel
    {
        public List<TeacherStudentViewModel> Students { get; set; } = new();

        public string? Search { get; set; }

        public int? CourseId { get; set; }

        public List<CourseFilterItem> Courses { get; set; } = new();

        public int TotalStudents { get; set; }

        public int TotalCourses { get; set; }

        public int NewStudentsCount { get; set; }

        public int CompletedCount { get; set; }
    }

    public class CourseFilterItem
    {
        public int Id { get; set; }

        public string Title { get; set; } = "";
    }
}