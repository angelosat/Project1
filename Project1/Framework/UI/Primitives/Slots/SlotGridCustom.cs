using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Project1.Core.Interfaces;
using Project1.Core.UI;

namespace Project1.Core.UI
{
    class SlotGridCustom<TSlot, TItem> : GroupBox
        where TSlot : SlotCustom<TItem>, new()
        where TItem : class, ISlottable
    {
        public SlotGridCustom(IEnumerable<TItem> items, int lineMax, Action<TSlot, TItem> slotInit = null)
        {
            int count = items.Count();
            int i = 0;
            foreach (var item in items)
            {
                var slot = new TSlot() { Tag = item, Location = new Vector2(i % lineMax * UIManager.SlotSprite.Width, i / lineMax * UIManager.SlotSprite.Height) };
                slotInit?.Invoke(slot, item);
                this.Controls.Add(slot);
                i++;
            }
        }
    }
}
