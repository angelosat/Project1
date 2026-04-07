using Project1.Core.Blocks.Comps;
using Project1.Core.Crafting;
using Project1.Core.Entities;
using Project1.Core.Helpers;
using Project1.Core.Towns.Stockpiles;
using Project1.Core.UI;
using Project1.Framework;
using Project1.Framework.Helpers;
using Project1.Framework.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Project1.Core.Blocks
{
    public enum WorkstationIOType
    {
        Input,
        Output
    }
    public sealed class BlockWorkstationComp : BlockComp
    {
        public new class Spec(WorkstationDef type) : BlockComp.Spec
        {
            public override Type CompType => typeof(BlockWorkstationComp);
            public WorkstationDef WorkstationType = type;
            public override BlockComp CreateComp()
            {
                return new BlockWorkstationComp(this);
            }
        }
        public override BlockCompDef CompDef => BlockCompDefOf.Workstation;

        public BlockWorkstationComp()
        {
            
        }
        public BlockWorkstationComp(Spec args)
        {
            this.WorkstationType = args.WorkstationType;
        }
        public WorkstationDef WorkstationType = WorkstationDefOf.Smeltery; // default
        public List<CraftingOrder> Orders = [];
        public ZoneId Input = ZoneId.Null, Output = ZoneId.Null;
        internal override void Initialize()
        {
            this.Parent.Name = this.WorkstationType.LabelReadable;
        }
        
        public IEnumerable<Entity> GetJunk()
        {
            foreach (var slot in this.Parent.CellsOccupied.Select(c => c.Above))
                foreach (var entity in this.Parent.Map.GetEntitiesAt(slot))
                    yield return entity;
        }


        //internal override void GetQuickButtons(Action<string, Type> register, MapBase map, IntVec3 vector3)
        //{
        //    register("Orders", typeof(WorkstationGuiNew));
        //}
        internal override IEnumerable<(string label, Type type)> GetSelectionTabs()
        {
            yield return ("Orders", typeof(WorkstationGuiNew));
        }

        internal void MoveUp(CraftingOrder orderSettings)
        {
            var currentIndex = this.Orders.IndexOf(orderSettings);
            if (currentIndex == 0)
                return;
            this.Orders.RemoveAt(currentIndex);
            this.Orders.Insert(currentIndex - 1, orderSettings);
            this.Map.Events.Post(new CraftOrderReorderedEvent(orderSettings));
        }

        internal void MoveDown(CraftingOrder orderSettings)
        {
            var currentIndex = this.Orders.IndexOf(orderSettings);
            if (currentIndex == this.Orders.Count - 1)
                return;
            this.Orders.RemoveAt(currentIndex);
            this.Orders.Insert(currentIndex + 1, orderSettings);
            this.Map.Events.Post(new CraftOrderReorderedEvent(orderSettings));
        }

        protected override void SaveExtra(SaveTag tag)
        {
            tag.Add(this.WorkstationType.Save("Type"));
            tag.Save("Orders", this.Orders);
            tag.Save("Input", this.Input);
            tag.Save("Output", this.Output);
        }
        public override void Load(SaveTag tag)
        {
            this.WorkstationType = tag.LoadDef<WorkstationDef>("Type");
            this.Orders = tag.LoadListOrDefault<CraftingOrder>("Orders");
            if (tag.TryLoadInt("Input", out var inputid)) this.Input = inputid;
            if (tag.TryLoadInt("Output", out var outputid)) this.Output = outputid;
            this.Resolve();
        }

        private void Resolve()
        {
            foreach (var order in this.Orders)
                order.Workstation = this;
        }

        internal bool IngredientsInPlace(List<InteractionTarget> targetsA)
        {
            var slots = this.Parent.CellsOccupied.Zip(targetsA);
            return slots.All(s => this.Parent.Map.GetEntitiesAt(s.First.Above).Any(c => c == s.Second.Object));
        }
        //public bool TryGetUnfinishedItem(out Entity item)
        //{
        //    //var entities = this.Parent.CellsOccupied.SelectMany(cell => this.Map.GetEntitiesAt(cell.Above));
        //    //item = entities.FirstOrDefault(e => e.Def == ItemDefOf.UnfinishedItem);
        //    //return item is not null;
        //    item = this.GetUnfinishedItem();
        //    return item is not null;
        //}
        public Entity GetUnfinishedItem()
        {
            var entities = this.Parent.CellsOccupied.SelectMany(cell => this.Map.GetEntitiesAt(cell.Above));
            return entities.FirstOrDefault(e => e.Def == ItemDefOf.UnfinishedItem);
        }
        internal IReadOnlySet<IntVec3> Modules => this.Parent.CellsOccupied;
        public override void Write(IDataWriter w)
        {
            this.WorkstationType.Write(w);
            w.Write(this.Orders);
            w.Write(this.Input);
            w.Write(this.Output);
        }
        public override ISerializable Read(IDataReader r)
        {
            this.WorkstationType = r.ReadDef<WorkstationDef>();
            this.Orders = r.ReadList<CraftingOrder>();
            this.Input = r.ReadInt32();
            this.Output = r.ReadInt32();

            this.Resolve();

            return this;
        }

        internal void SetStockpile(WorkstationIOType iotype, Stockpile stockpile)
        {
            switch (iotype)
            {
                case WorkstationIOType.Input:
                    this.Input = stockpile?.ID ?? -1;
                    break;

                case WorkstationIOType.Output:
                    this.Output = stockpile?.ID ?? -1;
                    break;

                default:
                    throw new Exception();
            }
            this.Map.Events.Post(new WorkstationUpdatedEvent(this));
        }
    }
}
