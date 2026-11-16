using Project1.Core.Entities;
using Project1.Core.Entities.Actors;
using Project1.Core.Entities.Stats;
using Project1.Core.Gear;
using Project1.Core.Systems.Tools;

namespace Project1.Core.Systems.ItemRoles;

sealed class ItemRoleWorker_Gear(GearSlotDef slotDef, StatDef statDef) : ItemRoleWorker
{
    public override int GetSituationalScore(Actor actor, Entity item)
    {
        var profile = (GearProfileDef)item.Profile;
        var gearRole = profile.Role;
        var slot = gearRole.Slot;
        if (slot != slotDef)
            return 0;
        //var inSlot = actor.Gear.GetGear(slot);
        var stat = statDef;
        var statValue = ToolSystem.CalculateStat(item, stat);
        //var inSlotStatValue = inSlot is Entity existing ? ToolSystem.CalculateStat(existing, stat) : null;
        //var diff = statValue.Value - (inSlotStatValue ?? 0);
        // compare to the currently equipped item here?
        // or just return the item's situational score and let the caller compare ?
        return (int)statValue.Value;
        //return (int)diff;
    }

    public override int GetInventoryScore(Actor actor, Entity item)
    {
        if (item.Def != ItemDefOf.Gear)
            return -1;
        if (item.Profile is not GearProfileDef profile)
            return -1;
        if (profile.Role.Slot != slotDef)
            return -1;
        // TODO need to differentiate between intrinsic gear stats like damage and speed
        // and actor stat bonuses that are rolled on the item
        var value = ToolSystem.CalculateStat(item, statDef);
        return value.HasValue ? (int)value.Value : -1;
    }
}
