using Project1.Core.Entities;
using Project1.Core.Entities.Actors;
using Project1.Core.Needs;
using System.Collections.Generic;
using System.Linq;

namespace Project1.Core.Systems.ItemRoles;

sealed class ItemRoleNutritionWorker : ItemRoleWorker
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

    public override IEnumerable<Def> GetValidTargetDefs()
        => [null];

    //public override IEnumerable<ItemRoleKey> GenerateKeys()
    //{
    //    yield return new ItemRoleKey_Nutrition();
    //}

    //public override int GetInventoryScore(Actor actor, Entity item, ItemRoleKey key)
    //{
    //    throw new System.NotImplementedException();
    //}

    //public override int GetSituationalScore(Actor actor, Entity item, ItemRoleKey key)
    //{
    //    throw new System.NotImplementedException();
    //}
}