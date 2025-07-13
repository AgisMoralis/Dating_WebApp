using DatingApp.API.Helpers;

namespace DatingApp.API.Interfaces;

public interface IUserRepository
{
    Task<Entities.Member?> GetMemberByIdAsync(int id);
    Task<Entities.Member?> GetMemberByUsernameAsync(string username);
    Task<Models.MemberDto?> GetMemberDtoByUsernameAsync(string username);
    Task<IEnumerable<Entities.Member>> GetMembersAsync();
    Task<PagedList<Models.MemberDto>> GetMemberDtosAsync(Models.MemberParametersDto memberParams);
    void Update(Entities.Member member);
    Task<bool> SaveAllAsync();
}
