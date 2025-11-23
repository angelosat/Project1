using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;

namespace Start_a_Town_.UI
{
    class SlotGrid<TSlot, TItem> : GroupBox
        where TSlot : Slot<TItem>, new()
        where TItem : class, ISlottable
    {
        public SlotGrid(IEnumerable<TItem> items, int lineMax, Action<TSlot, TItem> slotInit = null)
        {
            Action<TSlot, TItem> _slotInit = slotInit ?? ((slot, item) => { });
            int count = items.Count();
            int i = 0;
            foreach (var item in items)
            {
                var slot = new TSlot() { Tag = item, Location = new Vector2((i % lineMax) * UIManager.SlotSprite.Width, (i / lineMax) * UIManager.SlotSprite.Height) };
                _slotInit(slot, item);
                this.Controls.Add(slot);
                i++;
            }
        }
    }

    class SlotGrid<T> : GroupBox where T : Slot, new()
    {
        public SlotGrid(IEnumerable<GameObjectSlot> objSlots, int lineMax)
            : this(objSlots.ToList(), lineMax, (c) => { })
        {
        }
        public SlotGrid(List<GameObjectSlot> objSlots, int lineMax, Action<T> slotInit)
            : base()
        {
            for (int i = 0; i < objSlots.Count; i++)
            {
                var objslot = objSlots[i];
                var slot = new T() { Tag = objslot, Location = new Vector2((i % lineMax) * UIManager.SlotSprite.Width, (i / lineMax) * UIManager.SlotSprite.Height) };
                slotInit(slot);
                this.Controls.Add(slot);
            }
        }
        public SlotGrid(int count, int lineMax, Action<T> slotInit)
        {
            for (int i = 0; i < count; i++)
            {
                var slot = new T() { Tag = GameObjectSlot.Empty, Location = new Vector2((i % lineMax) * UIManager.SlotSprite.Width, (i / lineMax) * UIManager.SlotSprite.Height) };
                slotInit(slot);
                this.Controls.Add(slot);
            }
        }
    }
}
