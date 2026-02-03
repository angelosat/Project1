using System;
using Microsoft.Xna.Framework;
using Project1.Framework.Interactions;
using Project1.Framework.WorldGen;
using Start_a_Town_;

namespace Project1.Core.Interactions
{
    class InteractionPlaceItemLogic : InteractionLogic
    {
        class Context : InteractionContext
        {
            Cell _cachedCell;
            internal Cell Cell => _cachedCell ??= this.Target.Map.GetCell(this.Target.Global.Below());
        }
        protected override InteractionContext CreateContextInternal() => new Context();
        public override bool CanPerform(InteractionContext ctx) => ((Context)ctx).Cell.IsSolid();
        public override bool CanFinish(InteractionContext ctx) => this.CanPerform(ctx);
        internal override void OnFinish(Interaction i)
        {
            var ctx = i.Context;
            var actor = ctx.Actor;
            if (actor.Net.IsClient)
                return;
            var global = ctx.Target.Global;
            var count = ctx.Count;
            var hauled = actor.Hauled;
            ArgumentNullException.ThrowIfNull(hauled);
            if (count > hauled.StackSize)
                throw new Exception();
            InteractionHelpers.TryDepositCarriedItemInsideBlockOrSpawn(actor, global, count);
        }
    }
    //class InteractionPlaceItem : InteractionPerpetual
    //{
    //    int Amount;

    //    public InteractionPlaceItem()
    //         : this(-1)
    //    {
    //    }

    //    public InteractionPlaceItem(int amount) // -1 means whole stack
    //        : base(
    //        "UseHauledOnTarget")//, .4f)
    //    {
    //        if (amount == 0)
    //            throw new Exception();
    //        this.Amount = amount;
    //        this.CrossFadeAnimationLength = 25;
    //    }
    //    protected override void Done()
    //    {
    //        this.CachedAnimation.FadeOutAndRemove();
    //        if (this.Actor.Net.IsClient)
    //            return;
    //        var actor = this.Actor;
    //        var target = this.Target;
    //        var hauled = actor.Inventory.HaulSlot;// PersonalInventoryComponent.GetHauling(actor);
    //        var hauledObj = hauled.Object as Entity;
    //        ArgumentNullException.ThrowIfNull(hauledObj);
    //        if (this.Amount > hauledObj.StackSize)
    //            throw new Exception();
    //        //this.Animation.FadeOutAndRemove();
    //        var global = target.Global;
    //        switch (target.Type)
    //        {
    //            case TargetType.Position:
    //                InteractionHelpers.TryDepositCarriedItemInsideBlockOrSpawn(actor, global, this.Amount);
    //                //if (actor.Map.GetBlockEntity(global)?.TryConsume(hauledObj) ?? false)
    //                //    return;
    //                //if (actor.Map.GetBlock(global).TryConsume(actor, hauledObj, global, this.Amount == -1 ? hauledObj.StackSize : this.Amount))
    //                //    return;
    //                //actor.Map.Spawn(hauledObj, global, actor.Velocity);
    //                break;

    //            case TargetType.Entity:
    //                throw new NotImplementedException();

    //            default:
    //                break;
    //        }
    //        this.Finish();

    //    }

    //    // TODO: make it so i have access to the carried item's stacksize, and include it in the name ( Throw 1 vs Throw 16 for example)
    //    public override string ToString()
    //    {
    //        return this.Name + (this.Amount != -1 ? " x" + this.Amount.ToString() : "All");
    //    }
    //    [Obsolete]
    //    public bool InRange(Actor a, TargetArgs t)
    //    {
    //        var actorCoords = a.Global;
    //        var actorBox = new BoundingBox(actorCoords - new Vector3(1, 1, 0), actorCoords + new Vector3(1, 1, a.Physics.Height));
    //        var targetBox = new BoundingBox(t.Global - Vector3.One, t.Global + Vector3.One);
    //        return actorBox.Intersects(targetBox);
    //    }
    //    protected override void WriteExtra(IDataWriter w)
    //    {
    //        w.Write(this.Amount);
    //    }
    //    protected override void ReadExtra(IDataReader r)
    //    {
    //        this.Amount = r.ReadInt32();
    //    }
    //    protected override void AddSaveData(SaveTag tag)
    //    {
    //        tag.Add(this.Amount.Save("Amount"));
    //    }
    //    public override void LoadData(SaveTag tag)
    //    {
    //        tag.TryGetTagValueOrDefault<int>("Amount", out this.Amount);
    //    }
    //}
}
