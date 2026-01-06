using Start_a_Town_.Components;
using System;

namespace Start_a_Town_
{
    class InteractionChopLogic : InteractionLogic
    {
        public class Context : InteractionContext
        {
            Resource _hp;
            public Resource HitPoints => this._hp ??= this.Target.Object.GetResource(ResourceDefOf.HitPoints);
            PlantComponent _plantComp;
            public PlantComponent PlantComp => this._plantComp ??= this.Target.Object.GetComponent<PlantComponent>();
            public override float ProgressPercentage => 1 - this.HitPoints.Percentage;
        }
        protected override InteractionContext CreateContextInternal() => new Context();
        public override bool CanPerform(InteractionContext ctx) => this.CanPerform((Context)ctx);
        public override bool CanFinish(InteractionContext ctx) => this.CanFinish((Context)ctx);
        public override bool WillFinish(InteractionContext ctx, int workAmount) => this.WillFinish((Context)ctx, workAmount);
        public override void ApplyWork(InteractionContext ctx, int workAmount) => this.ApplyWork((Context)ctx, workAmount);
        bool CanPerform(Context ctx)
        {
            var plantTarget = ctx.Target;
            if (plantTarget.Object.Map != ctx.Actor.Map)
                return false;
            if (!ctx.Actor.Map.Town.DesignationManager.IsDesignation(plantTarget, DesignationDefOf.Chop))
                return false;
            return true;
        }
        bool CanFinish(Context ctx)
        {
            return this.CanPerform(ctx);
        }
        bool WillFinish(Context ctx, int workAmount)
        {
            return ctx.HitPoints.Value - workAmount <= 0;
        }
        void ApplyWork(Context ctx, int workAmount)
        {
            ctx.HitPoints.Value -= workAmount;
            ctx.PlantComp.Wiggle((float)Math.PI / 32f, 20, ctx.PlantComp.Species.StemMaterial.Density);
            ctx.Target.Map.Events.Post(new PlantChoppedEvent(ctx.Actor, ctx.Target, workAmount));
        }
        internal override void OnFinish(InteractionContext ctx)
        {
            var plant = ctx.Target.Object;
            var comp = plant.GetComponent<PlantComponent>();
            comp.Harvest(plant, ctx.Actor);
            comp.ChopDown(plant, ctx.Actor);
        }
    }
}
