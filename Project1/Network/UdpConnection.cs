using System;
using System.Net;
using System.Net.Sockets;
using System.Diagnostics;

namespace Start_a_Town_.Net
{
    public class UdpConnection 
    {
        public EndPoint IP;
        public Packet Partial;
        public PlayerData Player;
        public Socket Socket;
        public string Name;
        public byte[] Buffer;
        public TimeSpan RTT;
        public Stopwatch Ping;
       
        public UdpConnection(string name, Socket socket)
        {

            this.Name = name;
            this.Socket = socket;
        }
        public UdpConnection(string name, EndPoint ip)
        {
            this.Name = name;
            this.IP = ip;
        }
    }
}
