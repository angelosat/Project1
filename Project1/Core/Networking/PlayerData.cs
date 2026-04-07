using Microsoft.Xna.Framework;
using Project1.Core.Entities.Actors;
using Project1.Core.Helpers;
using Project1.Core.Input;
using Project1.Core.Simulation;
using Project1.Framework.Helpers;
using Project1.Framework.Serialization;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;

namespace Project1.Core.Networking;

public class PlayerData
{
    public Vector2 MousePosition;
    long _packetSeq = 1;
    public long PacketSequenceIncrement => _packetSeq++;
    public long OrderedReliableSequence = 0;//1;
    public long RemoteOrderedReliableSequence = -1;//0;
    public bool SendSnapshots;

    public Color Color;
    public ConcurrentQueue<Packet> IncomingAll = new();
    public MyPriorityQueue<long, Packet> IncomingOrderedReliable = new();
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

    public HashSet<(MapBase map, Chunk chunk)> SentChunks = [];
    public Dictionary<(MapBase map, Chunk chunk), byte[]> PendingChunks = [];

    public Vector2 CameraPosition;
    public ControlTool CurrentTool = ToolManager.Instance.GetDefaultTool();
    public float CameraZoom;
    public InteractionTarget Target = InteractionTarget.Null;
    public Vector2? LastPointer; // dont store this in the player class?
    public int SuggestedSpeed = 1;
    static readonly Random Random = new();
    public ConcurrentQueue<Packet> OutUnreliable = new();
    public ConcurrentQueue<Packet> OutReliable = new();

    private readonly MemoryStream MemReliable = new(), MemUnreliable = new();
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


    static public PlayerData Read(IDataReader reader)
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
    public PacketBuilder BeginReliable(int pType)
    {
        return PacketBuilder.Create(this.StreamReliable, pType);
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
    public void Write(IDataWriter w)
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

    internal void UpdateTarget(InteractionTarget target)
    {
        this.Target = target;
    }
    public override string ToString()
    {
        return this.Name;
    }
}
