using Project1.Core.Entities;
using System;

namespace Project1.Core.Systems.Materials
{
    internal class EntityComp_Ingredient : EntityComp
    {
        public override EntityCompDef CompDef => EntityCompDefOf.Ingredient;

        public override string Name => "Ingredient";

        internal override void PostLoad()
        {
            MaterialSystem.PostLoad(this);
        }
    }
}
