using DatingApp.API.Data;
using DatingApp.API.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;

namespace DatingApp.API.Controllers;

public class AccountController(DataContext context, ITokenService tokenService) : BaseAPIController
{
    [HttpPost("register")]
    public async Task<ActionResult<Models.AuthenticatedUserDTO>> RegisterAsync(Models.RegisterDTO registerDTO)
    {
        if (await UserExistsAsync(registerDTO.Username)) return BadRequest("Username already exists");

        using var hmac = new HMACSHA512();
        var user = new Entities.User
        {
            Username = registerDTO.Username.ToLower(),
            PasswordHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(registerDTO.Password)),
            PasswordSalt = hmac.Key
        };

        context.Users.Add(user);
        await context.SaveChangesAsync();

        var authenticatedUser = new Models.AuthenticatedUserDTO
        {
            Username = user.Username,
            Token = tokenService.CreateToken(user)
        };
        return Ok(authenticatedUser);
    }

    [HttpPost("login")]
    public async Task<ActionResult<Models.AuthenticatedUserDTO>> LoginAsync(Models.LoginDTO loginDTO)
    {
        var user = await context.Users
            .FirstOrDefaultAsync(u => u.Username.ToLower() == loginDTO.Username.ToLower());
        if (user == null) return Unauthorized("Invalid username");

        using var hmac = new HMACSHA512(user.PasswordSalt);
        var computedHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(loginDTO.Password));
        for (int i = 0; i < computedHash.Length; i++)
        {
            if (computedHash[i] != user.PasswordHash[i]) return Unauthorized("Invalid password");
        }

        var authenticatedUser = new Models.AuthenticatedUserDTO
        {
            Username = user.Username,
            Token = tokenService.CreateToken(user)
        };
        return Ok(authenticatedUser);
    }

    private async Task<bool> UserExistsAsync(string username)
    {
        return await context.Users.AnyAsync(u => u.Username.ToLower() == username.ToLower());
    }
}
