using DatingApp.API.Entities;

namespace DatingApp.API.Interfaces;

public interface IUserRepository
{
    Task<User?> GetUserByIdAsync(int id);
    Task<User?> GetUserByUsernameAsync(string username);
    Task<IEnumerable<User>> GetUsersAsync();
    void Update(User user);
    Task<bool> SaveAllAsync();
}
