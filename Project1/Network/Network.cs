using System;
using System.IO;
using Start_a_Town_.UI;
using Microsoft.Xna.Framework;

namespace Start_a_Town_.Net
{
    public delegate void PacketHandler(INetEndpoint net, BinaryReader r);
    public delegate void PacketHandlerWithPacket(INetEndpoint net, Packet packet);
    public delegate void PacketHandlerWithPlayer(INetEndpoint net, PlayerData player, BinaryReader r);

    public delegate void PacketHandlerServer(Server server, BinaryReader r); // in case i need to force packethandlers to only exist on server or client in the future
    public delegate void PacketHandlerClient(Client client, BinaryReader r); // in case i need to force packethandlers to only exist on server or client in the future
    public class Network
    {
        public class Packets
        {
            static public int PacketSyncReport, PacketTimestamped;
            static public void Init()
            {
                PacketSyncReport = RegisterPacketHandler(HandleSyncReport);
                PacketTimestamped = RegisterPacketHandlerWithPacket(ReceiveTimestamped);
            }
           
            private static void ReceiveTimestamped(INetEndpoint net, Packet packet)
            {
                if (net is Client client)
                    client.HandleTimestamped(packet);
            }
            public static void SendSyncReport(Server server, string text)
            {
                server.GetOutgoingStreamOrderedReliable().Write(PacketSyncReport, text);
            }
            private static void HandleSyncReport(INetEndpoint net, BinaryReader r)
            {
                if (net is not Net.Client)
                    throw new Exception();
                net.Report(r.ReadString());
            }
        }

        public static INetEndpoint CurrentNetwork;

        static public ConsoleBoxAsync Console { get { return LobbyWindow.Instance.Console; } }

        public Client Client;
        public Server Server;

        public const int RTT = 20000;// 5000;
        public const int CompressionThreshold = 140;

        static int PacketIDSequence = 10000;
        //public static int RegisterPacketHandler(Action<INetwork, BinaryReader> handler)
        //{
        //    var id = PacketIDSequence++;
        //    Server.RegisterPacketHandler(id, handler);
        //    Client.RegisterPacketHandler(id, handler);
        //    return id;
        //}
        public static int RegisterPacketHandler(PacketHandler handler)
        {
            var id = PacketIDSequence++;
            Server.RegisterPacketHandler(id, handler);
            Client.RegisterPacketHandler(id, handler);
            return id;
        }
        public static int RegisterPacketHandler(PacketHandlerWithPlayer handler)
        {
            var id = PacketIDSequence++;
            Server.RegisterPacketHandlerWithPlayer(id, handler);
            Client.RegisterPacketHandlerWithPlayer(id, handler);
            return id;
        }
        public static int RegisterPacketHandlerWithPacket(PacketHandlerWithPacket handler)
        {
            var id = PacketIDSequence++;
            Server.RegisterPacketHandler(id, handler);
            Client.RegisterPacketHandler(id, handler);
            return id;
        }
        public void CreateClient()
        {
            this.Client = Client.Instance;
        }

        public void CreateServer()
        {
            this.Server = Server.Instance;
        }
        static Network()
        {
            Packets.Init();
        }
        public Network()
        {
            this.CreateClient();
            this.CreateServer();
        }
        public void Update(GameTime gt)
        {
            CurrentNetwork = this.Server;
            this.Server.Update(gt);
            CurrentNetwork = this.Client;
            this.Client.Update();
            CurrentNetwork = null;
        }
        public static void SyncReport(Server server, string text)
        {
            Packets.SendSyncReport(server, text);
        }
        static public byte[] Serialize(Action<BinaryWriter> dataGetter)
        {
            using var m = new MemoryStream();
            using (var str = new BinaryWriter(m))
                dataGetter(str);
            return m.ToArray();
        }


    }
}
