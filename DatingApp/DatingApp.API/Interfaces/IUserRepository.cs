using DatingApp.API.Entities;
using DatingApp.API.Models;

namespace DatingApp.API.Interfaces;

public interface IUserRepository
{
    Task<Member?> GetMemberByIdAsync(int id);
    Task<Member?> GetMemberByUsernameAsync(string username);
    Task<MemberDto?> GetMemberDtoByUsernameAsync(string username);
    Task<IEnumerable<Member>> GetMembersAsync();
    Task<IEnumerable<MemberDto>> GetMemberDtosAsync();
    void Update(Member member);
    Task<bool> SaveAllAsync();
}
