using Project1.Core.Entities.Actors;
namespace Project1.Core.Systems.Quests;

public abstract class QuestResolver
{
    public abstract void Tick(Actor actor, QuestRuntime quest);
}
