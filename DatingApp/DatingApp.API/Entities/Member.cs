using Microsoft.AspNetCore.Identity;

namespace DatingApp.API.Entities;

public class Member : IdentityUser<int>
{
    public required DateOnly DateOfBirth { get; set; }
    public required string KnownAs { get; set; } = string.Empty;
    public DateTime Created { get; set; } = DateTime.UtcNow;
    public DateTime LastActive { get; set; } = DateTime.UtcNow;
    public required string Gender { get; set; }
    public string? Introduction { get; set; }
    public string? Interests { get; set; }
    public string? LookingFor { get; set; }
    public required string? City { get; set; }
    public required string? Country { get; set; }

    // Navigation properties
    public List<Photo> Photos { get; set; } = new List<Photo>();
    public List<UserLike> LikesUsers { get; set; } = [];
    public List<UserLike> LikedByUsers { get; set; } = [];
    public List<Message> MessagesSent { get; set; } = [];
    public List<Message> MessagesReceived { get; set; } = [];
    public ICollection<MemberRole> MemberRoles { get; set; } = [];
}