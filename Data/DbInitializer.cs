using Microsoft.AspNetCore.Identity;
using HouseholdNeedsManager.Data;

namespace HouseholdNeedsManager.Data;

/// <summary>
/// Database initializer to seed roles and initial data
/// </summary>
public static class DbInitializer
{
    /// <summary>
    /// Initializes the database with default roles
    /// </summary>
    public static async Task Initialize(IServiceProvider serviceProvider)
    {
        var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();

        string[] roleNames = { "Owner", "Admin", "Member" };

        foreach (var roleName in roleNames)
        {
            var roleExist = await roleManager.RoleExistsAsync(roleName);
            if (!roleExist)
            {
                await roleManager.CreateAsync(new IdentityRole(roleName));
            }
        }
    }
}
