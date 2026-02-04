using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Project1.Core.Gear;
using Project1.Framework.Animations;
using Project1.Framework.Attributes;
using Project1.Framework.Components;
using Project1.Framework.Base;
using Project1.Framework.Needs;
using Project1.Framework.Resources;
using Project1.Framework.Skills;
using Project1.Framework.Stats;
using Start_a_Town_;
using Start_a_Town_.UI;
using System;
using System.Collections.Generic;
using Project1.Framework.WorldGen;
using Project1.Framework.Rendering;
using Project1.Framework.Entities;
using Project1.Framework.UI;
using Project1.Framework.Entities.Actors;

namespace Project1.Framework.Interactions
{
    public class Interaction : Inspectable
    {
        public InteractionDef Def;
        public InteractionContext Context;
        public override string Label => this.Name;
        public static readonly float DefaultRange = (float)Math.Sqrt(2);
        public bool IsFinished => this.State == States.Finished || this.State == States.Failed;
        protected bool CanPerform() => this.Def.Logic.CanPerform(this.Context);
        protected bool CanFinish() => this.Def.Logic.CanFinish(this.Context);
        
        public void Initialize() => this.OnInitialize(this.Actor, this.Target); 
        protected virtual void OnInitialize(Actor actor, TargetArgs target) { }
        public override string ToString()
        {
            return $"Interaction: {this.Name}";
        }
        public enum States { Unstarted, Running, Finished, Failed, Finishing }
        public enum RunningTypes { Once, Continuous }
        public States State { get; protected set; } = States.Unstarted;

        public RunningTypes RunningType = RunningTypes.Once;

        public string Name { get; set; }
        public string Verb { get; set; }

        public float Length { get; set; }
        public float CurrentTick;
        public float Seconds { get; set; }
        Animation _cachedAnimation;
        protected Animation CachedAnimation => _cachedAnimation ??= this.Actor.SpriteComp.GetAnimation(this.AnimationDef);// = new(AnimationDef.Work);
        public AnimationDef AnimationDef => this.Def.Animation;// = AnimationDefOf.Work;
        internal Actor Actor => this.Context.Actor;
        internal TargetArgs Target => this.Context.Target;
        internal int Count;

        private bool _drawProgressBar;
        public Func<Vector3> BarPosition;
        public Func<float> BarProgress;

        public readonly ProgressInt Progress = new(100);
        public float ProgressPercentage => this.Def.ProgressHandler?.GetProgressPercentage(this) ?? this.Progress.Percentage;

        // TODO: i need a method that returns satisfaction score based on ai entity's state
        static readonly Dictionary<Need.Types, float> _needSatisfaction = new();
        public virtual Dictionary<Need.Types, float> NeedSatisfaction => _needSatisfaction;
        public Interaction()
        {
        }
        protected Interaction(string name, float seconds = 0)
            : this()
        {
            this.Name = name;
            this.Seconds = seconds;
            this.CurrentTick = this.Length = seconds * Ticks.PerSecond;
        }

        public virtual void Interrupt(bool success)
        {
            if (!success)
                this.Actor.Net.EventOccured((int)Message.Types.InteractionInterrupted, this.Actor, this);
            this.State = States.Finishing;
            if (this.AnimationDef is not null)
                this.CachedAnimation?.FadeOutAndRemove();
        }

        public virtual void Perform()
        {
        }
        protected int CrossFadeAnimationLength;
        public void Start()
        {
            this.Def.Logic.OnStart(this);
            if (this.AnimationDef is not null)
            {
                this._cachedAnimation =
                    this.Actor.SpriteComp.CrossFade(this.AnimationDef, false, 40); 
                // TODO maybe instead of a magic crossfade number, crossfade until the second keyframe
                if (this.AnimationDef == AnimationDefOf.Tool) // HACK
                {
                    this.Calculate(out _, out _, out var speed);
                    this.SetNextSwingSpeed(speed);
                }
            }
        }
        public void Update()
        {
            var actor = this.Actor;
            var target = this.Target;
            if (this.State == States.Finished)
                return;
            if (this.State == States.Finishing) // TODO: maybe check for failed state too?
            {
                if (this._cachedAnimation.State == AnimationStates.Removed)
                    this.State = States.Finished;
                return;
            }
            if (this.State == States.Unstarted)
            {
                this.Start();
                this.State = States.Running;
                return; // give one tick buffer for insteractions that finish instantly to have a chance to be ticked on clients
            }
            if (this.Def.ProgressHandler?.IsFinished(this) ?? this.State == States.Finished) // TODO: maybe check for failed state too?
            {
                if (this.Actor.Net.IsServer)
                {
                    this.Finish();
                    this.Actor.Map.Events.Post(new InteractionFinishedEvent(this.Actor));
                }
                return;
            }
            if (this.Def.ProgressHandler is not null)
                this.Def.ProgressHandler.Tick(this);
            else this.Perform();
        }
        public void UpdateOld()
        {
            var actor = this.Actor;
            var target = this.Target;
            if (this.State == States.Finished)
                return;
            if (this.State == States.Finishing) // TODO: maybe check for failed state too?
            {
                if (this._cachedAnimation.State == AnimationStates.Finished)
                    this.State = States.Finished;
                return;
            }

            if (this.State == States.Unstarted)
            {
                this.Start();
                this.State = States.Running;
            }
            else if (this.State == States.Finished)
            {
                this.StopAnimation();
                actor.AI.State.Log.Write("Success: " + this.GetCompletedText(actor, target));
                return;
            }
            if (this.RunningType == RunningTypes.Continuous)
            {
                this.Perform();
                if (this.State == States.Finished)
                {
                    this.StopAnimation();
                    actor.AI.State.Log.Write("Success: " + this.GetCompletedText(actor, target));
                }
                return;
            }
            this.CurrentTick--;
            if (this.CurrentTick <= 0)
            {
                this.Finish();
                this.Perform();
            }
        }
        public void Finish()
        {
            this.State = this.AnimationDef is not null ? States.Finishing : States.Finished;
            this.Def.Logic.OnFinish(this);
            this.StopAnimation();
        }
        internal void StopAnimation()
        {
            if (this.AnimationDef is not null)
                this.CachedAnimation.FadeOutAndRemove();// -.01f);
        }
        protected virtual void Fail()
        {
            this.CachedAnimation.FadeOutAndRemove();
            this.State = States.Failed;
        }
        public void GetTooltip(Control tooltip)
        {
            var panel = new PanelLabeled("Interact") { AutoSize = true, Location = tooltip.Controls.BottomLeft };
            panel.Controls.Add(new Label(this.Name + (this.Length > 0 ? TimeSpan.FromMilliseconds(this.Length).TotalSeconds.ToString(" #0.##s") : "")) { Location = panel.Controls.BottomLeft }); //this.Length.ToString("#0.##s")
            tooltip.Controls.Add(panel);
        }
     
        public virtual void DrawUI(SpriteBatch sb, Camera camera)
        {
            var actor = this.Actor;
            Bar.Draw(sb, camera, this.Actor.Global, this.Def.Label, this.Def.ProgressHandler?.GetProgressPercentage(this) ?? this.Progress.Percentage, camera.Zoom * .2f);
        }

        internal virtual void ResolveReferences()
        {
        }

        public virtual string GetCompletedText(Actor actor, TargetArgs target)
        {
            return this.Name + ": " + target.ToString();
        }
        public void Write(IDataWriter w)
        {
            w.Write(this.CurrentTick);
            w.Write((int)this.State);
            this.WriteExtra(w);
        }
        public void Read(IDataReader r)
        {
            this.CurrentTick = r.ReadSingle();
            this.State = (States)r.ReadInt32();
            this.ReadExtra(r);
        }
        protected virtual void WriteExtra(IDataWriter w) { }
        protected virtual void ReadExtra(IDataReader r) { }

        public SaveTag SaveAs(string name = "")
        {
            var tag = new SaveTag(SaveTag.Types.Compound, name);
            tag.Add(this.GetType().FullName.Save("Name"));
            tag.Add(((int)this.State).Save("State"));
            tag.Add(this.CurrentTick.Save("Progress"));
            this.AddSaveData(tag);
            return tag;
        }

        protected virtual void AddSaveData(SaveTag tag) { }
        public virtual void LoadData(SaveTag tag)
        {
        }

        public static Interaction Load(SaveTag tag)
        {
            var name = (string)tag["Name"].Value;
            var inter = Activator.CreateInstance(Type.GetType(name)) as Interaction;
            tag.TryGetTagValue<int>("State", t => inter.State = (States)t);
            tag.TryGetTagValueOrDefault("Progress", out inter.CurrentTick);
            inter.LoadData(tag);
            return inter;
        }
        internal virtual void Resolve(MapBase map)
        {
        }
        internal virtual void FinishAction()
        {
        }
        
        internal virtual void AfterLoad()
        {
            this.CachedAnimation.Entity = this.Actor;
        }

        public void DrawProgressBar(Func<Vector3> position, Func<float> progress, Func<string> label)
        {
            this._drawProgressBar = true;
            this.BarPosition = position;
            this.BarProgress = progress;
        }
        
        internal void AddProgress(int v)
        {
            this.Actor.Map.Events.Post(new InteractionProgressEvent(this.Actor, v));

            if (this.Def.ProgressHandler is not null)
                this.Def.ProgressHandler.AddProgress(this, v);
            else
                this.OnAddProgress(v);
            this.Def.Logic.OnProgressAdded(this, v);
        }
        protected virtual void OnAddProgress(int v)
        {
            this.Progress.ApplyDelta(v);
        }
        internal void OnToolContact()
        {
            if (this.Actor.Net.IsClient)
                return;
            if (!this.CanPerform())
            {
                this.Fail();
                return;
            }

            var actor = this.Actor;
            //var amount = this.CalculateWorkAmount();
            this.Calculate(out var tool, out var amount, out var speed);
            if (this.WillFinish(amount) && !this.CanFinish())
            {
                this.Fail();
                return;
            }
            var skill = this.Def.Skill;

            this.AddProgress(amount);
            DegradeTool(tool);
            this.TotalWorkApplied += amount;
            //this._cachedAnimation.Frame.ToConsole();

            //var speed = InteractionResolverDefOf.WorkSpeed.Worker.Resolve(actor);
            //this.CachedAnimation.Speed = actor[StatDefOf.WorkSpeed];
            this.SetNextSwingSpeed(speed);
            if (skill is not null)
            {
                if (this.SkillAwardType == SkillAwardTypes.OnSwing)
                    actor.Skills.Increase(skill, amount);

                var energyConsumption = this.GetEnergyConsumption(amount, actor.Skills[skill].Level);

                // "transfer" energy from stamina to strength
                actor.Attributes.ApplyDelta(AttributeDefOf.Strength, energyConsumption);
                actor.Resources.ApplyDelta(ResourceDefOf.Stamina, -energyConsumption);
            }
            // i moved the multiplication with the stamina threshold to inside the workspeed stat formula

            if (!this.Progress.IsFinished)
                return;

            if (skill is not null && this.SkillAwardType == SkillAwardTypes.OnFinish)
            {
                //throw new NotImplementedException();
                actor.Skills.Increase(skill, (int)this.TotalWorkApplied);
            }
            //this.Done();
            //this.Finish();
        }

        private static void DegradeTool(Entity tool)
        {
            if (tool is null)
                return;
            tool.Resources[ResourceDefOf.Durability].ApplyDelta(-1);
        }

        internal void SetNextSwingSpeed(float speed)
        {
            this.CachedAnimation.Speed = speed;
            this.Actor.Map.Events.Post(new InteractionNextSwingSpeedEvent(this.Actor, speed));
        }

        bool WillFinish(int amount) => this.Def.Logic.WillFinish(this.Context, amount);
        protected virtual float WorkDifficulty { get; } = 1;

        //protected virtual void Done() => this.Def.Logic?.OnFinish(this);
        protected virtual int CalculateWorkAmount()
        {
            var toolUse = this.Def.ToolUse;
            if (this.Actor.Gear.GetGear(GearTypeDefOf.Mainhand) is not Entity tool)
                return 1;
            var comp = tool.GetComponent<ToolComp>();
            var total = comp.GetWorkValue(toolUse) ?? 1;

            var skill = this.Actor.Skills[toolUse.Skill];
            var skillMult = skill.Level / 100f;
            total *= skillMult;

            total = Math.Max(1, total / this.WorkDifficulty);

            return (int)total;
        }
        protected virtual float CalculateNextSwingSpeed()
        {
            var toolUse = this.Def.ToolUse;
            var actor = this.Actor;
            if (actor.Gear.GetGear(GearTypeDefOf.Mainhand) is not Entity tool)
                return 1;
            var comp = tool.GetComponent<ToolComp>();
            var toolspeed = InteractionResolverDefOf.WorkSpeed.Worker.Resolve(actor);

            var skill = actor.Skills[toolUse.Skill];
            var skillMult = skill.Level / 100f;
            toolspeed /= toolspeed * (1 + skillMult);

            return toolspeed;
        }
        protected void Calculate(out Entity equippedTool, out int workamount, out float speed)
        {
            if (this.Actor.Gear.GetGear(GearTypeDefOf.Mainhand) is not Entity tool || 
                this.Def.ToolUse is not ToolUseDef toolUse)
            {
                workamount = 100; //60;// 
                speed = 1;
                equippedTool = null;
                return;
            }
            //var toolUse = this.Def.ToolUse;
            equippedTool = tool;
            var comp = tool.GetComponent<ToolComp>();
            var total = comp.GetWorkValue(toolUse) ?? 1;

            var skill = this.Actor.Skills[toolUse.Skill];
            var skillMult = skill.Level / 100f;
            total *= 1 + skillMult;

            total = Math.Max(1, total / this.WorkDifficulty);

            var toolspeed = 1 + tool?.Stats[StatDefOf.ToolSpeed] ?? 0;
            toolspeed /= 1 + skillMult;

            workamount = (int)total;
            speed = toolspeed;
        }
        protected virtual float GetToolEffectiveness()
        {
            //if (this.Actor.Gear.GetGear(GearType.Mainhand) is Item tool && tool.ToolComponent.ToolProperties.ToolUse == this.GetToolUse())
            if (this.Actor.Gear.GetGear(GearTypeDefOf.Mainhand) is Entity tool && tool.ToolComponent.ToolUse == this.Def.ToolUse)
                return tool[StatDefOf.ToolEffectiveness];
            else
                return this.Actor.GetMaterial(BoneDefOf.RightHand).Density;
        }
        protected virtual float GetEnergyConsumption(float workAmount, int skillLevel)
        {
            var toolWeight = this.Actor[GearTypeDefOf.Mainhand]?.TotalWeight ?? 1;
            var strength = this.Actor[AttributeDefOf.Strength].Level;
            var fromToolWeight = //10 * 
                toolWeight / strength;
            return fromToolWeight;
        }
        protected float TotalWorkApplied;

        //protected virtual ToolUseDef GetToolUse() => null;
        //protected virtual SkillDef GetSkill() => null;
        protected enum SkillAwardTypes { OnSwing, OnFinish }

        protected SkillAwardTypes SkillAwardType;//{ get; }

    }
}
