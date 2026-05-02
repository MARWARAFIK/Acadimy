using Acadimy.Services;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;

namespace Acadimy.Hubs
{
    public class ChatHub : Hub
    {
        private readonly OnlineUserTracker _tracker;

        public ChatHub(OnlineUserTracker tracker)
        {
            _tracker = tracker;
        }

        public override async Task OnConnectedAsync()
        {
            var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (!string.IsNullOrEmpty(userId))
            {
                _tracker.UserConnected(userId, Context.ConnectionId);
                await Clients.All.SendAsync("UserStatusChanged", userId, true);
            }

            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (!string.IsNullOrEmpty(userId))
            {
                _tracker.UserDisconnected(userId, Context.ConnectionId);
                await Clients.All.SendAsync("UserStatusChanged", userId, false);
            }

            await base.OnDisconnectedAsync(exception);
        }

        public async Task JoinThread(int threadId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, threadId.ToString());
        }

        public async Task Typing(int threadId, string userName)
        {
            await Clients.OthersInGroup(threadId.ToString())
                .SendAsync("UserTyping", userName);
        }

        public async Task StopTyping(int threadId)
        {
            await Clients.OthersInGroup(threadId.ToString())
                .SendAsync("UserStopTyping");
        }
        public async Task MessageUpdated(int threadId, int messageId, string content)
        {
            await Clients.Group(threadId.ToString())
                .SendAsync("MessageUpdated", messageId, content);
        }

        public async Task MessageDeleted(int threadId, int messageId)
        {
            await Clients.Group(threadId.ToString())
                .SendAsync("MessageDeleted", messageId);
        }
        public async Task MessageSeen(int threadId)
        {
            await Clients.OthersInGroup(threadId.ToString())
                .SendAsync("MessageSeen");
        }
    }
}