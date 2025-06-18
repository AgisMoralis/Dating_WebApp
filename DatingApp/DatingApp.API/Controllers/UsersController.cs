using AutoMapper;
using DatingApp.API.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DatingApp.API.Controllers;

[Authorize]
public class UsersController(IUserRepository userRepository) : BaseAPIController
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Models.UserDto>>> GetUsersAsync()
    {
        var users = await userRepository.GetUserDtosAsync();

        return Ok(users);
    }

    [HttpGet("{username}")]
    public async Task<ActionResult<Models.UserDto>> GetUserAsync(string username)
    {
        var user = await userRepository.GetUserDtoByUsernameAsync(username);

        if (user == null)
        {
            return NotFound();
        }
        
        return Ok(user);
    }
}
