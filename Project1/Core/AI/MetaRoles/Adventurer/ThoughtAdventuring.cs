using Project1.Core.AI.Thought;
using Project1.Core.Towns.AI.Needs;

namespace Project1.Core.AI.MetaRoles.Adventurer;

internal sealed class ThoughtAdventuring : ThoughtProcess
{
    internal override void TickOffMap(AIState state)
    {
        //var needDelta = (actor.IsSpawned ? -1 : 1) / (float)Ticks.PerGameMinute;
        //actor.Needs.ApplyAccumulatorDelta(AdventurerNeedsDefOf.Adventuring, needDelta);
        state.Owner.Needs.ApplyAccumulatorDelta(AdventurerNeedsDefOf.Adventuring, 1f / Ticks.PerGameMinute);

        //var meta = state.Owner.AI.Meta as RoleAdventurerData;
        //var activequest = state.Owner.Net.Map.Town.QuestManagerNew.GetQuest(meta.ActiveQuest);

    }

    internal override void TickOnMap(AIState state)
    {
        state.Owner.Needs.ApplyAccumulatorDelta(AdventurerNeedsDefOf.Adventuring, -1f / Ticks.PerGameMinute);
    }
}
