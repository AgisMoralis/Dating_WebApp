using AutoMapper;
using AutoMapper.QueryableExtensions;
using DatingApp.API.Entities;
using DatingApp.API.Helpers;
using DatingApp.API.Interfaces;
using DatingApp.API.Models;
using Microsoft.EntityFrameworkCore;

namespace DatingApp.API.Data;

public class UserRepository(DataContext context, IMapper mapper) : IUserRepository
{
    public async Task<Member?> GetMemberByIdAsync(int id)
    {
        return await context.Members.FindAsync(id);
    }

    public async Task<Member?> GetMemberByUsernameAsync(string username)
    {
        return await context.Members
            .Include(u => u.Photos)
            .SingleOrDefaultAsync(u => u.Username == username);
    }

    public async Task<MemberDto?> GetMemberDtoByUsernameAsync(string username)
    {
        return await context.Members
            .Where(u => u.Username == username)
            .ProjectTo<MemberDto>(mapper.ConfigurationProvider)
            .SingleOrDefaultAsync();
    }

    public async Task<IEnumerable<Member>> GetMembersAsync()
    {
        return await context.Members
            .Include(u => u.Photos)
            .ToListAsync();
    }

    public async Task<PagedList<MemberDto>> GetMemberDtosAsync(MemberParametersDto memberParams)
    {
        var query = context.Members.AsQueryable()
            .Where(u => u.Username != memberParams.CurrentUsername);

        if (memberParams.Gender != null)
        {
            query = query.Where(u => u.Gender == memberParams.Gender);
        }

        var minDateOfBirth = DateOnly.FromDateTime(DateTime.Today.AddYears(-memberParams.MaxAge - 1));
        var maxDateOfBirth = DateOnly.FromDateTime(DateTime.Today.AddYears(-memberParams.MinAge));

        query = query.Where(u => u.DateOfBirth >= minDateOfBirth && u.DateOfBirth <= maxDateOfBirth);

        return await PagedList<MemberDto>.CreateAsync(
            query.ProjectTo<MemberDto>(mapper.ConfigurationProvider),
            memberParams.PageNumber,
            memberParams.PageSize);
    }

    public void Update(Member member)
    {
        context.Entry(member).State = EntityState.Modified;
    }

    public async Task<bool> SaveAllAsync()
    {
        return await context.SaveChangesAsync() > 0;
    }
}
