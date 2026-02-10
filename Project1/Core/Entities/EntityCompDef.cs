using System;
using Project1.Core.Helpers;

namespace Project1.Core.Entities
{
    public class EntityCompDef(string name, Type compType) : Def(name)
    {
        public Type CompType = compType;
        public EntityComp CreateInstance() => ActivatorSafe<EntityComp>.CreateInstance(this.CompType);
    }
}
