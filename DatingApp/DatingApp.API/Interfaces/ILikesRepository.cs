using DatingApp.API.Helpers;

namespace DatingApp.API.Data;

public interface ILikesRepository
{
    Task<Entities.UserLike?> GetUserLike(int sourceUserId, int targetUserId);
    Task<PagedList<Models.MemberDto>> GetUserLikes(Models.LikesParametersDto likesParams);
    Task<IEnumerable<int>> GetCurrentUserLikeIds(int currentUserId);
    void DeleteLike(Entities.UserLike like);
    void AddLike(Entities.UserLike like);
    Task<bool> SaveAllAsync();
}
