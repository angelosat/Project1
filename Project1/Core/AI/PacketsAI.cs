using Project1.Core.World.WorldAreas;
using Project1.Core.Entities.Actors;
using Project1.Core.Helpers.Structs;
using Project1.Core.Helpers;
using Project1.Core.Networking;
using Project1.Framework;
using Project1.Framework.Events;
using Project1.Core.Networking;

namespace Project1.Core.AI
{
    [EnsureStaticCtorCall]
    internal static class PacketsAI
    {
        static readonly PacketId _pLocationDecision, _pLogEntry;
        static PacketsAI()
        {
            _pLocationDecision = Registry.PacketHandlers.Register(OnLocationDecision);
            _pLogEntry = Registry.PacketHandlers.Register(OnLogEntry);
            Registry.WorldEventHooksServer.Register<AILocationDecisionEvent>(SendLocationDecision);
            Registry.WorldEventHooksServer.Register<AILogEntryEvent>(SendLogEntry);
        }
        private static void SendLogEntry(AILogEntryEvent e)
        {
            Server.Instance.BeginPacket(_pLogEntry)
                .Write(e.Actor.RefId)
                .Write(e.Text);
        }
        private static void OnLogEntry(NetEndpoint endpoint, Packet packet)
        {
            var r = packet.PacketReader;
            var actor = endpoint.World.GetEntity<Actor>(r.ReadInt32());
            var text = r.ReadString();
            actor.AI.State.Log.Write(text);
        }
        private static void SendLocationDecision(AILocationDecisionEvent e)
        {
            Server.Instance.BeginPacket(_pLocationDecision)
                .Write(e.Actor.RefId)
                .Write(e.Frontier);
        }

        private static void OnLocationDecision(NetEndpoint endpoint, Packet packet)
        {
            var client = endpoint as Client;
            var r = packet.PacketReader;
            var actor = client.World.GetEntity<Actor>(r.ReadInt32());
            var frontier = r.ReadDef<FrontierDef>();
            actor.AI.Meta.SetTargetFrontier(frontier);
        }
    }
}
