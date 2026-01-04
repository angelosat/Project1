using Start_a_Town_.Net;

namespace Start_a_Town_
{
    internal static class PacketsBlocks
    {
        [EnsureStaticCtorCall]
        static class PacketsCrafting
        {
            readonly static int _pBlockEntityRemoved;
            static PacketsCrafting()
            {
                _pBlockEntityRemoved = Registry.PacketHandlers.Register(OnBlockEntityRemoved);
                Registry.MapEventHooksServer.Register<BlockEntityRemovedEvent>(SendBlockEntityRemoved);
            }
            private static void SendBlockEntityRemoved(BlockEntityRemovedEvent e)
            {
                Server.Instance.BeginPacket(_pBlockEntityRemoved)
                    .Write(e.Entity.OriginGlobal);
            }
            private static void OnBlockEntityRemoved(NetEndpoint endpoint, Packet packet)
            {
                endpoint.Map.RemoveBlockEntity(packet.PacketReader.ReadIntVec3());
            }
        }
    }
}
