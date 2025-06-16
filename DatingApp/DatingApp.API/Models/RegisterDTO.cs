using System.ComponentModel.DataAnnotations;

namespace DatingApp.API.Models;

public class RegisterDTO
{
    [Required]
    [MaxLength(100)]
    public string Username { get; set; } = string.Empty;

    [Required]
    [StringLength(20, MinimumLength = 6)]
    public string Password { get; set; } = string.Empty;
}
