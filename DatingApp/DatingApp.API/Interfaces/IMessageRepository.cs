using DatingApp.API.Helpers;

namespace DatingApp.API.Interfaces;

public interface IMessageRepository
{
    void AddMessage(Entities.Message message);
    void DeleteMessage(Entities.Message message);
    Task<Entities.Message?> GetMessageAsync(int id);
    Task<PagedList<Models.MessageDto>> GetMessagesForUserAsync(Models.MessageParametersDto messageParams);
    Task<IEnumerable<Models.MessageDto>> GetThreadMessagesAsync(string currentUsername, string recipientUsername);
    void AddGroup(Entities.Group group);
    void RemoveConnection(Entities.Connection connection);
    Task<Entities.Connection?> GetConnection(string connectionId);
    Task<Entities.Group?> GetMessageGroup(string groupName);
    Task<Entities.Group?> GetGroupForConnection(string connectionId);
}
