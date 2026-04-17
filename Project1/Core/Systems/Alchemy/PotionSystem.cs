using Project1.Core.Effects;
using Project1.Core.Resources;
using Project1.Core.Systems.Materials;
using Project1.Framework;
using System;
using System.Collections.Generic;
using System.Text;

namespace Project1.Core.Systems.Alchemy
{
    public class AlchemySubstanceDef(string name, MaterialDef mat, MaterialRefinementDef @ref, EffectDef effect, Def effectTarget) : Def(name)
    {
        public readonly MaterialDef Material = mat;
        public readonly MaterialRefinementDef Refinement = @ref;
        public readonly EffectDef Effect = effect;
        public readonly Def Target = effectTarget;
    }

    [EnsureStaticCtorCall]
    public static class AlchemySubstanceDefOf
    {
        public static readonly AlchemySubstanceDef Berry = new("Berry", 
            MaterialDefOf.Berry, 
            MaterialRefinementDefOf.Paste, 
            EffectDefOf.RestoreResource, 
            ResourceDefOf.Health);

        public static readonly AlchemySubstanceDef Human = new("Human",
            MaterialDefOf.Human,
            MaterialRefinementDefOf.Paste,
            EffectDefOf.FortifyResource,
            ResourceDefOf.Health);

        static AlchemySubstanceDefOf()
        {
            Def.Register(typeof(AlchemySubstanceDefOf));
        }
    }

    internal class PotionSystem
    {
    }
}
