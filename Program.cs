using Acadimy.Data;
using Acadimy.Models;
using Acadimy.Models.Teacher;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options => {
    options.Password.RequiredLength = 6;
    options.Password.RequireDigit = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireNonAlphanumeric = false;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

builder.Services.AddControllersWithViews();

var app = builder.Build();

// Initialisation des rôles SQL
using (var scope = app.Services.CreateScope())
{
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    string[] roleNames = { "Enseignant", "Étudiant" };
    foreach (var roleName in roleNames)
    {
        if (!await roleManager.RoleExistsAsync(roleName))
        {
            await roleManager.CreateAsync(new IdentityRole(roleName));
        }
    }
}
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

    // Ajout des catégories si la table est vide
    if (!context.CourseCategories.Any())
    {
        context.CourseCategories.AddRange(
            new CourseCategory { Name = "Développement Web" },
            new CourseCategory { Name = "Data Science" },
            new CourseCategory { Name = "Marketing" },
            new CourseCategory { Name = "Design" }
        );
    }

    // Ajout des niveaux si la table est vide
    if (!context.CourseLevels.Any())
    {
        context.CourseLevels.AddRange(
            new CourseLevel { Name = "Débutant" },
            new CourseLevel { Name = "Intermédiaire" },
            new CourseLevel { Name = "Avancé" }
        );
    }
    context.SaveChanges();
}

app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

// Route par défaut sur le Login
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Account}/{action=Login}/{id?}");

app.Run();