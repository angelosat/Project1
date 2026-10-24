using Project1.Core.Entities.Actors;

namespace Project1.Core.Entities.Stats.Resolvers;

public class StatResolver
{
    public StatResolver()
    {
        
    }

    public virtual float CalculateItemStat(Entity item) => 1f;
    public virtual float CalculateActorStat(StatDef def, Actor actor) => def.BaseValue;

    public virtual string GetName(StatDef def, Entity item) => def.LabelReadable;

    internal virtual bool IsApplicable(StatDef statDef, ActorDnaDef actorDef) => true;

    internal virtual string Format(StatDef def, Entity item)
        => def.LabelReadable;
}
