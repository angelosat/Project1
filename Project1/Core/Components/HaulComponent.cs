using System;
using Project1.Framework;
using Project1.Framework.Serialization;
using Project1.Core.Animations;
using Project1.Core.Entities;
using Project1.Core.Entities.Stats;
using Project1.Core.Resources;
using Project1.Core.Systems.Inventory;

namespace Project1.Core.Components
{
    partial class HaulComponent : EntityComp
    {
        public override EntityCompDef CompDef => EntityCompDefOf.Haul;
        public new class Spec : Spec<HaulComponent> { }

        public override string Name { get; } = "Haul"; 
        InventoryComp Inventory;
        public GameObjectSlot GetSlot()
        {
            return this.Inventory.HaulSlot;
        }
        public GameObject GetObject()
        {
            return this.Inventory.HaulSlot.Object;
        }

        public Animation AnimationHaul = new(AnimationDefOf.Haul) { Weight = 0 };

        internal override void Resolve()
        {
            this.Inventory = this.Owner.GetComponent<InventoryComp>();
            if (this.Inventory == null || this.Inventory.Capacity == 0)
                throw new Exception("HaulComponent requires a parent entity with PersonalInventoryComponent and an inventory of at least size 1");
            
        }
        internal override void InitializeOnce()
        {
            this.Owner.AddResourceModifier(new ResourceRateModifier(ResourceRateModifierDef.HaulingStaminaDrain));
            this.Owner.AddStatModifier(new StatNewModifier(StatModifierDef.WalkSpeedHaulingWeight));
            this.AnimationHaul = this.Owner.SpriteComp.AddAnimation(AnimationDefOf.Haul, weight: 0);
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