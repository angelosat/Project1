using Project1.Framework.Serialization;

namespace Project1.Core.World.WorldAreas
{
    public record struct WorldSpacePosition(float Value)
    {
        public static implicit operator float(WorldSpacePosition pos) => (float)pos.Value;
        public static implicit operator WorldSpacePosition(float pos) => new(pos);

        public static WorldSpacePosition ReadFrom(IDataReader r) => new(r.ReadSingle());
    }
}
