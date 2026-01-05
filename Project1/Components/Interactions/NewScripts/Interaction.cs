using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Start_a_Town_.Animations;
using Start_a_Town_.Components;
using Start_a_Town_.UI;
using System;
using System.Collections.Generic;

namespace Start_a_Town_
{
    public abstract class Interaction : Inspectable
    {
        public InteractionDef Def;
        public InteractionContext Context;
        public override string Label => this.Name;
        public static readonly float DefaultRange = (float)Math.Sqrt(2);
        public bool IsFinished => this.State == States.Finished || this.State == States.Failed;
        protected bool CanPerform() => this.Def.Logic.CanPerform(this.Context);
        protected bool CanFinish() => this.Def.Logic.CanFinish(this.Context);
        internal virtual void OnToolContact()
        {
        }
        public void Initialize() => this.OnInitialize(this.Actor, this.Target); 
        protected virtual void OnInitialize(Actor actor, TargetArgs target) { }
        public override string ToString()
        {
            return $"Interaction: {this.Name}";
        }
        public enum States { Unstarted, Running, Finished, Failed }
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
        private Func<string> BarLabel;

        public readonly ProgressInt Progress = new(100);
        //public virtual float PercentageComplete => (float)(1 - this.CurrentTick / this.Length);
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
            this.State = States.Finished;
            if (this.AnimationDef is not null)
                this.CachedAnimation?.FadeOutAndRemove();
        }

        public virtual void Perform()
        {
        }
        protected int CrossFadeAnimationLength;
        public void Start()
        {
            if (this.AnimationDef is not null)
            {
                if (this.CrossFadeAnimationLength == 0)
                    this.Actor.SpriteComp.AddAnimation(this.AnimationDef);
                else
                    this.Actor.SpriteComp.CrossFade(this.AnimationDef, false, this.CrossFadeAnimationLength);
            }
            this.OnStart();
        }
        protected virtual void OnStart() { }
        public void Update()
        {
            var actor = this.Actor;
            var target = this.Target;
            if (this.State == States.Unstarted)
            {
                this.Start();
                this.State = States.Running;
            }
            if (this.Def.ProgressHandler?.IsFinished(this) ?? this.State == States.Finished) // TODO: maybe check for failed state too?
            {
                this.Stop();
                return;
            }
            if (this.Def.ProgressHandler is not null)
                this.Def.ProgressHandler.Tick(this);
            else this.Perform();
            
            //else if (this.State == States.Finished)
            //{
            //    this.Stop();
            //    AILog.TryWrite(actor, "Success: " + this.GetCompletedText(actor, target));
            //    return;
            //}
            //if (this.RunningType == RunningTypes.Continuous)
            //{
            //    this.Perform();
            //    if (this.State == States.Finished)
            //    {
            //        this.Stop();
            //        AILog.TryWrite(actor, "Success: " + this.GetCompletedText(actor, target));
            //    }
            //    return;
            //}
            //this.CurrentTick--;
            //if (this.CurrentTick <= 0)
            //{
            //    this.Finish();
            //    this.Stop();
            //    this.Perform();
            //}
        }
        public void UpdateOld()
        {
            var actor = this.Actor;
            var target = this.Target;
            if (this.State == States.Finished) // TODO: maybe check for failed state too?
            {
                this.Stop();
                return;
            }

            if (this.State == States.Unstarted)
            {
                this.Start();
                this.State = States.Running;
            }
            else if (this.State == States.Finished)
            {
                this.Stop();
                AILog.TryWrite(actor, "Success: " + this.GetCompletedText(actor, target));
                return;
            }
            if (this.RunningType == RunningTypes.Continuous)
            {
                this.Perform();
                if (this.State == States.Finished)
                {
                    this.Stop();
                    AILog.TryWrite(actor, "Success: " + this.GetCompletedText(actor, target));
                }
                return;
            }
            this.CurrentTick--;
            if (this.CurrentTick <= 0)
            {
                this.Finish();
                this.Stop();
                this.Perform();
            }
        }

        internal void Stop()
        {
            this.State = States.Finished;
            if (this.AnimationDef is not null)
                this.CachedAnimation.FadeOutAndRemove();
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
            var parent = this.Actor;
            if (this._drawProgressBar)
            {
                Bar.Draw(sb, camera, this.BarPosition(), this.BarLabel(), this.BarProgress(), camera.Zoom * .2f);
                return;
            }
            if (this.RunningType == RunningTypes.Continuous)
                return;
            if (this.Length <= Ticks.PerSecond)
                return;
            var global = parent.Global;

            var bounds = camera.GetScreenBounds(global, parent.GetSprite().GetBounds());
            var scrLoc = new Vector2(bounds.X + bounds.Width / 2f, bounds.Y);//
            var barLoc = scrLoc - new Vector2(InteractionBar.DefaultWidth / 2, InteractionBar.DefaultHeight / 2);
            var textLoc = new Vector2(barLoc.X, scrLoc.Y);

            InteractionBar.Draw(sb, barLoc, InteractionBar.DefaultWidth, this.Def.ProgressHandler?.GetProgressPercentage(this) ?? this.Progress.Percentage);
            UIManager.DrawStringOutlined(sb, this.Verb, textLoc, Alignment.Horizontal.Left, Alignment.Vertical.Center, 0.5f);
        }

        internal virtual void ResolveReferences()
        {
        }

        //public abstract object Clone();

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
        public void Finish()
        {
            this.State = States.Finished;
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
            this.BarLabel = label;
        }

        internal void AddProgress(int v)
        {
            this.Actor.Map.Events.Post(new InteractionProgressEvent(this.Actor, v));

            if (this.Def.ProgressHandler is not null)
                this.Def.ProgressHandler.AddProgress(this, v);
            else
                this.OnAddProgress(v);
            //this.Progress.Add(v);
            //this.Actor.Map.Events.Post(new InteractionProgressEvent(this.Actor, v));
        }
        protected virtual void OnAddProgress(int v)
        {
            this.Progress.Add(v);
        }

        public bool HasFinished => this.State == States.Finished;
    }
}
