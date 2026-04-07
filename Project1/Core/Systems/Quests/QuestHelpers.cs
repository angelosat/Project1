using Project1.Core.AI.MetaRoles.Adventurer;
using Project1.Core.Entities.Actors;

namespace Project1.Core.Systems.Quests;

internal static class QuestHelpers
{
    extension(Actor actor)
    {
        public QuestRuntime ActiveQuest 
            => actor.Net.World.MainMap.Town.QuestManagerNew.GetQuest(actor.AI.GetMeta<RoleAdventurerData>().ActiveQuest);
    }
}
