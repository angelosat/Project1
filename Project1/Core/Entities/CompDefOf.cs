using Project1.Core.Components;
using Project1.Core.Entities.Stats;
using Project1.Core.Helpers;
using Project1.Core.Simulation.Physics;
using Project1.Core.Tools;
using Project1.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Project1.Core.Entities
{
    internal class CompDef(string name, Type compType) : Def(name)
    {
        internal Type CompType = compType;
        internal EntityComp Create() => ActivatorSafe<EntityComp>.CreateInstance(this.CompType);
    }
    [EnsureStaticCtorCall]
    internal class CompDefOf
    {
        static CompDefOf()
        {
            Assembly[] assemblies =
            [
                typeof(EntityComp).Assembly,
            ];
            IEnumerable<Type> compTypes =
                assemblies
                    .SelectMany(a => a.GetTypes())
                    .Where(t =>
                        !t.IsAbstract &&
                        typeof(EntityComp).IsAssignableFrom(t));
            foreach(var comptype in compTypes)
            {
                Def.Register(new CompDef(comptype.Name, comptype));
            }
        }
    }
}
