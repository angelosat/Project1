using Microsoft.Xna.Framework;
using Start_a_Town_.Net;
using System;

namespace Start_a_Town_
{
    [EnsureStaticCtorCall]
    internal static class PacketsInventory
    {
        static readonly int _pSync, _pSlot;
        static PacketsInventory()
        {
            _pSync = Registry.PacketHandlers.Register(OnInventorySync);
            _pSlot = Registry.PacketHandlers.Register(OnSlotUpdated);

            Registry.MapEventHooksServer.Register<InventoryUpdatedEvent>(SendInventoryUpdated);
            Registry.MapEventHooksServer.Register<SlotUpdatedEvent>(SendSlotUpdated);

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

    internal class SlotUpdatedEvent : IEventPayload//, ISerializableNew<SlotUpdatedEvent>
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
