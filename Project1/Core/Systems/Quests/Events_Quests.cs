using Project1.Core.Systems.Materials;
using Project1.Framework;
using Project1.Framework.Events;

namespace Project1.Core.Systems.Quests;

record struct PlayerRequestQuestCreationEvent(MapId MapId, MaterialRefinementDef RefinementDef, MaterialDef MaterialDef) : IEventPayload { }
record struct PlayerRequestQuestDeletionEvent(MapId MapId, QuestId Id) : IEventPayload { }
record struct QuestAssignedEvent(IntVec3 Board, EntityRefId ActorId, QuestId[] Quests) : IEventPayload { }
record struct QuestCompleteEvent(EntityRefId ActorId, QuestId QuestId) : IEventPayload { }
