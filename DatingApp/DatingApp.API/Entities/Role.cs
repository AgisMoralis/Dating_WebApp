using Microsoft.AspNetCore.Identity;

namespace DatingApp.API.Entities;

public class Role : IdentityRole<int>
{
    public ICollection<MemberRole> MemberRoles { get; set; } = [];
}
