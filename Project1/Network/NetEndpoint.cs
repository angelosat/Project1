using Microsoft.Xna.Framework;
using Start_a_Town_.Components;
using Start_a_Town_.UI;
using System;
using System.Collections.Generic;
using System.IO;

namespace Start_a_Town_.Net
{
    public abstract partial class NetEndpoint : INetEndpoint
    {
        public abstract bool IsServer { get; }
        public abstract bool IsClient { get; }

        protected readonly NetworkStream[] StreamsArray = [new(ReliabilityType.Unreliable), new(ReliabilityType.Reliable), new(ReliabilityType.OrderedReliable)];
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
        public abstract ConsoleBoxAsync ConsoleBox { get; }
        public abstract PlayerData CurrentPlayer { get; }
        public abstract TimeSpan Clock { get; }
        public abstract double CurrentTick { get; }
        public abstract MapBase Map { get; set; }
        public abstract WorldBase World { get; set; }
        public abstract int Speed { get; set; }
        public abstract bool DisposeObject(GameObject obj);
        public abstract bool DisposeObject(int netID);
        public abstract void Enqueue(PacketType packetType, byte[] payload, ReliabilityType sendType);
        public void EventOccured(int eventTypeId, params object[] p)
        {
            var e = new GameEvent(this.Clock.TotalMilliseconds, eventTypeId, p);
            this.OnGameEvent(e);
        }
        public void EventOccured<T>(T args)
        {
            var e = new GameEvent(this.Clock.TotalMilliseconds, args);
            this.OnGameEvent(e);
        }
        protected abstract void OnGameEvent(GameEvent e);
        public abstract BinaryWriter GetOutgoingStreamOrderedReliable();
        public abstract PlayerData GetPlayer(int id);
        public abstract PlayerData GetPlayer();
        public abstract IEnumerable<PlayerData> GetPlayers();
        public abstract GameObject Instantiate(GameObject obj);
        public abstract void Instantiator(GameObject o);
        public abstract bool LogStateChange(int netID);
        public abstract void PopLoot(GameObject loot, Vector3 startPosition, Vector3 startVelocity);
        public abstract void PopLoot(LootTable table, Vector3 startPosition, Vector3 startVelocity);
        public abstract void PostLocalEvent(GameObject recipient, ObjectEventArgs args);
        public abstract void PostLocalEvent(GameObject recipient, Message.Types type, params object[] args);
        public abstract void Report(string text);
        public abstract void SetSpeed(int playerID, int speed);
        public abstract void SyncReport(string text);
        public abstract bool TryGetNetworkObject(int netID, out Entity obj);
        public abstract void Write(string text);
        public abstract void WriteToStream(params object[] args);
    }
}
