using System.ComponentModel.DataAnnotations;

namespace DatingApp.API.Models;

public class LoginDTO
{
    [Required]
    [MaxLength(100)]
    public required string Username { get; set; }

    [Required]
    public required string Password { get; set; }
}
