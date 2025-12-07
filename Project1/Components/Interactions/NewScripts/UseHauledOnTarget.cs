using System;
using Start_a_Town_.Animations;

namespace Start_a_Town_
{
    class UseHauledOnTarget : Interaction
    {
        int Amount;

        public UseHauledOnTarget()
             : this(-1)
        {
     
        }

        public UseHauledOnTarget(int amount = -1) // -1 means whole stack
            : base("UseHauledOnTarget", .4f)
        {
            if (amount == 0)
                throw new Exception();
            this.Amount = amount;
            this.Animation = new Animation(AnimationDef.TouchItem);
            this.CrossFadeAnimationLength = 25;
        }

        public override void Perform()
        {
            var actor = this.Actor;
            var target = this.Target;
            var hauled = actor.Inventory.HaulSlot;// PersonalInventoryComponent.GetHauling(actor);
            var hauledObj = hauled.Object;
            if (this.Amount > hauledObj.StackSize) //thrown
                throw new Exception();
            this.Animation.FadeOutAndRemove();
            switch (target.Type)
            {
                case TargetType.Position:
                    actor.Map.GetBlock(target.Global).OnDrop(actor, hauledObj, target, this.Amount == -1 ? hauledObj.StackSize : this.Amount);
                    actor.CurrentTask?.AddPlacedObject(hauledObj);
                    break;

                case TargetType.Entity:
                    var o = target.Object;
                    if (o.StackSize + this.Amount > o.StackMax)
                        throw new Exception();
                    var amount = (this.Amount == -1) ? hauledObj.StackSize : this.Amount;
                    if (!o.CanAbsorb(hauledObj, amount))
                        throw new Exception();
                    var transferAmount = Math.Min(o.StackAvailableSpace, amount);
                    o.StackSize += transferAmount;
                    hauledObj.StackSize -= transferAmount;
                    break;

                default:
                    break;
            }
        }
        
        // TODO: make it so i have access to the carried item's stacksize, and include it in the name ( Throw 1 vs Throw 16 for example)
        public override string ToString()
        {
            return this.Name + (this.Amount != -1 ? " x" + this.Amount.ToString() : "All");
        }
        
        public override object Clone()
        {
            return new UseHauledOnTarget(this.Amount);
        }
        protected override void WriteExtra(System.IO.BinaryWriter w)
        {
            w.Write(this.Amount);
        }
        protected override void ReadExtra(IDataReader r)
        {
            this.Amount = r.ReadInt32();
        }
        protected override void AddSaveData(SaveTag tag)
        {
            tag.Add(this.Amount.Save("Amount"));
        }
        public override void LoadData(SaveTag tag)
        {
            tag.TryGetTagValueOrDefault<int>("Amount", out this.Amount);
        }
    }
}
