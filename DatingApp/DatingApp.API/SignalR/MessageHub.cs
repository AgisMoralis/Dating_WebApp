using AutoMapper;
using DatingApp.API.Extensions;
using DatingApp.API.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace DatingApp.API.SignalR;

[Authorize]
public class MessageHub(IUserRepository userRepository, IMessageRepository messageRepository,
IMapper mapper, IHubContext<PresenceHub> presenceHub) : Hub
{
    public override async Task OnConnectedAsync()
    {
        var httpContext = Context.GetHttpContext();
        var otherUser = httpContext?.Request.Query["user"];

        if (Context.User is null || string.IsNullOrEmpty(otherUser))
            throw new HubException("Cannot join group");
        var groupName = GetGroupName(Context.User.GetUsername(), otherUser);
        await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
        await AddToMessageGroupAsync(groupName);

        var messages = await messageRepository.GetThreadMessagesAsync(Context.User.GetUsername(), otherUser!);
        await Clients.Group(groupName).SendAsync("ReceiveMessageThread", messages);
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        await RemoveFromMessageGroup();
        await base.OnDisconnectedAsync(exception);
    }

    public async Task SendMessage(Models.CreateMessageDto newMessage)
    {
        var username = Context.User?.GetUsername() ?? throw new HubException("Cannot get current username");
        if (username == newMessage.RecipientUsername.ToLower())
        {
            throw new HubException("You cannot message yourself");
        }

        var sender = await userRepository.GetMemberByUsernameAsync(username);
        var recipient = await userRepository.GetMemberByUsernameAsync(newMessage.RecipientUsername);
        if (sender is null || recipient is null || sender.UserName == null || recipient.UserName == null)
        {
            throw new HubException("Cannot send message at this time");
        }

        var message = new Entities.Message
        {
            Sender = sender,
            Recipient = recipient,
            SenderUsername = sender.UserName,
            RecipientUsername = recipient.UserName,
            Content = newMessage.Content
        };

        var groupName = GetGroupName(sender.UserName, recipient.UserName);

        // If the group exists and the recipient is connected to that group, set the message as read
        var group = await messageRepository.GetMessageGroup(groupName);
        if (group != null && group.Connections.Any(c => c.Username == recipient.UserName))
        {
            message.DateRead = DateTime.UtcNow;
        }
        else
        {
            var connectionIds = await PresenceTracker.GetConnectionsForUser(recipient.UserName);
            if(connectionIds != null && connectionIds.Count > 0)
            {
                await presenceHub.Clients.Clients(connectionIds).SendAsync("NewMessageReceived",
                    new
                    {
                        username = sender.UserName,
                        knownAs = sender.KnownAs
                    });
            }
        }

        // Add the new message to the database
        messageRepository.AddMessage(message);

        if (await messageRepository.SaveAllAsync())
        {
            await Clients.Group(groupName).SendAsync("NewMessage", mapper.Map<Models.MessageDto>(message));
        }
    }

    private async Task<bool> AddToMessageGroupAsync(string groupName)
    {
        var username = Context.User?.GetUsername() ?? throw new HubException("Cannot get current username");

        // Find the existing group from the database or create a new one
        var group = await messageRepository.GetMessageGroup(groupName);
        if (group is null)
        {
            group = new Entities.Group { Name = groupName };
            messageRepository.AddGroup(group);
        }

        // Add the new connection to the group
        var connection = new Entities.Connection { ConnectionId = Context.ConnectionId, Username = username };
        group.Connections.Add(connection);

        return await messageRepository.SaveAllAsync();
    }

    private async Task RemoveFromMessageGroup()
    {
        var connection = await messageRepository.GetConnection(Context.ConnectionId);
        if (connection != null)
        {
            messageRepository.RemoveConnection(connection);
            await messageRepository.SaveAllAsync();
        }
    }

    private string GetGroupName(string? caller, string? other)
    {
        var stringCompare = string.CompareOrdinal(caller, other) < 0;
        return stringCompare ? $"{caller}-{other}" : $"{other}-{caller}";
    }
}
