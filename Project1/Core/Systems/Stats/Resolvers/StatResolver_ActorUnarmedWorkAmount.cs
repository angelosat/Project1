using Project1.Core.Entities.Actors;
using Project1.Core.Systems.Tools;
using System.Linq;

namespace Project1.Core.Entities.Stats.Resolvers
{
    sealed class StatResolver_ActorUnarmedWorkAmount : StatResolver
    {
        public StatResolver_ActorUnarmedWorkAmount(ToolUseDef toolUse)
        {
            ToolUse = toolUse;
        }

        public StatResolver_ActorUnarmedWorkAmount()
        {

        }
        readonly ToolUseDef ToolUse;
        public override float CalculateStat(Entity owner)
        {
            var actor = (Actor)owner;
            var skill = actor.Skills[ToolUse.Skill];
            return skill.Level * .01f;
        }
        public override float CalculateStatActor(StatDef def, Actor actor)
        {
            return 1f;
        }

        internal override string Format(StatDef def, Entity item)
        {
            return $"Unarmed {ToolUse.LabelReadable}";
        }
    }
}
