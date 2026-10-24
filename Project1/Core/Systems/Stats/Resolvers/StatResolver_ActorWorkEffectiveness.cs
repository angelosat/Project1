using Project1.Core.Entities.Actors;
using Project1.Core.Systems.Tools;
using System.Diagnostics;
using System.Linq;

namespace Project1.Core.Entities.Stats.Resolvers
{
    sealed class StatResolver_ActorWorkEffectiveness : StatResolver
    {
        public StatResolver_ActorWorkEffectiveness(ToolUseDef toolUse)
        {
            ToolUse = toolUse;
        }

        public StatResolver_ActorWorkEffectiveness()
        {

        }
        readonly ToolUseDef ToolUse;
        public override float CalculateItemStat(Entity owner)
        {
            throw new UnreachableException();
            var actor = (Actor)owner;
            var skill = actor.Skills[ToolUse.Skill];
            return skill.Level * .01f;
        }
        public override float CalculateActorStat(StatDef def, Actor actor)
        {
            var skill = actor.Skills[(def.TargetDef as ToolUseDef).Skill];
            var fromGear = actor.Gear.GetModifier(def);
            return skill.Level + fromGear;
        }

        internal override string Format(StatDef def, Entity item)
        {
            return $"{ToolUse.LabelReadable} effectiveness";
        }
    }
}
