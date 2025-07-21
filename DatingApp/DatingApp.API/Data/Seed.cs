using System.Text.Json;
using Microsoft.EntityFrameworkCore;

namespace DatingApp.API.Data;

public class Seed
{
    public static async Task SeedUsersAsync(DataContext context)
    {
        if (await context.Users.AnyAsync()) return;

        var userData = await File.ReadAllTextAsync("Data/UserSeedData.json");
        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        var users = JsonSerializer.Deserialize<List<Entities.Member>>(userData, options);

        if (users == null || users.Count == 0) return;

        foreach (var user in users)
        {
            context.Users.Add(user);
        }
        
        await context.SaveChangesAsync();
    }
}
