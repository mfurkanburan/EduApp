using EduApp.Web.Data;
using EduApp.Web.Models;
using EduApp.Web.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// DB - EduAppDb
string connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? "Data Source=eduApp.db";

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(connectionString));

// DI - CourseService
builder.Services.AddScoped<ICourseService, CourseService>();
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<ICourseSectionService, CourseSectionService>();

var app = builder.Build();

// Seed Data
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    
    if (!dbContext.Categories.Any())
    {
        dbContext.Categories.AddRange(
            new Category { Name = "Yazılım" },
            new Category { Name = "Tasarım" },
            new Category { Name = "Siber Güvenlik" }
        );

        dbContext.SaveChanges();
    }

    if (!dbContext.CourseSections.Any())
    {
        dbContext.CourseSections.AddRange(
            new CourseSection { Title = "Giriş", Order = 1, CourseId = 1 },
            new CourseSection { Title = "Temel Kavramlar", Order = 2, CourseId = 1 },
            new CourseSection { Title = "İleri Düzey Konular", Order = 3, CourseId = 1 }
        );

        dbContext.SaveChanges();
    }

    if (!dbContext.Lessons.Any())
    {
        dbContext.Lessons.AddRange(
            new Lesson { Title = "Ön Hazırlık", Order = 1, CourseSectionId = 1 },
            new Lesson { Title = "Gerekli Program Kurulumları", Order = 2, CourseSectionId = 1 },
            new Lesson { Title = "Programlama Temelleri", Order = 1, CourseSectionId = 2 },
            new Lesson { Title = "İleri Düzey Programlama", Order = 2, CourseSectionId = 2 }
        );

        dbContext.SaveChanges();
    }
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();


app.Run();