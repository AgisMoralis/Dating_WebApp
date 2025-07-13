namespace DatingApp.API.Data;

public interface ILikesRepository
{
    Task<Entities.UserLike?> GetUserLike(int sourceUserId, int targetUserId);
    Task<IEnumerable<Models.MemberDto>> GetUserLikes(string predicate, int userId);
    Task<IEnumerable<int>> GetCurrentUserLikeIds(int currentUserId);
    void DeleteLike(Entities.UserLike like);
    void AddLike(Entities.UserLike like);
    Task<bool> SaveAllAsync();
}
