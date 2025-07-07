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
    public async Task<ActionResult<Models.AuthenticatedUserDTO>> RegisterAsync(Models.RegisterDTO registerDTO)
    {
        if (await UserExistsAsync(registerDTO.Username)) return BadRequest("Username already exists");

        using var hmac = new HMACSHA512();
        var newUser = mapper.Map<Entities.Member>(registerDTO);
        newUser.Username = registerDTO.Username.ToLower();
        newUser.PasswordHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(registerDTO.Password));
        newUser.PasswordSalt = hmac.Key;

        context.Members.Add(newUser);
        await context.SaveChangesAsync();
        
        var authenticatedUser = new Models.AuthenticatedUserDTO
        {
            Username = newUser.Username,
            KnownAs = newUser.KnownAs,
            Token = tokenService.CreateToken(newUser)
        };
        return Ok(authenticatedUser);
    }

    [HttpPost("login")]
    public async Task<ActionResult<Models.AuthenticatedUserDTO>> LoginAsync(Models.LoginDTO loginDTO)
    {
        var user = await context.Members
            .Include(u => u.Photos)
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
            KnownAs = user.KnownAs,
            Token = tokenService.CreateToken(user),
            PhotoUrl = user.Photos.FirstOrDefault(p => p.IsMain)?.Url
        };
        return Ok(authenticatedUser);
    }

    private async Task<bool> UserExistsAsync(string username)
    {
        return await context.Members.AnyAsync(u => u.Username.ToLower() == username.ToLower());
    }
}
