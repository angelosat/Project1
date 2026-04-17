using Project1.Core.Effects;
using Project1.Core.Systems.Materials;
using Project1.Framework;
using System.Collections.Generic;

namespace Project1.Core.Systems.Alchemy;

//public class AlchemySubstanceDef(string name, MaterialDef mat, MaterialRefinementDef @ref, EffectDef effect, Def effectTarget) : Def(name)
//{
//    public readonly MaterialDef Material = mat;
//    public readonly MaterialRefinementDef Refinement = @ref;
//    public readonly EffectDef Effect = effect;
//    public readonly Def Target = effectTarget;
//}

//[EnsureStaticCtorCall]
//public static class AlchemySubstanceDefOf
//{
//    public static readonly AlchemySubstanceDef Berry = new("Berry", 
//        MaterialDefOf.Berry, 
//        MaterialRefinementDefOf.Paste, 
//        EffectDefOf.RestoreResource, 
//        ResourceDefOf.Health);

//    public static readonly AlchemySubstanceDef Animal = new("Animal",
//        MaterialDefOf.Animal,
//        MaterialRefinementDefOf.Paste,
//        EffectDefOf.RestoreResource,
//        ResourceDefOf.Mana);

//    public static readonly AlchemySubstanceDef Human = new("Human",
//        MaterialDefOf.Human,
//        MaterialRefinementDefOf.Paste,
//        EffectDefOf.FortifyResource,
//        ResourceDefOf.Health);

//    public static readonly AlchemySubstanceDef Insect = new("Insect",
//        MaterialDefOf.Insect,
//        MaterialRefinementDefOf.Paste,
//        EffectDefOf.FortifyResource,
//        ResourceDefOf.Stamina);

//    static AlchemySubstanceDefOf()
//    {
//        Def.Register(typeof(AlchemySubstanceDefOf));
//    }
//}

[EnsureStaticCtorCall]
internal class PotionSystem
{
    static readonly Dictionary<(EffectDef effect, Def target), List<MaterialDef>> _matsByEffect = [];
    static public IEnumerable<(EffectDef effect, Def target)> Recipes => _matsByEffect.Keys;

    static PotionSystem()
    {
        foreach (var s in MaterialSystem.MaterialsByType)
        {
            if (s.Key.AlchemyEffect is not EffectDef fx)
                continue;
            foreach (var m in s.Value)
            {
                if (m.AlchemyTarget is not Def target)
                    continue;
                var key = (fx, target);
                if (!_matsByEffect.TryGetValue(key, out var list))
                    _matsByEffect[key] = list = [];
                list.Add(m);
            }
        }
    }
}
