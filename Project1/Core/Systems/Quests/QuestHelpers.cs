using Project1.Core.AI.MetaRoles.Adventurer;
using Project1.Core.Entities.Actors;
using System;
using System.Collections.Generic;
using System.Text;

namespace Project1.Core.Systems.Quests
{
    internal static class QuestHelpers
    {
        extension(Actor actor)
        {
            public QuestRuntime ActiveQuest 
                => actor.Net.Map.Town.QuestManagerNew.GetQuest(actor.AI.GetMeta<RoleAdventurerData>().ActiveQuest);
        }
    }
}
