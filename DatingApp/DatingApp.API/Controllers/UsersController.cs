using DatingApp.API.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DatingApp.API.Controllers;

public class UsersController(DataContext context) : BaseAPIController
{
    [AllowAnonymous]
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Entities.User>>> GetUsersAsync()
    {
        var users = await context.Users.ToListAsync();

        return Ok(users);
    }

    [Authorize]
    [HttpGet("{id:int}")]
    public async Task<ActionResult<Entities.User>> GetUserAsync(int id)
    {
        var user = await context.Users.FindAsync(id);

        if (user == null)
        {
            return NotFound();
        }
        return Ok(user);
    }
}
