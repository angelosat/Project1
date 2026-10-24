using Project1.Core.Entities;
using Project1.Core.Stats;
using Project1.Framework.UI;
using System.Collections.Generic;

namespace Project1.Core.Systems.Tools;

sealed class GearRoleWorker_MainHand : GearRoleWorker
{
    public override void InitIntrinsicStats(Entity item)
    {
        var comp = item.ToolComponent;
        var power = StatDefOf.ItemPower.Worker.CalculateItemStat(item);
        var speed = StatDefOf.ItemSpeed.Worker.CalculateItemStat(item);
        comp.IntrinsicStats[StatDefOf.ItemPower] = new(StatDefOf.ItemPower, power);
        comp.IntrinsicStats[StatDefOf.ItemSpeed] = new(StatDefOf.ItemSpeed, speed);
    }
    public override IEnumerable<LabelNew> GetControls(Entity item)
    {
        var comp = item.ToolComponent;
        var profile = item.Profile as GearProfileDef;
        var stats = comp.IntrinsicStats;
        //if (profile.BaseSpeed.HasValue)
        //    yield return new LabelNew($"{stats[StatDefOf.ItemSpeed].Value.ToString(StatDefOf.ItemSpeed.Format)} {StatDefOf.ItemSpeed.LabelReadable}");

        if (profile.Damage is not null)
        {
            var magnitude = stats[StatDefOf.ItemPower].Value;
            //var magnitude = stats[StatDefOf.Damage].Value;
            //yield return new LabelNew($"Intrinsic2: {magnitude.ToString(StatDefOf.ItemPower.Format)} {profile.Damage.LabelReadable}");
            yield return new LabelNew($"{magnitude.ToString(StatDefOf.ItemPower.Format)} {profile.Damage.LabelReadable}");
        }
        if (profile.ToolUses is not null)
            foreach (var usage in profile.ToolUses)
            {
                //var magnitude = stats[StatDefOf.ToolEffectiveness].Value;
                //var profileMod = profile.ToolUsesModifiers[usage];
                //yield return new LabelNew($"Intrinsic2: {magnitude * profileMod} {usage.LabelReadable}");
                var final = ToolSystem.GetFinalValue(item, usage);
                //yield return new LabelNew($"Intrinsic2: {final.ToString(StatDefOf.ItemPower.Format)} {usage.LabelReadable}");
                yield return new LabelNew($"{final.ToString(StatDefOf.ItemPower.Format)} {usage.LabelReadable}");
            }
        yield break;
    }
}