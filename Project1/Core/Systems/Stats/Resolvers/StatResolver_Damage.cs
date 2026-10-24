using Project1.Core.Systems.Tools;
using System.Linq;

namespace Project1.Core.Entities.Stats.Resolvers
{
    sealed class StatResolver_Damage : StatResolver
    {
        public override string GetName(StatDef def, Entity item)
        {
            var profile = item.Profile as GearProfileDef;
            return $"{profile.Damage.LabelReadable} {def.LabelReadable}";
        }
    }
}
