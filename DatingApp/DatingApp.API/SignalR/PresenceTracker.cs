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

    public Task<bool> UserConnected(string username, string connectionId)
    {
        var userBecomesConnected = false;
        lock (OnlineUsers)
        {
            if (OnlineUsers.ContainsKey(username))
            {
                OnlineUsers[username].Add(connectionId);
            }
            else
            {
                OnlineUsers.Add(username, new List<string> { connectionId });
                userBecomesConnected = true;
            }
        }

        // Returns true only if the user was not connected from another device before
        // and therefore the user is considered online now and we should notify others about it.
        return Task.FromResult(userBecomesConnected);
    }

    public Task<bool> UserDisconnected(string username, string connectionId)
    {
        var userBecomesDisconnected = false;
        lock (OnlineUsers)
        {
            if (!OnlineUsers.ContainsKey(username)) return Task.FromResult(userBecomesDisconnected);

            OnlineUsers[username].Remove(connectionId);

            if (OnlineUsers[username].Count == 0)
            {
                OnlineUsers.Remove(username);
                userBecomesDisconnected = true;
            }
        }

        // Returns true only if the user was connected before from any device and now has no active connections
        // and therefore the user is considered offline now and we should notify others about it.
        return Task.FromResult(userBecomesDisconnected);
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
