using Start_a_Town_.UI;
using System;
using System.Collections.Generic;
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
        //public BlockWorkstationComp(WorkstationDef def)
        //{
        //    this.WorkstationType = def;
        //}
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
        //public ObservableCollection<OrderSettings> Orders = [];
        //public IntVec3 MasterCell;
        //BlockEntityCompWorkstation _cachedMaster;
        //public BlockEntityCompWorkstation Master => this.MasterCell == this.Global ? this : this._cachedMaster ??= this.Map.GetBlockEntityComp<BlockEntityCompWorkstation>(this.MasterCell);
        //HashSet<IntVec3> LinkedModules = [];
        //void Unlink()
        //{
        //    this._cachedMaster = null;
        //    this.MasterCell = this.Parent.OriginGlobal;
        //}
        //void LinkTo(BlockEntityCompWorkstation master)
        //{
        //    this._cachedMaster = master;
        //    this.MasterCell = master.Global;
        //}
        //internal override void OnNeighborChanged(MapBase map, IntVec3 source)
        //{
        //    // if the new cell was the master workstation and the new one isn't even a workstation
        //    if (source == this.MasterCell && !map.TryGetBlockEntityComp<BlockEntityCompWorkstation>(source, out _))
        //    {
        //        // fetch its bookkeeping
        //        var remaining = new HashSet<BlockEntityCompWorkstation>(this.Master.LinkedModules);

        //        // remove the master
        //        remaining.Remove(this.Master);

        //        //var remaining = new HashSet<BlockEntityCompWorkstation>(oldLinkedModules);//.Except([this.Master]);
        //        var unvisited = new HashSet<BlockEntityCompWorkstation>(remaining);

        //        while (unvisited.Count > 0)
        //        {
        //            var seed = unvisited.First();
        //            var fragment = FloodFill(seed, remaining);

        //            // seed becomes master of this fragment
        //            seed.MasterCell = seed.Global;
        //            seed._cachedMaster = null;
        //            seed.LinkedModules.Clear();

        //            foreach (var cell in fragment)
        //                cell.LinkTo(seed);

        //            unvisited.RemoveWhere(c => fragment.Contains(c));
        //        }
        //    }
        //}
        //HashSet<BlockEntityCompWorkstation> FloodFill(
        //    BlockEntityCompWorkstation seed,
        //    HashSet<BlockEntityCompWorkstation> allowed)
        //{
        //    var result = new HashSet<BlockEntityCompWorkstation>();
        //    var stack = new Stack<BlockEntityCompWorkstation>();

        //    stack.Push(seed);
        //    result.Add(seed);

        //    while (stack.Count > 0)
        //    {
        //        var current = stack.Pop();

        //        foreach (var dir in IntVec3.AdjacentIntVec3)
        //        {
        //            var neighborPos = current.Global + dir;

        //            if (!current.Map.TryGetBlockEntityComp<BlockEntityCompWorkstation>(
        //                    neighborPos, out var neighbor))
        //                continue;

        //            if (!allowed.Contains(neighbor))
        //                continue;

        //            if (result.Add(neighbor))
        //                stack.Push(neighbor);
        //        }
        //    }

        //    return result;
        //}
        //public override void OnSpawned(BlockEntity entity, MapBase map, IntVec3 global)
        //{
        //    foreach (var dir in IntVec3.AdjacentIntVec3)
        //    {
        //        var neighborPos = global + dir;

        //        if (!map.TryGetBlockEntityComp<BlockEntityCompWorkstation>(neighborPos, out var neighborComp))
        //            continue;

        //        // Only consider compatible types
        //        if (neighborComp.WorkstationType != this.WorkstationType)
        //            continue;

        //        // candidate for linking
        //        var neighborMaster = neighborComp.Master;

        //        // collect distinct masters from all neighbors
        //        var neighborMasters = new HashSet<BlockEntityCompWorkstation> { neighborMaster };

        //        this.MasterCell = this.Global;
        //        this._cachedMaster = null;
        //        this.LinkedModules.Clear();
        //        this.LinkedModules.Add(this);

        //        // If multiple neighbor masters: merge into new master (this workstation)
        //        if (neighborMasters.Count > 0)
        //        {
        //            foreach (var master in neighborMasters)
        //            {
        //                foreach (var cell in master.LinkedModules)
        //                    cell.LinkTo(this); // this becomes new master
        //            }
        //        }
        //    }
        //}
        internal override void GetQuickButtons(SelectionManager uISelectedInfo, MapBase map, IntVec3 vector3)
        {
            uISelectedInfo.AddTabAction("Orders", this.ShowUI);
        }

        public void ShowUI()
        {
            UIManager.ToggleUnique<WorkstationGuiNew>(new TargetArgs(this.Parent.Map, this.Parent.OriginGlobal));
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
