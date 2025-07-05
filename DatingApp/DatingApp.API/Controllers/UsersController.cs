using System.Security.Claims;
using AutoMapper;
using DatingApp.API.Extensions;
using DatingApp.API.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DatingApp.API.Controllers;

[Authorize]
public class UsersController(IUserRepository userRepository, IMapper mapper, IPhotoService photoService) : BaseAPIController
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
        var user = await userRepository.GetMemberByUsernameAsync(User.GetUsername());
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

    [HttpPost("add-photo")]
    public async Task<ActionResult<Models.PhotoDto>> AddPhotoAsync(IFormFile newPhoto)
    {
        var user = await userRepository.GetMemberByUsernameAsync(User.GetUsername());
        if (user == null)
        {
            return BadRequest("Could not update user");
        }

        var result = await photoService.AddPhotoAsync(newPhoto);
        if (result.Error != null)
        {
            return BadRequest(result.Error.Message);
        }

        var photo = new Entities.Photo
        {
            Url = result.SecureUrl.AbsoluteUri,
            PublicId = result.PublicId
        };
        user.Photos.Add(photo);

        if (await userRepository.SaveAllAsync())
        {
            return CreatedAtAction(
                nameof(GetUserAsync).Replace("Async", ""),
                new { username = user.Username },
                mapper.Map<Models.PhotoDto>(photo));
        }
        return BadRequest("A problem occurred while trying to add a new photo");
    }
}
