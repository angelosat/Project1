using Project1.Core.Entities;
using Project1.Core.Systems.Consumables;
using Project1.Core.Systems.Effects;
using Project1.Core.Systems.Materials;
using Project1.Core.Systems.Quality;
using Project1.Framework;
using System.Collections.Generic;
using System.Linq;

#nullable enable

namespace Project1.Core.Systems.Alchemy;

[EnsureStaticCtorCall]
internal static class PotionSystem
{
    static readonly Dictionary<(EffectDef effect, Def target), List<MaterialDef>> _matsByEffect = [];
    static public IEnumerable<(EffectDef effect, Def target)> Recipes => _matsByEffect.Keys;

    static public MaterialDef GetMaterialRequired(EffectDef effect, Def target) => _matsByEffect[(effect, target)].First();

    static PotionSystem()
    {
        CacheRecipes();
    }

    private static void CacheRecipes()
    {
        foreach (var s in MaterialSystem.MaterialsByType)
        {
            if (s.Key.AlchemyEffect is not EffectDef fx)
                continue;
            foreach (var m in s.Value)
            {
                if (m.AlchemyTarget is not Def target)
                    continue;
                if (!fx.TargetDefType.IsAssignableFrom(target.GetType()))
                    throw new System.Exception();
                var key = (fx, target);
                if (!_matsByEffect.TryGetValue(key, out var list))
                    _matsByEffect[key] = list = [];
                list.Add(m);
            }
        }
    }

    //public static Entity Create(EffectDef effect, Def target, byte level)
    //{
    //    var potion = ItemDefOf.Consumable.Create(ConsumableDefOf.Potion);
    //    var comp = potion.GetComponent<ConsumableComp>();
    //    var wrapper = new EntityEffectWrapper(effect, target, effect.BaseMagnitude, 1, effect.BaseDuration);
    //    comp.Tier = level;
    //    comp.Add(wrapper);
    //    potion.Name = $"{ConsumableDefOf.Potion.LabelReadable} of {effect.Verb} {target.LabelReadable}";
    //    return potion;
    //}

    public static void PostProcess(Entity entity)
    {
        var mat = entity.Body.Material;
        var effect = mat.Type.AlchemyEffect;
        var target = mat.AlchemyTarget;
        // todo calculate potion magnitude/level from item quality
        var quality = entity.QualityComp.Tier;
        var qualityMod = quality.Multiplier;
        //var finalMagnitude = effect.BaseMagnitude * (1 + quality.Multiplier / 100f);
        var finalMagnitude = effect.BaseMagnitude * quality.Multiplier;
        var wrapper = new EntityEffectWrapper(effect, target, effect.BaseMagnitude, 1, effect.BaseDuration);
        var comp = entity.GetComponent<ConsumableComp>();
        comp.Add(wrapper);
        entity.Name = GetName(comp);
        entity.Body.TintΟverride = effect.Worker.GetTint(target);
    }

    internal static string GetName(ConsumableComp comp)
    {
        var effect = comp.EffectsNew.First();
        return $"{ConsumableDefOf.Potion.LabelReadable} of {effect.Def.Verb} {effect.Target.LabelReadable}";
    }

    internal static IEnumerable<GameObject> GenerateTemplates()
    {
        //foreach (var (effect, target) in Recipes)
        //    yield return Create(effect, target, 1);
        foreach (var recipe in _matsByEffect)
            foreach (var mat in recipe.Value)
                yield return ConsumableSystem.Create(ConsumableDefOf.Potion, mat, QualitySystem.Random);
    }

    public static Entity Create(EffectDef effect, Def target, QualityDef? quality)
        => ConsumableSystem.Create(ConsumableDefOf.Potion, GetMaterialRequired(effect, target), quality ?? QualitySystem.Random);
}
