using System.Linq;
using Project1.Framework;
using Project1.Framework.Serialization;
using Project1.Core.Entities.Actors;
using Project1.Core.Helpers;
using Project1.Core.Networking;
using Project1.Core.Networking.Simulation;
using Project1.Framework.Helpers;
using System;
using Project1.Core.Simulation;
using Project1.Core.Screens;

namespace Project1.Core.Blocks
{
    [EnsureStaticCtorCall]
    internal static class PacketsBlocks
    {
        readonly static PacketId _pBlockEntityRemoved, _pBlockEntityAdded, _pBlockEntityCompUpdated, _pBlockSet, _pBlocksUpdated, _pOwnerChanged, _pOwnerChangedByPlayer;
        readonly static PacketId _pBlockDamaged = Registry.PacketHandlers.Register(ReceiveBlockDamaged);

      

        static PacketsBlocks()
        {
            _pBlockEntityRemoved = Registry.PacketHandlers.Register(OnBlockEntityRemoved);
            _pBlocksUpdated = Registry.PacketHandlers.Register(OnBlocksUpdated);
            _pBlockSet = Registry.PacketHandlers.Register(OnBlockSet);
            _pBlockEntityAdded = Registry.PacketHandlers.Register(OnBlockEntityAdded);

            Registry.PlayerInputEventHooks.Register<PlayerChangedBlockOwnerEvent>(HandlePlayerChangedBlockOwnerEvent);
            Registry.MapEventHooksServer.Register<BlockOwnerChangedEvent>(SendBlockOwnerChanged);
            _pOwnerChanged = Registry.PacketHandlers.Register(OnBlockOwnerChanged);
            _pOwnerChangedByPlayer = Registry.PacketHandlers.Register(OnBlockOwnerChangedByPlayer);

            Registry.MapEventHooksServer.Register<BlockEntityCompUpdatedEvent>(SendBlockEntityCompUpdated);
            _pBlockEntityCompUpdated = Registry.PacketHandlers.Register(OnBlockEntityCompUpdated);

            Registry.MapEventHooksServer.Register<BlockDamagedEvent>(HandleBlockDamageApplied);
        }

        private static void HandleBlockDamageApplied(BlockDamagedEvent e)
        {
            var net = e.Map.Net;
            SendBlockDamaged(net, e.Map, e.Cell, e.Delta);
        }
        private static void SendBlockDamaged(NetEndpoint endpoint, MapBase map, IntVec3 cell, int delta)
        {
            if (!endpoint.IsServer)
                throw new Exception();
            endpoint
                .BeginPacket(_pBlockDamaged)
                .Write(map.ID)
                .Write(cell)
                .Write(delta);
        }
        private static void ReceiveBlockDamaged(NetEndpoint endpoint, Packet packet)
        {
            if (endpoint is not Client)
                throw new Exception();
            var r = packet.PacketReader;
            var mapid = r.ReadInt32();
            var cell = r.ReadIntVec3();
            var delta = r.ReadInt32();
            var map = Client.Instance.Map;
            map.ApplyBlockDamage(cell, delta);
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
            var owner = e.NewOwner;
            Server.Instance.BeginPacketImmediate(_pOwnerChanged)
                .Write(entity.Map.ID)
                .Write(entity.OriginGlobal)
                .Write(owner?.RefId ?? EntityRefId.Null)
                //.Write(e.PreviousOwner);
            ;
        }

        private static void OnBlockOwnerChanged(NetEndpoint endpoint, Packet packet)
        {
            var client = endpoint as Client;
            var r = packet.PacketReader;
            var mapid = r.ReadInt32();
            var map = client.Map;
            var entity = map.GetBlockEntity(r.ReadIntVec3());
            var ownerid = r.ReadEntityRefId();
            //var previousOwnerid = r.ReadEntityRefId();
            var owner = ownerid != EntityRefId.Null ? map.World.GetEntity<Actor>(ownerid) : null;
            var comp = entity.GetComp<BlockOwnershipComp>();
            comp.SetOwner(owner);
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
            e.Args.Write(w);
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
            endpoint.Map.RemoveBlockEntityInternal(packet.PacketReader.ReadIntVec3());
        }
    }
}
