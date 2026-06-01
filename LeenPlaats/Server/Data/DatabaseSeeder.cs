using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Server.Models;

namespace Server.Data;

public static class DatabaseSeeder
{
    public static async Task SeedAsync(IServiceProvider services)
    {
        var db = services.GetRequiredService<AppDbContext>();
        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();

        if (await db.Users.AnyAsync()) return;

        await CreateUser(userManager, "user1@test.com", "User Een", "test1234");
        await CreateUser(userManager, "user2@test.com", "User Twee", "test1234");
    }

    private static async Task CreateUser(UserManager<ApplicationUser> userManager, string email, string displayName, string password)
    {
        var user = new ApplicationUser
        {
            UserName = email,
            Email = email,
            DisplayName = displayName,
            EmailConfirmed = true,
        };

        var result = await userManager.CreateAsync(user, password);
        if (!result.Succeeded)
            throw new Exception($"Seeding user {email} failed: {string.Join(", ", result.Errors.Select(e => e.Description))}");
    }
}
