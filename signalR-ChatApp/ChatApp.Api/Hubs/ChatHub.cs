using System.Collections.Concurrent;
using Microsoft.AspNetCore.SignalR;

namespace ChatApp.Api.Hubs;

public class ChatHub : Hub
{
    private static readonly ConcurrentDictionary<string, string> Users = new();

    public async Task Register(string user)
    {
        Users.AddOrUpdate(user, Context.ConnectionId, (_, _) => Context.ConnectionId);
        Context.Items["user"] = user;
        await Clients.Caller.SendAsync("UserList", Users.Keys.ToArray());
        await Clients.Others.SendAsync("UserList", Users.Keys.ToArray());
    }

    public async Task SendMessage(string user, string message)
    {
        // Exclude caller — sender did optimistic append
        await Clients.Others.SendAsync("ReceiveMessage", user, message, DateTime.UtcNow);
    }

    public async Task SendPrivate(string fromUser, string toUser, string message)
    {
        if (!Users.TryGetValue(toUser, out var targetConnId)) return;
        await Clients.Client(targetConnId)
            .SendAsync("ReceivePrivate", fromUser, message, DateTime.UtcNow);
    }

    public Task Typing(string user, string? toUser)
    {
        if (string.IsNullOrEmpty(toUser))
            return Clients.Others.SendAsync("UserTyping", user, (string?)null);

        return Users.TryGetValue(toUser, out var connId)
            ? Clients.Client(connId).SendAsync("UserTyping", user, toUser)
            : Task.CompletedTask;
    }

    public Task StoppedTyping(string user, string? toUser)
    {
        if (string.IsNullOrEmpty(toUser))
            return Clients.Others.SendAsync("UserStoppedTyping", user, (string?)null);

        return Users.TryGetValue(toUser, out var connId)
            ? Clients.Client(connId).SendAsync("UserStoppedTyping", user, toUser)
            : Task.CompletedTask;
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        if (Context.Items.TryGetValue("user", out var u) && u is string user)
        {
            if (Users.TryGetValue(user, out var owningConn) && owningConn == Context.ConnectionId)
            {
                Users.TryRemove(user, out _);
            }
            await Clients.All.SendAsync("UserList", Users.Keys.ToArray());
        }
        await base.OnDisconnectedAsync(exception);
    }
}