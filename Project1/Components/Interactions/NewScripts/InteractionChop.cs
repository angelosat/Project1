using Microsoft.Xna.Framework;
using Start_a_Town_.Components;
using Start_a_Town_.Net;
using System;
using System.Collections.Generic;

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
        }
    }
    class InteractionChop : InteractionToolUse
    {
        Resource HitPoints => this.Target.Object.GetResource(ResourceDefOf.HitPoints);
        Plant Plant => this.Target.Object as Plant;

        protected override float WorkDifficulty => this.Plant.PlantComponent.Species.StemMaterial.Density;
        //protected override float Progress => 1 - this.HitPoints.Percentage;
        protected override SkillAwardTypes SkillAwardType { get; } = SkillAwardTypes.OnSwing;

        public InteractionChop() : base("Chopping")
        {

        }

        public override object Clone()
        {
            throw new NotImplementedException();
        }

        protected override void OnApplyWork(int workAmount)
        {
            this.Def.Logic.ApplyWork(this.Context, workAmount);
            //this.HitPoints.Value -= workAmount;
            //this.Plant.PlantComponent.Wiggle((float)Math.PI / 32f, 20, this.Plant.PlantComponent.Species.StemMaterial.Density);
        }

        protected override void Done()
        {
            var plant = this.Plant;
            var comp = plant.PlantComponent;
            comp.Harvest(plant, this.Actor);
            comp.ChopDown(plant, this.Actor);
        }
        [EnsureStaticCtorCall]
        static class PacketChopDown
        {
            static readonly int _packetTypeId;
            static PacketChopDown()
            {
                _packetTypeId = Registry.PacketHandlers.Register(Receive);
            }
            static public void Send(Actor actor, Plant plant)
            {
                var server = actor.Net as Server;
                server.BeginPacket(_packetTypeId)
                    .Write(actor.RefId)
                    .Write(plant.RefId);
            }
            private static void Receive(NetEndpoint endpoint, Packet packet)
            {
                var client = endpoint as Client;
                var r = packet.PacketReader;
                var actor = client.World.GetEntity<Actor>(r.ReadInt32());
                var plant = client.World.GetEntity<Plant>(r.ReadInt32());
                plant.PlantComponent.Harvest(plant, actor);
                plant.PlantComponent.ChopDown(plant, actor);
            }
        }

        protected override Color GetParticleColor()
        {
            return this.Plant.PlantComponent.Species.StemMaterial.Color;
        }

        protected override List<Rectangle> GetParticleRects()
        {
            return ItemContent.LogsGrayscale.AtlasToken.Rectangle.Divide(25);
        }

        protected override SkillDef GetSkill()
        {
            return SkillDefOf.Plantcutting;
        }

        protected override ToolUseDef GetToolUse()
        {
            return ToolUseDefOf.Chopping;
        }
    }
}
