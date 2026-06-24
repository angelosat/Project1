using Project1.Core.Entities.Stats;
using System.Collections.Generic;

namespace Project1.Core.Systems.Gear;

internal class StatsCache
{
    readonly Dictionary<StatDef, float> Cache = [];
    bool StatsDirty = true;
}
