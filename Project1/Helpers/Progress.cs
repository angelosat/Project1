using System;
using System.Globalization;

namespace Start_a_Town_
{
    public sealed class ProgressInt : IProgressBar, ISerializableNew<ProgressInt>, ISaveableNewNew<ProgressInt>
    {
        public int Max { get; private set; }
        public int Value { get; private set; }
        public float Percentage => (float)this.Value / this.Max;
        public int Missing => this.Max - this.Value;
        public bool IsFinished => this.Value == this.Max;

        public int Add(int value) => this.Value = Math.Min(this.Max, Math.Max(0, this.Value + value));
        public int Set(int value) => this.Value = Math.Min(this.Max, Math.Max(0, value));
        public void SetMax(int max)
        {
            this.Max = max;
            this.Value = Math.Clamp(this.Value, 0, this.Max);
        }
        public int Reset() => this.Value = 0;
        public int Complete() => this.Value = this.Max;

        public ProgressInt(int max, int value = 0)
        {
            Max = max;
            Value = value;
        }

        ProgressInt()
        {
            
        }
        public ProgressInt Read(IDataReader r)
        {
            this.Max = r.ReadInt32();
            this.Value = r.ReadInt32();
            return this;
        }

        public void Write(IDataWriter w)
        {
            w.Write(this.Max);
            w.Write(this.Value);
        }

        public static ProgressInt Create(IDataReader r) => new ProgressInt().Read(r);

        public SaveTag Save(string name = "")
        {
            var tag = new SaveTag(SaveTag.Types.Compound, name);
            tag.Save("Max", this.Max);
            tag.Save("Value", this.Value);
            return tag;
        }
        ProgressInt Load(SaveTag tag)
        {
            this.Max = tag.LoadInt("Max");
            this.Value = tag.LoadInt("Value");
            return this;
        }
        public static ProgressInt Create(SaveTag tag) => new ProgressInt().Load(tag);
    }
    public class Progress : IProgressBar
    {
        public virtual float Min { get; set; }
        public virtual float Max { get; set; }
        float _Value;
        public virtual float Value { get => this._Value; set => this._Value = Math.Max(this.Min, Math.Min(this.Max, value)); }
        public virtual float Percentage
        {
            get => this.Value / this.Max;
            set => this.Value = this.Max * value;
        }
        public virtual bool IsFinished => this.Value >= this.Max;
        public void ModifyValue(float value)
        {
            this.Value += value;
        }
        public Progress()
        {
            this.Min = this.Value = 0;
            this.Max = 100;
        }
        public Progress(float min, float max, float value)
        {
            this.Min = min;
            this.Max = max;
            this.Value = value;
        }

        public void Write(IDataWriter io)
        {
            io.Write(this.Min);
            io.Write(this.Max);
            io.Write(this.Value);
            this.WriteExtra(io);
        }
        protected virtual void WriteExtra(IDataWriter w) { }
        public void Read(IDataReader io)
        {
            this.Min = io.ReadSingle();
            this.Max = io.ReadSingle();
            this.Value = io.ReadSingle();
            this.ReadExtra(io);
        }
        protected virtual void ReadExtra(IDataReader r) { }

        public Progress(IDataReader io)
        {
            this.Min = io.ReadSingle();
            this.Max = io.ReadSingle();
            this.Value = io.ReadSingle();
        }
        public Progress Save(SaveTag tag, string name)
        {
            tag.Add(this.Save(name));
            return this;
        }
        public SaveTag Save(string name)
        {
            SaveTag tag = new SaveTag(SaveTag.Types.Compound, name);
            tag.Add(new SaveTag(SaveTag.Types.Float, "Min", this.Min));
            tag.Add(new SaveTag(SaveTag.Types.Float, "Max", this.Max));
            tag.Add(new SaveTag(SaveTag.Types.Float, "Value", this.Value));
            this.SaveExtra(tag);
            return tag;
        }
        protected virtual void SaveExtra(SaveTag tag) { }
        public void Load(SaveTag tag)
        {
            this.Min = tag.GetValue<float>("Min");
            this.Max = tag.GetValue<float>("Max");
            this.Value = tag.GetValue<float>("Value");
            this.LoadExtra(tag);
        }
        protected virtual void LoadExtra(SaveTag tag) { }
        public Progress(SaveTag tag)
        {
            this.Min = tag.GetValue<float>("Min");
            this.Max = tag.GetValue<float>("Max");
            this.Value = tag.GetValue<float>("Value");
        }

        public Progress(Progress toCopy)
        {
            this.Min = toCopy.Min;
            this.Max = toCopy.Max;
            this.Value = toCopy.Value;
        }

        public override string ToString()
        {
            return this.Value.ToString() + "/" + this.Max.ToString();
        }
        public string ToStringAsSeconds()
        {
            if (this.Max == 0)
                return "";
            var ts = TimeSpan.FromMilliseconds(1000 * this.Value / 60f);
            string fmt = "";
            if (ts.Hours > 0)
                fmt += "%h'h '";
            if (ts.Minutes > 0)
                fmt += "%m'm '";
            if (ts.Seconds > 0)
                fmt += "%s's'";
            return ts.ToString(fmt);
        }
        public string ToStringPercentage()
        {
            return $"{this.Percentage.ToString("P0", CultureInfo.InvariantCulture)}";
        }
        public UI.Bar GetGui(string text = "Progress")
        {
            return new UI.Bar(this)
            {
                TextFunc = () => text,
                HoverFunc = () => $"{this.Value:0.00} / {this.Max:0}"
            };
        }
        public void Add(int work)
        {
            this.Value += work;
        }
    }
}
