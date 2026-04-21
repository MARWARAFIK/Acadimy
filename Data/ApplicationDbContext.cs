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

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

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

            builder.Entity<TeacherExpertise>()
                .HasOne(e => e.User)
                .WithMany(u => u.TeacherExpertises)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}