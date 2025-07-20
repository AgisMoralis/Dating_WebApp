using DatingApp.API.Helpers;

namespace DatingApp.API.Interfaces;

public interface IMessageRepository
{
    void AddMessage(Entities.Message message);
    void DeleteMessage(Entities.Message message);
    Task<Entities.Message?> GetMessageAsync(int id);
    Task<PagedList<Models.MessageDto>> GetMessagesForUserAsync(Models.MessageParametersDto messageParams);
    Task<IEnumerable<Models.MessageDto>> GetThreadMessagesAsync(string currentUsername, string recipientUsername);
    Task<bool> SaveAllAsync();
}
