using Project1.Core.Entities;
using Project1.Core.Stats;

namespace Project1.Core.Systems.Tools;

sealed class GearRoleWorker_Apparel : GearRoleWorker
{
    public override void InitIntrinsicStats(Entity item)
    {
        var comp = item.ToolComponent;
        var power = StatDefOf.Armor.Worker.CalculateItemStat(item);
        comp.IntrinsicStats[StatDefOf.Armor] = new(StatDefOf.Armor, power);
    }
}
