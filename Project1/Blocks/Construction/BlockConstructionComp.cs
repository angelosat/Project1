using Start_a_Town_.UI;
using System;
using System.Collections.ObjectModel;
namespace Start_a_Town_
{
    public class BlockConstructionComp : BlockEntityComp
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
            container.AddControls(new Label($"Construction: {this.Block}"));
            container.AddControls(new Label($"Materials: {this.Args} {this.Fulfiment}"));
        }
        IngredientFulfilment Fulfiment;
        ConstructionDesignationArgs Args;
        public (MaterialRefinementDef refinement, MaterialDef material) Requirement => (this.Args.Refinement, this.Args.Material);
        public bool IsReady => this.Fulfiment.Current >= this.Fulfiment.Required;
        public int Missing => this.Fulfiment.Missing;
        public void SetArgs(ConstructionDesignationArgs args)
        {
            this.Block = args.Block;
            var ingredientCount = this.Block.Size.Volume * ItemDefOf.Ingredient.StackCapacity / this.Block.ConstructionProfile.Dimension;
            this.Fulfiment.Required = ingredientCount;
            this.Args = args;
        }
        internal void Deposit(Entity entity, int quantity)
        {
            if (entity.Def != ItemDefOf.Ingredient)
                throw new ArgumentException($"deposited {entity} in construction is not a {ItemDefOf.Ingredient}");
            if (entity.Profile != this.Args.Refinement)
                throw new ArgumentException($"deposited {entity} in construction is not a {this.Args.Refinement}");
            if (quantity > this.Fulfiment.Missing)
                throw new ArgumentException($"deposited quantity: {quantity} larger than missing quantity: {this.Fulfiment.Missing}");
            this.Fulfiment.Current += quantity;
            entity.Consume(quantity);
            //if (entity.IsEmpty)
            //    this.Map.World.DisposeEntityAndSync(entity);
            if (this.IsReady)
                this.Map.Events.Post(new ConstructionReadyEvent(this));
        }

        internal bool Accepts(Entity entity)
        {
            return 
                this.Fulfiment.Missing > 0 &&
                entity.Def == ItemDefOf.Ingredient &&
                entity.Profile == this.Args.Refinement &&
                entity.PrimaryMaterial == this.Args.Material;
        }

        internal override bool TryConsume(Entity item)
        {
            if (!this.Accepts(item))
                return false;
            this.Deposit(item, item.StackSize);
            return true;
        }

        struct IngredientFulfilment
        {
            internal int Required, Current;
            public readonly int Missing => Required - Current;
            public override readonly string ToString() => $"{this.Current} / {this.Required}";
        }
    }
}
