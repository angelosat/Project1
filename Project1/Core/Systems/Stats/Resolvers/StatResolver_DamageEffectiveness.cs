using Project1.Core.Animations;
using Project1.Core.Systems.Quality;
using Project1.Core.Systems.Tools;
using System.Linq;

namespace Project1.Core.Entities.Stats.Resolvers
{
    sealed class StatResolver_DamageEffectiveness : StatResolver
    {
        public StatResolver_DamageEffectiveness()
        {

        }
        public override float CalculateStat(Entity owner)
        {
            var material = owner.GetMaterial(BoneDefOf.ToolHead);
            if (material is null)
                return 1; // is it ever possible for this to be null?
            var role = (owner.Profile as GearProfileDef).Role;
            return material.Density * owner.QualityComp.Tier.Multiplier;
        }

        internal override string Format(StatDef def, Entity item)
        {
            //var comp = item.GetComponent<ToolComp>();
            var profile = item.Profile as GearProfileDef;
            return $"{profile.ToolUse.LabelReadable} effectiveness";
        }
    }
}
