using Project1.Core.AI.Thought;
using Project1.Core.Systems.Quests;
using System.Linq;

namespace Project1.Core.AI.MetaRoles.Adventurer;

internal class ThoughtQuests : ThoughtProcess
{
    public override void TickOffMap(AIState state)
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
            return;
        var quest = quests.First();
        activequestId = quest.Id;
        var qruntime = manager.GetQuest(activequestId);
        meta.ActiveQuest = activequestId;
        quest.Def.Resolver.Tick(actor, quest);
        state.Log.Write($"I set {qruntime.LabelReadable} as my active quest.");
    }

    public override void TickOnMap(AIState state)
    {
    }
}
