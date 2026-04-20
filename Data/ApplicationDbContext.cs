using Acadimy.Models;
using Acadimy.Models.Student;
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

        protected override void OnModelCreating(ModelBuilder builder)
        {
            // IMPORTANT : Garder le base.OnModelCreating pour Identity
            base.OnModelCreating(builder);

            // Configuration pour StudentPostLike
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

            // Configuration pour StudentPostComment (Prévention multiple cascade paths)
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
        }
    }
}