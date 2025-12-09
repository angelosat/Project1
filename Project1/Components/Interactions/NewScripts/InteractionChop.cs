using Microsoft.Xna.Framework;
using Start_a_Town_.Net;
using System;
using System.Collections.Generic;

namespace Start_a_Town_
{
    class InteractionChop : InteractionToolUse
    {
        Resource HitPoints => this.Target.Object.GetResource(ResourceDefOf.HitPoints);
        Plant Plant => this.Target.Object as Plant;

        protected override float WorkDifficulty => this.Plant.PlantComponent.PlantProperties.StemMaterial.Density;
        protected override float Progress => 1 - this.HitPoints.Percentage;
        protected override SkillAwardTypes SkillAwardType { get; } = SkillAwardTypes.OnSwing;

        public InteractionChop() : base("Chopping")
        {

        }

        public override object Clone()
        {
            throw new NotImplementedException();
        }

        protected override void ApplyWork(float workAmount)
        {
            this.HitPoints.Value -= workAmount;
            this.Plant.PlantComponent.Wiggle((float)Math.PI / 32f, 20, this.Plant.PlantComponent.PlantProperties.StemMaterial.Density);
        }

        protected override void Done()
        {
            if (this.Actor.Net.IsClient)
                return;
            var plant = this.Plant;
            var comp = plant.PlantComponent;
            comp.Harvest(plant, this.Actor);
            comp.ChopDown(plant, this.Actor);
            PacketChopDown.Send(this.Actor, plant);
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
            return this.Plant.PlantComponent.PlantProperties.StemMaterial.Color;
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
