using Acadimy.Models;
using Acadimy.Models.Community;
using Acadimy.Models.Live;
using Acadimy.Models.Marketplace;
using Acadimy.Models.Messaging;
using Acadimy.Models.Student;
using Acadimy.Models.Teacher;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Acadimy.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<StudentPost> StudentPosts { get; set; }
        public DbSet<StudentPostLike> StudentPostLikes { get; set; }
        public DbSet<StudentPostComment> StudentPostComments { get; set; }
        public DbSet<StudentPostCommentLike> StudentPostCommentLikes { get; set; }

        public DbSet<TeacherExpertise> TeacherExpertises { get; set; }
        public DbSet<TeacherPost> TeacherPosts { get; set; }
        public DbSet<TeacherPostLike> TeacherPostLikes { get; set; }
        public DbSet<TeacherPostComment> TeacherPostComments { get; set; }
        public DbSet<TeacherPostCommentLike> TeacherPostCommentLikes { get; set; }

        public DbSet<TeacherCourse> TeacherCourses { get; set; }
        public DbSet<CourseCategory> CourseCategories { get; set; }
        public DbSet<CourseLevel> CourseLevels { get; set; }
        public DbSet<TeacherEnrollment> TeacherEnrollments { get; set; }
        public DbSet<CourseLesson> CourseLessons { get; set; }
        public DbSet<StudentLessonProgress> StudentLessonProgresses { get; set; }

        public DbSet<CommunityPost> CommunityPosts { get; set; }
        public DbSet<CommunityPostLike> CommunityPostLikes { get; set; }
        public DbSet<CommunityComment> CommunityComments { get; set; }
        public DbSet<CommunityCommentLike> CommunityCommentLikes { get; set; }

        public DbSet<TeacherAssignment> TeacherAssignments { get; set; }
        public DbSet<TeacherAssignmentSubmission> TeacherAssignmentSubmissions { get; set; }

        public DbSet<Notification> Notifications { get; set; }
        public DbSet<StudentSkill> StudentSkills { get; set; }
        public DbSet<MessageThread> MessageThreads { get; set; }
        public DbSet<Message> Messages { get; set; }
        public DbSet<CourseGroupMessage> CourseGroupMessages { get; set; }
        public DbSet<LiveClass> LiveClasses { get; set; }

        public DbSet<ProjectPost> ProjectPosts { get; set; }
        public DbSet<ProjectComment> ProjectComments { get; set; }
        public DbSet<ProjectRating> ProjectRatings { get; set; }
        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // ================= STUDENT =================
            builder.Entity<Message>()
    .HasOne(m => m.Sender)
    .WithMany()
    .HasForeignKey(m => m.SenderId)
    .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<CourseGroupMessage>()
                .HasOne(m => m.TeacherCourse)
                .WithMany()
                .HasForeignKey(m => m.TeacherCourseId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<CourseGroupMessage>()
                .HasOne(m => m.Sender)
                .WithMany()
                .HasForeignKey(m => m.SenderId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<StudentSkill>()
     .HasOne(s => s.User)
     .WithMany(u => u.StudentSkills)
     .HasForeignKey(s => s.UserId)
     .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<Notification>()
     .HasOne(n => n.User)
     .WithMany()
     .HasForeignKey(n => n.UserId)
     .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<TeacherAssignmentSubmission>()
    .Property(s => s.Grade)
    .HasPrecision(5, 2); 

            builder.Entity<TeacherAssignment>()
    .HasOne(a => a.User)
    .WithMany()
    .HasForeignKey(a => a.UserId)
    .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<TeacherAssignment>()
                .HasOne(a => a.TeacherCourse)
                .WithMany()
                .HasForeignKey(a => a.TeacherCourseId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<TeacherAssignmentSubmission>()
                .HasOne(s => s.TeacherAssignment)
                .WithMany(a => a.Submissions)
                .HasForeignKey(s => s.TeacherAssignmentId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<TeacherAssignmentSubmission>()
                .HasOne(s => s.Student)
                .WithMany()
                .HasForeignKey(s => s.StudentId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<StudentPostLike>()
                .HasOne(l => l.StudentPost)
                .WithMany(p => p.Likes)
                .HasForeignKey(l => l.StudentPostId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<StudentPostLike>()
                .HasOne(l => l.User)
                .WithMany()
                .HasForeignKey(l => l.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<StudentPostComment>()
                .HasOne(c => c.StudentPost)
                .WithMany(p => p.Comments)
                .HasForeignKey(c => c.StudentPostId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<StudentPostComment>()
                .HasOne(c => c.User)
                .WithMany()
                .HasForeignKey(c => c.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<StudentPostCommentLike>()
                .HasOne(l => l.StudentPostComment)
                .WithMany()
                .HasForeignKey(l => l.StudentPostCommentId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<StudentPostCommentLike>()
                .HasOne(l => l.User)
                .WithMany()
                .HasForeignKey(l => l.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            // ================= TEACHER =================

            builder.Entity<TeacherExpertise>()
                .HasOne(e => e.User)
                .WithMany(u => u.TeacherExpertises)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<TeacherPostLike>()
                .HasOne(l => l.TeacherPost)
                .WithMany(p => p.Likes)
                .HasForeignKey(l => l.TeacherPostId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<TeacherPostLike>()
                .HasOne(l => l.User)
                .WithMany()
                .HasForeignKey(l => l.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<TeacherPostComment>()
                .HasOne(c => c.TeacherPost)
                .WithMany(p => p.Comments)
                .HasForeignKey(c => c.TeacherPostId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<TeacherPostComment>()
                .HasOne(c => c.User)
                .WithMany()
                .HasForeignKey(c => c.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<TeacherPostComment>()
                .HasOne(c => c.ParentComment)
                .WithMany(c => c.Replies)
                .HasForeignKey(c => c.ParentCommentId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<TeacherPostCommentLike>()
                .HasOne(l => l.TeacherPostComment)
                .WithMany(c => c.Likes)
                .HasForeignKey(l => l.TeacherPostCommentId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<TeacherPostCommentLike>()
                .HasOne(l => l.User)
                .WithMany()
                .HasForeignKey(l => l.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<TeacherEnrollment>()
                .HasOne(e => e.Student)
                .WithMany(u => u.TeacherEnrollments)
                .HasForeignKey(e => e.StudentId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<TeacherEnrollment>()
                .HasOne(e => e.TeacherCourse)
                .WithMany(c => c.Enrollments)
                .HasForeignKey(e => e.TeacherCourseId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<CourseLesson>()
                .HasOne(l => l.TeacherCourse)
                .WithMany(c => c.Lessons)
                .HasForeignKey(l => l.TeacherCourseId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<StudentLessonProgress>()
                .HasOne(p => p.Student)
                .WithMany()
                .HasForeignKey(p => p.StudentId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<StudentLessonProgress>()
                .HasOne(p => p.CourseLesson)
                .WithMany()
                .HasForeignKey(p => p.CourseLessonId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<StudentLessonProgress>()
                .HasIndex(p => new { p.StudentId, p.CourseLessonId })
                .IsUnique();

            // ================= COMMUNITY =================

            builder.Entity<CommunityPost>()
                .HasOne(p => p.User)
                .WithMany()
                .HasForeignKey(p => p.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<CommunityPostLike>()
                .HasOne(l => l.Post)
                .WithMany(p => p.Likes)
                .HasForeignKey(l => l.CommunityPostId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<CommunityPostLike>()
                .HasOne(l => l.User)
                .WithMany()
                .HasForeignKey(l => l.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<CommunityPostLike>()
                .HasIndex(l => new { l.CommunityPostId, l.UserId })
                .IsUnique();

            builder.Entity<CommunityComment>()
                .HasOne(c => c.Post)
                .WithMany(p => p.Comments)
                .HasForeignKey(c => c.CommunityPostId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<CommunityComment>()
                .HasOne(c => c.User)
                .WithMany()
                .HasForeignKey(c => c.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<CommunityComment>()
                .HasOne(c => c.ParentComment)
                .WithMany(c => c.Replies)
                .HasForeignKey(c => c.ParentCommentId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<CommunityCommentLike>()
                .HasOne(l => l.Comment)
                .WithMany(c => c.Likes)
                .HasForeignKey(l => l.CommunityCommentId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<CommunityCommentLike>()
                .HasOne(l => l.User)
                .WithMany()
                .HasForeignKey(l => l.UserId)
                .OnDelete(DeleteBehavior.Restrict);
            // ================= MESSAGING =================

            builder.Entity<MessageThread>()
                .HasOne(t => t.User1)
                .WithMany()
                .HasForeignKey(t => t.User1Id)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<MessageThread>()
                .HasOne(t => t.User2)
                .WithMany()
                .HasForeignKey(t => t.User2Id)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Message>()
                .HasOne(m => m.Thread)
                .WithMany(t => t.Messages)
                .HasForeignKey(m => m.ThreadId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<MessageThread>()
                .HasIndex(t => new { t.User1Id, t.User2Id });
        }
    }
}