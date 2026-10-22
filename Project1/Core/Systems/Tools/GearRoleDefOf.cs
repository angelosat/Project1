using Project1.Core.Entities;
using Project1.Core.Gear;
using Project1.Core.Stats;
using Project1.Core.Systems.Gear;
using Project1.Framework;
using Project1.Framework.UI;
using SharpDX.XInput;
using System.Collections.Generic;

namespace Project1.Core.Systems.Tools;

[EnsureStaticCtorCall]
static class GearRoleDefOf
{
    //public static readonly GearRoleDef Tool = new("Tool", slot: GearSlotDefOf.Mainhand, bones: BoneStructureDefOf.Tool, stats: [StatDefOf.ToolSpeed, StatDefOf.ToolEffectiveness]);
    public static readonly GearRoleDef MainHand = new("MainHand", slot: GearSlotDefOf.Mainhand, bones: BoneStructureDefOf.Tool, stats: [StatDefOf.ToolSpeed, StatDefOf.ToolEffectiveness])
    { Worker = new GearRoleWorker_MainHand()};
    public static readonly GearRoleDef Head = new("Head", slot: GearSlotDefOf.Head, bones: BoneStructureDefOf.Armor, stats: [StatDefOf.Armor]);
    public static readonly GearRoleDef Feet = new("Feet", slot: GearSlotDefOf.Feet, bones: BoneStructureDefOf.Armor, stats: [StatDefOf.Armor, StatDefOf.WalkSpeed]);
    public static readonly GearRoleDef Torso = new("Torso", slot: GearSlotDefOf.Chest, bones: BoneStructureDefOf.Armor, stats: [StatDefOf.Armor]);
    public static readonly GearRoleDef Legs = new("Legs", slot: GearSlotDefOf.Legs, bones: BoneStructureDefOf.Armor, stats: [StatDefOf.Armor]);

    static GearRoleDefOf()
    {
        Def.Register(typeof(GearRoleDefOf));
    }
}
public class GearRoleWorker
{
    public virtual IEnumerable<LabelNew> GetControls(Entity item) => [];
}

sealed class GearRoleWorker_MainHand : GearRoleWorker
{
    public override IEnumerable<LabelNew> GetControls(Entity item)
    {
        var comp = item.ToolComponent;
        var profile = item.Profile as GearProfileDef;
        var stats = comp.IntrinsicStats;
        if(profile.BaseSpeed.HasValue)
            yield return new LabelNew($"Intrinsic2: {stats[StatDefOf.ToolSpeed].Value} speed");

        if (profile.Damage is not null)
        {
            var magnitude = stats[StatDefOf.ToolEffectiveness].Value;
            yield return new LabelNew($"Intrinsic2: {magnitude} {profile.Damage.LabelReadable}");
        }
        if (profile.ToolUses is not null)
            foreach (var usage in profile.ToolUses)
            {
                var magnitude = stats[StatDefOf.ToolEffectiveness].Value;
                yield return new LabelNew($"Intrinsic2: {magnitude} {usage.LabelReadable}");
            }
        yield break;
    }
}