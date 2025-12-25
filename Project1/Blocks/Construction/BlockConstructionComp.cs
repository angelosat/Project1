using Start_a_Town_.UI;
using System;
using System.Configuration;
namespace Start_a_Town_
{
    internal class BlockConstructionComp : BlockEntityComp
    {
        internal new class Spec : BlockEntityComp.Spec
        {
            public override Type CompType => typeof(BlockConstructionComp);

            public override BlockEntityComp CreateComp()
            {
                return new BlockConstructionComp();
            }
        }
        public override string Name => $"{this}";

        public Block Block;
        internal override void GetSelectionInfo(Control container)
        {
            container.AddControls(new Label(this.Block));
        }
        IngredientFulfilment Fulfiment;
        public void SetArgs(ConstructionDesignationArgs args)
        {
            this.Block = args.Block;
            var ingredientCount = this.Block.Size.Volume * ItemDefOf.Ingredient.StackCapacity / this.Block.ConstructionProfile.Dimension;
            this.Fulfiment.Required = ingredientCount;
        }
        struct IngredientFulfilment
        {
            internal int Required, Current;
        }
    }
}
