using Microsoft.Xna.Framework;
using Project1.Core.Entities;
using Project1.Core.Loot;
using Project1.Core.Screens;
using Project1.Core.Simulation;
using Project1.Core.UI;
using Project1.Framework.Events;
using Project1.Framework.Serialization;
using System;
using System.Collections.Generic;
using System.IO;

namespace Project1.Core.Networking;

public abstract partial class NetEndpoint : INetEndpoint
{
    public abstract bool IsServer { get; }
    public abstract bool IsClient { get; }
    internal MapViewport MainViewport;

    protected readonly NetworkStream[] StreamsArray = [new(ReliabilityType.Unreliable, false), new(ReliabilityType.Reliable), new(ReliabilityType.OrderedReliable)];
    protected NetworkStream GetStream(ReliabilityType reliability)
    {
        foreach (var s in this.StreamsArray)
            if (s.Reliability == reliability)
                return s;
        throw new Exception("Stream not found");
    }

    public BinaryWriter BeginPacketOld(int pType)
    {
        var w = this.GetStream(ReliabilityType.OrderedReliable).Writer;
        w.Write(pType);
        return w;
    }
    public IDataWriter BeginPacketNew(ReliabilityType rType, int pType)
    {
        return PacketBuilder.Create(this.GetStream(rType).Writer, pType);
    }
    public abstract IDataWriter BeginPacketImmediate(PacketId pType);
    public IDataWriter BeginPacket(int pType)
    {
        return PacketBuilder.Create(this.GetStream(ReliabilityType.OrderedReliable).Writer, pType);
    }
    public void HandlePacket(int pType, Packet pck)
    {
        if (Registry.PacketHandlers.TryGet(pType, out var hhh))
            hhh(this, pck);
        // silently drop packet if next data is garbage
    }
    public ChatService ChatService { get; init; }
    public abstract ConsoleBoxAsync ConsoleBox { get; }
    public abstract PlayerData CurrentPlayer { get; }
    public abstract ulong CurrentTick { get; }
    public abstract WorldBase World { get; set; }
    public abstract void ViewMap(MapId mapid);
    public abstract int Speed { get; protected set; }
    public abstract bool DisposeObject(Entity obj);
    public abstract bool DisposeObject(int netID);
    public abstract void Enqueue(PacketType packetType, byte[] payload, ReliabilityType sendType); 
    [Obsolete]
    public void EventOccured(int eventTypeId, params object[] p)
    {
        var e = new GameEvent(this.CurrentTick, eventTypeId, p);
        this.Post(e);
    }
    [Obsolete]
    protected abstract void Post(GameEvent e);
    public abstract BinaryWriter GetOutgoingStreamOrderedReliable();
    public abstract PlayerData GetPlayer(int id);
    public abstract PlayerData GetPlayer();
    public abstract IEnumerable<PlayerData> GetPlayers();
    public virtual bool LogStateChange(Entity entity) => false;
    public abstract void PopLoot(GameObject loot, Vector3 startPosition, Vector3 startVelocity);
    public abstract void PopLoot(LootTable table, Vector3 startPosition, Vector3 startVelocity);
    public abstract void SetSpeed(int playerID, int speed);
    public abstract bool TryGetNetworkObject(int netID, out Entity obj);
    public abstract void Report(string text);
    //public abstract void WriteToStream(params object[] args);
    public EventBus Events { get; } = new();
}
