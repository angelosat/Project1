using Project1.Core.Blocks.Comps;
using Project1.Core.Helpers;
using Project1.Core.Systems.Materials;
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

        public int Amount { get; private set; }

        internal override IEnumerable<Control> GetInspectorControls()
        {
            if (this.IngredientUsed is null)
                yield break;
            yield return new LabelNew($"Made from: {this.IngredientUsed.LabelReadable} x{this.Amount}");
        }

        internal void SetIngredient(MaterialRefinementDef refinement, int amount)
        {
            this.IngredientUsed = refinement;
            this.Amount = amount;
            this.Map.Events.Post(new BlockEntityCompUpdatedEvent(this));
        }

        public override void Write(IDataWriter w)
        {
            w.Write(this.IngredientUsed);
            w.Write(this.Amount);
        }

        public override ISerializable Read(IDataReader r)
        {
            this.IngredientUsed = r.ReadDef<MaterialRefinementDef>();
            this.Amount = r.ReadInt32();
            return this;
        }

        protected override void SaveExtra(SaveTag tag)
        {
            if(this.IngredientUsed is not null)
                tag.Save("Ingredient", this.IngredientUsed);
            tag.Save("Amount", this.Amount);
        }

        public override void Load(SaveTag tag)
        {
            if (tag.TryLoadDefOut<MaterialRefinementDef>("Ingredient", out var ingr))
                this.IngredientUsed = ingr;
            this.Amount = tag.TryLoadInt("Amount", out var amount) ? amount : 1;
        }

        public new class Spec() : BlockComp.Spec
        {
            public override Type CompType => typeof(BlockBuildingComp);
            public override BlockBuildingComp CreateComp() => new();
        }
    }
}
