using Microsoft.Xna.Framework;
using Project1.Core.Entities;
using Project1.Core.Loot;
using Project1.Core.Simulation;
using Project1.Core.UI;
using Project1.Framework.Serialization;
using System.Collections.Generic;

namespace Project1.Core.Networking;

public interface INetEndpoint
{
    ConsoleBoxAsync ConsoleBox { get; }
    PlayerData CurrentPlayer { get; }
    double CurrentTick { get; }
    //MapBase Map { get; }
    WorldBase World { get; }
    int Speed { get; }
    bool TryGetNetworkObject(int netID, out Entity obj);
    void Enqueue(PacketType packetType, byte[] payload, ReliabilityType sendType);
    IEnumerable<PlayerData> GetPlayers();
    PlayerData GetPlayer(int id);
    PlayerData GetPlayer();
    bool DisposeObject(Entity obj);
    bool DisposeObject(int netID);
    void PopLoot(GameObject loot, Vector3 startPosition, Vector3 startVelocity);
    void PopLoot(LootTable table, Vector3 startPosition, Vector3 startVelocity);
    void EventOccured(int eventTypeId, params object[] p);
    IDataWriter BeginPacketNew(ReliabilityType rType, int pType);
    void SetSpeed(int playerID, int speed);
    void Report(string text);
}
