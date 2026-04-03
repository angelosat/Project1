using Project1.Core.AI.MetaRoles.Adventurer;
using Project1.Core.Entities.Actors;
using Project1.Core.Simulation;
using Project1.Core.World.WorldAreas;

namespace Project1.Core.Systems.Quests;

internal sealed class QuestResolver_Deliver : QuestResolver
{
    public override void Tick(Actor actor, QuestRuntime quest)
    {
        var typedQ = (QuestRuntime_Deliver)quest;
        var meta = actor.AI.GetMeta<RoleAdventurerData>();
        meta.NextDesiredLoot = typedQ.Key;

        //var tier = typedQ.Material.Tier;
        //var targetFrontier = FrontierManager.GetFrontier(tier);
        //meta.SetTargetFrontier(targetFrontier);
    }
}
