using System;
using Microsoft.Xna.Framework;
using Start_a_Town_.Animations;

namespace Start_a_Town_
{
    class UseHauledOnTargetNew : Interaction
    {
        int Amount;

        public UseHauledOnTargetNew()
             : this(-1)
        {
        }

        public UseHauledOnTargetNew(int amount) // -1 means whole stack
            : base(
            "UseHauledOnTarget", .4f)
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
            if(hauledObj == null)
            {
                actor.Net.ConsoleBox.Write(actor.Name + " tried to drop hauled object but was wan't hauling anything");
                this.State = States.Failed;
                return;
            }
            if (this.Amount > hauledObj.StackSize)
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
                    var amount = (this.Amount == -1) ? hauledObj.StackSize : this.Amount;
                    var transferAmount = Math.Min(o.StackAvailableSpace, amount); // do i want to check this here? 
                    if (o.StackSize + transferAmount > o.StackMax)
                        throw new Exception();
                    if (!o.CanAbsorb(hauledObj, transferAmount))
                        throw new Exception();
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
        [Obsolete]
        public bool InRange(Actor a, TargetArgs t)
        {
            var actorCoords = a.Global;
            var actorBox = new BoundingBox(actorCoords - new Vector3(1, 1, 0), actorCoords + new Vector3(1, 1, a.Physics.Height));
            var targetBox = new BoundingBox(t.Global - Vector3.One, t.Global + Vector3.One);
            return actorBox.Intersects(targetBox);
        }
        public override object Clone()
        {
            return new UseHauledOnTargetNew(this.Amount);
        }
        protected override void WriteExtra(System.IO.BinaryWriter w)
        {
            w.Write(this.Amount);
        }
        protected override void ReadExtra(System.IO.BinaryReader r)
        {
            this.Amount = r.ReadInt32();
        }
        protected override void AddSaveData(SaveTag tag)
        {
            tag.Add(this.Amount.Save("Amount"));
        }
        public override void LoadData(SaveTag tag)
        {
            tag.TryGetTagValue<int>("Amount", out this.Amount);
        }
    }
}
