using Project1.Core.Entities;
using Project1.Core.Entities.Actors;
using Project1.Core.Systems.Effects;
using Project1.Core.Systems.Materials;
using System;

namespace Project1.Core.Needs;

public static class HungerUtility
{
    public static int GetNutrition(Actor actor, Entity item)
        => item.Def == ItemDefOf.Ingredient ? GetNutrition(actor, item.PrimaryMaterial) : 0;
    
    public static int GetNutrition(Actor actor, MaterialDef mat)
    {
        var profile = actor.Profile as ActorDnaDef;
        if (!profile.Diet.Contains(mat.Type))
            return 0;
        return mat.Tier * 10;
    }

    public static void ActorDigesting(Actor actor, Entity item)
    {
        var nutrition = GetNutrition(actor, item);
        var effect = new EntityEffectWrapper(EffectDefOf.ModifyNeed, NeedDefOf.Hunger, budget: nutrition, ticksPerUnit: 0);
        actor.Effects.Apply(effect);
        item.Consume(1);
    }
}
