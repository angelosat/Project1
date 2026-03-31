using Project1.Core.Entities;
using Project1.Core.Entities.Actors;
using Project1.Core.Input;
using Project1.Core.Systems.Inventory;
using Project1.Core.Towns.Storage;
using Project1.Framework;
using System;

namespace Project1.Core.Networking
{
    [EnsureStaticCtorCall]
    internal static class PacketsInventory
    {
        static readonly PacketId _pSync, _pSlot, _pPlayerForcedDropInventoryItem,
            _pInventoryAdded = Registry.PacketHandlers.Register(OnInventoryItemAdded),
            _pInventoryRemoved = Registry.PacketHandlers.Register(OnInventoryItemRemoved),
            _pBlockInventoryAdded = Registry.PacketHandlers.Register(OnBlockInventoryItemAdded),
            _pBlockInventoryRemoved = Registry.PacketHandlers.Register(OnBlockInventoryItemRemoved)
            ;
        static PacketsInventory()
        {
            _pSync = Registry.PacketHandlers.Register(OnInventorySync);
            _pSlot = Registry.PacketHandlers.Register(OnSlotUpdated);
            _pPlayerForcedDropInventoryItem = Registry.PacketHandlers.Register(OnReceivePlayerForcedDropInventoryItem);

            Registry.WorldEventHooksServer.Register<InventoryUpdatedEvent>(SendInventoryUpdated);
            Registry.WorldEventHooksServer.Register<SlotUpdatedEvent>(SendSlotUpdated);

            Registry.PlayerInputEventHooks.Register<PlayerForcedDropInventoryItemEvent>(HandlePlayerForcedDropInventoryItem);

            Registry.WorldEventHooksServer.Register<InventoryItemAddedEvent>(HandleInventoryItemAdded);
            Registry.WorldEventHooksServer.Register<InventoryItemRemovedEvent>(HandleInventoryItemRemoved);

            Registry.MapEventHooksServer.Register<BlockInventoryItemAddedEvent>(HandleBlockInventoryItemAdded);
            Registry.MapEventHooksServer.Register<BlockInventoryItemRemovedEvent>(HandleBlockInventoryItemRemoved);
        }

        private static void OnInventoryItemAdded(NetEndpoint endpoint, Packet packet)
        {
            var client = endpoint as Client;
            var r = packet.PacketReader;
            var actor = client.World.Get<Actor>(r.ReadInt32());
            var item = client.World.GetEntity(r.ReadInt32());
            actor.Inventory.Contents.AddInternal(item);
            //actor.Inventory.Insert(item);
        }
        private static void OnInventoryItemRemoved(NetEndpoint endpoint, Packet packet)
        {
            var client = endpoint as Client;
            var r = packet.PacketReader;
            var actor = client.World.Get<Actor>(r.ReadInt32());
            var item = client.World.GetEntity(r.ReadInt32());
            actor.Inventory.Contents.RemoveInternal(item);
        }
        private static void HandleInventoryItemAdded(InventoryItemAddedEvent e)
        {
            var server = e.Actor.Net as Server;
            server.BeginPacket(_pInventoryAdded)
                .Write(e.Actor.RefId)
                .Write(e.Item.RefId);
        }
        private static void HandleInventoryItemRemoved(InventoryItemRemovedEvent e)
        {
            var server = e.Actor.Net as Server;
            server.BeginPacket(_pInventoryRemoved)
                .Write(e.Actor.RefId)
                .Write(e.Item.RefId);
        }
        private static void HandleBlockInventoryItemAdded(BlockInventoryItemAddedEvent e)
        {
            var server = e.Entity.Map.Net as Server;
            server.BeginPacket(_pBlockInventoryAdded)
                .Write(e.Entity.Map.ID)
                .Write(e.Entity.OriginGlobal)
                .Write(e.Item.RefId);
        }
        private static void HandleBlockInventoryItemRemoved(BlockInventoryItemRemovedEvent e)
        {
            var server = e.Entity.Map.Net as Server;
            server.BeginPacket(_pBlockInventoryRemoved)
                .Write(e.Entity.Map.ID)
                .Write(e.Entity.OriginGlobal)
                .Write(e.Item.RefId);
        }
        private static void OnBlockInventoryItemAdded(NetEndpoint endpoint, Packet packet)
        {
            var client = endpoint as Client;
            var r = packet.PacketReader;
            var mapid = r.ReadInt32();
            var be = client.Map.GetBlockEntity(r.ReadIntVec3());
            var item = client.World.GetEntity(r.ReadInt32());
            be.GetComp<BlockInventoryComp>().Insert(item);
        }
        private static void OnBlockInventoryItemRemoved(NetEndpoint endpoint, Packet packet)
        {
            var client = endpoint as Client;
            var r = packet.PacketReader;
            var mapid = r.ReadInt32();
            var be = client.Map.GetBlockEntity(r.ReadIntVec3());
            var item = client.World.GetEntity(r.ReadInt32());
            be.GetComp<BlockInventoryComp>().Remove(item);
        }

        private static void OnReceivePlayerForcedDropInventoryItem(NetEndpoint endpoint, Packet packet)
        {
            var server = endpoint as Server;
            var map = server.Map;
            var r = packet.PacketReader;
            var ownerid = r.ReadInt32();
            var owner = map.World.Get<Actor>(ownerid);
            var itemid = r.ReadInt32();
            var item = map.World.GetEntity(itemid);
            var count = r.ReadInt32();
            owner.AI.State.ItemPreferences.ForceDrop(item); 
        }

        private static void HandlePlayerForcedDropInventoryItem(PlayerForcedDropInventoryItemEvent e)
        {
            var owner = e.Owner as Actor;
            var item = e.Item;
            var count = e.Count;
            var net = owner.Net;
            if (net is Client client)
                SendPlayerForceDropInventoryItem(client, owner, item, count);
            else if (net is Server server)
                owner.AI.State.ItemPreferences.ForceDrop(item);
        }

        private static void SendPlayerForceDropInventoryItem(NetEndpoint net, Entity owner, Entity item, int count)
        {
            net.BeginPacketImmediate(_pPlayerForcedDropInventoryItem)
                .Write(owner.RefId)
                .Write(item.RefId)
                .Write(count);
        }

        private static void SendSlotUpdated(SlotUpdatedEvent e)
        {
            e.Write(Server.Instance.BeginPacket(_pSlot));
        }
        private static void OnSlotUpdated(NetEndpoint endpoint, Packet packet)
        {
            var client = endpoint as Client;
            var r = packet.PacketReader;
            var e = SlotUpdatedEvent.Create(r);
            var entity = client.World.GetEntity(e.Owner);
            var slot = entity.GetSlot(e.SlotIndex);
            slot.Assign(e.Content);
        }

        

        private static void SendInventoryUpdated(InventoryUpdatedEvent e)
        {
            throw new NotImplementedException();
        }

        private static void OnInventorySync(NetEndpoint endpoint, Packet packet)
        {
            throw new NotImplementedException();
        }
    }

    
}
