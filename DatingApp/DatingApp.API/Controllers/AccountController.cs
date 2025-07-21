using AutoMapper;
using DatingApp.API.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DatingApp.API.Controllers;

public class AccountController(UserManager<Entities.Member> userManager, ITokenService tokenService, IMapper mapper) : BaseAPIController
{
    [HttpPost("register")]
    public async Task<ActionResult<Models.AuthenticatedUserDto>> RegisterAsync(Models.RegisterDto registerDTO)
    {
        if (await UserExistsAsync(registerDTO.Username)) return BadRequest("Username already exists");

        var newUser = mapper.Map<Entities.Member>(registerDTO);
        newUser.UserName = registerDTO.Username.ToLower();

        var result = await userManager.CreateAsync(newUser, registerDTO.Password);
        if (!result.Succeeded) return BadRequest(result.Errors);
        
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
        var user = await userManager.Users
            .Include(u => u.Photos)
            .FirstOrDefaultAsync(u => u.NormalizedUserName == loginDTO.Username.ToUpper());
        if (user == null || user.UserName == null) return Unauthorized("Invalid username");

        var result = await userManager.CheckPasswordAsync(user, loginDTO.Password);
        if (!result) return Unauthorized();

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
        return await userManager.Users.AnyAsync(u => u.NormalizedUserName == username.ToUpper());
    }
}
