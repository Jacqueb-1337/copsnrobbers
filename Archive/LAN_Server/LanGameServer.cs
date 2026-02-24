using System;
using System.Net;

namespace CopsNRobbers.LanServer
{
    /// <summary>
    /// DEPRECATED: Use LanGameServerUdp instead
    /// Kept for backwards compatibility
    /// </summary>
    public class LanGameServer
    {
        public bool IsRunning { get; set; }
        public int Port { get; set; }

        public LanGameServer(int port = GameConstants.GameServerPort)
        {
            Port = port;
        }

        public bool Start() => false;
        public void Stop() { }
        public void Update() { }
    }
}
