using DatingApp.API.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DatingApp.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UsersController(DataContext context) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Entities.User>>> GetUsersAsync()
    {
        var users = await context.Users.ToListAsync();
        
        return Ok(users);
    }

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
