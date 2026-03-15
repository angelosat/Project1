using Project1.Core.Entities;
using Project1.Core.Needs;
using Project1.Core.Entities.Actors;
using System.Linq;

namespace Project1.Core.Towns.AI.Behaviors.ItemEvaluators.ItemRoles
{
    class ItemRoleNutritionWorker : ItemRoleWorker
    {
        public override int GetSituationalScore(Actor actor, Entity item, ItemRoleDef role)
        {
            var needDef = NeedDefOf.Hunger;
            if (!((ActorDnaDef)actor.Profile).Diet.Contains(item.PrimaryMaterial.Type))
                return -100;
            if (actor.Needs.GetPercentage(needDef) > .9f)
                return -100;
            var nutrition = HungerUtility.GetNutrition(actor, item.PrimaryMaterial);
        
            if (nutrition <= 0)
                return -100;
            var need = actor.GetNeed(needDef);
            var needDeficit = need.Max - need.Value;// need.Deficit;
            return (int)(nutrition * needDeficit);
        }
        public override int GetInventoryScore(Actor actor, Entity item, ItemRoleDef role)
            => HungerUtility.GetNutrition(actor, item) * item.StackMax;
    }
}