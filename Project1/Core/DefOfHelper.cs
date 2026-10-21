using Project1.Core.AI.Personality;
using Project1.Core.Entities.Stats;
using Project1.Core.Resources;
using Project1.Core.Stats;
using Project1.Core.Systems.Stats;
using Project1.Core.Systems.Tools;
using System.Runtime.CompilerServices;

namespace Project1.Core;

public static class DefOfHelper
{
    public static void Init()
    {
        RuntimeHelpers.RunClassConstructor(typeof(ToolUseDefOf).TypeHandle);
        RuntimeHelpers.RunClassConstructor(typeof(TraitDefOf).TypeHandle);
        RuntimeHelpers.RunClassConstructor(typeof(ResourceDefOf).TypeHandle);
        RuntimeHelpers.RunClassConstructor(typeof(DamageDefOf).TypeHandle);
        RuntimeHelpers.RunClassConstructor(typeof(StatDefOf).TypeHandle);
    }
}
