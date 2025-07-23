using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DatingApp.API.Controllers;

public class AdminController(UserManager<Entities.Member> userManager) : BaseAPIController
{
    [Authorize(Policy = "RequireAdminRole")]
    [HttpGet("users-with-roles")]
    public async Task<ActionResult> GetUsersWithRolesAsync()
    {
        var users = await userManager.Users
            .OrderBy(x => x.UserName)
            .Select(x => new
            {
                x.Id,
                Username = x.UserName,
                Roles = x.MemberRoles.Select(r => r.Role.Name).ToList()
            }).ToListAsync();

        return Ok(users);
    }

    [Authorize(Policy = "RequireAdminRole")]
    [HttpPost("edit-roles/{username}")]
    public async Task<ActionResult> EditRolesAsync(string username, string roles)
    {
        if (string.IsNullOrEmpty(roles)) return BadRequest("You must select at least one role");
        var selectedRoles = roles.Split(",").ToArray();

        var user = await userManager.FindByNameAsync(username);
        if (user == null) return BadRequest("User not found");

        var userRoles = await userManager.GetRolesAsync(user);

        var addRoleResult = await userManager.AddToRolesAsync(user, selectedRoles.Except(userRoles));
        if (!addRoleResult.Succeeded) return BadRequest("Failed to add to roles");

        var removeRoleResult = await userManager.RemoveFromRolesAsync(user, userRoles.Except(selectedRoles));
        if (!removeRoleResult.Succeeded) return BadRequest("Failed to remove from roles");

        return Ok(await userManager.GetRolesAsync(user));
    }

    [Authorize(Policy = "ModeratePhotoRole")]
    [HttpGet("photos-for-moderation")]
    public ActionResult GetPhotosForModerationAsync()
    {
        return Ok("Admins or moderators can see this");
    }
}
