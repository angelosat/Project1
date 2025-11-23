using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace Start_a_Town_
{
    interface IConstructible
    {
        bool IsReadyToBuild(out ItemDef def, out MaterialDef material, out int amount);
        bool IsValidHaulDestination(ItemDef objid);
        int GetMissingAmount(ItemDef objid);
        Progress BuildProgress { get; }
        List<IntVec3> Children { get; }
    }
}
