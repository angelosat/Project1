using Project1.Core.Entities;
using Project1.Core.Base;
using Project1.Core.Blocks;

namespace Project1.Core
{
    abstract class BlockWithEntity : Block
    {
        protected BlockWithEntity(string name, float transparency = 0, float density = 1, bool opaque = true, bool solid = true) : base(name, transparency, density, opaque, solid)
        {
        }
        public override bool TryConsume(GameObject actor, GameObject dropped, IntVec3 global, int amount = -1)
        {
            return false;
            //actor.Map.GetBlockEntity(global).OnDrop(actor, dropped, target, amount);
        }
    }
}
