using System;
using System.Collections.Generic;
using System.Linq;
namespace Start_a_Town_
{
    public class BlockWorkstationComp : BlockEntityComp
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
        //public void ShowUI()
        //{
        //    UIManager.ToggleUnique<WorkstationGuiNew>(new TargetArgs(this.Parent.Map, this.Parent.OriginGlobal));
        //}

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

        //internal void AddCell(IntVec3 module)
        //{
        //    this.LinkedModules.Add(module);
        //}
        public override void AddSaveData(SaveTag tag)
        {
            tag.Add(this.WorkstationType.Save("Type"));
        }
        public override void Load(SaveTag tag)
        {
            //if (tag.TryGetTagValueOut<string>("Type", out var defName)) this.WorkstationType = Def.GetDef<WorkstationDef>(defName);
            this.WorkstationType = tag.LoadDef<WorkstationDef>("Type");
        }
        internal bool IngredientsInPlace(List<TargetArgs> targetsA)
        {
            var slots = this.Parent.CellsOccupied.Zip(targetsA);
            return slots.All(s => this.Parent.Map.GetEntitiesAt(s.First.Above).Any(c => c == s.Second.Object));
        }
        public override void Write(IDataWriter w)
        {
            this.WorkstationType.Write(w);
        }
        public override ISerializable Read(IDataReader r)
        {
            this.WorkstationType = r.ReadDef<WorkstationDef>();
            return this;
        }

        
    }
}
