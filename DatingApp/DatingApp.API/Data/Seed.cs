using System.Text.Json;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace DatingApp.API.Data;

public class Seed
{
    public static async Task SeedUsersAsync(UserManager<Entities.Member> userManager, RoleManager<Entities.Role> roleManager)
    {
        if (await userManager.Users.AnyAsync()) return;

        var userData = await File.ReadAllTextAsync("Data/UserSeedData.json");
        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        var users = JsonSerializer.Deserialize<List<Entities.Member>>(userData, options);

        if (users == null || users.Count == 0) return;

        // Create some roles in the database
        var roles = new List<Entities.Role>()
        {
            new() { Name = "Member" },
            new() { Name = "Admin" },
            new() { Name = "Moderator" },
        };
        foreach (var role in roles)
        {
            await roleManager.CreateAsync(role);
        }

        // Create password for each "Member" user and store them in the database
        foreach (var user in users)
        {
            user.UserName = user.UserName!.ToLower();
            await userManager.CreateAsync(user, "pa$$w0rd");
            await userManager.AddToRoleAsync(user, "Member");
        }

        // Create a new "Admin" user with a password and store it in the database
        var admin = new Entities.Member
        {
            UserName = "admin",
            KnownAs = "Admin",
            DateOfBirth = default,
            Gender = "",
            City = "",
            Country = ""
        };
        await userManager.CreateAsync(admin, "pa$$w0rd");
        await userManager.AddToRolesAsync(admin, [ "Admin", "Moderator" ]);
    }
}
