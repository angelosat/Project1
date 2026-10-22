using Project1.Core.Entities;
using Project1.Core.Entities.Stats;
using Project1.Core.Gear;
using Project1.Core.Stats;
using Project1.Core.Systems.Gear;
using Project1.Framework;
using Project1.Framework.UI;
using System.Collections.Generic;

namespace Project1.Core.Systems.Tools;

[EnsureStaticCtorCall]
static class GearRoleDefOf
{
    //public static readonly GearRoleDef Tool = new("Tool", slot: GearSlotDefOf.Mainhand, bones: BoneStructureDefOf.Tool, stats: [StatDefOf.ToolSpeed, StatDefOf.ToolEffectiveness]);
    public static readonly GearRoleDef MainHand = new("MainHand", slot: GearSlotDefOf.Mainhand, bones: BoneStructureDefOf.Tool, stats: [StatDefOf.ItemSpeed, StatDefOf.ItemPower])
    { 
        Worker = new GearRoleWorker_MainHand()
    };
    public static readonly GearRoleDef Head = new("Head", slot: GearSlotDefOf.Head, bones: BoneStructureDefOf.Armor, stats: [StatDefOf.Armor])
    {
        Worker = new GearRoleWorker_Apparel()
    };
    public static readonly GearRoleDef Feet = new("Feet", slot: GearSlotDefOf.Feet, bones: BoneStructureDefOf.Armor, stats: [StatDefOf.Armor, StatDefOf.WalkSpeed])
    {
        Worker = new GearRoleWorker_Feet()
    };
    public static readonly GearRoleDef Torso = new("Torso", slot: GearSlotDefOf.Chest, bones: BoneStructureDefOf.Armor, stats: [StatDefOf.Armor])
    {
        Worker = new GearRoleWorker_Apparel()
    };
    public static readonly GearRoleDef Legs = new("Legs", slot: GearSlotDefOf.Legs, bones: BoneStructureDefOf.Armor, stats: [StatDefOf.Armor])
    {
        Worker = new GearRoleWorker_Apparel()
    };

    static GearRoleDefOf()
    {
        Def.Register(typeof(GearRoleDefOf));
    }
}
public class GearRoleWorker
{
    public virtual IEnumerable<LabelNew> GetControls(Entity item) => [];
    public virtual void Handle(Entity item) { }
}
sealed class GearRoleWorker_Apparel : GearRoleWorker
{
    public override void Handle(Entity item)
    {
        var comp = item.ToolComponent;
        //var profile = item.Profile as GearProfileDef;
        var power = StatDefOf.ItemPower.Worker.CalculateStat(item);
        comp.IntrinsicStats[StatDefOf.Armor] = new(StatDefOf.Armor, power);
    }
}
sealed class GearRoleWorker_Feet : GearRoleWorker
{
    public override void Handle(Entity item)
    {
        var comp = item.ToolComponent;
        var profile = item.Profile as GearProfileDef;
        var power = StatDefOf.ItemPower.Worker.CalculateStat(item);
        comp.IntrinsicStats[StatDefOf.Armor] = new(StatDefOf.Armor, power);
        comp.IntrinsicStats[StatDefOf.WalkSpeed] = new(StatDefOf.WalkSpeed, power);
    }
}
sealed class GearRoleWorker_MainHand : GearRoleWorker
{
    public override void Handle(Entity item)
    {
        var comp = item.ToolComponent;
        var profile = item.Profile as GearProfileDef;
        var power = StatDefOf.ItemPower.Worker.CalculateStat(item);
        var speed = StatDefOf.ItemSpeed.Worker.CalculateStat(item);
        comp.IntrinsicStats[StatDefOf.ItemSpeed] = new(StatDefOf.ItemSpeed, speed);
        var dmgStat = StatSystem.GetDerivedStat(profile.Damage);
        comp.IntrinsicStats[dmgStat] = new(dmgStat, power);
        foreach (var capability in profile.ToolUses)
        {
            var stat = StatSystem.GetDerivedStat(capability);
            var mod = profile.ToolUsesModifiers[capability];
            comp.IntrinsicStats[stat] = new(stat, power * mod);
        }
    }
    public override IEnumerable<LabelNew> GetControls(Entity item)
    {
        var comp = item.ToolComponent;
        var profile = item.Profile as GearProfileDef;
        var stats = comp.IntrinsicStats;
        //if(profile.BaseSpeed.HasValue)
        //    yield return new LabelNew($"Intrinsic2: {stats[StatDefOf.ToolSpeed].Value.ToString(StatDefOf.ToolSpeed.Format)} speed");

        if (profile.Damage is not null)
        {
            var magnitude = stats[StatDefOf.ItemPower].Value;
            yield return new LabelNew($"Intrinsic2: {magnitude.ToString(StatDefOf.ItemPower.Format)} {profile.Damage.LabelReadable}");
        }
        if (profile.ToolUses is not null)
            foreach (var usage in profile.ToolUses)
            {
                //var magnitude = stats[StatDefOf.ToolEffectiveness].Value;
                //var profileMod = profile.ToolUsesModifiers[usage];
                //yield return new LabelNew($"Intrinsic2: {magnitude * profileMod} {usage.LabelReadable}");
                var final = ToolSystem.GetFinalValue(item, usage);
                yield return new LabelNew($"Intrinsic2: {final.ToString(StatDefOf.ItemPower.Format)} {usage.LabelReadable}");
            }
        yield break;
    }
}