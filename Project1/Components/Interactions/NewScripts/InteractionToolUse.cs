using Microsoft.Xna.Framework;
using Start_a_Town_.Particles;
using System;
using System.Collections.Generic;

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
            var a = this.Actor;
            var t = this.Target;
            if (a.Net is Net.Client && this.ParticleRects is not null)
            {
                this.EmitterStrike.Emit(ItemContent.LogsGrayscale.AtlasToken.Atlas.Texture, this.ParticleRects, Vector3.Zero);
                a.Map.ParticleManager.AddEmitter(this.EmitterStrike);
            }
            var toolEffect = GetToolEffectiveness();
            var amount = (int)Math.Max(1, toolEffect / WorkDifficulty);
            this.ApplyWork(amount);
            this.TotalWorkAmount += amount;
            var skill = this.GetSkill();
            if (this.SkillAwardType == SkillAwardTypes.OnSwing)
                a[skill].Award(amount);
            var energyConsumption = this.GetEnergyConsumption(amount, a.Skills[skill].Level); //amount / a.Skills[skill].Level;

            ConsumeEnergy(a, energyConsumption);
            /// i moved the multiplication with the stamina threshold to inside the workspeed stat formula
            this.Animation.Speed = a[StatDefOf.WorkSpeed];

            if (this.Progress < 1)
                return;
            if (this.SkillAwardType == SkillAwardTypes.OnFinish)
                a[skill].Award(this.TotalWorkAmount);
            this.Done();
            this.Finish();
        }

        private static void ConsumeEnergy(Actor a, float energyConsumption)
        {
            var stamina = a.Resources[ResourceDefOf.Stamina];
            stamina.Adjust(-energyConsumption);
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
        protected abstract void ApplyWork(float workAmount);
        protected abstract void Done();
        protected abstract ToolUseDef GetToolUse();
        protected abstract SkillDef GetSkill();
        protected abstract List<Rectangle> GetParticleRects();
        protected abstract Color GetParticleColor();
    }
}