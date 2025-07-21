using AutoMapper;
using DatingApp.API.Data;
using DatingApp.API.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;

namespace DatingApp.API.Controllers;

public class AccountController(DataContext context, ITokenService tokenService, IMapper mapper) : BaseAPIController
{
    [HttpPost("register")]
    public async Task<ActionResult<Models.AuthenticatedUserDto>> RegisterAsync(Models.RegisterDto registerDTO)
    {
        if (await UserExistsAsync(registerDTO.Username)) return BadRequest("Username already exists");

        using var hmac = new HMACSHA512();
        var newUser = mapper.Map<Entities.Member>(registerDTO);
        newUser.UserName = registerDTO.Username.ToLower();

        context.Users.Add(newUser);
        await context.SaveChangesAsync();
        
        var authenticatedUser = new Models.AuthenticatedUserDto
        {
            Username = newUser.UserName,
            KnownAs = newUser.KnownAs,
            Gender = newUser.Gender,
            Token = tokenService.CreateToken(newUser)
        };
        return Ok(authenticatedUser);
    }

    [HttpPost("login")]
    public async Task<ActionResult<Models.AuthenticatedUserDto>> LoginAsync(Models.LoginDto loginDTO)
    {
        var user = await context.Users
            .Include(u => u.Photos)
            .FirstOrDefaultAsync(u => u.NormalizedUserName == loginDTO.Username.ToUpper());
        if (user == null || user.UserName == null) return Unauthorized("Invalid username");

        var authenticatedUser = new Models.AuthenticatedUserDto
        {
            Username = user.UserName,
            KnownAs = user.KnownAs,
            Gender = user.Gender,
            Token = tokenService.CreateToken(user),
            PhotoUrl = user.Photos.FirstOrDefault(p => p.IsMain)?.Url
        };
        return Ok(authenticatedUser);
    }
    
    private async Task<bool> UserExistsAsync(string username)
    {
        return await context.Users.AnyAsync(u => u.NormalizedUserName == username.ToUpper());
    }
}
