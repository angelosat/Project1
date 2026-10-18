using Project1.Core.Stats;
using Project1.Core.Systems.Tools;
using System;
using System.Linq;
using System.Runtime.CompilerServices;

namespace Project1.Core;

public static class DefOfHelper
{
    public static void Init()
    {
        RuntimeHelpers.RunClassConstructor(typeof(ToolUseDefOf).TypeHandle);
        RuntimeHelpers.RunClassConstructor(typeof(StatDefOf).TypeHandle);
    }
}
