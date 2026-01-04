using Start_a_Town_.Net;
using System;

namespace Start_a_Town_
{
    internal static class PacketsBlocks
    {
        [EnsureStaticCtorCall]
        static class PacketsCrafting
        {
            readonly static int _pBlockEntityRemoved, _pBlockEntityAdded;
            static PacketsCrafting()
            {
                _pBlockEntityRemoved = Registry.PacketHandlers.Register(OnBlockEntityRemoved);
                //_pBlockEntityAdded = Registry.PacketHandlers.Register(OnBlockEntityAdded);
                Registry.MapEventHooksServer.Register<BlockEntityRemovedEvent>(SendBlockEntityRemoved);
                //Registry.MapEventHooksServer.Register<BlockEntityAddedEvent>(SendBlockEntityAdded);
            }

            //private static void SendBlockEntityAdded(BlockEntityAddedEvent e)
            //{
            //    Server.Instance.BeginPacket(_pBlockEntityAdded)
            //        .Write(e.Entity.OriginGlobal);
            //}

            //private static void OnBlockEntityAdded(NetEndpoint endpoint, Packet packet)
            //{
            //    endpoint.Map.RemoveBlockEntity(packet.PacketReader.ReadIntVec3());
            //}

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
