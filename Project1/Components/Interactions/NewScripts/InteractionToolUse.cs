using Microsoft.Xna.Framework;
using Start_a_Town_.Net;
using Start_a_Town_.Particles;
using System;
using System.Collections.Generic;
using static Start_a_Town_.GlobalVars;

namespace Start_a_Town_
{
    abstract class InteractionToolUse : InteractionPerpetual
    {
        protected enum SkillAwardTypes { OnSwing, OnFinish }
        protected ParticleEmitterSphere EmitterStrike;
        protected List<Rectangle> ParticleRects;
        float TotalWorkAmount;
        protected InteractionToolUse(string name) : base(name)
        {
            this.DrawProgressBar(() => this.Actor.Global, () => this.Progress, () => this.Name);
        }
        protected sealed override void Start()
        {
            var a = this.Actor;
            var t = this.Target;
            this.Animation.Speed = StatDefOf.WorkSpeed.GetValue(a);
            this.Init();
            var particleColor = this.GetParticleColor();
            this.EmitterStrike = new ParticleEmitterSphere
            {
                Source = t.Global + Vector3.UnitZ,
                SizeBegin = 1,
                SizeEnd = 1,
                ParticleWeight = 1,
                Radius = 1f,// .5f;
                Force = .1f,
                Friction = .5f,
                AlphaBegin = 1,
                AlphaEnd = 0,
                ColorBegin = particleColor,
                ColorEnd = particleColor,
                Lifetime = Ticks.PerSecond * 2,
                Rate = 0
            };
            this.ParticleRects = GetParticleRects();
        }

        public sealed override void OnUpdate()
        {
            var actor = this.Actor;
            var t = this.Target;
            if (actor.Net.IsClient && this.ParticleRects is not null)
            {
                this.EmitterStrike.Emit(ItemContent.LogsGrayscale.AtlasToken.Atlas.Texture, this.ParticleRects, Vector3.Zero);
                actor.Map.ParticleManager.AddEmitter(this.EmitterStrike);
                return; /// TODO: separate logic from server and client
            }
            var toolEffect = GetToolEffectiveness();
            var amount = (int)Math.Max(1, toolEffect / WorkDifficulty);

            // apply work authoritatively and sync it to clients
            // TODO should i separate logic from cosmetic effects?

            this.ApplyWorkAndSync(amount);

            var skill = this.GetSkill();

            if (this.SkillAwardType == SkillAwardTypes.OnSwing)
                actor.Skills.AwardAndSync(skill, amount);

            this.ConsumeEnergyAndSync(actor, amount, skill);

            /// i moved the multiplication with the stamina threshold to inside the workspeed stat formula
            this.Animation.Speed = actor[StatDefOf.WorkSpeed];
            this.Animation.Sync();
            //Animation.Packets.SyncAnimation(actor, this.Animation);
            
            if (this.Progress < 1)
                return;

            if (this.SkillAwardType == SkillAwardTypes.OnFinish)
                actor.Skills.AwardAndSync(skill, this.TotalWorkAmount);

            this.Done();
            this.Finish();
        }

        private void ConsumeEnergyAndSync(Actor actor, int amount, SkillDef skill)
        {
            var energyConsumption = this.GetEnergyConsumption(amount, actor.Skills[skill].Level); //amount / a.Skills[skill].Level;
            ConsumeEnergy(actor, energyConsumption);
        }

        //private void AwardSkillAndSync(Actor actor, int amount, out SkillDef skill)
        //{
        //    skill = this.GetSkill();
        //    if (this.SkillAwardType == SkillAwardTypes.OnSwing)
        //        actor.Skills.AwardAndSync(skill, amount);
        //}

        private void ApplyWorkAndSync(int amount)
        {
            this.ApplyWork(amount);
            Packets.SyncApplyWork(this.Actor, amount);
        }

        private void ApplyWork(int amount)
        {
            this.OnApplyWork(amount);
            this.TotalWorkAmount += amount;
        }

        private static void ConsumeEnergy(Actor a, float energyConsumption)
        {
            var stamina = a.Resources[ResourceDefOf.Stamina];
            stamina.Adjust(-energyConsumption);
            a.Resources.AdjustAndSync(ResourceDefOf.Stamina, -energyConsumption);
            a[AttributeDefOf.Strength].Award(a, energyConsumption);
        }

        protected virtual float GetToolEffectiveness()
        {
            if (this.Actor.Gear.GetGear(GearType.Mainhand) is Tool tool && tool.ToolComponent.Props.ToolUse == this.GetToolUse())
                return tool[StatDefOf.ToolEffectiveness];
            else
                return this.Actor.GetMaterial(BoneDefOf.RightHand).Density;
        }
        protected virtual float GetEnergyConsumption(float workAmount, int skillLevel)
        {
            var toolWeight = this.Actor[GearType.Mainhand]?.TotalWeight ?? 1;
            var strength = this.Actor[AttributeDefOf.Strength].Level;
            var fromToolWeight = //10 * 
                toolWeight / strength;
            return fromToolWeight;
        }

        protected abstract float Progress { get; }
        protected abstract float WorkDifficulty { get; }
        protected abstract SkillAwardTypes SkillAwardType { get; }
        protected virtual void Init() { }
        protected abstract void OnApplyWork(float workAmount);
        protected abstract void Done();
        protected abstract ToolUseDef GetToolUse();
        protected abstract SkillDef GetSkill();
        protected abstract List<Rectangle> GetParticleRects();
        protected abstract Color GetParticleColor();
        [EnsureStaticCtorCall]
        static class Packets
        {
            static int _pTypeId;
            static Packets()
            {
                _pTypeId = Registry.PacketHandlers.Register(Receive);
            }
            internal static void SyncApplyWork(Actor actor, int amount)
            {
                var server = actor.Net as Server;
                server.BeginPacket(_pTypeId)
                    .Write(actor.RefId)
                    .Write(amount);
            }
            private static void Receive(NetEndpoint endpoint, Packet packet)
            {
                var client = endpoint as Client;
                var r = packet.PacketReader;
                var actor = client.World.GetEntity(r.ReadInt32()) as Actor;
                var amount = r.ReadInt32();
                var task = actor.Work.Task as InteractionToolUse;
                task.ApplyWork(amount);
            }
        }
    }
}