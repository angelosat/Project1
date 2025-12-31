using Start_a_Town_.Net;

namespace Start_a_Town_
{
    [EnsureStaticCtorCall]
    internal static class PacketsNeeds
    {
        static int _pTypeIdModifyNeed;

        static PacketsNeeds()
        {
            _pTypeIdModifyNeed = Registry.PacketHandlers.Register(OnNeedOverride);

            Registry.MapEventHooksServer.Register<ActorNeedOverridenEvent>(SendNeedOverride);
        }

        private static void SendNeedOverride(ActorNeedOverridenEvent e)
        {
            Server.Instance.BeginPacket(_pTypeIdModifyNeed)
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
