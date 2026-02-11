using System.Collections.Generic;
using System.Linq;
using Project1.Framework;
using Project1.Framework.UI;
using Project1.Core.Entities.Actors;
using Project1.Core.Entities;

namespace Project1.Core.Gear
{
    class GearContainer(Actor owner) : Inspectable
    {
        readonly Actor Owner = owner;
        readonly Dictionary<GearTypeDef, GameObjectSlot> Slots = [];
        public IEnumerable<GameObjectSlot> AllSlots => this.Slots.Values;
        public void Register(GearTypeDef def)
        {
            this.Slots.Add(def, new GameObjectSlot() { Owner = this.Owner });
        }
        public GameObjectSlot GetSlot(GameObject item) => this.Slots.Values.FirstOrDefault(s => s.Object == item);
        public GameObjectSlot GetSlot(GearTypeDef def) => this.Slots[def];
        public Entity GetSlotContent(GearTypeDef def) => this.Slots[def].Object as Entity;

        public GroupBox GetGui()
        {
            var box = new GroupBox();
            var table = new Table<(GearTypeDef def, GameObjectSlot slot)>()
                .AddColumn("geardef", 64, v => new LabelNew(v.def), 1)
                //.AddColumn("slot", 128, v => new LabelNew(() => v.slot.Object?.Name ?? ""), 0);
                .AddColumn("slot", 128, v => new LabelNew(() => v.slot.Object?.Name ?? "") { TooltipFunc = v.slot.GetTooltipInfo }.Bind(v.slot), 0);
            table.AddItems(this.Slots.Select(vk => (vk.Key, vk.Value)));
            box.Controls.Add(table);
            //this.Owner.World.Events.ListenTo<ActorGearUpdatedEvent>(onGearUpdated);
            //void onGearUpdated(ActorGearUpdatedEvent e)
            //{
            //    if (e.Actor == this.Owner)
            //        table.Invalidate(true);
            //}
            return box;
        }
    }
}
