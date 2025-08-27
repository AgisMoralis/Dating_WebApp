using DatingApp.API.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace DatingApp.API.SignalR;

[Authorize]
public class PresenceHub(PresenceTracker tracker) : Hub
{
    public override async Task OnConnectedAsync()
    {
        if (Context.User is null) throw new HubException("Cannot get current user claim");

        var userTurnedOnlineNotification = await tracker.UserConnected(Context.User.GetUsername(), Context.ConnectionId);
        if (userTurnedOnlineNotification) await Clients.Others.SendAsync("UserIsOnline", Context.User?.GetUsername());

        var currentUsers = await tracker.GetOnlineUsers();
        await Clients.Caller.SendAsync("GetOnlineUsers", currentUsers);
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        if (Context.User is null) throw new HubException("Cannot get current user claim");

        var userTurnedOfflineNotification =await tracker.UserDisconnected(Context.User.GetUsername(), Context.ConnectionId);
        if (userTurnedOfflineNotification) await Clients.Others.SendAsync("UserIsOffline", Context.User?.GetUsername());

        await base.OnDisconnectedAsync(exception);
    }
}
