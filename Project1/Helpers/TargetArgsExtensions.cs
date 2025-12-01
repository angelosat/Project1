using Start_a_Town_;
using Microsoft.Xna.Framework;

namespace Start_a_Town_
{
    internal static class TargetArgsExtensions
    {
        static public TargetArgs At(this IntVec3 pos, MapBase map)
        {
            return new TargetArgs(map, pos);
        }
        static public TargetArgs At(this Vector3 pos, MapBase map)
        {
            return new TargetArgs(map, pos);
        }
    }
}
