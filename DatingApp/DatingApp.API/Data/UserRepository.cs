using AutoMapper;
using AutoMapper.QueryableExtensions;
using DatingApp.API.Entities;
using DatingApp.API.Interfaces;
using DatingApp.API.Models;
using Microsoft.EntityFrameworkCore;

namespace DatingApp.API.Data;

public class UserRepository(DataContext context, IMapper mapper) : IUserRepository
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

    public async Task<UserDto?> GetUserDtoByUsernameAsync(string username)
    {
        return await context.Users
            .Where(u => u.Username == username)
            .ProjectTo<UserDto>(mapper.ConfigurationProvider)
            .SingleOrDefaultAsync();
    }

    public async Task<IEnumerable<User>> GetUsersAsync()
    {
        return await context.Users
            .Include(u => u.Photos)
            .ToListAsync();
    }

    public async Task<IEnumerable<UserDto>> GetUserDtosAsync()
    {
        return await context.Users
            .ProjectTo<UserDto>(mapper.ConfigurationProvider)
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
