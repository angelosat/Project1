using System;
using Microsoft.Xna.Framework;
using Start_a_Town_.Animations;

namespace Start_a_Town_.Components
{
    partial class HaulComponent : EntityComp
    {

        public override string Name { get; } = "Haul"; 
        InventoryComponent Inventory;
        public GameObjectSlot GetSlot()
        {
            return this.Inventory.HaulSlot;
        }
        public GameObject GetObject()
        {
            return this.Inventory.HaulSlot.Object;
        }

        public Animation AnimationHaul = new(AnimationDef.Haul) { Weight = 0 };

        public override void Resolve()
        {
            this.Inventory = this.Owner.GetComponent<InventoryComponent>();
            if (this.Inventory == null || this.Inventory.Capacity == 0)
                throw new Exception("HaulComponent requires a parent entity with PersonalInventoryComponent and an inventory of at least size 1");
            this.Owner.AddResourceModifier(new ResourceRateModifier(ResourceRateModifierDef.HaulingStaminaDrain));
            this.Owner.AddStatModifier(new StatNewModifier(StatModifierDef.WalkSpeedHaulingWeight));
            this.Owner.AddAnimation(this.AnimationHaul);
        }
        
        static public GameObjectSlot GetHolding(GameObject parent)
        {
            return parent.GetComponent<HaulComponent>().Holding;
        }
        public GameObjectSlot Holding
        {
            get
            {
                GameObjectSlot slot = this.GetSlot();//.Slot;
                return slot;
            }
        }

        public override object Clone()
        {
            return new HaulComponent();
        }
        public override void Write(IDataWriter w)
        {
            this.AnimationHaul.Write(w);

        }
        public override void Read(IDataReader r)
        {
            this.AnimationHaul.Read(r);
        }
        
        internal override void SaveExtra(SaveTag tag)
        {
            tag.Add(this.AnimationHaul.Save("AnimationHaul"));
        }
        internal override void LoadExtra(SaveTag save)
        {
            save.TryGetTag("AnimationHaul", this.AnimationHaul.Load);
        }
    }
}
