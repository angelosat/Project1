using Start_a_Town_.Net;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Start_a_Town_
{
    [EnsureStaticCtorCall]
    internal static class PacketsBlocks
    {
            readonly static int _pBlockEntityRemoved, _pBlockEntityAdded, _pBlockSet, _pBlocksUpdated;
        static PacketsBlocks()
        {
            _pBlockEntityRemoved = Registry.PacketHandlers.Register(OnBlockEntityRemoved);
            _pBlocksUpdated = Registry.PacketHandlers.Register(OnBlocksUpdated);
            _pBlockSet = Registry.PacketHandlers.Register(OnBlockSet);
            _pBlockEntityAdded = Registry.PacketHandlers.Register(OnBlockEntityAdded);
            Registry.MapEventHooksServer.Register<BlockEntityRemovedEvent>(SendBlockEntityRemoved);
            Registry.MapEventHooksServer.Register<BlockEntityAddedEvent>(SendBlockEntityAdded);
            Registry.MapEventHooksServer.Register<BlockSetEvent>(SendBlockSet);
            Registry.MapEventHooksServer.Register<BlocksChangedEvent>(SendBlocksChanged);
        }

        private static void SendBlockEntityAdded(BlockEntityAddedEvent e)
        {
            var w = Server.Instance.BeginPacket(_pBlockEntityAdded);
                //.Write(e.Entity.OriginGlobal);
            e.Entity.Write(w);
        }

        private static void OnBlockEntityAdded(NetEndpoint endpoint, Packet packet)
        {
            var client = endpoint as Client;
            var r = packet.PacketReader;
            var entity = BlockEntity.Create(r);
            endpoint.Map.AddBlockEntityInternal(entity);
        }

        private static void SendBlockSet(BlockSetEvent e)
        {
            var w = Server.Instance.BeginPacket(_pBlockSet);
            e.args.Write(w);
        }

        private static void OnBlockSet(NetEndpoint endpoint, Packet packet)
        {
            var client = endpoint as Client;
            var r = packet.PacketReader;
            var args = SetBlockArgs.Create(r);
            client.Map.SetBlock(args);
        }
        private static void SendBlocksChanged(BlocksChangedEvent e)
        {
            Server.Instance.BeginPacket(_pBlocksUpdated)
                .Write(e.Map.ID)
                .Write(e.Changes.ToList());
        }
        private static void OnBlocksUpdated(NetEndpoint endpoint, Packet packet)
        {
            var client = endpoint as Client;
            var r = packet.PacketReader;
            //var args = SetBlockArgs.ReadFrom(r);
            //var dic = new Dictionary<IntVec3, SetBlockArgs>
            //{
            //    { args.Global, args }
            //};
            _ = r.ReadInt32();
            var list = r.ReadList<SetBlockArgs>();
            client.Map.SetBlockInternal(list.ToDictionary(a => a.Global, a => a));
        }
      

        private static void SendBlockEntityRemoved(BlockEntityRemovedEvent e)
        {
            Server.Instance.BeginPacket(_pBlockEntityRemoved)
                .Write(e.Entity.OriginGlobal);
        }
        private static void OnBlockEntityRemoved(NetEndpoint endpoint, Packet packet)
        {
            //endpoint.Map.RemoveBlockEntity(packet.PacketReader.ReadIntVec3());
            endpoint.Map.RemoveBlockEntityInternal(packet.PacketReader.ReadIntVec3());
        }
    }
}
