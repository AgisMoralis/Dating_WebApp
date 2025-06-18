using System;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace DatingApp.API.Data;

public class Seed
{
    public static async Task SeedUsersAsync(DataContext context)
    {
        if (await context.Users.AnyAsync()) return;

        var userData = await File.ReadAllTextAsync("Data/UserSeedData.json");
        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        var users = JsonSerializer.Deserialize<List<Entities.User>>(userData, options);

        if (users == null || users.Count == 0) return;

        foreach (var user in users)
        {
            using var hmac = new System.Security.Cryptography.HMACSHA512();

            user.Username = user.Username.ToLower();
            user.PasswordHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes("pa$$word"));
            user.PasswordSalt = hmac.Key;

            context.Users.Add(user);
        }
        
        await context.SaveChangesAsync();
    }
}
