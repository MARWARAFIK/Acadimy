namespace Acadimy.ViewModels.Teacher
{
    public class TeacherStudentViewModel
    {
        public int EnrollmentId { get; set; }

        public string StudentId { get; set; } = "";

        public string FullName { get; set; } = "";

        public string? Email { get; set; }

        // 🔥 زيدي هاد 2 (كانو ناقصين)
        public string? ProfileImagePath { get; set; }
        public int TeacherCourseId { get; set; }

        public string CourseTitle { get; set; } = "";

        public string? Level { get; set; }

        public DateTime EnrolledAt { get; set; }

        public int ProgressPercent { get; set; }

        public string Status { get; set; } = "";
    }
}