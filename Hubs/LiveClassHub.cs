using Microsoft.AspNetCore.SignalR;

namespace Acadimy.Hubs
{
    public class LiveClassHub : Hub
    {
        public async Task JoinClass(string classId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, classId);
        }

        public async Task SendSignal(string classId, object signal)
        {
            await Clients.OthersInGroup(classId)
                .SendAsync("ReceiveSignal", signal);
        }
    }
}