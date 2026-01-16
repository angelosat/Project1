using Start_a_Town_.Net;
using System;

namespace Start_a_Town_
{
    [EnsureStaticCtorCall]
    internal static class PacketsInventory
    {
        static readonly PacketId _pSync, _pSlot, _pPlayerForcedDropInventoryItem, _pInventoryDelta;
        static PacketsInventory()
        {
            _pSync = Registry.PacketHandlers.Register(OnInventorySync);
            _pSlot = Registry.PacketHandlers.Register(OnSlotUpdated);
            _pPlayerForcedDropInventoryItem = Registry.PacketHandlers.Register(OnReceivePlayerForcedDropInventoryItem);

            _pInventoryDelta = Registry.PacketHandlers.Register(OnInventoryDelta);

            Registry.MapEventHooksServer.Register<InventoryUpdatedEvent>(SendInventoryUpdated);
            Registry.MapEventHooksServer.Register<SlotUpdatedEvent>(SendSlotUpdated);

            Registry.PlayerInputEventHooks.Register<PlayerForcedDropInventoryItemEvent>(HandlePlayerForcedDropInventoryItem);

            Registry.WorldEventHooksServer.Register<ItemAddedToInventoryEvent>(HandleItemAddedToInventory);
        }

        private static void OnInventoryDelta(NetEndpoint endpoint, Packet packet)
        {
            var client = endpoint as Client;
            var r = packet.PacketReader;
            var actor = client.World.GetEntity<Actor>(r.ReadInt32());
            var item = client.World.GetEntity(r.ReadInt32());
            actor.Inventory.Contents.AddInternal(item);
        }

        private static void HandleItemAddedToInventory(ItemAddedToInventoryEvent e)
        {
            var server = e.Actor.Net as Server;
            server.BeginPacket(_pInventoryDelta)
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
            //if (endpoint is Server server)
            //    SendPlayerForceDropInventoryItem(server, owner, item, count);
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
                .Write(owner.RefId)
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
            client.World.GetEntity(e.Owner).GetSlot(e.SlotIndex).Assign(e.Content);
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

    internal sealed class SlotUpdatedEvent : IEventPayload//, ISerializableNew<SlotUpdatedEvent>
    {
        public readonly SlotIndex SlotIndex;
        public readonly EntityRefId Owner;
        public readonly EntityRefId Content;
        public readonly GameObjectSlot Slot;
        public SlotUpdatedEvent(GameObjectSlot slot)
        {
            this.Slot = slot;
            this.SlotIndex = slot.ID;
            this.Owner = slot.Owner.RefId;
            this.Content = slot.Object?.RefId ?? EntityRefId.Null;
        }
        public SlotUpdatedEvent(SlotIndex slotIndex, EntityRefId ownerRefId, EntityRefId contentRefId)
        {
            this.SlotIndex = slotIndex;
            this.Owner = ownerRefId;
            this.Content = contentRefId;
        }

        public static SlotUpdatedEvent Create(IDataReader r)
        {
            var owner = new EntityRefId(r.ReadInt32());
            var slot = new SlotIndex(r.ReadInt32());
            var content = new EntityRefId(r.ReadInt32());
            return new(slot, owner, content);
        }

        public SlotUpdatedEvent Read(IDataReader r)
        {
            throw new Exception();
            return this;
        }

        public void Write(IDataWriter w)
        {
            w.Write(this.Owner);
            w.Write(this.SlotIndex);
            w.Write(this.Content);
        }
    }

    internal class InventoryUpdatedEvent : IEventPayload
    {

    }
}
