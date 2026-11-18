using EduApp.Web.Constants;
using EduApp.Web.Models.Identity;
using Microsoft.AspNetCore.Identity;

namespace EduApp.Web.Data.Seed;

public static class IdentitySeed
{
    public static async Task SeedRolesAsync(IServiceProvider serviceProvider)
    {
        RoleManager<IdentityRole> roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        UserManager<ApplicationUser> userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();

        string[] roles =
        {
            ApplicationRoles.Student,
            ApplicationRoles.Instructor,
            ApplicationRoles.Admin
        };

        foreach (string roleName in roles)
        {
            bool roleExists = await roleManager.RoleExistsAsync(roleName);

            if (!roleExists)
            {
                IdentityResult roleResult = await roleManager.CreateAsync(new IdentityRole(roleName));

                if (!roleResult.Succeeded)
                {
                    string errors = string.Join(", ", roleResult.Errors.Select(e => e.Description));

                    throw new InvalidOperationException($"'{roleName}' rolü oluşturulamadı: {errors}");
                }
            }
        }
    }
}