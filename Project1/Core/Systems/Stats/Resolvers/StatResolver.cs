using Project1.Core.Entities.Actors;

namespace Project1.Core.Entities.Stats.Resolvers;

public abstract class StatResolver
{
    public abstract float CalculateStat(Entity item);
    public virtual float CalculateStatActor(StatDef def, Actor actor) => def.BaseValue;

    internal virtual string Format(StatDef def, Entity item)
        => def.LabelReadable;
}
