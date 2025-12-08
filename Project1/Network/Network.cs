using System;
using System.IO;
using Start_a_Town_.UI;
using Microsoft.Xna.Framework;

namespace Start_a_Town_.Net
{
    public class Network
    {
        public class Packets
        {
            static public int PacketSyncReport, PacketTimestamped;
            static public void Init()
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
                server.GetOutgoingStreamOrderedReliable().Write(PacketSyncReport, text);
            }
            private static void HandleSyncReport(NetEndpoint net, Packet packet)
            {
                var r = packet.PacketReader;
                if (net is not Net.Client)
                    throw new Exception();
                net.Report(r.ReadString());
            }
        }

        public static NetEndpoint CurrentNetwork;

        static public ConsoleBoxAsync Console { get { return LobbyWindow.Instance.Console; } }

        public Client Client;
        public Server Server;

        public const int RTT = 20000;// 5000;
        public const int CompressionThreshold = 140;

        static int PacketIDSequence = 10000;
        
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
        //static public byte[] Serialize(Action<BinaryWriter> dataGetter)
        //{
        //    using var m = new MemoryStream();
        //    using (var str = new BinaryWriter(m))
        //        dataGetter(str);
        //    return m.ToArray();
        //}
        //static public byte[] Serialize(Action<BinaryWriter> dataGetter)
        //{
        //    using var m = new MemoryStream();
        //    using (var str = new BinaryWriter(m))
        //        dataGetter(str);
        //    return m.ToArray();
        //}
        static public byte[] Serialize(Action<IDataWriter> dataGetter)
        {
            //using var m = new MemoryStream();
            //using (var str = new BinaryWriter(m))
            var str = new DataWriter();
                dataGetter(str);
            return str.BaseStream.ToArray();
        }
    }
}
