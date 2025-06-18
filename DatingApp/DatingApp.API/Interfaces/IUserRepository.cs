using DatingApp.API.Entities;
using DatingApp.API.Models;

namespace DatingApp.API.Interfaces;

public interface IUserRepository
{
    Task<User?> GetUserByIdAsync(int id);
    Task<User?> GetUserByUsernameAsync(string username);
    Task<UserDto?> GetUserDtoByUsernameAsync(string username);
    Task<IEnumerable<User>> GetUsersAsync();
    Task<IEnumerable<UserDto>> GetUserDtosAsync();
    void Update(User user);
    Task<bool> SaveAllAsync();
}
