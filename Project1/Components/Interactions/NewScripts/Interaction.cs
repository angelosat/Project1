using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Start_a_Town_.Animations;
using Start_a_Town_.Components;
using Start_a_Town_.UI;
using System;
using System.Collections.Generic;
using System.IO;

namespace Start_a_Town_
{
    public abstract class Interaction : Inspectable, ICloneable
    {
        public override string Label => this.Name;
        public bool IsFinished => this.State == States.Finished;
        public static readonly float DefaultRange = (float)Math.Sqrt(2);

        static readonly Dictionary<string, Func<Interaction>> Factory = new();

        public static void AddInteraction<T>(Func<Interaction> factory)
        {
            Factory[typeof(T).FullName] = factory;
        }
        public static void AddInteraction<T>() where T : Interaction, new()
        {
            Factory[typeof(T).FullName] = () => new T();
        }

        internal virtual void OnToolContact()
        {
        }

        public static void Initialize()
        {

        }
        public override string ToString()
        {
            return "Interaction: " + this.Name;
        }

        public enum States { Unstarted, Running, Finished, Failed }
        public enum RunningTypes { Once, Continuous }

        public int GetID()
        {
            return this.Name.GetHashCode();
        }

        public States State { get; protected set; } = States.Unstarted;

        public RunningTypes RunningType = RunningTypes.Once;

        public string Name { get; set; }
        public string Verb { get; set; }

        public float Length { get; set; }
        public float CurrentTick;
        public ToolUseDef ToolUse { get; set; }
        public float Seconds { get; set; }
        public Animation Animation = new(AnimationDef.Work);
        internal Actor Actor;
        internal TargetArgs Target;

        private bool _drawProgressBar;
        public Func<Vector3> BarPosition;
        public Func<float> BarProgress;
        private Func<string> BarLabel;

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
            this.Animation?.FadeOutAndRemove();
        }

        public virtual void Perform()
        {
        }
        protected int CrossFadeAnimationLength;
        public void StartBase()
        {
            if (this.Animation is not null)
            {
                if (this.CrossFadeAnimationLength == 0)
                    this.Actor.AddAnimation(this.Animation);
                else
                    this.Actor.CrossFade(this.Animation, false, this.CrossFadeAnimationLength);
            }
            this.Start();
        }
        protected virtual void Start()
        {

        }
        internal bool Evaluate()
        {
            return true;
        }

        public virtual void Update()
        {
            var actor = this.Actor;
            var target = this.Target;
            if (this.State == States.Finished) // TODO: maybe check for failed state too?
            {
                this.Stop();
                return;
            }

            if (this.State == States.Unstarted)
                this.StartBase();
            else if (this.State == States.Finished)
            {
                this.Stop();
                AILog.TryWrite(actor, "Success: " + this.GetCompletedText(actor, target));
                return;
            }

            this.State = States.Running;
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

        protected virtual void Stop()
        {
            this.Animation.FadeOutAndRemove();
        }

        public void GetTooltip(Control tooltip)
        {
            var panel = new PanelLabeled("Interact") { AutoSize = true, Location = tooltip.Controls.BottomLeft };
            panel.Controls.Add(new Label(this.Name + (this.Length > 0 ? TimeSpan.FromMilliseconds(this.Length).TotalSeconds.ToString(" #0.##s") : "")) { Location = panel.Controls.BottomLeft }); //this.Length.ToString("#0.##s")
            tooltip.Controls.Add(panel);
        }

        public float Percentage => (float)(1 - this.CurrentTick / this.Length);
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

            InteractionBar.Draw(sb, barLoc, InteractionBar.DefaultWidth, this.Percentage);
            UIManager.DrawStringOutlined(sb, this.Verb, textLoc, Alignment.Horizontal.Left, Alignment.Vertical.Center, 0.5f);
        }

        internal virtual void ResolveReferences()
        {
        }

        public abstract object Clone();

        public virtual string GetCompletedText(Actor actor, TargetArgs target)
        {
            return this.Name + ": " + target.ToString();
        }
        public void Write(IDataWriter w)
        {
            w.Write(this.CurrentTick);
            w.Write((int)this.State);
            w.Write(this.Animation is not null);
            if (this.Animation is not null) // added this because InteractionSleepInBed doesn't have an animation
                this.Animation.Write(w);
            this.WriteExtra(w);
        }
        public void Read(IDataReader r)
        {
            this.CurrentTick = r.ReadSingle();
            this.State = (States)r.ReadInt32();
            if (r.ReadBoolean())
                this.Animation.Read(r);
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
            this.Animation?.Save(tag, "Animation");
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
            tag.TryGetTag("Animation", t => inter.Animation = new Animation(t));
            inter.LoadData(tag);
            return inter;
        }
        internal virtual void Synced(MapBase map)
        {
        }

        internal virtual void InitAction()
        {
            if (this.Length == 0)
            {
                this.Perform();
                this.Finish();
            }
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
            this.Animation.Entity = this.Actor;
        }

        public void DrawProgressBar(Func<Vector3> position, Func<float> progress, Func<string> label)
        {
            this._drawProgressBar = true;
            this.BarPosition = position;
            this.BarProgress = progress;
            this.BarLabel = label;
        }
        public bool HasFinished => this.State == States.Finished;
    }
}
