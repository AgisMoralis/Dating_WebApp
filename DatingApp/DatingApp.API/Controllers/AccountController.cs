using DatingApp.API.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;

namespace DatingApp.API.Controllers;

public class AccountController(DataContext context) : BaseAPIController
{
    [HttpPost("register")]
    public async Task<ActionResult<Entities.User>> RegisterAsync(Models.RegisterDTO registerDTO)
    {
        if (await UserExistsAsync(registerDTO.Username)) return BadRequest("Username already exists");

        using var hmac = new HMACSHA512();
        var user = new Entities.User
        {
            Name = registerDTO.Username.ToLower(),
            PasswordHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(registerDTO.Password)),
            PasswordSalt = hmac.Key
        };

        context.Users.Add(user);
        await context.SaveChangesAsync();

        return Ok(user);
    }

    private async Task<bool> UserExistsAsync(string username)
    {
        return await context.Users.AnyAsync(u => u.Name.ToLower() == username.ToLower());
    }
}
