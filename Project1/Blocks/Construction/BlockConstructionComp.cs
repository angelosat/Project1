using Start_a_Town_.Net;
using Start_a_Town_.UI;
using System;
namespace Start_a_Town_
{
    [EnsureStaticCtorCall]
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
            container.AddControls(new Label($"Materials: {this.Args} {this.Fulfilment}"));
        }
        IngredientFulfilment Fulfilment;
        ConstructionDesignationArgs Args;
        internal Progress Progress = new();

        public (MaterialRefinementDef refinement, MaterialDef material) Requirement => (this.Args.Refinement, this.Args.Material);
        public bool IsReady => this.Fulfilment.Current >= this.Fulfilment.Required;
        public int Missing => this.Fulfilment.Missing;
        public void SetArgs(ConstructionDesignationArgs args)
        {
            this.Block = args.Block;
            var ingredientCount = this.Block.Size.Volume * ItemDefOf.Ingredient.StackCapacity / this.Block.ConstructionProfile.Dimension;
            this.Fulfilment.Required = ingredientCount;
            this.Args = args;
        }
       
        internal void Deposit(Entity entity, int quantity)
        {
            if (entity.Def != ItemDefOf.Ingredient)
                throw new ArgumentException($"deposited {entity} in construction is not a {ItemDefOf.Ingredient}");
            if (entity.Profile != this.Args.Refinement)
                throw new ArgumentException($"deposited {entity} in construction is not a {this.Args.Refinement}");
            if (quantity > this.Fulfilment.Missing)
                throw new ArgumentException($"deposited quantity: {quantity} larger than missing quantity: {this.Fulfilment.Missing}");
            this.Fulfilment.Current += quantity;
            entity.Consume(quantity);

            // solidify the designation into a construction block 
            foreach (var cell in this.Parent.CellsOccupied)
                this.Map.SetBlock(cell, BlockDefOf.Construction, this.Args.Material, 0, 0, this.Args.Orientation);

            this.ValidateReadiness();
        }

        private void ValidateReadiness()
        {
            if (this.IsReady)
            {
                this.Map.Events.Post(new ConstructionReadyEvent(this));
                //this.Progress = new();
            }
        }

        internal bool Accepts(Entity entity)
        {
            return 
                this.Fulfilment.Missing > 0 &&
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

        public void Advance(int work)
        {
            if (!this.IsReady)
                throw new InvalidOperationException("Tried to advance construction without all materials present");
            this.Progress.Add(work);
            if (this.Progress.IsFinished)
                this.Complete();
            return;
        }

        public void Complete()
        {
            var map = this.Parent.Map;
            map.Events.Post(new ConstructionFinishedEvent(this));

            foreach (var cell in this.Parent.CellsOccupied)
                map.SetBlock(cell, this.Args.Block, this.Args.Material, 0, 0, this.Args.Orientation);
            map.RemoveBlockEntity(this.Parent);
        }
        public override void Write(IDataWriter w)
        {
            w.Write(this.Fulfilment.Current);
            w.Write(this.IsReady);
            if (this.IsReady)
                w.Write(this.Progress.Value);
        }
        public override ISerializable Read(IDataReader r)
        {
            this.Fulfilment.Current = r.ReadInt32();
            if(r.ReadBoolean())
            {
                if (this.Progress is null)
                    new Progress();
                this.Progress.Value = r.ReadSingle();
            }
            return this;
        }

        struct IngredientFulfilment
        {
            internal int Required, Current;
            public readonly int Missing => Required - Current;
            public override readonly string ToString() => $"{this.Current} / {this.Required}";
        }
    }
}
