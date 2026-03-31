using Project1.Framework.Events;
using Project1.Framework.Interfaces;
using Project1.Framework.Serialization;
using System;

namespace Project1.Framework.Helpers
{
    public sealed class ProgressIntSigned : IProgressBar, ISerializableNewNew<ProgressIntSigned>, ISaveableNewNew<ProgressIntSigned>
    {
        public event Action Updated;
        public IDisposable Subscribe(Action handler)
        {
            this.Updated += handler;
            return new Subscription(() => this.Updated -= handler);
        }
        public int Min { get; private set; }
        public int Max { get; private set; }
        int _value;
        public int Value
        {
            get => this._value;
            private set
            {
                _value = value;
                this.Updated?.Invoke();
            }
        }
        public float Percentage => this.Value >= 0 ? (float)this.Value / this.Max : -(float)this.Value / this.Min;
        public int Missing => this.Max - this.Value;
        public bool IsFinished => this.Value == this.Max;

        public int ApplyDelta(int value) => this.Value = Math.Min(this.Max, Math.Max(0, this.Value + value));
        public int ApplyPercentageDelta(float delta)
            => this.Value = (int)Math.Min(this.Max, Math.Max(0, (this.Percentage + delta) * this.Max));
        public int Set(int value) => this.Value = Math.Min(this.Max, Math.Max(0, value));
        public void SetMax(int max)
        {
            this.Max = max;
            this.Value = Math.Clamp(this.Value, 0, this.Max);
        }
        public int Reset() => this.Value = 0;
        public int Complete() => this.Value = this.Max;

        public ProgressIntSigned(int max, int value = 0)
        {
            this.Max = max;
            this.Value = value;
        }
        public ProgressIntSigned(int min, int max, int value)
        {
            this.Min = min;
            this.Max = max;
            this.Value = value;
        }
        ProgressIntSigned()
        {

        }
        public ProgressIntSigned Read(IDataReader r)
        {
            this.Max = r.ReadInt32();
            this.Value = r.ReadInt32();
            return this;
        }

        public IDataWriter Write(IDataWriter w)
        {
            w.Write(this.Max);
            w.Write(this.Value);
            return w;
        }
        public override string ToString() => $"{this.Value} / {this.Max}";

        public static ProgressIntSigned Create(IDataReader r) => new ProgressIntSigned().Read(r);

        public SaveTag Save(string name = "")
        {
            var tag = new SaveTag(SaveTag.Types.Compound, name);
            tag.Save("Max", this.Max);
            tag.Save("Value", this.Value);
            return tag;
        }
        ProgressIntSigned Load(SaveTag tag)
        {
            this.Max = tag.LoadInt("Max");
            this.Value = tag.LoadInt("Value");
            return this;
        }
        public static ProgressIntSigned Create(SaveTag tag) => new ProgressIntSigned().Load(tag);

        internal void SetValue(int value)
        {
            this.Value = value;
        }
        internal void SetValue(float value)
        {
            this.Value = (int)value;
        }
    }

    public sealed class ProgressInt : IProgressBar, ISerializableNewNew<ProgressInt>, ISaveableNewNew<ProgressInt>
    {
        public event Action Updated;
        public IDisposable Subscribe(Action handler)
        {
            this.Updated += handler;
            return new Subscription(() => this.Updated -= handler);
        }
        public int Max { get; private set; }
        int _value;
        public int Value
        {
            get => this._value;
            private set
            {
                _value = value;
                this.Updated?.Invoke();
            }
        }
        public float Percentage => (float)this.Value / this.Max;
        public int Missing => this.Max - this.Value;
        public bool IsFinished => this.Value == this.Max;

        public int ApplyDelta(int value) => this.Value = Math.Min(this.Max, Math.Max(0, this.Value + value));
        public int ApplyPercentageDelta(float delta) 
            => this.Value = (int)Math.Min(this.Max, Math.Max(0, (this.Percentage + delta) * this.Max));
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
            this.Max = max;
            this.Value = value;
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

        public IDataWriter Write(IDataWriter w)
        {
            w.Write(this.Max);
            w.Write(this.Value);
            return w;
        }
        public override string ToString() => $"{this.Value} / {this.Max}";

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

        internal void SetValue(int value)
        {
            this.Value = value;
        }
        internal void SetValue(float value)
        {
            this.Value = (int)value;
        }
    }
}
