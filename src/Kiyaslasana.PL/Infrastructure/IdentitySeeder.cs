using System.Text;
using Kiyaslasana.EL.Entities;
using Microsoft.AspNetCore.Identity;

namespace Kiyaslasana.PL.Infrastructure;

public static class IdentitySeeder
{
    public static async Task SeedAsync(IServiceProvider services, IConfiguration configuration)
    {
        using var scope = services.CreateScope();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

        foreach (var roleName in IdentityRoles.All)
        {
            if (!await roleManager.RoleExistsAsync(roleName))
            {
                var createRole = await roleManager.CreateAsync(new IdentityRole(roleName));
                EnsureSucceeded(createRole, $"Role seed failed for '{roleName}'.");
            }
        }

        var seedEnabled = configuration.GetValue<bool>("Seed:Enabled");
        if (!seedEnabled)
        {
            return;
        }

        var adminEmail = configuration["Seed:AdminEmail"]?.Trim();
        var adminPassword = configuration["Seed:AdminPassword"];

        if (string.IsNullOrWhiteSpace(adminEmail) || string.IsNullOrWhiteSpace(adminPassword))
        {
            throw new InvalidOperationException("Seed:AdminEmail and Seed:AdminPassword are required when Seed:Enabled is true.");
        }

        var adminUser = await userManager.FindByEmailAsync(adminEmail);
        if (adminUser is null)
        {
            adminUser = new ApplicationUser
            {
                UserName = adminEmail,
                Email = adminEmail,
                EmailConfirmed = true
            };

            var createUser = await userManager.CreateAsync(adminUser, adminPassword);
            EnsureSucceeded(createUser, "Admin user seed failed.");
        }
        else if (!adminUser.EmailConfirmed)
        {
            adminUser.EmailConfirmed = true;
            var updateUser = await userManager.UpdateAsync(adminUser);
            EnsureSucceeded(updateUser, "Admin user email confirmation update failed during seed.");
        }

        if (!await userManager.IsInRoleAsync(adminUser, IdentityRoles.Admin))
        {
            var addToAdminRole = await userManager.AddToRoleAsync(adminUser, IdentityRoles.Admin);
            EnsureSucceeded(addToAdminRole, "Admin role assignment failed during seed.");
        }
    }

    private static void EnsureSucceeded(IdentityResult result, string message)
    {
        if (result.Succeeded)
        {
            return;
        }

        var details = string.Join("; ", result.Errors.Select(x => x.Description));
        var builder = new StringBuilder(message);

        if (!string.IsNullOrWhiteSpace(details))
        {
            builder.Append(' ').Append(details);
        }

        throw new InvalidOperationException(builder.ToString());
    }
}
