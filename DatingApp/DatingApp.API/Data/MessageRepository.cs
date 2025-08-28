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
        // Create the query that shall return the message thread
        var messagesQuery = context.Messages
            .Where(x =>
                (x.RecipientUsername == currentUsername && !x.RecipientDeleted && x.SenderUsername == recipientUsername) ||
                (x.SenderUsername == currentUsername && !x.SenderDeleted && x.RecipientUsername == recipientUsername))
            .OrderBy(x => x.MessageSent)
            .AsQueryable();

        // Execute the first query that updates all unread messages, by assigning the date now
        var unreadMessages = messagesQuery.Where(m => m.DateRead == null && m.RecipientUsername == currentUsername).ToList();
        if (unreadMessages.Count != 0)
        {
            unreadMessages.ForEach(x => x.DateRead = DateTime.UtcNow);
        }

        return await messagesQuery.ProjectTo<MessageDto>(mapper.ConfigurationProvider).ToListAsync();
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
