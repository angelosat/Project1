using Project1.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Project1.Core.Systems.Thoughts;

[EnsureStaticCtorCall]
internal static class ThoughtSystem
{
    static readonly List<IWorldEventHandler> WorldHandlers = [];
    static ThoughtSystem()
    {
        foreach (var t in Assembly.GetExecutingAssembly().GetTypes().Where(t => typeof(IWorldEventHandler).IsAssignableFrom(t) && !t.IsAbstract))
            WorldHandlers.Add(Activator.CreateInstance(t) as IWorldEventHandler);
        foreach (var w in WorldHandlers)
            w.Register();
    }
}
