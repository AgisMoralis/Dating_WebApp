using Microsoft.AspNetCore.Identity;

namespace DatingApp.API.Entities;

public class MemberRole : IdentityUserRole<int>
{
    public Member Member { get; set; } = null!;
    public Role Role { get; set; } = null!;
}
