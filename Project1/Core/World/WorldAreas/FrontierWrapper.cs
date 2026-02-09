using System.Collections.Generic;
using Project1.Core.Entities;
using Project1.Core.Entities.Actors;

namespace Project1.Core.World.WorldAreas
{
    public class FrontierWrapper
    {
        public readonly FrontierDef Def;
        List<Entity> LootPool = [];
        public FrontierWrapper(FrontierDef def)
        {
            this.Def = def;
        }
        internal void Tick(Actor actor)
        {
            // roll encounter
            // roll random loot
            // etc
        }
    }
}
