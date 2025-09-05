namespace DatingApp.API.Entities;

public class UserLike
{
    // Navigation properties
    public Member SourceUser { get; set; } = null!;
    public int SourceUserId { get; set; }
    public Member TargetUser { get; set; } = null!;
    public int TargetUserId { get; set; }
}
