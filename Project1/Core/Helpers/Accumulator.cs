namespace Project1.Core.Helpers
{
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
        public override string ToString()
            => this._inner.ToString();
    }
}
