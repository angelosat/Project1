using Start_a_Town_.UI;
using System;
using System.Collections.Generic;
using System.IO;

namespace Start_a_Town_
{
    public class AttributeRuntime : Inspectable, ISaveableNewNew<AttributeRuntime>, ISerializableNew<AttributeRuntime>, IListable, IDefWrapper<AttributeDef>
    {
        public class ValueModifier
        {
            readonly float _Value;
            readonly Func<float> ValueGetter;
            public ValueModifier(float value)
            {
                this._Value = value;
                this.ValueGetter = () => this._Value;
            }
            public ValueModifier(Func<float> valueGetter)
            {
                this.ValueGetter = valueGetter;
            }
            public float GetValue()
            {
                return this.ValueGetter();
            }
        }

        public float Tick = Ticks.PerSecond / 0.5f; //1 tick per 2 seconds
        public float Timer = 0;
        public float RegenerationRate = 1;
        public Progress Rec = new(0, Ticks.PerSecond, Ticks.PerSecond);
        public float DecayRate = -0.5f;
        public float GainRate = 0;
        public List<ValueModifier> Modifiers = new();
        public ProgressLeveledExp Progress;
        public AttributeDef AttributeDef;
        public AttributeDef Def => this.AttributeDef;

        public int Level { get => this.Progress.Level; set => this.Progress.Level = value; }//.SetLevel(value); }
        public int Min = 10;
        const int BaseXpToLevel = 100;//5; //placeholder
        public AttributeRuntime(AttributeDef def, int value = 10)
        {
            this.AttributeDef = def;
            this.Progress = new ProgressLeveledExp(BaseXpToLevel, value);
        }
        public AttributeRuntime()
        {
            this.Progress = new ProgressLeveledExp(BaseXpToLevel, 10);
        }
        public void Update(GameObject parent)
        {
            this.AttributeDef.Worker.Tick(parent, this);
        }

        public override string ToString()
        {
            return this.AttributeDef.Name + ": " + this.Level;
        }
        public void Award(GameObject parent, float p)
        {
            this.AttributeDef.Worker.Award(parent, this, p);
        }
        internal void AddToProgress(float p)
        {
            this.Progress.Value += p;
            if (p > 0)
                this.Rec.Value = this.Rec.Max;
        }

        internal Control GetProgressControl()
        {
            return this.Progress.GetControl();
        }

        public AttributeRuntime Clone()
        {
            return new AttributeRuntime(this.AttributeDef, this.Level);
        }
        public SaveTag Save(string name = "")
        {
            var tag = new SaveTag(SaveTag.Types.Compound, name);
            //tag.Add(this.AttributeDef.Save("Def"));
            tag.SaveDef("Def", this.AttributeDef);
            tag.Add(this.Progress.Save("Progress"));
            return tag;
        }

        //public ISaveable Load(SaveTag tag)
        //{
        //    //tag.TryGetTag("Def", out var def) 
        //    this.Def = tag["Def"].LoadDef<AttributeDef>();
        //    tag.TryGetTag("Progress", this.Progress.Load);
        //    return this;
        //}
        public static AttributeRuntime Create(SaveTag tag)
        {
            var att = new AttributeRuntime();
            att.AttributeDef = tag["Def"].LoadDef<AttributeDef>();
            tag.TryGetTag("Progress", att.Progress.Load);
            //att.Load(tag);
            return att;
        }
        public void Write(IDataWriter w)
        {
            w.Write(this.Def);
            this.Progress.Write(w);
        }

        public AttributeRuntime Read(IDataReader r)
        {
            this.AttributeDef = r.ReadDef<AttributeDef>();
            this.Progress.Read(r);
            return this;
        }

        public Control GetListControlGui()
        {
            return new Bar(this.Progress, 200, () => $"{this.AttributeDef.Label}: {this.Level}")
            {
                TooltipFunc = t => t.AddControls(this.Progress.GetControl())
            };
        }

        public static AttributeRuntime Create(IDataReader r) => new AttributeRuntime().Read(r);

       
    }
}
