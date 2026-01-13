using System;

namespace Start_a_Town_
{
    class InteractionHaulLogic : InteractionLogic
    {
        public sealed class Context : InteractionContext
        {

        }
        protected override Context CreateContextInternal() => new Context();
        public override bool CanPerform(InteractionContext ctx)
        {
            if (ctx.Target.Object.Map != ctx.Actor.Map)
                return false;
            return true;
        }
        public override bool CanFinish(InteractionContext ctx)
        {
            return this.CanPerform(ctx);
        }
        internal override void OnFinish(Interaction i)
        {
            var actor = i.Context.Actor;
            var target = i.Context.Target;
            var count = i.Context.Count;
            if (actor.Net.IsClient) return;
            actor.Inventory.HaulNew(target.Object, count);
        }
    }
    class InteractionHaul : InteractionPerpetual
    {
        int Amount;
        public InteractionHaul()
            : this(-1)
        {
        }
        public InteractionHaul(int amount)
            : base("Haul")
        {
            //this.AnimationDef = AnimationDef.TouchItem;
            this.Amount = amount;
            this.CrossFadeAnimationLength = 25;
        }

        public override string ToString() => "Haul " + (this.Amount == -1 ? " All" : " x" + this.Amount.ToString());
        

        protected override void OnStart()
        {
            var a = this.Actor;
        }
        protected override void Done()
        {
            if (this.Actor.Net.IsClient) return;
            var actor = this.Actor;
            var target = this.Target;
            if (target.Object is Actor)
                throw new Exception();
            switch (target.Type)
            {
                //case TargetType.Position:
                //    // check if hauling and drop at target position
                //    GameObject held = actor.GetComponent<HaulComponent>().Holding.Take();

                //    if (held == null)
                //        break;
                //    held.Spawn(actor.Net.Map, target.FinalGlobal);
                //    break;

                // new: if inventoryable insert to inventory, if carryable carry
                // dont carry inventoriables (test)
                case TargetType.Entity:
                    actor.Inventory.HaulNew(target.Object, this.Amount);
                    break;


                default:
                    break;
            }
            this.Finish();
        }
        protected override void WriteExtra(IDataWriter w)
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
