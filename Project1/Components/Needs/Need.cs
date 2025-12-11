using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Start_a_Town_.UI;

namespace Start_a_Town_
{
    public sealed class Need : MetricWrapper, IProgressBar, ISaveable, INamed, ISerializableNew<Need>, ISaveableNew
    {
        Dictionary<EffectDef, List<NeedMod>> ModsNew = [];
        internal void AddMod(EffectDef needLetDef, float value, float rate)
        {
            if (this.Mods.Any(n => n.Def == needLetDef))
                throw new Exception();
            var needLet = new NeedMod(needLetDef, rate);//, value, rate);
            this.Mods.Add(needLet);
        }
        internal void AddMod(EntityEffectWrapper source, float rate)
        {
            if (this.Mods.Any(n => n.Def == source.Def))
                throw new Exception();
            var needLet = new NeedMod(source.Def, rate);//, value, rate);
            this.Mods.Add(needLet);
        }
        internal void RemoveMod(EffectDef def) => this.Mods.RemoveAll(n => n.Def == def);
        
        public NeedDef NeedDef;
        public enum Types { Hunger, Water, Sleep, Achievement, Work, Brains, Curiosity, Social, Energy }
        const string Format = "P0";
        public string Name => this.NeedDef.Label;
        public float DecayDelay, DecayDelayMax = 3;
        public float _Value;
        public double LastTick;
        public float Value
        {
            get => this._valueInt;
            set => this._valueInt = (int)MathHelper.Clamp(value, 0, 100);
        }
        public int _valueInt = 100;
        public float TicksPerNaturalDecay = 1 / Ticks.FromSeconds(10);
        public float Accumulator;
        public readonly float Min = 0f;
        public readonly float Max = 100f;
        public float Percentage => this.Value / this.Max;
        public float Mod;
        public readonly List<NeedMod> Mods = new();
        public float Tolerance { get; set; }
        public float Threshold { get { return this.NeedDef.BaseThreshold; } }
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
            this.Parent = parent;
        }

        public Need(Actor parent, NeedDef needDef) : this(parent)
        {
            this.NeedDef = needDef;
        }

        public sealed override void Tick()
        {
            this.NeedDef.Worker.Tick(this);
            //return;
            //this.LastTick = this.Parent.Net.CurrentTick;
            //float newValue;
            //var mod = this.Mods.Sum(d => d.RateMod);
            //if (mod != 0)
            //{
            //    newValue = this._Value + mod;
            //    this.DecayDelay = this.DecayDelayMax;
            //}
            //else
            //{
            //    if (this.DecayDelay > 0)
            //    {
            //        this.DecayDelay--;
            //        return;
            //    }
            //    else
            //    {
            //        // TODO: is exponential decay better? maybe have both exp and linear and choose between them for each need?
            //        var p = 1 - Value / 100f;
            //        float d = this.Decay * (1 + 5 * p * p);
            //        d = this.Decay;
            //        d = this.NeedDef.BaseDecayRate;
            //        d *= this.FinalDecayMultiplier;
            //        newValue = this._Value - d;
            //    }
            //}
            //SetValue(newValue, this.Parent);
        }
        public void TickLong(GameObject parent) { }
        public float FinalDecayMultiplier => 1;
        public AITask GetTask(GameObject parent) { return null; }
        
        public TaskGiver TaskGiver { get { return this.NeedDef.TaskGiver; } }

        public void SetValue(float newVal, GameObject parent)
        {
            float oldVal = Value;
            if (oldVal >= Tolerance && newVal < Tolerance)
            {
            }
            this.Value = Math.Max(0, Math.Min(100, newVal));
            if (this.Value > oldVal)
                this.DecayDelay = DecayDelayMax;
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
            this.Mods.WriteNew(w);
            this.ModsNew.WriteNew(w, k => k.Write(w), v => v.WriteNew(w));
        }
        public Need Read(IDataReader r)
        {
            this.NeedDef = r.ReadDef<NeedDef>();
            this.Value = r.ReadSingle();
            this.Mod = r.ReadSingle();
            this.DecayDelay = r.ReadSingle();
            this.Mods.Read(r);
            this.ModsNew.ReadFromFlat(r, r => r.ReadDef<EffectDef>(), r => r.ReadListNew<NeedMod>());// new List<NeedMod>().LoadNew(r)); //
            return this;
        }
        static public Need Create(IDataReader r) => new Need().Read(r);
        //{
        //    var need = new Need();
        //    need.NeedDef = r.ReadDef<NeedDef>();
        //    need.Value = r.ReadSingle();
        //    need.Mod = r.ReadSingle();
        //    need.DecayDelay = r.ReadSingle();
        //    need.Mods.Read(r);
        //    need.ModsNew.ReadNew(r, r => r.ReadDef<EffectDef>(), r => r.ReadListNew<NeedMod>());// new List<NeedMod>().LoadNew(r)); //
        //    return need;
        //}

        public SaveTag Save(string name = "")
        {
            var tag = new SaveTag(SaveTag.Types.Compound, name);
            this.NeedDef.Save(tag, "Def");
            tag.Add(this.Value.Save("Value"));
            tag.Add(this.Mod.Save("Mod"));
            tag.Add(this.DecayDelay.Save("DecayTimer"));
            tag.Add(this.Mods.SaveNewBEST("Mods"));
            tag.Add(this.ModsNew.Save("ModsDic", k => k.Save(), v => v.Save()));
            return tag;
        }
        public ISaveable Load(SaveTag tag)
        {
            tag.TryGetTagValue<string>("Def", v => this.NeedDef = Def.GetDef<NeedDef>(v));
            tag.TryGetTagValueOrDefault<float>("Value", out this._Value);
            tag.TryGetTagValueOrDefault<float>("Mod", out this.Mod);
            tag.TryGetTagValueOrDefault<float>("DecayTimer", out this.DecayDelay);
            this.Mods.TryLoadMutable(tag, "Mods");
            this.ModsNew.LoadNewNewNew(tag["ModsDic"],
                                       k => Def.GetDef<EffectDef>((string)k.Value),
                                       v => v.LoadListNew<NeedMod>());
            return this;
        }

        static public ISaveableNew Create(SaveTag tag)
        {
            var need = new Need();
            tag.TryGetTagValue<string>("Def", v => need.NeedDef = Def.GetDef<NeedDef>(v));
            tag.TryGetTagValueOrDefault<float>("Value", out need._Value);
            tag.TryGetTagValueOrDefault<float>("Mod", out need.Mod);
            tag.TryGetTagValueOrDefault<float>("DecayTimer", out need.DecayDelay);
            need.Mods.TryLoadMutable(tag, "Mods");
            return need;
        }

    }
}
