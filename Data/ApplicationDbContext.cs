using Acadimy.Models;
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

        public DbSet<TeacherExpertise> TeacherExpertises { get; set; }
        public DbSet<TeacherPost> TeacherPosts { get; set; }
        public DbSet<TeacherPostLike> TeacherPostLikes { get; set; }
        public DbSet<TeacherPostComment> TeacherPostComments { get; set; }
        public DbSet<TeacherPostCommentLike> TeacherPostCommentLikes { get; set; }
        public DbSet<TeacherCourse> TeacherCourses { get; set; }
        public DbSet<CourseCategory> CourseCategories { get; set; }
        public DbSet<CourseLevel> CourseLevels { get; set; }
        public DbSet<TeacherEnrollment> TeacherEnrollments { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // ================= STUDENT =================

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

            // ================= TEACHER EXPERTISE =================

            builder.Entity<TeacherExpertise>()
                .HasOne(e => e.User)
                .WithMany(u => u.TeacherExpertises)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // ================= TEACHER POSTS =================

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

            // ================= TEACHER ENROLLMENTS =================

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
        }
    }
}