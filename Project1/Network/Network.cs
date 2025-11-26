using System;
using System.IO;
using System.IO.Compression;
using Start_a_Town_.UI;
using Microsoft.Xna.Framework;

namespace Start_a_Town_.Net
{
    public enum NetworkSideType { Local, Server }
    public delegate void PacketHandler(INetwork net, BinaryReader r);
    public delegate void PacketHandlerWithPlayer(INetwork net, PlayerData player, BinaryReader r);
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
                PacketTimestamped = RegisterPacketHandler(ReceiveTimestamped);
            }

            private static void ReceiveTimestamped(INetwork net, BinaryReader r)
            {
                if(net is Client client)
                    client.HandleTimestamped(r);
            }

            public static void SendSyncReport(Server server, string text)
            {
                server.GetOutgoingStream().Write(PacketSyncReport, text);
            }
            private static void HandleSyncReport(INetwork net, BinaryReader r)
            {
                if (net is not Net.Client)
                    throw new Exception();
                net.Report(r.ReadString());
            }
        }

        public static INetwork CurrentNetwork;

        static public ConsoleBoxAsync Console { get { return LobbyWindow.Instance.Console; } }

        public Client Client;
        public Server Server;

        public const int RTT = 20000;// 5000;

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
            // with compression
            byte[] data;
            using (MemoryStream output = new())
            {
                using (MemoryStream mem = new())
                using (BinaryWriter bin = new(mem))
                using (GZipStream zip = new(output, CompressionMode.Compress))
                {
                    dataGetter(bin);
                    mem.Position = 0;
                    mem.CopyTo(zip);
                }
                data = output.ToArray();
            }
            return data;
        }

        static public T Deserialize<T>(byte[] compressed, Func<BinaryReader, T> dataReader)
        {
            using MemoryStream input = new(compressed);
            using GZipStream zip = new(input, CompressionMode.Decompress);
            using BinaryReader reader = new(zip);
            return dataReader(reader);
        }
        static public void Deserialize(byte[] compressed, Action<BinaryReader> dataReader)
        {
            using MemoryStream memStream = new(compressed);
            using GZipStream decompress = new(memStream, CompressionMode.Decompress);
            using MemoryStream decompressed = new();
            decompress.CopyTo(decompressed);
            decompressed.Position = 0;
            using (BinaryReader reader = new BinaryReader(decompressed))
                dataReader(reader);
        }

        
    }
}
