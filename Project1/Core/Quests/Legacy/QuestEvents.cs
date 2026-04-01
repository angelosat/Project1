using Project1.Framework.Events;

namespace Project1.Core.Quests.Legacy
{
    internal record struct QuestDefsUpdatedEvent(QuestDef[] Added, QuestDef[] Removed) : IEventPayload { }
    internal record struct QuestDefAssignedEvent(QuestDef Quest) : IEventPayload { }
}
