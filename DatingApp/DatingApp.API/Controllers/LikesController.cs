using DatingApp.API.Extensions;
using DatingApp.API.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DatingApp.API.Controllers;

[Authorize]
public class LikesController(IUnitOfWork unitOfWork) : BaseAPIController
{
    [HttpPost("{targetUserId:int}")]
    public async Task<ActionResult> ToggleLikeAsync(int targetUserId)
    {
        var sourceUserId = User.GetUserId();
        if (sourceUserId == targetUserId)
        {
            return BadRequest("You cannot like yourself");
        }

        var existingLike = await unitOfWork.LikesRepository.GetUserLikeAsync(sourceUserId, targetUserId);
        if (existingLike == null)
        {
            var newLike = new Entities.UserLike
            {
                SourceUserId = sourceUserId,
                TargetUserId = targetUserId
            };
            unitOfWork.LikesRepository.AddLike(newLike);
        }
        else
        {
            unitOfWork.LikesRepository.DeleteLike(existingLike);
        }

        if (await unitOfWork.CompleteAsync())
        {
            return Ok();
        }
        return BadRequest("Failed to update like");
    }

    [HttpGet("{list}")]
    public async Task<ActionResult<IEnumerable<int>>> GetCurrentUserLikeIdsAsync()
    {
        return Ok(await unitOfWork.LikesRepository.GetCurrentUserLikeIdsAsync(User.GetUserId()));
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Models.MemberDto>>> GetUserLikesAsync([FromQuery]Models.LikesParametersDto likesParams)
    {
        likesParams.UserId = User.GetUserId();
        var users = await unitOfWork.LikesRepository.GetUserLikesAsync(likesParams);

        Response.AddPaginationHeader(users);

        return Ok(users);
    }
}
