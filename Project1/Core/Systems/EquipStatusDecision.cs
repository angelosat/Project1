using System;

#nullable enable

namespace Project1.Core.Systems
{
    internal record struct EquipStatusDecision(EntityRefId ItemId, int Delta)
    {
        internal readonly bool IsEquip => Delta > 0;
        internal readonly bool IsUnequip => Delta < 0;
    }
}
