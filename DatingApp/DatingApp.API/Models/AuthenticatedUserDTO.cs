namespace DatingApp.API.Models;

public class AuthenticatedUserDTO
{
    public required string Username { get; set; }

    public required string Token { get; set; }
}
