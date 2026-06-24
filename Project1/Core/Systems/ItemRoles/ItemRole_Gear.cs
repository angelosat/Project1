using Project1.Core.Entities;
using Project1.Core.Entities.Actors;
using Project1.Core.Entities.Stats;
using Project1.Core.Systems.Tools;
using System.Collections.Generic;

namespace Project1.Core.Systems.ItemRoles;

sealed class ItemRole_Gear : ItemRoleWorker
{
    public override int GetSituationalScore(Actor actor, Entity item, ItemRoleDef role)
    {
        var profile = (GearProfileDef)item.Profile;
        var gearRole = profile.Role;
        var slot = gearRole.Slot;
        var inSlot = actor.Gear.GetGear(slot);
        var stat = (StatDef)role.Def;
        var statValue = ToolSystem.CalculateStat(item, stat);
        var inSlotStatValue = inSlot is Entity existing ? ToolSystem.CalculateStat(existing, stat) : null;
        var diff = statValue.Value - (inSlotStatValue ?? 0);
        return (int)diff;
    }

    public override int GetInventoryScore(Actor actor, Entity item, ItemRoleDef role)
    {
        if (item.Def != ItemDefOf.Gear)
            return -1;
        if (item.Profile is not GearProfileDef profile)
            return -1;
        var statDef = (StatDef)role.Def;
        var value = ToolSystem.CalculateStat(item, statDef);
        return value.HasValue ? (int)value.Value : -1;
    }

    public override IEnumerable<Def> GetValidTargetDefs()
        => Def.Get<StatDef>();
}