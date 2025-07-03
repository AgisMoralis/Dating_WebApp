using System.Security.Claims;
using DatingApp.API.Entities;
using DatingApp.API.Interfaces;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.IdentityModel.Tokens;

namespace DatingApp.API.Services;

public class TokenService(IConfiguration config) : ITokenService
{
    public string CreateToken(Member user)
    {
        var tokenKey = config["TokenKey"] ?? throw new ArgumentNullException("Cannot access TokenKey from appsettings.json");
        if (tokenKey.Length < 64) throw new ArgumentException("TokenKey must be at least 64 characters long");

        var key = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(tokenKey));
        var claims = new Claim[]
        {
            new (ClaimTypes.NameIdentifier, user.Username),
        };
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddDays(7),
            SigningCredentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature)
        };
        var tokenHandler = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler();
        var token = tokenHandler.CreateToken(tokenDescriptor);

        return tokenHandler.WriteToken(token);
    }
}
