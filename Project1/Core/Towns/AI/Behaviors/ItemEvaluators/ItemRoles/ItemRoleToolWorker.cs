using Project1.Core.Entities;
using Project1.Core.Entities.Actors;
using Project1.Core.Resources;
using Project1.Core.Stats;
using Project1.Core.Tools;

namespace Project1.Core.Towns.AI.Behaviors.ItemEvaluators.ItemRoles
{
    sealed class ItemRoleToolWorker : ItemRoleWorker
    {
        public override int GetSituationalScore(Actor actor, Entity item, ItemRoleDef role)
        {
            var task = actor.AI.State.Behavior?.Plan;
            if (task is null)
                return 0;
            var interaction = task.Def.Interaction;
            if (interaction is null)
                return 0;
            var durability = item.Resources.View(ResourceDefOf.Durability);
            if (durability.Value == 0)
                return -100;
            //if (durability.Percentage < 1)
            //        return (int)(100 * (durability.Percentage - 1));
            var tooluse = interaction.ToolUse;
            if (tooluse == role.Def)
                return 100;
            return -100;
        }
        
        public override int GetInventoryScore(Actor actor, Entity item, ItemRoleDef role)
        {
            if (item.Def != ItemDefOf.Tool)
                return -1;
            if (item.Profile is not ToolProfileDef toolProfile)
                return -1;
            if (toolProfile.ToolUse != role.Def)
                return -1;
            return (int)StatDefOf.ToolEffectiveness.CalculateFor(item);
        }
    }
}