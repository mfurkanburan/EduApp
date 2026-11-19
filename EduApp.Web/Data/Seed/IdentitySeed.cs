using EduApp.Web.Constants;
using EduApp.Web.Models.Identity;
using Microsoft.AspNetCore.Identity;

namespace EduApp.Web.Data.Seed;

public static class IdentitySeed
{
    public static async Task SeedAsync(IServiceProvider serviceProvider, IConfiguration configuration)
    {
        RoleManager<IdentityRole> roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();

        UserManager<ApplicationUser> userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();

        await CreateRolesAsync(roleManager);

        await CreateAdminUserAsync(userManager, configuration);
    }

    public static async Task CreateRolesAsync(RoleManager<IdentityRole> roleManager)
    {
        string[] roleNames = { ApplicationRoles.Admin, ApplicationRoles.Instructor, ApplicationRoles.Student };

        foreach (string roleName in roleNames)
        {
            bool roleExists = await roleManager.RoleExistsAsync(roleName);

            if (roleExists) continue;

            IdentityResult result = await roleManager.CreateAsync(new IdentityRole(roleName));

            if (!result.Succeeded)
            {
                throw new Exception(CreateErrorMessage($"{roleName} rolü oluşturulamadı", result));
            }
        }
    }

    private static async Task CreateAdminUserAsync(UserManager<ApplicationUser> userManager, IConfiguration configuration)
    {
        string? email = configuration["Admin:Email"];
        string? password = configuration["Admin:Password"];
        string? firstName = configuration["Admin:FirstName"] ?? "EduApp";
        string? lastName = configuration["Admin:LastName"] ?? "Yönetici";

        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password)) return;

        ApplicationUser? adminUser = await userManager.FindByEmailAsync(email);

        if (adminUser is null)
        {
            adminUser = new ApplicationUser
            {
                FirstName = firstName,
                LastName = lastName,
                UserName = email,
                Email = email,
                EmailConfirmed = true,
                CreatedAt = DateTime.UtcNow
            };

            IdentityResult createResult = await userManager.CreateAsync(adminUser, password);

            if (!createResult.Succeeded)
            {
                throw new Exception(CreateErrorMessage("Admin kullanıcısı oluşturulamadı", createResult));
            }
        }

        bool isAdmin = await userManager.IsInRoleAsync(adminUser, ApplicationRoles.Admin);

        if (!isAdmin)
        {
            IdentityResult roleResult = await userManager.AddToRoleAsync(adminUser, ApplicationRoles.Admin);

            if (!roleResult.Succeeded)
            {
                throw new Exception(CreateErrorMessage("Admin rolü atanamadı", roleResult));
            }
        }
    }

    private static string CreateErrorMessage(string title, IdentityResult result)
    {
        string errors = string.Join(", ", result.Errors.Select(e => e.Description));
        return $"{title}: {errors}";
    }
}