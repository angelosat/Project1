using System;
using Microsoft.Xna.Framework;
using Project1.Framework;
using Project1.Framework.Serialization;
using Project1.Core.UI;
using Project1.Core.Screens;
using Project1.Framework.Events;

namespace Project1.Core.Net
{
    public class Network
    {
        [EnsureStaticCtorCall]
        public class Packets
        {
            static public int PacketSyncReport, PacketTimestamped;
            static Packets()
            {
                PacketSyncReport = Registry.PacketHandlers.Register(HandleSyncReport);
                PacketTimestamped = Registry.PacketHandlers.Register(ReceiveTimestamped);
            }
           
            private static void ReceiveTimestamped(NetEndpoint net, Packet packet)
            {
                if (net is Client client)
                    client.HandleTimestamped(packet);
            }
            public static void SendSyncReport(Server server, string text)
            {
                server.BeginPacket(PacketSyncReport).Write(text);
                server.Report(text);
            }
            private static void HandleSyncReport(NetEndpoint net, Packet packet)
            {
                var r = packet.PacketReader;
                if (net is not Client)
                    throw new Exception();
                net.Report(r.ReadString());
            }
        }

        public static NetEndpoint CurrentNetwork;

        static public ConsoleBoxAsync Console { get { return LobbyWindow.Instance.Console; } }

        public Client _client;
        public Server _server;

        public const int RTT = 20000;// 5000;
        public const int CompressionThreshold = 140;

        static int PacketIDSequence = 10000;
        
        public void CreateClient()
        {
            this._client = Client.Instance;
        }

        public void CreateServer()
        {
            this._server = Server.Instance;
        }
        public Network()
        {
            this.CreateClient();
            this.CreateServer();
        }
        public void Update(GameTime gt)
        {
            CurrentNetwork = this._server;
            this._server.Tick(gt);
            CurrentNetwork = this._client;
            this._client.Tick();
            CurrentNetwork = null;
        }
        public static void SyncReport(Server server, string text)
        {
            Packets.SendSyncReport(server, text);
        }
        
        static public byte[] Serialize(Action<IDataWriter> dataGetter)
        {
            var str = new DataWriter();
                dataGetter(str);
            return str.BaseStream.ToArray();
        }
    }
}
