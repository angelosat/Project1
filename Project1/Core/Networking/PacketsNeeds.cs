using Project1.Framework.Base;
using Project1.Framework.Entities.Actors;
using Project1.Framework.Needs;
using Project1.Framework.Net;
using Start_a_Town_;

namespace Project1.Core.Networking
{
    [EnsureStaticCtorCall]
    internal static class PacketsNeeds
    {
        static PacketId _pTypeIdModifyNeed, _pNeedUpdated;

        static PacketsNeeds()
        {
            _pTypeIdModifyNeed = Registry.PacketHandlers.Register(OnNeedOverride);
            _pNeedUpdated = Registry.PacketHandlers.Register(OnNeedUpdated);

            Registry.WorldEventHooksServer.Register<ActorNeedOverridenEvent>(SendNeedOverride);
            Registry.WorldEventHooksServer.Register<ActorNeedUpdatedEvent>(SendNeedUpdated);
        }

        private static void OnNeedUpdated(NetEndpoint endpoint, Packet packet)
        {
            var client = endpoint as Client;
            var r = packet.PacketReader;
            var actor = client.World.GetEntity<Actor>(r.ReadInt32());
            var needdef = r.ReadDef<NeedDef>();
            var need = actor.GetNeed(needdef);
            var val = r.ReadInt32();
            need.SetValue(val);
        }

        private static void SendNeedUpdated(ActorNeedUpdatedEvent e)
        {
            var server = e.Need.Owner.Net as Server;
            server.BeginPacket(_pNeedUpdated)
                .Write(e.Need.Owner.RefId)
                .Write(e.Need.Def)
                .Write(e.Need.Value);
        }

        private static void SendNeedOverride(ActorNeedOverridenEvent e)
        {
            Server.Instance.BeginPacketImmediate(_pTypeIdModifyNeed)
                .Write(e.Actor.RefId)
                .Write(e.Need)
                .Write(e.Percentage);
        }
        private static void OnNeedOverride(NetEndpoint endpoint, Packet packet)
        {
            var client = endpoint as Client;
            var r = packet.PacketReader;
            var actor = client.World.GetEntity<Actor>(r.ReadInt32());
            var need = r.ReadDef<NeedDef>();
            var percentage = r.ReadSingle();

            actor.Needs.OverridePercentage(need, percentage);
        }
    }

}
