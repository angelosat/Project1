using Microsoft.Xna.Framework;
using Project1.Framework.Base;
using Project1.Framework.WorldGen;

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
