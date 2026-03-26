using System.Collections.Generic;
using Project1.Core.Entities;
using Project1.Core.Systems.Materials;
using Project1.Framework;
using Project1.Framework.Helpers;

namespace Project1.Core.Towns.Constructions
{
    interface IConstructible
    {
        bool IsReadyToBuild(out ItemDef def, out MaterialDef material, out int amount);
        bool IsValidHaulDestination(ItemDef objid);
        int GetMissingAmount(ItemDef objid);
        ProgressFloat BuildProgress { get; }
        List<IntVec3> Children { get; }
    }
}
