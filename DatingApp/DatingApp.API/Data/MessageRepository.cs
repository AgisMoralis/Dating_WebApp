using AutoMapper;
using AutoMapper.QueryableExtensions;
using DatingApp.API.Entities;
using DatingApp.API.Helpers;
using DatingApp.API.Interfaces;
using DatingApp.API.Models;
using Microsoft.EntityFrameworkCore;

namespace DatingApp.API.Data;

public class MessageRepository(DataContext context, IMapper mapper) : IMessageRepository
{
    public void AddMessage(Message message)
    {
        context.Messages.Add(message);
    }

    public void DeleteMessage(Message message)
    {
        context.Messages.Remove(message);
    }

    public async Task<Message?> GetMessageAsync(int id)
    {
        return await context.Messages.FindAsync(id);
    }

    public async Task<PagedList<MessageDto>> GetMessagesForUserAsync(MessageParametersDto messageParams)
    {
        var query = context.Messages
            .OrderByDescending(x => x.MessageSent)
            .AsQueryable();
        query = messageParams.Container switch
        {
            "Inbox" => query.Where(m => m.Recipient.UserName == messageParams.CurrentUsername && !m.RecipientDeleted),
            "Outbox" => query.Where(m => m.Sender.UserName == messageParams.CurrentUsername && !m.SenderDeleted),
            _ => query.Where(m => m.Recipient.UserName == messageParams.CurrentUsername && m.DateRead == null && !m.RecipientDeleted),
        };

        var messages = query.ProjectTo<MessageDto>(mapper.ConfigurationProvider);
        return await PagedList<MessageDto>.CreateAsync(messages, messageParams.PageNumber, messageParams.PageSize);
    }

    public async Task<IEnumerable<MessageDto>> GetThreadMessagesAsync(string currentUsername, string recipientUsername)
    {
        // Bulk update unread messages
        var unreadMessages = await context.Messages
            .Where(x =>
                x.DateRead == null && x.RecipientUsername == currentUsername &&
                (
                    (x.RecipientUsername == currentUsername && !x.RecipientDeleted && x.SenderUsername == recipientUsername) ||
                    (x.SenderUsername == currentUsername && !x.SenderDeleted && x.RecipientUsername == recipientUsername)
                )
            )
            .ExecuteUpdateAsync(setters => setters.SetProperty(m => m.DateRead, DateTime.UtcNow));

        // Query and return the message thread
        var messages = await context.Messages
            .Where(x =>
                (x.RecipientUsername == currentUsername && !x.RecipientDeleted && x.SenderUsername == recipientUsername) ||
                (x.SenderUsername == currentUsername && !x.SenderDeleted && x.RecipientUsername == recipientUsername))
            .OrderBy(x => x.MessageSent)
            .ProjectTo<MessageDto>(mapper.ConfigurationProvider)
            .ToListAsync();

        return messages;
    }

    public async Task<bool> SaveAllAsync()
    {
        return await context.SaveChangesAsync() > 0;
    }

    public void AddGroup(Group group)
    {
        context.Groups.Add(group);
    }

    public void RemoveConnection(Connection connection)
    {
        context.Connections.Remove(connection);
    }

    public async Task<Connection?> GetConnection(string connectionId)
    {
        return await context.Connections.FindAsync(connectionId);
    }

    public async Task<Group?> GetMessageGroup(string groupName)
    {
        return await context.Groups
            .Include(x => x.Connections)
            .FirstOrDefaultAsync(x => x.Name == groupName);
    }

    public async Task<Group?> GetGroupForConnection(string connectionId)
    {
        return await context.Groups
            .Include(g => g.Connections)
            .Where(g => g.Connections.Any(c => c.ConnectionId == connectionId))
            .FirstOrDefaultAsync();
    }
}
