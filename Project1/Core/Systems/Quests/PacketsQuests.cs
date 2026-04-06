using Project1.Core.Entities.Actors;
using Project1.Core.Helpers;
using Project1.Core.Networking;
using Project1.Core.Screens;
using Project1.Core.Systems.Materials;
using Project1.Framework;
using System;
using System.Linq;

namespace Project1.Core.Systems.Quests;

[EnsureStaticCtorCall]
internal static class PacketsQuests
{
    static readonly PacketId
        _pCreateQuest = Registry.PacketHandlers.Register(ReceiveCreateQuest),
        _pPlayerCreateQuest = Registry.PacketHandlers.Register(ReceivePlayerCreateQuest),
        _pPlayerDeleteQuest = Registry.PacketHandlers.Register(ReceivePlayerDeleteQuest),
        _pActorAcceptedQuests = Registry.PacketHandlers.Register(ReceiveActorAcceptedQuests),
        _pQuestComplete = Registry.PacketHandlers.Register(ReceiveQuestComplete)
        ;

   

    static PacketsQuests()
    {
        Registry.PlayerInputEventHooks.Register<PlayerRequestQuestCreationEvent>(HandlePlayerCreateQuest);
        Registry.PlayerInputEventHooks.Register<PlayerRequestQuestDeletionEvent>(HandlePlayerDeleteQuest);
        Registry.MapEventHooksServer.Register<QuestAssignedEvent>(HandleActorAcceptedQuests);
        Registry.WorldEventHooksServer.Register<QuestCompleteEvent>(HandleQuestComplete);
    }

    private static void HandleQuestComplete(QuestCompleteEvent e)
    {
        SendQuestComplete(Server.Instance, e.ActorId, e.QuestId);
    }

    private static void SendQuestComplete(NetEndpoint endpoint, EntityRefId actorId, QuestId questId)
    {
        endpoint.BeginPacket(_pQuestComplete)
            .Write(actorId)
            .Write(questId);
    }
    private static void ReceiveQuestComplete(NetEndpoint endpoint, Packet packet)
    {
        var r = packet.PacketReader;
        var actorid = r.ReadEntityRefId();
        //var actor = endpoint.World.Get<Actor>();
        var qid = (QuestId)r.ReadInt32();
        endpoint.Map.Town.QuestManagerNew.UnassignQuest(actorid, qid);
    }
    private static void HandleActorAcceptedQuests(QuestAssignedEvent e)
    {
        var server = Server.Instance;
        server.BeginPacket(_pActorAcceptedQuests)
            .Write(e.Board)
            .Write(e.ActorId)
            .Write(e.Quests.Select(q=>(int)q).ToArray());
    }
    private static void ReceiveActorAcceptedQuests(NetEndpoint endpoint, Packet packet)
    {
        var r = packet.PacketReader;
        var board = r.ReadIntVec3();
        var actorid = r.ReadEntityRefId();
        var questIds = r.ReadListInt32().Select(id => (QuestId)id);
        endpoint.Map.Town.QuestManagerNew.TryAcceptAllQuestsInt(board, endpoint.World.Get<Actor>(actorid), questIds);
    }
    private static void HandlePlayerDeleteQuest(PlayerRequestQuestDeletionEvent e)
    {
        if (Ingame.Net.IsServer)
        {
            Ingame.CurrentMap.Town.QuestManagerNew.DeleteQuest(e.Id);
        }
        SendPlayerDeleteQuest(Ingame.Net, e.Id);
    }

    private static void SendPlayerDeleteQuest(NetEndpoint net, QuestId id)
    {
        net.BeginPacketImmediate(_pPlayerDeleteQuest)
            .Write(id);
    }
    private static void ReceivePlayerDeleteQuest(NetEndpoint endpoint, Packet packet)
    {
        var r = packet.PacketReader;
        var qid = (QuestId)r.ReadInt32();
        endpoint.Map.Town.QuestManagerNew.DeleteQuest(qid);

        if (endpoint.IsServer)
            SendDeleteQuest(endpoint, qid);
    }

    private static void SendDeleteQuest(NetEndpoint endpoint, QuestId qid)
    {
        endpoint.BeginPacketImmediate(_pPlayerDeleteQuest)
            .Write(qid);
    }
  
    private static void HandlePlayerCreateQuest(PlayerRequestQuestCreationEvent e)
    {
        if (Ingame.Net.IsServer)
        {
            if (!Ingame.CurrentMap.Town.QuestManagerNew.TryCreateQuest(e.RefinementDef, e.MaterialDef))
                return;
        }
        SendPlayerCreateQuest(Ingame.Net, e);
    }

    private static void SendPlayerCreateQuest(NetEndpoint client, PlayerRequestQuestCreationEvent e)
    {
        client.BeginPacketImmediate(_pPlayerCreateQuest)
            .Write(e.RefinementDef)
            .Write(e.MaterialDef);
    }

    private static void ReceivePlayerCreateQuest(NetEndpoint endpoint, Packet packet)
    {
        var r = packet.PacketReader;
        var player = packet.Player;
        var refdef = r.ReadDef<MaterialRefinementDef>();
        var matdef = r.ReadDef<MaterialDef>();
        if(endpoint.Map.Town.QuestManagerNew.TryCreateQuest(refdef, matdef))
            SendCreateQuest(endpoint as Server, refdef, matdef);
    }

    private static void SendCreateQuest(Server server, MaterialRefinementDef refdef, MaterialDef matdef)
    {
        server.BeginPacketImmediate(_pCreateQuest)
            .Write(refdef)
            .Write(matdef);
    }
    private static void ReceiveCreateQuest(NetEndpoint endpoint, Packet packet)
    {
        var client = endpoint as Client;
        var r = packet.PacketReader;
        var refdef = r.ReadDef<MaterialRefinementDef>();
        var matdef = r.ReadDef<MaterialDef>();
        if (!client.Map.Town.QuestManagerNew.TryCreateQuest(refdef, matdef))
            throw new InvalidOperationException();
    }
}
