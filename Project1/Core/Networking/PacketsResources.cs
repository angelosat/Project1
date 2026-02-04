using Project1.Framework.Attributes;
using Project1.Framework.Base;
using Project1.Framework.Net;
using Project1.Framework.Resources;
using Start_a_Town_;

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
