using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Project1.Core.Animations;
using Project1.Core.Attributes;
using Project1.Core.Entities;
using Project1.Core.Entities.Actors;
using Project1.Core.Gear;
using Project1.Core.Resources;
using Project1.Core.Simulation;
using Project1.Core.Stats;
using Project1.Core.Systems.Tools;
using Project1.Framework;
using Project1.Framework.Helpers;
using Project1.Framework.Serialization;
using Project1.Framework.UI;
using System;

namespace Project1.Core.Interactions
{
    public sealed class Interaction : Inspectable
    {
        public InteractionDef Def;
        public InteractionContext Context;
        public override string LabelReadable => this.Name;
        public static readonly float DefaultRange = (float)Math.Sqrt(2);
        public bool IsFinished => this.State == States.Finished || this.State == States.Failed;
        bool CanPerform() => this.Def.Logic.CanPerform(this.Context);
        bool CanFinish() => this.Def.Logic.CanFinish(this.Context);
        
        //public void Initialize() => this.OnInitialize(this.Actor, this.Target); 
        //protected virtual void OnInitialize(Actor actor, TargetArgs target) { }
        public override string ToString()
            => $"{this.Def.LabelReadable}";
        public enum States { Unstarted, Running, Failed, Succeeded, Finishing, Finished}
        public enum RunningTypes { Once, Continuous }
        public States State { get; private set; } = States.Unstarted;

        public RunningTypes RunningType = RunningTypes.Once;

        public string Name { get; set; }
        public string Verb { get; set; }

        public float Length { get; set; }
        public float CurrentTick;
        public float Seconds { get; set; }
        Animation _cachedAnimation;
        Animation CachedAnimation => _cachedAnimation ??= this.Actor.SpriteComp.GetAnimation(this.AnimationDef);// = new(AnimationDef.Work);
        public AnimationDef AnimationDef => this.Def.Animation;// = AnimationDefOf.Work;
        internal Actor Actor => this.Context.Actor;
        internal InteractionTarget Target => this.Context.Target;
        //internal int Count;

        private bool _drawProgressBar;
        public Func<Vector3> BarPosition;
        public Func<float> BarProgress;

        public readonly ProgressInt Progress = new(100);
        public float ProgressPercentage => this.Def.Controller?.GetProgressBarPercentage(this) ?? this.Progress.Percentage;

        // TODO: i need a method that returns satisfaction score based on ai entity's state
        //static readonly Dictionary<Need.Types, float> _needSatisfaction = new();
        //public Dictionary<Need.Types, float> NeedSatisfaction => field ??= new();
        public Interaction()
        {
        }
      
        public void Interrupt(bool success)
        {
            this.State = States.Finishing;
            if (this.AnimationDef is not null)
                this.CachedAnimation?.FadeOutAndRemove();
        }

        int CrossFadeAnimationLength;
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
        public void Tick()
        {
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
            if (this.Actor.Net.IsServer)
            {
                if (this.Def.Logic.HasSucceeded(this))
                {
                    this.Def.Logic.OnSuccess(this);
                    this.Finish();
                    return;
                }
                if (this.Def.Logic.HasFailed(this))
                {
                    this.Def.Logic.OnFailure(this);
                    this.Finish();
                    return;
                }
            }
            if (this.Def.Controller?.IsFinished(this) ?? this.State == States.Finished) // TODO: maybe check for failed state too?
            {
                if (this.Actor.Net.IsServer)
                {
                    this.Finish();
                    this.Actor.Map.Events.Post(new InteractionFinishedEvent(this.Actor));
                }
                return;
            }
            this.Def.Controller?.Tick(this);
        }
        public void TickOld()
        {
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
            if (this.Def.Controller?.IsFinished(this) ?? this.State == States.Finished) // TODO: maybe check for failed state too?
            {
                if (this.Actor.Net.IsServer)
                {
                    this.Finish();
                    this.Actor.Map.Events.Post(new InteractionFinishedEvent(this.Actor));
                }
                return;
            }
            this.Def.Controller?.Tick(this);
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
        void Fail()
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
     
        public void DrawUI(SpriteBatch sb, Camera camera)
        {
            var actor = this.Actor;
            Bar.Draw(
                sb, 
                camera, 
                this.Actor.Global, 
                this.Def.LabelReadable, 
                this.Def.Controller?.GetProgressBarPercentage(this) ?? this.Progress.Percentage, 
                camera.Zoom * .2f,
                this.Def.ProgressBarColor
                );
        }

        public string GetCompletedText(Actor actor, InteractionTarget target)
        {
            return this.Name + ": " + target.ToString();
        }
        public void Write(IDataWriter w)
        {
            w.Write(this.CurrentTick);
            w.Write((int)this.State);
            //this.WriteExtra(w);
        }
        public void Read(IDataReader r)
        {
            this.CurrentTick = r.ReadSingle();
            this.State = (States)r.ReadInt32();
        }
       
        public SaveTag SaveAs(string name = "")
        {
            var tag = new SaveTag(SaveTag.Types.Compound, name);
            tag.Add(this.GetType().FullName.Save("Name"));
            tag.Add(((int)this.State).Save("State"));
            tag.Add(this.CurrentTick.Save("Progress"));
            return tag;
        }

        public static Interaction Load(SaveTag tag)
        {
            var name = (string)tag["Name"].Value;
            var inter = Activator.CreateInstance(Type.GetType(name)) as Interaction;
            tag.TryGetTagValue<int>("State", t => inter.State = (States)t);
            tag.TryGetTagValueOrDefault("Progress", out inter.CurrentTick);
            //inter.LoadData(tag);
            return inter;
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

            if (this.Def.Controller is not null)
                this.Def.Controller.AddProgressFromToolSwing(this, v);
            else
                this.OnAddProgress(v);
            this.Def.Logic.OnProgressAdded(this, v);
        }
        void OnAddProgress(int v)
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
        }

        private static void DegradeTool(Entity tool)
        {
            if (tool is null)
                return;
            tool.Resources.ApplyDelta(ResourceDefOf.Durability , - 1);
        }

        internal void SetNextSwingSpeed(float speed)
        {
            this.CachedAnimation.Speed = speed;
            this.Actor.Map.Events.Post(new InteractionNextSwingSpeedEvent(this.Actor, speed));
        }

        bool WillFinish(int amount) => this.Def.Logic.WillFinish(this.Context, amount);
        float WorkDifficulty { get; } = 1;

        int CalculateWorkAmount()
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
        float CalculateNextSwingSpeed()
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
                workamount = 10;// 100; //60;// 
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
        float GetToolEffectiveness()
        {
            //if (this.Actor.Gear.GetGear(GearType.Mainhand) is Item tool && tool.ToolComponent.ToolProperties.ToolUse == this.GetToolUse())
            if (this.Actor.Gear.GetGear(GearTypeDefOf.Mainhand) is Entity tool && tool.ToolComponent.ToolUse == this.Def.ToolUse)
                return tool[StatDefOf.ToolEffectiveness];
            else
                return this.Actor.GetMaterial(BoneDefOf.RightHand).Density;
        }
        float GetEnergyConsumption(float workAmount, int skillLevel)
        {
            var toolWeight = this.Actor[GearTypeDefOf.Mainhand]?.TotalWeight ?? 1;
            var strength = this.Actor[AttributeDefOf.Strength].Level;
            var fromToolWeight = //10 * 
                toolWeight / strength;
            return fromToolWeight;
        }

        internal void OnAnimationHook()
        {
            this.Def.Logic.OnAnimationHook(this);
        }

        float TotalWorkApplied;

        enum SkillAwardTypes { OnSwing, OnFinish }

        SkillAwardTypes SkillAwardType;
    }
}
