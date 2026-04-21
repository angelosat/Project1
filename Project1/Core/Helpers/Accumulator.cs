using Project1.Framework;
using Project1.Framework.Serialization;

namespace Project1.Core.Helpers;

public sealed class Accumulator
{
    float _inner;
    public void Add(float delta)
        => this._inner += delta;
    public int Flush()
    {
        int whole = (int)this._inner;
        if (whole != 0)
        {
            this._inner -= whole;
            return whole;
        }
        return 0;
    }
    bool TryFlush(out int value)
    {
        int whole = (int)this._inner;
        if (whole != 0)
        {
            this._inner -= whole;
            value = whole;
            return true;
        }
        value = 0;
        return false;
    }
    public bool AddAndTryFlush(float delta, out int value)
    {
        this.Add(delta);
        return this.TryFlush(out value);
    }
    public override string ToString()
        => $"Accumulator({nameof(this._inner)}={this._inner})";
}

public sealed class AccumulatorWithRate : ISaveableNewNew<AccumulatorWithRate>, ISerializableNewNew<AccumulatorWithRate>
{
    float _inner, _rate;

    public void ApplyRateDelta(float delta)
        => this._rate += delta;
    
    public void Tick(out int whole)
    {
        this.Add(this._rate);
        whole = (int)this._inner;
        if (whole != 0)
            this._inner -= whole;
    }
    public void Add(float delta)
      => this._inner += delta;

    public SaveTag Save(string name = "")
    {
        var tag = new SaveTag(SaveTag.Types.Compound, name);
        tag.Save("Inner", this._inner);
        tag.Save("Rate", this._rate);
        return tag;
    }

    public static AccumulatorWithRate Create(SaveTag tag)
    {
        var inner = tag.LoadSingle("Inner");
        var rate = tag.LoadSingle("Rate");
        return new() { _rate = rate, _inner = inner };
    }
    public IDataWriter Write(IDataWriter w)
    {
        w.Write(this._inner);
        w.Write(this._rate);
        return w;
    }
    public static AccumulatorWithRate Create(IDataReader r)
    {
        var inner = r.ReadSingle();
        var rate = r.ReadSingle();
        return new() { _rate = rate, _inner = inner };
    }
    public override string ToString()
        => $"Accumulator({nameof(this._inner)}={this._inner} {nameof(this._rate)}={this._rate})";
}