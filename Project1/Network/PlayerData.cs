using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Text;
using System.IO;
using System.Net;
using Microsoft.Xna.Framework;

namespace Start_a_Town_.Net
{
    public class PlayerData
    {
        public Vector2 MousePosition;
        long _packetSeq = 1;
        public long PacketSequenceIncrement => _packetSeq++; 
        public long OrderedReliableSequence = 1;
        public Color Color;
        public ConcurrentQueue<Packet> Incoming = new();
        public ConcurrentDictionary<long, Packet> WaitingForAck = new();
        public readonly ConcurrentQueue<long> AckQueue = new();

        public Queue<Packet> OrderedPackets = new();
        public UdpConnection Connection;
        public EndPoint IP;
        public int ID;
        public string Name;
        public int CharacterID;
        public Actor ControllingEntity;
        public int Ping;
        public bool IsActive;
        public Dictionary<Vector2, byte[]> PendingChunks = [];
        public HashSet<Vector2> SentChunks = [];
        public Vector2 CameraPosition;
        public ControlTool CurrentTool = ToolManager.Instance.GetDefaultTool();
        public float CameraZoom;
        public TargetArgs Target = TargetArgs.Null;
        public Vector2? LastPointer; // dont store this in the player class?
        public int SuggestedSpeed = 1;
        static readonly Random Random = new();
        public ConcurrentQueue<Packet> OutUnreliable = new();
        public ConcurrentQueue<Packet> OutReliable = new();

        private MemoryStream MemReliable = new(), MemUnreliable = new();
        public readonly BinaryWriter StreamReliable, StreamUnreliable;
        public PlayerData()
        {
            this.StreamReliable = new(this.MemReliable);
            this.StreamUnreliable = new(this.MemUnreliable);
        }
        public PlayerData(EndPoint ip) : this()
        {
            this.CharacterID = 0;
            this.Name = ip.ToString();
            this.IP = ip;
            this.Color = Random.GetColor();
        }

        public PlayerData(string name) : this()
        {
            this.CharacterID = 0;
            this.Name = name;
            this.Color = Random.GetColor();
        }


        static public PlayerData Read(BinaryReader reader)
        {
            int id = reader.ReadInt32();
            int namelength = reader.ReadInt32();
            string name = Encoding.ASCII.GetString(reader.ReadBytes(namelength));
            int charID = reader.ReadInt32();
            int rtt = reader.ReadInt32();
            var speed = reader.ReadInt32();
            var col = reader.ReadColor();
            return new PlayerData(name) { ID = id, CharacterID = charID, Ping = rtt, SuggestedSpeed = speed, Color = col };
        }

        public void Write(BinaryWriter w)
        {
            w.Write(this.ID);
            byte[] encoded = Encoding.ASCII.GetBytes(this.Name);
            w.Write(encoded.Length);
            w.Write(encoded);
            w.Write(CharacterID);
            w.Write(Ping);
            w.Write(this.SuggestedSpeed);
            w.Write(this.Color);
        }
        public bool IsWithin(Vector3 global, int radius = Engine.ChunkRadius)
        {
            return GameMode.Current.IsPlayerWithinRangeForPacket(this, global);
        }
       
        static public Vector2 GetMousePosition(Vector2 cameraPos, Vector2 mousePos, float zoom, Camera camera)
        {
            throw new NotImplementedException();
        }

        internal void UpdateTarget(TargetArgs target)
        {
            this.Target = target;
        }
        public override string ToString()
        {
            return this.Name;
        }
    }
}
