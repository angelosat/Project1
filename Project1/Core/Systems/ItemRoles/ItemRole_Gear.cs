using Project1.Core.Entities;
using Project1.Core.Entities.Actors;
using Project1.Core.Entities.Stats;
using Project1.Core.Gear;
using Project1.Core.Systems.Tools;
using Project1.Framework.Helpers;
using System;
using System.Collections.Generic;

namespace Project1.Core.Systems.ItemRoles;

sealed class ItemRole_GearNew(GearSlotDef slotDef, StatDef statDef) : ItemRoleWorker
{
    GearSlotDef SlotDef = slotDef;
    StatDef StatDef = statDef;

    public override int GetInventoryScore(Actor actor, Entity item, ItemRoleDef _)
    {
        var profile = (GearProfileDef)item.Profile;
        var gearRole = profile.Role;
        var slot = gearRole.Slot;
        var inSlot = actor.Gear.GetGear(slot);
        var stat = this.StatDef;// (StatDef)role.Source;
        var statValue = ToolSystem.CalculateStat(item, stat);
        var inSlotStatValue = inSlot is Entity existing ? ToolSystem.CalculateStat(existing, stat) : null;
        var diff = statValue.Value - (inSlotStatValue ?? 0);
        return (int)diff;
    }

    public override int GetSituationalScore(Actor actor, Entity item, ItemRoleDef _)
    {
        if (item.Def != ItemDefOf.Gear)
            return -1;
        if (item.Profile is not GearProfileDef profile)
            return -1;
        var statDef = this.StatDef;// (StatDef)role.Source;
        // TODO need to differentiate between intrinsic gear stats like damage and speed
        // and actor stat bonuses that are rolled on the item
        // also i have to rethink the fact that itemrole gear prefs should be per slot 
        var value = ToolSystem.CalculateStat(item, statDef);
        return value.HasValue ? (int)value.Value : -1;
    }

    public override IEnumerable<Def> GetValidTargetDefs()
    {
        throw new System.NotImplementedException();
    }
}
sealed class ItemRole_Gear : ItemRoleWorker
{
    public override int GetSituationalScore(Actor actor, Entity item, ItemRoleDef role)
    {
        var profile = (GearProfileDef)item.Profile;
        var gearRole = profile.Role;
        var slot = gearRole.Slot;
        var inSlot = actor.Gear.GetGear(slot);
        var stat = (StatDef)role.Source;
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
        var statDef = (StatDef)role.Source;
        // TODO need to differentiate between intrinsic gear stats like damage and speed
        // and actor stat bonuses that are rolled on the item
        // also i have to rethink the fact that itemrole gear prefs should be per slot 
        var value = ToolSystem.CalculateStat(item, statDef);
        return value.HasValue ? (int)value.Value : -1;
    }

    public override IEnumerable<Def> GetValidTargetDefs()
        => Def.Get<StatDef>();



    //public override IEnumerable<ItemRoleDef> GenerateRoles()
    //{
    //    var slotDefs = Def.Get<GearSlotDef>();
    //    var stats = StatSystem.AllStats;
    //    foreach(var slot in slotDefs)
    //    {
    //        foreach(var stat in stats)
    //        {
    //            //var def = new ItemRoleDef()
    //        }
    //    }
    //    yield break;
    //}
}

internal sealed class ItemRoleContextWorker_Gear : ItemRoleContextWorker
{
    internal override IEnumerable<ItemRoleDef> GenerateRoles(ItemRoleContextDef contextDef)
    {
        var gearSlotDefs = Def.Get<GearSlotDef>();
        var statDefs = Def.Get<StatDef>();
        foreach (var slotDef in gearSlotDefs)
        {
            foreach (var statDef in statDefs)
            {
                yield return new ItemRoleDef($"{contextDef.LabelReadable}:{slotDef.LabelReadable}:{statDef.LabelReadable}", contextDef, null)
                { 
                    Worker = new ItemRole_GearNew(slotDef, statDef) 
                };
            }
        }
    }
}

internal sealed class ItemRoleContextWorker_Tool : ItemRoleContextWorker
{
    internal override IEnumerable<ItemRoleDef> GenerateRoles(ItemRoleContextDef contextDef)
    {
        var defs = Def.Get<ToolUseDef>();
        foreach (var def in defs)
        {
            yield return new ItemRoleDef($"{contextDef.LabelReadable}:{def.LabelReadable}", contextDef, null)
            {
                Worker = new ItemRole_Tool()
            };
        }
    }
}

//internal abstract class ItemRoleGenerator 
//{
//    static ItemRoleGenerator()
//    {
//        var contexts = Def.Get<ItemRoleContextDef>();
//        foreach(var context in contexts)
//        {
//            //foreach(var subContext in context.Gen.sub)
//        }
//    }

//    internal abstract IEnumerable<Def> GetApplicableDefsInt();
//    private IEnumerable<ItemRoleNode> GenerateNodes()
//    {
//        if (this.SubGen is null)
//        {

//        }
//        foreach (var def in this.GetApplicableDefsInt())
//        {


//        }
//        //if (this.SubGen is not null)
//        //{
//        //    foreach ()
//        //}
//        //else
//        //    return this.GetApplicableDefsInt();
//    }
//    internal ItemRoleGenerator SubGen;
//}

//internal sealed class ItemRoleNode
//{
//    internal Def Target;
//    internal ItemRoleNode? Child;
//}