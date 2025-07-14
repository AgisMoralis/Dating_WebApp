using AutoMapper;
using AutoMapper.QueryableExtensions;
using DatingApp.API.Entities;
using DatingApp.API.Models;
using Microsoft.EntityFrameworkCore;

namespace DatingApp.API.Data;

public class LikesRepository(DataContext context, IMapper mapper) : ILikesRepository
{
    public async Task<UserLike?> GetUserLike(int sourceUserId, int targetUserId)
    {
        return await context.Likes.FindAsync(sourceUserId, targetUserId);
    }

    public async Task<IEnumerable<MemberDto>> GetUserLikes(string predicate, int userId)
    {
        var likes = context.Likes.AsQueryable();

        switch (predicate)
        {
            case "likes":
                return await likes
                    .Where(l => l.SourceUserId == userId)
                    .Select(l => l.TargetUser)
                    .ProjectTo<MemberDto>(mapper.ConfigurationProvider)
                    .ToListAsync();
            case "likedBy":
                return await likes
                    .Where(l => l.TargetUserId == userId)
                    .Select(l => l.SourceUser)
                    .ProjectTo<MemberDto>(mapper.ConfigurationProvider)
                    .ToListAsync();
            default:
                // Mutual Likes
                var currentUserLikeIds = await GetCurrentUserLikeIds(userId);
                return await likes
                    .Where(l => l.TargetUserId == userId && currentUserLikeIds.Contains(l.SourceUserId))
                    .Select(l => l.SourceUser)
                    .ProjectTo<MemberDto>(mapper.ConfigurationProvider)
                    .ToListAsync();
        }
    }

    public async Task<IEnumerable<int>> GetCurrentUserLikeIds(int currentUserId)
    {
        return await context.Likes
            .Where(l => l.SourceUserId == currentUserId)
            .Select(l => l.TargetUserId)
            .ToListAsync();
    }

    public void DeleteLike(UserLike like)
    {
         context.Likes.Remove(like);
    }

    public void AddLike(UserLike like)
    {
        context.Likes.Add(like);
    }

    public async Task<bool> SaveAllAsync()
    {
        return await context.SaveChangesAsync() > 0;
    }
}
