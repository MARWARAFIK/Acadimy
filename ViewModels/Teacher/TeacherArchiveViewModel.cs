namespace Acadimy.ViewModels.Teacher
{
    public class TeacherArchiveViewModel
    {
        public List<TeacherPostViewModel> ArchivedPosts { get; set; } = new();
        public List<TeacherCourseViewModel> ArchivedCourses { get; set; } = new();
    }
}