using Microsoft.Xna.Framework;
using Project1.Core.Stats;
using Project1.Core.Base;
using Project1.Core.Graphics.Particles;
using Project1.Core.Helpers;
using Project1.Core.Interactions;
using System;
using System.Collections.Generic;

namespace Project1.Core.Interactions
{
    abstract class InteractionToolUse : InteractionPerpetual
    {
        //protected enum SkillAwardTypes { OnSwing, OnFinish }
        protected ParticleEmitterSphere EmitterStrike;
        protected List<Rectangle> ParticleRects;
        //protected float TotalWorkApplied;
        protected virtual Progress ProgressNew { get; }
        protected InteractionToolUse(string name) : base(name)
        {
            this.DrawProgressBar(() => this.Actor.Global, () => this.ProgressPercentage, () => this.Name);
        }
        protected void OnStart()
        {
            var a = this.Actor;
            var t = this.Target;
            this.CachedAnimation.Speed = StatDefOf.WorkSpeed.CalculateFor(a);
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
            this.Init();

        }
        //internal void OnToolContactOld()
        //{
        //    if (this.Actor.Net.IsClient)
        //        return;
        //    if (!this.CanPerform())
        //    {
        //        this.Fail();
        //        return;
        //    }
        
        //    var actor = this.Actor;
        //    var toolEffect = GetToolEffectiveness();
        //    var amount = (int)Math.Max(1, toolEffect / WorkDifficulty);
        //    if(this.WillFinish(amount) && !this.CanFinish())
        //    {
        //        this.Fail();
        //        return;
        //    }
        //    var skill = this.GetSkill();

        //    this.AddProgress(amount);
        //    this.TotalWorkApplied += amount;

        //    if (this.SkillAwardType == SkillAwardTypes.OnSwing)
        //        actor.Skills.Increase(skill, amount);

        //    var energyConsumption = this.GetEnergyConsumption(amount, actor.Skills[skill].Level); 

        //    // "transfer" energy from stamina to strength
        //    actor.Attributes.Adjust(AttributeDefOf.Strength, energyConsumption);
        //    actor.Resources.Adjust(ResourceDefOf.Stamina, -energyConsumption);

        //    // i moved the multiplication with the stamina threshold to inside the workspeed stat formula
        //    this.CachedAnimation.Speed = actor[StatDefOf.WorkSpeed];

        //    if(!this.Progress.IsFinished)
        //        return;

        //    if (this.SkillAwardType == SkillAwardTypes.OnFinish)
        //    {
        //        //throw new NotImplementedException();
        //        actor.Skills.Increase(skill, (int)this.TotalWorkApplied);
        //    }
        //    this.Done();
        //    this.Finish();
        //}

        bool WillFinish(int amount) => this.Def.Logic.WillFinish(this.Context, amount);

        //protected virtual float GetToolEffectiveness()
        //{
        //    //if (this.Actor.Gear.GetGear(GearType.Mainhand) is Item tool && tool.ToolComponent.ToolProperties.ToolUse == this.GetToolUse())
        //    if (this.Actor.Gear.GetGear(GearType.Mainhand) is Item tool && tool.ToolComponent.ToolUse == this.GetToolUse())
        //        return tool[StatDefOf.ToolEffectiveness];
        //    else
        //        return this.Actor.GetMaterial(BoneDefOf.RightHand).Density;
        //}
        //protected virtual float GetEnergyConsumption(float workAmount, int skillLevel)
        //{
        //    var toolWeight = this.Actor[GearType.Mainhand]?.TotalWeight ?? 1;
        //    var strength = this.Actor[AttributeDefOf.Strength].Level;
        //    var fromToolWeight = //10 * 
        //        toolWeight / strength;
        //    return fromToolWeight;
        //}

        //protected virtual float Progress => this.Context.ProgressPercentage;
        //protected abstract float WorkDifficulty { get; }
        //protected SkillAwardTypes SkillAwardType;//{ get; }
        protected virtual void Init() { }
        [Obsolete($"use {nameof(this.OnAddProgress)}")]
        protected virtual void OnApplyWork(int workAmount) { }
        //protected virtual void Done() { }
        //protected abstract ToolUseDef GetToolUse();
        //protected abstract SkillDef GetSkill();
        protected abstract List<Rectangle> GetParticleRects();
        protected abstract Color GetParticleColor();
        
    }
}