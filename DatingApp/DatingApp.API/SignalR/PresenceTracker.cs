using System;

namespace DatingApp.API.SignalR;

/// <summary>
/// NOTE: This is a simple in-memory presence tracker to keep track of online users.
/// This class is not a scalable solution for tracking online users in a real-world application.
/// It is intended for demonstration purposes only.
/// </summary>
public class PresenceTracker
{
    /// <summary>
    /// The key is the username, and the value is a list of connection IDs for that user.
    /// Each connection ID represents a separate connection from different devices or browser tabs.
    /// </summary>
    private static readonly Dictionary<string, List<string>> OnlineUsers = new();

    public Task UserConnected(string username, string connectionId)
    {
        lock (OnlineUsers)
        {
            if (OnlineUsers.ContainsKey(username))
            {
                OnlineUsers[username].Add(connectionId);
            }
            else
            {
                OnlineUsers.Add(username, new List<string> { connectionId });
            }
        }
        return Task.CompletedTask;
    }

    public Task UserDisconnected(string username, string connectionId)
    {
        lock (OnlineUsers)
        {
            if (!OnlineUsers.ContainsKey(username)) return Task.CompletedTask;

            OnlineUsers[username].Remove(connectionId);

            if (OnlineUsers[username].Count == 0)
            {
                OnlineUsers.Remove(username);
            }
        }
        return Task.CompletedTask;
    }

    public Task<string[]> GetOnlineUsers()
    {
        string[] onlineUsers;
        lock (OnlineUsers)
        {
            onlineUsers = OnlineUsers.OrderBy(k => k.Key).Select(k => k.Key).ToArray();
        }
        return Task.FromResult(onlineUsers);
    }

    public static Task<List<string>> GetConnectionsForUser(string username)
    {
        List<string> connectionIds;
        lock (OnlineUsers)
        {
            connectionIds = OnlineUsers.TryGetValue(username, out var ids) ? ids : [];
        }
        return Task.FromResult(connectionIds);
    }
}
