using Project1.Core.Entities;
using Project1.Core.UI;
using Project1.Core.Base;
using Project1.Core.Helpers;
using Project1.Core.Helpers.Structs;
using Project1.Core.Interfaces;
using Project1.Core.Simulation;
using System;
using System.Collections.Generic;
using System.Linq;
using Project1.Core.Towns.Crafting;
using Project1.Framework.IO;
using Project1.Framework.Math;
namespace Project1.Core
{
    public enum WorkstationIOType
    {
        Input,
        Output
    }
    public sealed class BlockWorkstationComp : BlockEntityComp
    {
        public new class Spec(WorkstationDef type) : BlockEntityComp.Spec
        {
            public override Type CompType => typeof(BlockWorkstationComp);
            public WorkstationDef WorkstationType = type;
            public override BlockEntityComp CreateComp()
            {
                return new BlockWorkstationComp(this);
            }
        }

        public BlockWorkstationComp()
        {
            
        }
        public BlockWorkstationComp(Spec args)
        {
            this.WorkstationType = args.WorkstationType;
        }
        public override string Name => "WorkstationComp";
        public WorkstationDef WorkstationType = WorkstationDefOf.Smeltery; // default
        public List<OrderSettings> Orders = [];
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


        internal override void GetQuickButtons(Action<string, Type> register, MapBase map, IntVec3 vector3)
        {
            register("Orders", typeof(WorkstationGuiNew));
        }

        internal void MoveUp(OrderSettings orderSettings)
        {
            var currentIndex = this.Orders.IndexOf(orderSettings);
            if (currentIndex == 0)
                return;
            this.Orders.RemoveAt(currentIndex);
            this.Orders.Insert(currentIndex - 1, orderSettings);
            this.Map.Events.Post(new CraftOrderReorderedEvent(orderSettings));
        }

        internal void MoveDown(OrderSettings orderSettings)
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
            this.Orders = tag.LoadListOrDefault<OrderSettings>("Orders");
            if (tag.TryLoadInt("Input", out var inputid)) this.Input = inputid;
            if (tag.TryLoadInt("Output", out var outputid)) this.Output = outputid;
            this.Resolve();
        }

        private void Resolve()
        {
            foreach (var order in this.Orders)
                order.Workstation = this;
        }

        internal bool IngredientsInPlace(List<TargetArgs> targetsA)
        {
            var slots = this.Parent.CellsOccupied.Zip(targetsA);
            return slots.All(s => this.Parent.Map.GetEntitiesAt(s.First.Above).Any(c => c == s.Second.Object));
        }
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
            this.Orders = r.ReadList<OrderSettings>();
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
