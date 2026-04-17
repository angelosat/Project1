using Project1.Core.Effects;
using Project1.Core.Systems.Materials;
using Project1.Framework;
using System.Collections.Generic;

namespace Project1.Core.Systems.Alchemy;

[EnsureStaticCtorCall]
internal class PotionSystem
{
    static readonly Dictionary<(EffectDef effect, Def target), List<MaterialDef>> _matsByEffect = [];
    static public IEnumerable<(EffectDef effect, Def target)> Recipes => _matsByEffect.Keys;

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
}
