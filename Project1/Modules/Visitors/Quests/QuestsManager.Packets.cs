using System;
using System.Linq;
using Start_a_Town_.Net;
using System.IO;

namespace Start_a_Town_
{
    public partial class QuestsManager
    {
        static class Packets
        {
            static int pCreate, pRemove, pCreateObj, pRemoveObj, pAssign, pMod;
            static public void Init()
            {
                pCreate= Registry.PacketHandlers.Register(ReceiveQuestCreate);
                pRemove = Registry.PacketHandlers.Register(ReceiveRemoveQuest);
                pCreateObj = Registry.PacketHandlers.Register(ReceiveQuestCreateObjective);
                pRemoveObj = Registry.PacketHandlers.Register(ReceiveQuestRemoveObjective);
                pAssign = Registry.PacketHandlers.Register(ReceiveQuestGiverAssign);
                pMod = Registry.PacketHandlers.Register(ReceiveQuestModify);
            }
            public static void SendQuestModify(INetEndpoint net, PlayerData player, QuestDef quest, int maxConcurrentModValue)
            {
                if (net is Server)
                    quest.MaxConcurrent = maxConcurrentModValue;
                //net.GetOutgoingStreamOrderedReliable().Write(pMod, player.ID, quest.ID, maxConcurrentModValue);
                net.BeginPacket(ReliabilityType.OrderedReliable, pMod).Write(player.ID, quest.ID, maxConcurrentModValue);

            }
            private static void ReceiveQuestModify(NetEndpoint net, Packet pck)
            {
                var r = pck.PacketReader;
                var player = net.GetPlayer(r.ReadInt32());
                var quest = net.Map.Town.QuestManager.GetQuest(r.ReadInt32());
                var maxConcurrentModValue = r.ReadInt32();
                if (net is Client)
                    quest.MaxConcurrent = maxConcurrentModValue;
                else
                    SendQuestModify(net, player, quest, maxConcurrentModValue);
            }

            public static void SendQuestGiverAssign(INetEndpoint net, PlayerData player, QuestDef quest, Actor actor)
            {
                if(net is Server)
                    quest.Giver = actor;
                //net.GetOutgoingStreamOrderedReliable().Write(pAssign, player.ID, quest.ID, actor?.RefId ?? -1);
                net.BeginPacket(ReliabilityType.OrderedReliable, pAssign).Write(player.ID, quest.ID, actor?.RefId ?? -1);

            }
            private static void ReceiveQuestGiverAssign(NetEndpoint net, Packet pck)
            {
                var r = pck.PacketReader;
                var player = net.GetPlayer(r.ReadInt32());
                var quest = net.Map.Town.QuestManager.GetQuest(r.ReadInt32());
                var actorid = r.ReadInt32();
                var actor = actorid == -1 ? null : net.GetNetworkEntity(actorid) as Actor;
                if (net is Client)
                    quest.Giver = actor;
                else
                    SendQuestGiverAssign(net, player, quest, actor);
            }

            public static void SendQuestObjectiveRemove(INetEndpoint net, PlayerData player, QuestDef quest, QuestObjective qobj)
            {
                var index = quest.GetObjectives().ToList().FindIndex(i => i == qobj);
                if (net is Server server)
                    quest.RemoveObjective(qobj);
                //var w = net.GetOutgoingStreamOrderedReliable();
                //w.Write(pRemoveObj);
                var w = net.BeginPacket(ReliabilityType.OrderedReliable, pRemoveObj);
                w.Write(player.ID);
                w.Write(quest.ID);
                w.Write(index);
            }
            private static void ReceiveQuestRemoveObjective(NetEndpoint net, Packet pck)
            {
                var r = pck.PacketReader;
                var player = net.GetPlayer(r.ReadInt32());
                var quest = net.Map.Town.QuestManager.GetQuest(r.ReadInt32());
                var objectiveIndex = r.ReadInt32();
                var objective = quest.GetObjectives().ElementAt(objectiveIndex);
                if (net is Server)
                    SendQuestObjectiveRemove(net, player, quest, objective);
                else
                    quest.RemoveObjective(objective);
            }

            public static void SendQuestCreateObjective(INetEndpoint net, PlayerData player, QuestDef quest, QuestObjective qobj)
            {
                if (net is Server server)
                {
                    quest.AddObjective(qobj);
                }
                //var w = net.GetOutgoingStreamOrderedReliable();
                //w.Write(pCreateObj);
                var w = net.BeginPacket(ReliabilityType.OrderedReliable, pCreateObj);

                w.Write(player.ID);
                w.Write(quest.ID);
                w.Write(qobj.GetType().FullName);
                qobj.Write(w);
            }
            private static void ReceiveQuestCreateObjective(NetEndpoint net, Packet pck)
            {
                var r = pck.PacketReader;
                var player = net.GetPlayer(r.ReadInt32());
                var quest = net.Map.Town.QuestManager.GetQuest(r.ReadInt32());
                var qObj = Activator.CreateInstance(Type.GetType(r.ReadString()), quest) as QuestObjective;
                qObj.Read(r);
                if (net is Server)
                {
                    SendQuestCreateObjective(net, player, quest, qObj);
                }
                else
                {
                    quest.AddObjective(qObj);
                }
            }
            internal static void SendAddQuestGiver(INetEndpoint net, int playerID)
            {
                //var w = net.GetOutgoingStreamOrderedReliable();
                //w.Write(pCreate);
                var w = net.BeginPacket(ReliabilityType.OrderedReliable, pCreate);

                w.Write(playerID);
                if (net is Server server)
                {
                    var manager = server.Map.Town.QuestManager;
                    var q = manager.CreateQuest();
                    manager.AddQuest(q);
                    w.Write(q.ID);
                }
            }
            private static void ReceiveQuestCreate(NetEndpoint net, Packet pck)
            {
                var r = pck.PacketReader;
                var playerID = r.ReadInt32();
                if (net is Server server)
                    SendAddQuestGiver(server, playerID);
                else
                {
                    var questID = r.ReadInt32();
                    var manager = net.Map.Town.QuestManager;
                    manager.AddQuest(questID);
                }
            }
            internal static void RemoveQuest(QuestsManager manager, int playerID, QuestDef quest)
            {
                var net = manager.Town.Net;
                //var w = net.GetOutgoingStreamOrderedReliable();
                //w.Write(pRemove);
                var w = net.BeginPacket(ReliabilityType.OrderedReliable, pRemove);
                w.Write(playerID);
                w.Write(quest.ID);
                if(net is Server)
                {
                    manager.RemoveQuest(quest.ID);
                }
            }
            static void ReceiveRemoveQuest(NetEndpoint net, Packet pck)
            {
                var r = pck.PacketReader;
                var manager = net.Map.Town.QuestManager;
                var player = net.GetPlayer(r.ReadInt32());
                var questID = r.ReadInt32();
                if (net is Server)
                    RemoveQuest(manager, player.ID, manager.GetQuest(questID)); // LOL 1
                else
                    manager.RemoveQuest(questID); // LOL 2
            }
        }
    }
}
