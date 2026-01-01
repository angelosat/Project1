using System;
using System.Collections.Generic;
using System.Linq;
namespace Start_a_Town_
{
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
        internal override void Initialize()
        {
            this.Parent.Name = this.WorkstationType.Label;
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
        }
        public override void Load(SaveTag tag)
        {
            this.WorkstationType = tag.LoadDef<WorkstationDef>("Type");
            this.Orders = tag.LoadListOrDefault<OrderSettings>("Orders");
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
        }
        public override ISerializable Read(IDataReader r)
        {
            this.WorkstationType = r.ReadDef<WorkstationDef>();
            this.Orders = r.ReadList<OrderSettings>();
            this.Resolve();

            return this;
        }
    }
}
