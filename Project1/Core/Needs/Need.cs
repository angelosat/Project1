using Microsoft.Xna.Framework;
using Project1.Core.AI;
using Project1.Core.AI.Planners;
using Project1.Core.Effects;
using Project1.Core.Entities;
using Project1.Core.Entities.Actors;
using Project1.Core.Helpers;
using Project1.Framework;
using Project1.Framework.Input;
using Project1.Framework.Interfaces;
using Project1.Framework.Serialization;
using Project1.Framework.UI;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Project1.Core.Needs
{
    public sealed class Need : MetricWrapper, IProgressBar, IDefWrapper<NeedDef>, INamed, ISerializableNew<Need>, ISaveableNewNew<Need>
    {
        internal void AddMod(EffectDef needLetDef, float ticksUntilChange)
        {
            if (this.Mods.Any(n => n.EffectDef == needLetDef))
                throw new Exception();
            var needLet = new NeedMod(needLetDef, 1f / ticksUntilChange);
            this.Mods.Add(needLet);
        }
        internal void RemoveMod(EffectDef def) => this.Mods.RemoveAll(n => n.EffectDef == def);
        
        public NeedDef NeedDef;
        public enum Types { Hunger, Water, Sleep, Achievement, Work, Brains, Curiosity, Social, Energy }
        const string Format = "P0";
        public string Name => this.NeedDef.LabelReadable;
        public float DecayDelay, DecayDelayMax = 3;
        public float _Value;
        public double LastTick;
        public int Value
        {
            get => this._valueInt;
            set => this._valueInt = (int)MathHelper.Clamp(value, 0, 100);
        }
        public int _valueInt = 100;
        [Obsolete("let defs declare natural decay or let systems explicitly apply accumulator deltas")]
        public float TicksPerNaturalDecay = 0;//1 / Ticks.FromSeconds(10);
        public float Accumulator;
        public readonly float Min = 0f;
        public readonly float Max = 100f;
        public float Percentage => this.Value / this.Max;
        public float Mod;
        public readonly List<NeedMod> Mods = new();
        public float Tolerance { get; set; }
        public float Threshold { get { return this.NeedDef.BaseThreshold; } }
        public int Deficit => (int)this.Max - this.Value;
        public bool IsBelowThreshold { get { return this.Value < this.Threshold; } }
        public override string ToString()
        {
            var txt = $"{Name}: {this.Percentage:P0}";

            foreach (var needlet in Mods)
                txt += $"\n{needlet}";
            return txt;
        }
        public  Need()
        {
            this._Value = this.Max;

        }
        public Need(Actor parent) : this()
        {
            this.Owner = parent;
        }

        public Need(Actor parent, NeedDef needDef) : this(parent)
        {
            this.NeedDef = needDef;
        }

        public sealed override void Tick()
        {
            if (this.Owner.Net.IsClient)
                return;
            this.NeedDef.Worker.Tick(this);
        }
        public void TickLong(GameObject parent) { }
        public float FinalDecayMultiplier => 1;
        public Plan GetTask(GameObject parent) { return null; }
        
        public PlannerDef Planner { get { return this.NeedDef.Planner; } }

        public NeedDef Def => this.NeedDef;

        public void SetValue(int value)
        {
            this.Value = value;
            this.Owner.World.Events.Post(new ActorNeedUpdatedEvent(this));
        }
        public void SetValue(int newVal, GameObject parent)
        {
            float oldVal = Value;
            if (oldVal >= Tolerance && newVal < Tolerance)
            {
            }
            this.Value = Math.Max(0, Math.Min(100, newVal));
            if (this.Value > oldVal)
                this.DecayDelay = DecayDelayMax;
        }
        public void ApplyDelta(int delta)
        {
            this.SetValue(this.Value + delta);
        }
        public Bar ToBar(GameObject parent)
        {
            var bar = new Bar()
            {
                ColorFunc = () => Color.Lerp(Color.Red, Color.Lime, this.Value / 100f),
                Object = this,
                NameFunc = () => this.Name,
                HoverFunc = () => this.ToString(),
                HoverFormat = this.Name + ": " + Format,
            };
            bar.LeftClickAction = () =>
            {
                if (InputState.IsKeyDown(System.Windows.Forms.Keys.ControlKey))
                {
                    "todo: request need change from server".ToConsole();
                    var val = 100 - (bar.ScreenLocation.X + bar.Width - UIManager.MouseScaled.X);
                    PacketNeedModify.SendSet(parent.Net, parent.RefId, this.Def, val);
                    return;
                }
            };
            return bar;
        }

        public Panel GetUI(GameObject entity)
        {
            var panel = new Panel() { AutoSize = true, BackgroundStyle = BackgroundStyle.TickBox};
            panel.Controls.Add(this.ToBar(entity));
            return panel;
        }

        public void Write(IDataWriter w)
        {
            this.NeedDef.Write(w);
            w.Write(this.Value);
            w.Write(this.Mod);
            w.Write(this.DecayDelay);
            this.Mods.Write(w);
        }
        public Need Read(IDataReader r)
        {
            this.NeedDef = r.ReadDef<NeedDef>();
            this.Value = r.ReadInt32();
            this.Mod = r.ReadSingle();
            this.DecayDelay = r.ReadSingle();
            this.Mods.Read(r);
            return this;
        }
        static public Need Create(IDataReader r) => new Need().Read(r);
   
        public SaveTag Save(string name = "")
        {
            var tag = new SaveTag(SaveTag.Types.Compound, name);
            this.NeedDef.Save(tag, "Def");
            tag.Add(this.Value.Save("Value"));
            tag.Add(this.Mod.Save("Mod"));
            tag.Add(this.DecayDelay.Save("DecayTimer"));
            tag.Add(this.Mods.SaveNewBEST("Mods"));
            return tag;
        }
      
        static public Need Create(SaveTag tag)
        {
            var need = new Need();
            need.NeedDef = tag.LoadDef<NeedDef>("Def");
            need.Value = tag.LoadInt("Value");
            tag.TryGetTagValueOrDefault<float>("Mod", out need.Mod);
            tag.TryGetTagValueOrDefault<float>("DecayTimer", out need.DecayDelay);
            need.Mods.TryLoadMutable(tag, "Mods");
            return need;
        }

    }
}
