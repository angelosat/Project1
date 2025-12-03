using Microsoft.Xna.Framework;
using Start_a_Town_.Net;
using Start_a_Town_.UI;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;

namespace Start_a_Town_
{
    public interface INetEndpoint
    {
        ConsoleBoxAsync ConsoleBox { get; }
        PlayerData CurrentPlayer { get; }
        TimeSpan Clock { get; }
        double CurrentTick { get; }
        MapBase Map { get; }
        WorldBase World { get; }
        int Speed { get; set; }

        GameObject GetNetworkEntity(int netID);
        T GetNetworkObject<T>(int netID) where T : Entity;// GameObject;

        //IEnumerable<GameObject> GetNetworkObjects(IEnumerable<int> ids);
        IEnumerable<GameObject> GetNetworkObjects();

        bool TryGetNetworkObject(int netID, out Entity obj);
        void Enqueue(PacketType packetType, byte[] payload, ReliabilityType sendType);

        IEnumerable<PlayerData> GetPlayers();
        PlayerData GetPlayer(int id);
        PlayerData GetPlayer();

        GameObject Instantiate(GameObject obj);

        bool DisposeObject(GameObject obj);
        bool DisposeObject(int netID);

        void Instantiator(GameObject o);

        void SyncReport(string text);

        bool LogStateChange(int netID);

        void PopLoot(GameObject loot, Vector3 startPosition, Vector3 startVelocity);
        void PopLoot(LootTable table, Vector3 startPosition, Vector3 startVelocity);

        void PostLocalEvent(GameObject recipient, ObjectEventArgs args);
        void PostLocalEvent(GameObject recipient, Components.Message.Types type, params object[] args);

        void EventOccured(Components.Message.Types type, params object[] p);

        BinaryWriter GetOutgoingStreamOrderedReliable();
        //BinaryWriter this[ReliabilityType reliabilityChannel] { get; }
        //NetworkStream GetStream(ReliabilityType reliabilityChannel);
        void WriteToStream(params object[] args);

        void SetSpeed(int playerID, int speed);
        void Write(string text);
        void Report(string text);
    }
}
