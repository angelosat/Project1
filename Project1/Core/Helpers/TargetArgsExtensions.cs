using Microsoft.Xna.Framework;
using Project1.Core.Simulation;
using Project1.Framework;

namespace Project1.Core.Helpers
{
    internal static class TargetArgsExtensions
    {
        static public InteractionTarget At(this IntVec3 pos, MapBase map)
        {
            return new InteractionTarget(map, pos);
        }
        static public InteractionTarget At(this Vector3 pos, MapBase map)
        {
            return new InteractionTarget(map, pos);
        }
    }
}
