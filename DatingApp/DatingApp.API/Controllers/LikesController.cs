using DatingApp.API.Data;
using DatingApp.API.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DatingApp.API.Controllers;

[Authorize]
public class LikesController(ILikesRepository likesRepository) : BaseAPIController
{
    [HttpPost("{targetUserId:int}")]
    public async Task<ActionResult> ToggleLikeAsync(int targetUserId)
    {
        var sourceUserId = User.GetUserId();
        if (sourceUserId == targetUserId)
        {
            return BadRequest("You cannot like yourself");
        }

        var existingLike = await likesRepository.GetUserLikeAsync(sourceUserId, targetUserId);
        if (existingLike == null)
        {
            var newLike = new Entities.UserLike
            {
                SourceUserId = sourceUserId,
                TargetUserId = targetUserId
            };
            likesRepository.AddLike(newLike);
        }
        else
        {
            likesRepository.DeleteLike(existingLike);
        }

        if (await likesRepository.SaveAllAsync())
        {
            return Ok();
        }
        return BadRequest("Failed to update like");
    }

    [HttpGet("{list}")]
    public async Task<ActionResult<IEnumerable<int>>> GetCurrentUserLikeIdsAsync()
    {
        return Ok(await likesRepository.GetCurrentUserLikeIdsAsync(User.GetUserId()));
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Models.MemberDto>>> GetUserLikesAsync([FromQuery]Models.LikesParametersDto likesParams)
    {
        likesParams.UserId = User.GetUserId();
        var users = await likesRepository.GetUserLikesAsync(likesParams);

        Response.AddPaginationHeader(users);

        return Ok(users);
    }
}
