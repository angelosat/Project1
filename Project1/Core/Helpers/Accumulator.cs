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
        //=> this._inner.ToString();
        => $"Accumulator({nameof(this._inner)}={this._inner})";
}
