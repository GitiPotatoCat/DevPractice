using Microsoft.AspNetCore.SignalR;
using System.Collections.Concurrent;

namespace SignalRChat.Hubs
{
    public class ChatHub : Hub
    {
        private static readonly ConcurrentDictionary<string, string> ConnectedUsers = new();

        public override async Task OnConnectedAsync()
        {
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            if (ConnectedUsers.TryRemove(Context.ConnectionId, out string? username))
            {
                await Clients.All.SendAsync("UserDisconnected", username);
                await Clients.All.SendAsync("UpdateOnlineUsers", ConnectedUsers.Values);
            }
            await base.OnDisconnectedAsync(exception);
        }

        public async Task JoinChat(string username)
        {
            ConnectedUsers.TryAdd(Context.ConnectionId, username);
            await Clients.All.SendAsync("UserConnected", username);
            await Clients.All.SendAsync("UpdateOnlineUsers", ConnectedUsers.Values);
        }

        public async Task SendMessage(string user, string message)
        {
            await Clients.All.SendAsync("ReceiveMessage", new
            {
                User = user,
                Message = message,
                Time = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ")
            });
        }

        public async Task SendTypingIndicator(string username, bool isTyping)
        {
            await Clients.Others.SendAsync("UserTyping", username, isTyping);
        }
    }
}