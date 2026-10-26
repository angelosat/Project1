namespace Project1.Core
{
    public record struct Tier(byte Value)
    {
        public static implicit operator byte(Tier pos) => pos.Value;
        public static implicit operator Tier(byte pos) => new(pos);
    }
}
