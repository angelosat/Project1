using System;
using Project1.Framework;
using Project1.Core.Entities;
using Project1.Core.Inventory;
using Project1.Core.Entities.Actors;
using Project1.Core.Helpers.Structs;
using Project1.Core.Net;
using Project1.Core.Input;
using Project1.Framework.Events;

namespace Project1.Core.Networking
{
    [EnsureStaticCtorCall]
    internal static class PacketsInventory
    {
        static readonly PacketId _pSync, _pSlot, _pPlayerForcedDropInventoryItem, _pInventoryAdded, _pInventoryRemoved;
        static PacketsInventory()
        {
            _pSync = Registry.PacketHandlers.Register(OnInventorySync);
            _pSlot = Registry.PacketHandlers.Register(OnSlotUpdated);
            _pPlayerForcedDropInventoryItem = Registry.PacketHandlers.Register(OnReceivePlayerForcedDropInventoryItem);

            _pInventoryAdded = Registry.PacketHandlers.Register(OnInventoryItemAdded);
            _pInventoryRemoved = Registry.PacketHandlers.Register(OnInventoryItemRemoved);

            Registry.WorldEventHooksServer.Register<InventoryUpdatedEvent>(SendInventoryUpdated);
            Registry.WorldEventHooksServer.Register<SlotUpdatedEvent>(SendSlotUpdated);

            Registry.PlayerInputEventHooks.Register<PlayerForcedDropInventoryItemEvent>(HandlePlayerForcedDropInventoryItem);

            Registry.WorldEventHooksServer.Register<InventoryItemAddedEvent>(HandleInventoryItemAdded);
            Registry.WorldEventHooksServer.Register<InventoryItemRemovedEvent>(HandleInventoryItemRemoved);
        }

        private static void OnInventoryItemAdded(NetEndpoint endpoint, Packet packet)
        {
            var client = endpoint as Client;
            var r = packet.PacketReader;
            var actor = client.World.GetEntity<Actor>(r.ReadInt32());
            var item = client.World.GetEntity(r.ReadInt32());
            actor.Inventory.Contents.AddInternal(item);
        }
        private static void OnInventoryItemRemoved(NetEndpoint endpoint, Packet packet)
        {
            var client = endpoint as Client;
            var r = packet.PacketReader;
            var actor = client.World.GetEntity<Actor>(r.ReadInt32());
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
        private static void OnReceivePlayerForcedDropInventoryItem(NetEndpoint endpoint, Packet packet)
        {
            var server = endpoint as Server;
            var map = server.Map;
            var r = packet.PacketReader;
            var ownerid = r.ReadInt32();
            var owner = map.World.GetEntity<Actor>(ownerid);
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
