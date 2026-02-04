using Project1.Framework.Base;
using Project1.Framework.Entities;
using Start_a_Town_;
using System;

namespace Project1.Framework.Inventory
{
    internal sealed class SlotUpdatedEvent : IEventPayload
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
