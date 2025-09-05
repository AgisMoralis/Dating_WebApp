using AutoMapper;
using DatingApp.API.Extensions;
using DatingApp.API.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DatingApp.API.Controllers;

[Authorize]
public class UsersController(IUnitOfWork unitOfWork, IMapper mapper, IPhotoService photoService) : BaseAPIController
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Models.MemberDto>>> GetUsersAsync([FromQuery]Models.MemberParametersDto memberParams)
    {
        memberParams.CurrentUsername = User.GetUsername();
        var users = await unitOfWork.UserRepository.GetMemberDtosAsync(memberParams);

        Response.AddPaginationHeader(users);

        return Ok(users);
    }

    [HttpGet("{username}")]
    public async Task<ActionResult<Models.MemberDto>> GetUserAsync(string username)
    {
        var user = await unitOfWork.UserRepository.GetMemberDtoByUsernameAsync(username);

        if (user == null)
        {
            return NotFound();
        }

        return Ok(user);
    }

    [HttpPut]
    public async Task<ActionResult> UpdateUserAsync(Models.MemberUpdateDto memberUpdateDto)
    {
        var user = await unitOfWork.UserRepository.GetMemberByUsernameAsync(User.GetUsername());
        if (user == null)
        {
            return BadRequest("Could not find user");
        }

        mapper.Map(memberUpdateDto, user);
        if (await unitOfWork.CompleteAsync())
        {
            return NoContent();
        }
        return BadRequest("Failed to update the user");
    }

    [HttpPost("add-photo")]
    public async Task<ActionResult<Models.PhotoDto>> AddPhotoAsync(IFormFile newPhoto)
    {
        var user = await unitOfWork.UserRepository.GetMemberByUsernameAsync(User.GetUsername());
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
        if(user.Photos.Count == 0)
        {
            photo.IsMain = true;
        }
        user.Photos.Add(photo);

        if (await unitOfWork.CompleteAsync())
        {
            return CreatedAtAction(
                nameof(GetUserAsync).Replace("Async", ""),
                new { username = user.UserName },
                mapper.Map<Models.PhotoDto>(photo));
        }
        return BadRequest("A problem occurred while trying to add a new photo");
    }

    [HttpPut("set-main-photo/{photoId:int}")]
    public async Task<ActionResult> SetMainPhotoAsync(int photoId)
    {
        var user = await unitOfWork.UserRepository.GetMemberByUsernameAsync(User.GetUsername());
        if (user == null)
        {
            return BadRequest("Could not find user");
        }

        var photo = user.Photos.FirstOrDefault(p => p.Id == photoId);
        if (photo == null || photo.IsMain)
        {
            return BadRequest("Cannot use this as the main photo");
        }

        var currentMainPhoto = user.Photos.FirstOrDefault(p => p.IsMain);
        if (currentMainPhoto != null)
        {
            currentMainPhoto.IsMain = false;
        }
        photo.IsMain = true;

        if (await unitOfWork.CompleteAsync())
        {
            return NoContent();
        }
        return BadRequest("A problem occurred while trying to set the main photo");
    }

    [HttpDelete("delete-photo/{photoId:int}")]
    public async Task<ActionResult> DeletePhotoAsync(int photoId)
    {
        var user = await unitOfWork.UserRepository.GetMemberByUsernameAsync(User.GetUsername());
        if (user == null)
        {
            return BadRequest("Could not find user");
        }

        var photo = user.Photos.FirstOrDefault(p => p.Id == photoId);
        if (photo == null || photo.IsMain)
        {
            return BadRequest("Cannot delete this photo");
        }

        if (photo.PublicId != null)
        {
            var result = await photoService.DeletePhotoAsync(photo.PublicId);
            if (result.Error != null)
            {
                return BadRequest(result.Error.Message);
            }
        }

        user.Photos.Remove(photo);
        if (await unitOfWork.CompleteAsync())
        {
            return Ok();
        }
        return BadRequest("A problem occurred while trying to delete the photo");
    }
}
