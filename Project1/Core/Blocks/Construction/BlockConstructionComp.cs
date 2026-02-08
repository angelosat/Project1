using Project1.Core.Entities;
using Project1.Core.Base;
using Project1.Core.Blocks;
using Project1.Core.Helpers;
using Project1.Core.Interfaces;
using Project1.Core.Materials;
using System;
using Project1.Core.Simulation;
using Project1.Framework.IO;
using Project1.Framework.UI;
namespace Project1.Core
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

        public Block Block => this.Args.Block.Worker;
        internal override void GetSelectionInfo(Control container)
        {
            container.AddControls(new Label($"Materials: {this.Fulfillment} {this.Args}"));
        }
        internal ConstructionDesignationArgs Args { get; private set; }
        internal ProgressInt Progress, Fulfillment;

        public (MaterialRefinementDef refinement, MaterialDef material) Requirement => (this.Args.Refinement, this.Args.Material);
        public bool IsReady => this.Fulfillment.IsFinished;
        public int Missing => this.Fulfillment.Missing;
        public void SetArgs(ConstructionDesignationArgs args)
        {
            //this.Block = args.Block.Worker;
            //var ingredientCount = this.Block.Size.Volume * ItemDefOf.Ingredient.StackCapacity / this.Block.ConstructionProfile.Dimension;
            this.Args = args;
            this.Parent.Name = $"Construction: {this.Args.Block.LabelReadable}";

            var ingredientCount = this.Block.Size.Volume / this.Block.BlockDef.ConstructionProfile.Dimension;
            this.Fulfillment = new(ingredientCount);
            this.Progress = new(100);
        }
       
        internal void Deposit(Entity entity, int quantity)
        {
            if (entity.Def != ItemDefOf.Ingredient)
                throw new ArgumentException($"deposited {entity} in construction is not a {ItemDefOf.Ingredient}");
            if (entity.Profile != this.Args.Refinement)
                throw new ArgumentException($"deposited {entity} in construction is not a {this.Args.Refinement}");

            // only take what i need
            quantity = Math.Min(quantity, this.Missing);
            this.Fulfillment.ApplyDelta(quantity);
            entity.Consume(quantity);
            var args = this.Args;
            // solidify the designation into a construction block 
            //foreach (var cell in this.Parent.CellsOccupied)
            //    this.Map.SetBlock(cell, BlockDefOf.Construction.Worker, this.Args.Material, 0, this.Parent.OriginGlobal - cell, 0, this.Args.Orientation);
            MapEdit.PaintWithOrigin(MapEditContext.Simulation, this.Map, this.Parent.CellsOccupied, BlockDefOf.Construction.Worker, args.Material, 0, 0, args.Orientation);

            this.ValidateReadiness();
            this.Map.Events.Post(new ConstructionUpdatedEvent(this));

        }

        private void ValidateReadiness()
        {
            if (this.IsReady)
            {
                this.Map.Events.Post(new ConstructionReadyEvent(this));
            }
        }

        internal bool Accepts(Entity entity)
        {
            return 
                this.Fulfillment.Missing > 0 &&
                entity.Def == ItemDefOf.Ingredient &&
                entity.Profile == this.Args.Refinement &&
                entity.PrimaryMaterial == this.Args.Material;
        }
        public int DemandFor(Entity entity)
        {
            if (this.Missing == 0)
                return 0;
            if (this.Accepts(entity))
                return this.Missing;
            return 0;
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
            this.Progress.ApplyDelta(work);

            var map = this.Map; // capture map in case the construction is completed and the block entity gets removed from the map
            //if (this.Progress.IsFinished)
            //    this.Complete();
            //else
                map.Events.Post(new ConstructionUpdatedEvent(this));
            return;
        }
        public bool IsFinished => this.Progress.IsFinished;
        //public void Complete()
        //{
        //    var map = this.Parent.Map;
        //    map.Events.Post(new ConstructionFinishedEvent(this));

        //    var cells = this.Parent.CellsOccupied;
        //    // remove block entity first because this implicitly sets all occupied cells to air
        //    map.RemoveBlockEntity(this.Parent);
        //    //MapEdit.Paint(map, cells, this.Args.Block.Worker, this.Args.Material, 0, 0, this.Args.Orientation);
        //    //return;
        //    foreach (var cell in cells)
        //        map.SetBlock(cell, this.Args.Block.Worker, this.Args.Material, 0, 0, this.Args.Orientation);
        //}

        protected override void SaveExtra(SaveTag tag)
        {
            tag.Add(this.Fulfillment.Save("Fulfillment"));
            tag.Add(this.Progress.Save("Progress"));
            tag.Add(this.Args.Save("Args"));
        }
        public override void Load(SaveTag tag)
        {
            this.Progress = ProgressInt.Create(tag["Progress"]);
            this.Fulfillment = ProgressInt.Create(tag["Fulfillment"]);
            this.Args = ConstructionDesignationArgs.Create(tag["Args"]);
        }

        public override void Write(IDataWriter w)
        {
            this.Progress.Write(w);
            this.Fulfillment.Write(w);
            this.Args.Write(w);
        }
        public override ISerializable Read(IDataReader r)
        {
            this.Progress = ProgressInt.Create(r);
            this.Fulfillment = ProgressInt.Create(r);
            this.Args = ConstructionDesignationArgs.Create(r);
            return this;
        }
    }
    public record struct ConstructionDesignationArgs(BlockDef Block, MaterialRefinementDef Refinement, MaterialDef Material, int Amount, byte Orientation = 0)
        : ISerializableNew<ConstructionDesignationArgs>
        , ISaveableNewNew<ConstructionDesignationArgs>
    {
        //public Block Block = block;
        //public MaterialRefinementDef Refinement = refinement;
        //public MaterialDef Material = material;
        //public int Amount = amount;
        //public byte Orientation = orientation;
        public override readonly string ToString() => $"{this.Material.LabelReadable} {this.Refinement.LabelReadable} x{this.Amount}";
        public static ConstructionDesignationArgs Create(IDataReader r) => new ConstructionDesignationArgs().Read(r);

        public static ConstructionDesignationArgs Create(SaveTag tag)
        {
            var args = new ConstructionDesignationArgs();
            args.Block = tag.LoadDef<BlockDef>("Block");
            args.Refinement = tag.LoadDef<MaterialRefinementDef>("Refinement");
            args.Material = tag.LoadDef<MaterialDef>("Material");
            args.Amount = tag.LoadInt("Amount");
            args.Orientation = tag.LoadByte("Orientation");
            return args;
        }

        public ConstructionDesignationArgs Read(IDataReader r)
        {
            this.Block = r.ReadDef<BlockDef>();
            this.Refinement = r.ReadDef<MaterialRefinementDef>();
            this.Material = r.ReadDef<MaterialDef>();
            this.Amount = r.ReadInt32();
            this.Orientation = r.ReadByte();
            return this;
        }

        public readonly SaveTag Save(string name = "")
        {
            var tag = new SaveTag(SaveTag.Types.Compound, name);
            tag.Save("Block", this.Block);
            tag.Save("Refinement", this.Refinement);
            tag.Save("Material", this.Material);
            tag.Save("Amount", this.Amount);
            tag.Save("Orientation", this.Orientation);
            return tag;
        }

        public readonly void Write(IDataWriter w)
        {
            w.Write(this.Block);
            w.Write(this.Refinement);
            w.Write(this.Material);
            w.Write(this.Amount);
            w.Write(this.Orientation);
        }
    }
}