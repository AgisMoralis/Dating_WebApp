using AutoMapper;
using DatingApp.API.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DatingApp.API.Controllers;

[Authorize]
public class UsersController(IUserRepository userRepository) : BaseAPIController
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Models.MemberDto>>> GetUsersAsync()
    {
        var users = await userRepository.GetMemberDtosAsync();

        return Ok(users);
    }

    [HttpGet("{username}")]
    public async Task<ActionResult<Models.MemberDto>> GetUserAsync(string username)
    {
        var user = await userRepository.GetMemberDtoByUsernameAsync(username);

        if (user == null)
        {
            return NotFound();
        }
        
        return Ok(user);
    }
}
