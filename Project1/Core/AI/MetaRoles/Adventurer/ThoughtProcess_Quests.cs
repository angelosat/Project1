using Project1.Core.AI.Thought;
using Project1.Core.Entities.Actors;
using Project1.Core.Systems.Quests;
using Project1.Core.World.WorldAreas;
using System.Linq;

namespace Project1.Core.AI.MetaRoles.Adventurer;

internal class ThoughtProcess_Quests : ThoughtProcess
{
    internal override void TickOffMap(AIState state)
    {
        var actor = state.Owner;
        if (actor.Net.IsClient)
            return;
        var manager = actor.Net.Map.Town.QuestManagerNew;
        var quests = manager.GetAcceptedQuestsByActor(actor);
        if (!quests.Any())
            return;
        var meta = actor.AI.Meta as RoleAdventurerData;
        var activequestId = meta.ActiveQuest;
        if (activequestId != QuestId.Null)
        {
            var activequest = manager.GetQuest(activequestId);
            if (manager.IsComplete(actor, activequest))
            {
                meta.NextDesiredLoot = null;
                meta.ActiveQuest = QuestId.Null;
                state.Log.Write($"I finished quest {activequest.LabelReadable}");
            }
            return;
        }
        var quest = quests.FirstOrDefault(q => !manager.IsComplete(actor, q));
        if (quest is null)
            return;
        activequestId = quest.Id;
        var qruntime = manager.GetQuest(activequestId);
        meta.ActiveQuest = activequestId;
        quest.Def.Resolver.Tick(actor, quest);
        state.Log.Write($"I set {qruntime.LabelReadable} as my active quest.");
    }

    internal override int GetFrontierScore(Actor actor, FrontierDef frontier)
    {
        return base.GetFrontierScore(actor, frontier);
    }

    internal override void TickOnMap(AIState state)
    {
    }
}
