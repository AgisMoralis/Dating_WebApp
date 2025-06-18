using DatingApp.API.Entities;
using DatingApp.API.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DatingApp.API.Data;

public class UserRepository(DataContext context) : IUserRepository
{
    public async Task<User?> GetUserByIdAsync(int id)
    {
        return await context.Users.FindAsync(id);
    }

    public async Task<User?> GetUserByUsernameAsync(string username)
    {
        return await context.Users
        .Include(u => u.Photos)
        .SingleOrDefaultAsync(u => u.Username == username);
    }

    public async Task<IEnumerable<User>> GetUsersAsync()
    {
        return await context.Users
        .Include(u => u.Photos)
        .ToListAsync();
    }

    public void Update(User user)
    {
        context.Entry(user).State = EntityState.Modified;
    }

    public async Task<bool> SaveAllAsync()
    {
        return await context.SaveChangesAsync() > 0;
    }
}
