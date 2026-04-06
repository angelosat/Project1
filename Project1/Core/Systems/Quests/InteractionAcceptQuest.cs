using Project1.Core.Interactions;

namespace Project1.Core.Systems.Quests;

sealed class InteractionCompleteQuest : InteractionLogic
{
    sealed class Context : InteractionContext { }
    protected override InteractionContext CreateContextInt() => new Context();
    internal override void OnFinish(Interaction i)
    {
        var actor = i.Actor;
        if (actor.Net.IsClient)
            return;
        var map = actor.Map;
        var manager = map.Town.QuestManagerNew;
        InteractionHelpers.TryDepositCarriedItemInsideBlockOrSpawn(actor, i.Target.Global);
        var reward = manager.CompleteNextQuest(actor, i.Target.Global);
        actor.Inventory.HaulNew(reward, reward.StackSize);
    }
}
sealed class InteractionAcceptQuest : InteractionLogic
{
    sealed class Context : InteractionContext { }
    protected override InteractionContext CreateContextInt() => new Context();
    internal override void OnFinish(Interaction i)
    {
        var actor = i.Actor;
        if (actor.Net.IsClient)
            return;
        var map = actor.Map;
        var manager = map.Town.QuestManagerNew;
        manager.TryAcceptAllQuests(i.Target.Global, actor);
    }
}
