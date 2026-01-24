using Start_a_Town_.Net;
using System;
using System.DirectoryServices.ActiveDirectory;
using System.Linq;
using System.Net;

namespace Start_a_Town_
{
    [EnsureStaticCtorCall]
    internal static class PacketsBlocks
    {
            readonly static PacketId _pBlockEntityRemoved, _pBlockEntityAdded, _pBlockEntityCompUpdated, _pBlockSet, _pBlocksUpdated, _pOwnerChanged, _pOwnerChangedByPlayer;
        static PacketsBlocks()
        {
            _pBlockEntityRemoved = Registry.PacketHandlers.Register(OnBlockEntityRemoved);
            _pBlocksUpdated = Registry.PacketHandlers.Register(OnBlocksUpdated);
            _pBlockSet = Registry.PacketHandlers.Register(OnBlockSet);
            _pBlockEntityAdded = Registry.PacketHandlers.Register(OnBlockEntityAdded);
            //Registry.MapEventHooksServer.Register<BlockEntityRemovedEvent>(SendBlockEntityRemoved);
            //Registry.MapEventHooksServer.Register<BlockEntityAddedEvent>(SendBlockEntityAdded);
            //Registry.MapEventHooksServer.Register<BlockSetEvent>(SendBlockSet);
            //Registry.MapEventHooksServer.Register<BlocksChangedEvent>(SendBlocksChanged);

            Registry.PlayerInputEventHooks.Register<PlayerChangedBlockOwnerEvent>(HandlePlayerChangedBlockOwnerEvent);
            Registry.MapEventHooksServer.Register<BlockOwnerChangedEvent>(SendBlockOwnerChanged);
            _pOwnerChanged = Registry.PacketHandlers.Register(OnBlockOwnerChanged);
            _pOwnerChangedByPlayer = Registry.PacketHandlers.Register(OnBlockOwnerChangedByPlayer);

            Registry.MapEventHooksServer.Register<BlockEntityCompUpdatedEvent>(SendBlockEntityCompUpdated);
            _pBlockEntityCompUpdated = Registry.PacketHandlers.Register(OnBlockEntityCompUpdated);

        }

        private static void OnBlockEntityCompUpdated(NetEndpoint endpoint, Packet packet)
        {
            var client = endpoint as Client;
            var r = packet.PacketReader;
            _ = r.ReadInt32();
            var originglobal = r.ReadIntVec3();
            var blockentity = client.Map.GetBlockEntity(originglobal);
            var compindex = r.ReadInt32();
            var comp = blockentity.Comps.GetComp(compindex);
            comp.Read(r);
        }

        private static void SendBlockEntityCompUpdated(BlockEntityCompUpdatedEvent e)
        {
            Server.Instance.BeginPacket(_pBlockEntityCompUpdated)
                .Write(e.Comp.Parent.Map.ID)
                .Write(e.Comp.Parent.OriginGlobal)
                .Write(e.Comp.RuntimeIndex)
                .Write(e.Comp);
        }

        private static void HandlePlayerChangedBlockOwnerEvent(PlayerChangedBlockOwnerEvent e)
        {
            var entity = e.Entity;
            var owner = e.Actor;
            Client.Instance.BeginPacketImmediate(_pOwnerChangedByPlayer)
                .Write(entity.Map.ID)
                .Write(entity.OriginGlobal)
                .Write(owner?.RefId ?? EntityRefId.Null);
        }

        private static void SendBlockOwnerChanged(BlockOwnerChangedEvent e)
        {
            var entity = e.Entity;
            var owner = e.Actor;
            Server.Instance.BeginPacketImmediate(_pOwnerChanged)
                .Write(entity.Map.ID)
                .Write(entity.OriginGlobal)
                .Write(owner?.RefId ?? EntityRefId.Null);
        }

        //private static void SendBlockOwnerChanged(NetEndpoint endpoint, BlockEntity entity, Actor owner)
        //{
        //    endpoint.BeginPacketImmediate(_pOwnerChanged)
        //        .Write(entity.Map.ID)
        //        .Write(entity.OriginGlobal)
        //        .Write(owner.RefId);
        //}
        private static void OnBlockOwnerChanged(NetEndpoint endpoint, Packet packet)
        {
            var client = endpoint as Client;
            var r = packet.PacketReader;
            var mapid = r.ReadInt32();
            var map = client.Map;
            var entity = map.GetBlockEntity(r.ReadIntVec3());
            var ownerid = (EntityRefId)r.ReadEntityRefId();
            var owner = ownerid != EntityRefId.Null ? map.World.GetEntity<Actor>(ownerid) : null;
            var comp = entity.GetComp<BlockOwnershipComp>();
            comp.SetOwner(owner);
            //if (endpoint is Server server)
            //    SendBlockOwnerChanged(server, entity, owner);
        }
        private static void OnBlockOwnerChangedByPlayer(NetEndpoint endpoint, Packet packet)
        {
            var r = packet.PacketReader;
            var mapid = r.ReadInt32();
            var map = endpoint.Map;
            var entity = map.GetBlockEntity(r.ReadIntVec3());
            var ownerid = (EntityRefId)r.ReadEntityRefId();
            var owner = ownerid != EntityRefId.Null ? map.World.GetEntity<Actor>(ownerid) : null;
            var comp = entity.GetComp<BlockOwnershipComp>();
            // internally fires BlockOwnerChangedEvent to be replicated
            comp.SetOwner(owner); 
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
