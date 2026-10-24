using Project1.Core.Entities.Actors;
using Project1.Core.Systems.Tools;

namespace Project1.Core.Entities.Stats.Resolvers;

sealed class StatResolver_ActorUnarmedWorkAmount : StatResolver
{
    readonly ToolUseDef ToolUse;

    public StatResolver_ActorUnarmedWorkAmount(ToolUseDef toolUse)
    {
        this.ToolUse = toolUse;
    }

    public StatResolver_ActorUnarmedWorkAmount()
    {

    }
    public override float CalculateItemStat(Entity owner)
    {
        var actor = (Actor)owner;
        var skill = actor.Skills[ToolUse.Skill];
        return skill.Level * .01f;
    }
    public override float CalculateActorStat(StatDef def, Actor actor)
    {
        return 0f;
    }

    internal override string Format(StatDef def, Entity item)
    {
        return $"Unarmed {this.ToolUse.LabelReadable}";
    }
}
