using Project1.Framework;
using Project1.Core.Resources;
using Project1.Core.Helpers;
using Project1.Core.Helpers.Structs;
using Project1.Core.Networking;
using Project1.Framework.Events;

namespace Project1.Core.Networking
{
    [EnsureStaticCtorCall]
    internal static class PacketsResources
    {
        readonly static PacketId _pResourceAdjusted;
        static PacketsResources()
        {
            Registry.WorldEventHooksServer.Register<ResourceAdjustedEvent>(SendResourceAdjusted);
            _pResourceAdjusted = Registry.PacketHandlers.Register(OnResourceAdjusted);
        }
        private static void SendResourceAdjusted(ResourceAdjustedEvent @event)
        {
            Server.Instance.BeginPacket(_pResourceAdjusted)
                .Write(@event.Owner.RefId)
                .Write(@event.Def)
                .Write(@event.Value);
        }
        static void OnResourceAdjusted(NetEndpoint endpoint, Packet packet)
        {
            endpoint.World
                .GetEntity(packet.PacketReader.ReadEntityRefId())
                .Resources.SetValue(packet.PacketReader.ReadDef<ResourceDef>(), packet.PacketReader.ReadSingle());
        }
    }
}
