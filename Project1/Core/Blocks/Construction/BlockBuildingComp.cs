using Project1.Core.Blocks.Comps;
using Project1.Core.Helpers;
using Project1.Core.Materials;
using Project1.Framework;
using Project1.Framework.Serialization;
using Project1.Framework.UI;
using System;
using System.Collections.Generic;

namespace Project1.Core.Blocks.Construction
{
    internal class BlockBuildingComp : BlockComp
    {
        public override BlockCompDef CompDef => BlockCompDefOf.Building;

        public MaterialRefinementDef IngredientUsed { get; private set; }

        internal override IEnumerable<Control> GetInspectorControls()
        {
            yield return new LabelNew($"Ingredient: {(this.IngredientUsed.LabelReadable ?? "<null>")}");
        }

        internal void SetIngredient(MaterialRefinementDef refinement)
        {
            this.IngredientUsed = refinement;
            this.Map.Events.Post(new BlockEntityCompUpdatedEvent(this));
        }

        public override void Write(IDataWriter w)
        {
            w.Write(this.IngredientUsed);
        }

        public override ISerializable Read(IDataReader r)
        {
            this.IngredientUsed = r.ReadDef<MaterialRefinementDef>();
            return this;
        }

        protected override void SaveExtra(SaveTag tag)
        {
            if(this.IngredientUsed is not null)
                tag.Save("Ingredient", this.IngredientUsed);
        }

        public override void Load(SaveTag tag)
        {
            if (tag.TryLoadDefOut<MaterialRefinementDef>("Ingredient", out var ingr))
                this.IngredientUsed = ingr;
        }

        public new class Spec() : BlockComp.Spec
        {
            public override Type CompType => typeof(BlockBuildingComp);
            public override BlockBuildingComp CreateComp() => new();
        }
    }
}
