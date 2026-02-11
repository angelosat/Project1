using System;
using Project1.Framework;
using Project1.Core.Entities;

namespace Project1.Core.Blocks
{
    [Obsolete]
    abstract class BlockWithEntity : Block
    {
        protected BlockWithEntity(string name, float transparency = 0, float density = 1, bool opaque = true, bool solid = true) : base(name, transparency, density, opaque, solid)
        {
        }
        public override bool TryConsume(GameObject actor, GameObject dropped, IntVec3 global, int amount = -1)
        {
            return false;
        }
    }
}