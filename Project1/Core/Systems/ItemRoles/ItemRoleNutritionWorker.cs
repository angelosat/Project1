using Project1.Core.Entities;
using Project1.Core.Needs;
using Project1.Core.Entities.Actors;
using System.Linq;

namespace Project1.Core.Systems.ItemRoles
{
    class ItemRoleNutritionWorker : ItemRoleWorker
    {
        public override int GetSituationalScore(Actor actor, Entity item, ItemRoleDef role)
        {
            if (item.Def != ItemDefOf.Ingredient)
                return -100;
            if (!((ActorDnaDef)actor.Profile).Diet.Contains(item.PrimaryMaterial.Type))
                return -100;
            if (actor.Needs.GetPercentage(NeedDefOf.Hunger) > .9f)
                return -100;
            var nutrition = HungerUtility.GetNutrition(actor, item.PrimaryMaterial);
            if (nutrition <= 0)
                return -100;
            var needDeficit = actor.Needs.GetDeficit(NeedDefOf.Hunger);
            return nutrition * needDeficit;
        }
        public override int GetInventoryScore(Actor actor, Entity item, ItemRoleDef role)
            => HungerUtility.GetNutrition(actor, item) * item.StackMax;
    }
}