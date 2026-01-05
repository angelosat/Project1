using Start_a_Town_.Net;
using System;

namespace Start_a_Town_
{
    [EnsureStaticCtorCall]
    internal static class PacketsBlocks
    {
            readonly static int _pBlockEntityRemoved, _pBlockEntityAdded, _pBlockSet;
        static PacketsBlocks()
        {
            _pBlockEntityRemoved = Registry.PacketHandlers.Register(OnBlockEntityRemoved);
            _pBlockSet = Registry.PacketHandlers.Register(OnBlockSet);
            //_pBlockEntityAdded = Registry.PacketHandlers.Register(OnBlockEntityAdded);
            Registry.MapEventHooksServer.Register<BlockEntityRemovedEvent>(SendBlockEntityRemoved);
            //Registry.MapEventHooksServer.Register<BlockEntityAddedEvent>(SendBlockEntityAdded);

            Registry.MapEventHooksServer.Register<BlockSetEvent>(SendBlockSet);

        }
        private static void SendBlockSet(BlockSetEvent e)
        {
            var w = Server.Instance.BeginPacket(_pBlockSet);
            e.args.WriteTo(w);
            
        }

        private static void OnBlockSet(NetEndpoint endpoint, Packet packet)
        {
            var client = endpoint as Client;
            var r = packet.PacketReader;
            var args = SetBlockArgs.ReadFrom(r);
            client.Map.SetBlock(args);
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
