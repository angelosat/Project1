namespace Start_a_Town_
{
    abstract class BlockWithEntity : Block
    {
        protected BlockWithEntity(string name, float transparency = 0, float density = 1, bool opaque = true, bool solid = true) : base(name, transparency, density, opaque, solid)
        {
        }
        public override bool TryConsume(GameObject actor, GameObject dropped, TargetArgs target, int amount = -1)
        {
            return false;
            actor.Map.GetBlockEntity(target.Global).OnDrop(actor, dropped, target, amount);
        }
    }
}
