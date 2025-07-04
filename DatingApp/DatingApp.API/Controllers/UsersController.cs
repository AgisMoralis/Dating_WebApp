using System.Security.Claims;
using AutoMapper;
using DatingApp.API.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DatingApp.API.Controllers;

[Authorize]
public class UsersController(IUserRepository userRepository, IMapper mapper) : BaseAPIController
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

    [HttpPut]
    public async Task<ActionResult> UpdateUserAsync(Models.MemberUpdateDto memberUpdateDto)
    {
        var username = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (username == null)
        {
            return BadRequest("Wrong username in token");
        }

        var user = await userRepository.GetMemberByUsernameAsync(username);
        if (user == null)
        {
            return BadRequest("Could not find user");
        }

        mapper.Map(memberUpdateDto, user);
        if (await userRepository.SaveAllAsync())
        {
            return NoContent();
        }
        return BadRequest("Failed to update the user");
    }
}
