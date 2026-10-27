using Project1.Framework.Events;
using Project1.Framework.Interfaces;
using Project1.Framework.Serialization;
using System;

namespace Project1.Framework.Helpers;

public sealed class ProgressIntSigned : IProgressBar, ISerializableNewNew<ProgressIntSigned>, ISaveableNewNew<ProgressIntSigned>
{
    public event Action Updated;
    public IDisposable Subscribe(Action handler)
    {
        Updated += handler;
        return new Subscription(() => Updated -= handler);
    }
    public int Min { get; private set; }
    public int Max { get; private set; }
    int _value;
    public int Value
    {
        get => _value;
        private set
        {
            _value = value;
            Updated?.Invoke();
        }
    }
    public float Percentage => Value >= 0 ? (float)Value / Max : -(float)Value / Min;
    public int Missing => Max - Value;
    public bool IsFinished => Value == Max;

    //public int ApplyDelta(int value) => this.Value = Math.Min(this.Max, Math.Max(0, this.Value + value));
    public int ApplyDelta(int value) => Value = Math.Clamp(Value + value, Min, Max);
    public int ApplyPercentageDelta(float delta)
         => Value = (int)Math.Clamp(Min, (Percentage + delta) * Max, Max);
    public int Set(int value) => Value = Math.Clamp(Value + value, Min, Max);
    public void SetMax(int max)
    {
        Max = max;
        Value = Math.Clamp(Value, Min, Max);
    }
    public int Reset() => Value = 0;
    public int Complete() => Value = Max;

    public ProgressIntSigned(int max, int value = 0)
    {
        Max = max;
        Min = -max;
        Value = value;
    }
    public ProgressIntSigned(int min, int max, int value)
    {
        Min = min;
        Max = max;
        Value = value;
    }
    ProgressIntSigned()
    {

    }
    public ProgressIntSigned Read(IDataReader r)
    {
        Max = r.ReadInt32();
        Value = r.ReadInt32();
        return this;
    }

    public IDataWriter Write(IDataWriter w)
    {
        w.Write(Max);
        w.Write(Value);
        return w;
    }
    public override string ToString() => $"{Value} / {Max}";

    public static ProgressIntSigned Create(IDataReader r) => new ProgressIntSigned().Read(r);

    public SaveTag Save(string name = "")
    {
        var tag = new SaveTag(SaveTag.Types.Compound, name);
        tag.Save("Max", Max);
        tag.Save("Value", Value);
        return tag;
    }
    ProgressIntSigned Load(SaveTag tag)
    {
        Max = tag.LoadInt("Max");
        Value = tag.LoadInt("Value");
        return this;
    }
    public static ProgressIntSigned Create(SaveTag tag) => new ProgressIntSigned().Load(tag);

    internal void SetValue(int value)
    {
        Value = value;
    }
    internal void SetValue(float value)
    {
        Value = (int)value;
    }
}
