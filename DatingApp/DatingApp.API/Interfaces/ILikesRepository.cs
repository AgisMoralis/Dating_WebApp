using DatingApp.API.Helpers;

namespace DatingApp.API.Data;

public interface ILikesRepository
{
    Task<Entities.UserLike?> GetUserLikeAsync(int sourceUserId, int targetUserId);
    Task<PagedList<Models.MemberDto>> GetUserLikesAsync(Models.LikesParametersDto likesParams);
    Task<IEnumerable<int>> GetCurrentUserLikeIdsAsync(int currentUserId);
    void DeleteLike(Entities.UserLike like);
    void AddLike(Entities.UserLike like);
    Task<bool> SaveAllAsync();
}
