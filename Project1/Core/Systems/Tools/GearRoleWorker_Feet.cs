using Project1.Core.Entities;
using Project1.Core.Stats;

namespace Project1.Core.Systems.Tools;

sealed class GearRoleWorker_Feet : GearRoleWorker
{
    public override void InitIntrinsicStats(Entity item)
    {
        var comp = item.ToolComponent;
        var armor = StatDefOf.Armor.Worker.CalculateItemStat(item);
        var speed = StatDefOf.MoveSpeed.Worker.CalculateItemStat(item);
        comp.IntrinsicStats[StatDefOf.Armor] = new(StatDefOf.Armor, armor);
        comp.IntrinsicStats[StatDefOf.MoveSpeed] = new(StatDefOf.MoveSpeed, speed);
    }
}
