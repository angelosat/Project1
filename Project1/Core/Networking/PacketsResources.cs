using Project1.Core.Helpers;
using Project1.Core.Resources;
using Project1.Framework;

namespace Project1.Core.Networking
{
    [EnsureStaticCtorCall]
    internal static class PacketsResources
    {
        readonly static PacketId _pResourceAdjusted;
        static PacketsResources()
        {
            Registry.WorldEventHooksServer.Register<ResourceModifiedEvent>(SendResourceAdjusted);
            _pResourceAdjusted = Registry.PacketHandlers.Register(OnResourceAdjusted);
        }
        private static void SendResourceAdjusted(ResourceModifiedEvent @event)
        {
            Server.Instance.BeginPacket(_pResourceAdjusted)
                .Write(@event.Entity.RefId)
                .Write(@event.Def)
                .Write(@event.Delta);
        }
        static void OnResourceAdjusted(NetEndpoint endpoint, Packet packet)
        {
            endpoint.World
                .GetEntity(packet.PacketReader.ReadEntityRefId())
                .Resources.ApplyDelta(packet.PacketReader.ReadDef<ResourceDef>(), packet.PacketReader.ReadSingle());
        }
    }
}
