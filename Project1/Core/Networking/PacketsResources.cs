using Project1.Core.Blocks.Comps;
using Project1.Core.Helpers;
using Project1.Core.Input;
using Project1.Core.Resources;
using Project1.Framework;
using System;

namespace Project1.Core.Networking
{
    [EnsureStaticCtorCall]
    internal static class PacketsResources
    {
        readonly static PacketId 
            _pResourceDelta = Registry.PacketHandlers.Register(ReceiveResourceDelta),
            _pBlockResourceDelta = Registry.PacketHandlers.Register(ReceiveBlockResourceDelta);

       

        static PacketsResources()
        {
            Registry.WorldEventHooksServer.Register<ResourceModifiedEvent>(SendResourceDelta);
            Registry.WorldEventHooksServer.Register<BlockResourceModifiedEvent>(SendBlockResourceDelta);
        }
        private static void SendResourceDelta(ResourceModifiedEvent e)
        {
            Server.Instance.BeginPacket(_pResourceDelta)
                .Write(e.Entity.RefId)
                .Write(e.Def)
                .Write(e.Delta);
        }
        static void ReceiveResourceDelta(NetEndpoint endpoint, Packet packet)
        {
            endpoint.World
                .GetEntity(packet.PacketReader.ReadEntityRefId())
                .Resources.ApplyDelta(packet.PacketReader.ReadDef<ResourceDef>(), packet.PacketReader.ReadSingle());
        }
        static void SendBlockResourceDelta(BlockResourceModifiedEvent e)
        {
            Server.Instance.BeginPacket(_pBlockResourceDelta)
             .Write(e.Map.ID)
             .Write(e.Cell)
             .Write(e.Def)
             .Write(e.Delta);
        }
        private static void ReceiveBlockResourceDelta(NetEndpoint endpoint, Packet packet)
        {
            var r = packet.PacketReader;
            var mapid = r.ReadInt32();
            var cell = r.ReadIntVec3();
            var resDef = r.ReadDef<ResourceDef>();
            var delta = r.ReadSingle();
            endpoint
                .Map.Query(cell).BlockEntity.GetComp<BlockResourcesComp>()
                .ApplyDelta(resDef, delta);
        }
    }
}
