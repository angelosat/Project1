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
        protected virtual Progress ProgressNew { get; }
        protected InteractionToolUse(string name) : base(name)
        {
            this.DrawProgressBar(() => this.Actor.Global, () => this.Progress, () => this.Name);
        }
        protected sealed override void Start()
        {
            var a = this.Actor;
            var t = this.Target;
            this._animation.Speed = StatDefOf.WorkSpeed.GetValue(a);
            //this.Init();
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
       
        public sealed override void OnUpdate()
        {
            if (!this.CanPerform())
            {
                this.Fail();
                return;
            }
            if (this.Actor.Net.IsClient)
                return;
            var actor = this.Actor;
            var t = this.Target;
            var toolEffect = GetToolEffectiveness();
            var amount = (int)Math.Max(1, toolEffect / WorkDifficulty);
            if(this.WillFinish(amount) && !this.CanFinish())
            {
                this.Fail();
                return;
            }
            if (actor.Net.IsClient && this.ParticleRects is not null)
            {
                this.EmitterStrike.Emit(this.ParticleRects, Vector3.Zero);
                actor.Map.ParticleManager.AddEmitter(this.EmitterStrike);
            }

            this.OnApplyWork(amount);
            this.TotalWorkAmount += amount;

            var skill = this.GetSkill();

            if (this.SkillAwardType == SkillAwardTypes.OnSwing)
                //actor.Skills.Adjust(skill, amount);
                actor.Skills.Increase(skill, amount);


            var energyConsumption = this.GetEnergyConsumption(amount, actor.Skills[skill].Level); //amount / a.Skills[skill].Level;

            // "transfer" energy from stamina to strength
            //actor.Attributes.Adjust(AttributeDefOf.Strength, energyConsumption);
            //actor.Resources.Adjust(ResourceDefOf.Stamina, -energyConsumption);
            actor.Attributes.AdjustAndSync(AttributeDefOf.Strength, energyConsumption);
            actor.Resources.AdjustAndSync(ResourceDefOf.Stamina, -energyConsumption);

            // i moved the multiplication with the stamina threshold to inside the workspeed stat formula
            this._animation.Speed = actor[StatDefOf.WorkSpeed];

            if (this.Progress < 1)
                return;

            if (this.SkillAwardType == SkillAwardTypes.OnFinish)
            {
                throw new NotImplementedException();
                //actor.Skills.Increase(skill, this.TotalWorkAmount);
            }
            this.Done();
            this.Finish();
        }

        bool WillFinish(int amount) => this.Def.Logic.WillFinish(this.Context, amount);

        private void ApplyWork(int amount)
        {
            this.OnApplyWork(amount);
            this.TotalWorkAmount += amount;
        }

       
        protected virtual float GetToolEffectiveness()
        {
            //if (this.Actor.Gear.GetGear(GearType.Mainhand) is Item tool && tool.ToolComponent.ToolProperties.ToolUse == this.GetToolUse())
            if (this.Actor.Gear.GetGear(GearType.Mainhand) is Item tool && tool.ToolComponent.ToolUse == this.GetToolUse())
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

        //protected abstract float Progress { get; }
        protected float Progress => this.Context.ProgressPercentage;
        protected abstract float WorkDifficulty { get; }
        protected SkillAwardTypes SkillAwardType;//{ get; }
        protected virtual void Init() { }
        protected abstract void OnApplyWork(int workAmount);
        protected abstract void Done();
        protected abstract ToolUseDef GetToolUse();
        protected abstract SkillDef GetSkill();
        protected abstract List<Rectangle> GetParticleRects();
        protected abstract Color GetParticleColor();
        //[EnsureStaticCtorCall]
        //static class Packets
        //{
        //    static int _pTypeId, _pTypeOnUpdate;
        //    static Packets()
        //    {
        //        _pTypeId = Registry.PacketHandlers.Register(Receive);
        //    }
        //    internal static void SyncOnUpdate(Actor actor, int amount)
        //    {
        //        var server = actor.Net as Server;
        //        server.BeginPacket(_pTypeOnUpdate)
        //            .Write(actor.RefId)
        //            .Write(amount);
        //    }
        //    internal static void SyncApplyWork(Actor actor, int amount)
        //    {
        //        var server = actor.Net as Server;
        //        server.BeginPacket(_pTypeId)
        //            .Write(actor.RefId)
        //            .Write(amount);
        //    }
        //    private static void Receive(NetEndpoint endpoint, Packet packet)
        //    {
        //        var client = endpoint as Client;
        //        var r = packet.PacketReader;
        //        var actor = client.World.GetEntity(r.ReadInt32()) as Actor;
        //        var amount = r.ReadInt32();
        //        var task = actor.Work.Task as InteractionToolUse;
        //        task.ApplyWork(amount);
        //    }
        //}
    }
}