namespace Acadimy.Services
{
    public class OnlineUserTracker
    {
        private readonly Dictionary<string, HashSet<string>> _onlineUsers = new();

        public void UserConnected(string userId, string connectionId)
        {
            lock (_onlineUsers)
            {
                if (!_onlineUsers.ContainsKey(userId))
                    _onlineUsers[userId] = new HashSet<string>();

                _onlineUsers[userId].Add(connectionId);
            }
        }

        public void UserDisconnected(string userId, string connectionId)
        {
            lock (_onlineUsers)
            {
                if (!_onlineUsers.ContainsKey(userId)) return;

                _onlineUsers[userId].Remove(connectionId);

                if (_onlineUsers[userId].Count == 0)
                    _onlineUsers.Remove(userId);
            }
        }

        public bool IsOnline(string userId)
        {
            lock (_onlineUsers)
            {
                return _onlineUsers.ContainsKey(userId);
            }
        }
    }
}