using System;
using System.Net.Sockets;
using System.Net;
using System.IO;
using System.Diagnostics;

namespace Start_a_Town_.Net
{
    public enum PacketType
    {
        PlayerConnecting = 1, 
        PlayerDisconnected, 
        PlayerEnterWorld, 
        AssignCharacter, 
        RequestConnection,
        MapData,
        WorldData, 
        Chunk,
        ObjectEvent,
        ServerBroadcast,
        PlayerSlotClick,
        SpawnChildObject,
        PlayerServerCommand,
        PlayerRemoteCall,
        EntityInventoryChange,
        MergedPackets,
        SetSaving,
        //Acks
    }

    [Flags]
    public enum SendType { Unreliable = 0, Ordered = 0x1, Reliable = 0x2, OrderedReliable = 0x3 }

    public class Packet
    {
        public const int MaxAttempts = 5;
        public const int Size = 65535 * 2;
        public long ID;
        public long OrderedReliableID;
        public EndPoint Sender;
        public EndPoint Recipient;
        public SendType SendType;
        public double Tick;
        public bool Synced;
        /// <summary>
        /// The connection from which the packet has been received, is null if the packet has just been created
        /// </summary>
        public UdpConnection Connection;
        public Stopwatch RTT;
        public PacketType PacketType;
        public int Length;
        public byte[] Payload;
        public byte[] Decompressed;
        public Socket Socket;
        public int Retries;
        public PlayerData Player;
        public System.Threading.Timer ResendTimer;
        protected Packet() { }
        public Packet(long id, PacketType type, int length, byte[] payload)
        {
            this.RTT = new Stopwatch();
            this.ID = id;
            this.PacketType = type;
            this.Length = length;
            this.Payload = payload;
            this.Retries = MaxAttempts;
        }
        
        static public Packet Read(byte[] data)
        {
            using BinaryReader reader = new(new MemoryStream(data));
            long orderReliableseq = 0;
            long id = reader.ReadInt64();
            SendType sendType = (SendType)reader.ReadInt32(); //read and write sendtype as 2 bits
            if (sendType == SendType.OrderedReliable)
                orderReliableseq = reader.ReadInt64();
            PacketType type = (PacketType)reader.ReadByte();
            int length = reader.ReadInt32();

            byte[] payload = reader.ReadBytes(length);
            byte[] decompressed = payload.Decompress(); // TODO: FIX: i already have a decompressed payload and i still deserialize everything when handling packets???
            bool synced = reader.ReadBoolean();
            double tick = reader.ReadDouble();
            return new Packet(id, type, length, payload) { SendType = sendType, Decompressed = decompressed, OrderedReliableID = orderReliableseq, Tick = tick, Synced = synced };
        }
        static public Packet Create(PlayerData reciepient, PacketType type, byte[] data, SendType sendType = SendType.Unreliable)
        {
            return new Packet(reciepient.PacketSequenceIncrement, type, data.Length, data) { 
                Player = reciepient, 
                SendType = sendType,
                OrderedReliableID = sendType == SendType.OrderedReliable ? reciepient.OrderedReliableSequence++ : 0
            };
        }
        static public Packet Create(long id, PacketType type, byte[] data)
        {
            return new Packet(id, type, data.Length, data);
        }
        static public Packet Create(long id, PacketType type, SendType sendType, byte[] data)
        {
            return new Packet(id, type, data.Length, data) { SendType = sendType };
        }
        static public Packet Create(long id, PacketType type)
        {
            return new Packet(id, type, 0, new byte[] { });
        }
        
        public byte[] ToArray()
        {
            var mem = new MemoryStream();
            using BinaryWriter writer = GetWriter(mem);
            writer.Write(this.ID);
            writer.Write((int)this.SendType);
            if (this.SendType == SendType.OrderedReliable)
            {
                writer.Write(this.OrderedReliableID);
            }
            writer.Write((byte)this.PacketType);
            writer.Write(this.Payload.Length);
            writer.Write(this.Payload);
            writer.Write(this.Synced);
            writer.Write(this.Tick);
            return mem.ToArray();
        }

        private static BinaryWriter GetWriter(MemoryStream mem)
        {
            return new BinaryWriter(mem);
        }

        public override string ToString()
        {
            return "ID: " + this.ID + " / Type: " + this.PacketType + " / Size: " + this.Length + " / Attempts: " + Retries;
        }

        public void BeginSendTo(Socket socket, EndPoint ip)
        {
            this.BeginSendTo(socket, ip, ar => { });
        }
        public void BeginSendTo(Socket socket, EndPoint ip, AsyncCallback callback)
        {
            this.RTT.Restart();
            byte[] array = this.ToArray();

            try
            {
                socket.BeginSendTo(array, 0, array.Length, SocketFlags.None, ip, a =>
                {
                    socket.EndSend(a);
                    callback(a);
                }, socket);
            }
            catch (ObjectDisposedException) { }
        }

        static public void Send(long id, PacketType type, byte[] payload, Socket socket, EndPoint remoteIP)
        {
            Create(id, type, payload).BeginSendTo(socket, remoteIP);
        }
        public virtual void Send(Socket socket, EndPoint remoteIP)
        {
            Network.Serialize(this.Write).Send(this.ID, this.PacketType, socket, remoteIP);
        }

        public virtual void Write(BinaryWriter w) { }

        public virtual void Read(INetwork net, byte[] data) { }
        public virtual void Read(INetwork net, BinaryReader r) { }
    }
}
