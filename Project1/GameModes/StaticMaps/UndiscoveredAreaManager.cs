using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Diagnostics;
using Start_a_Town_.Net;

namespace Start_a_Town_
{
    class UndiscoveredAreaManager
    {
        readonly MapBase Map;
        public UndiscoveredAreaManager(MapBase map)
        {
            this.Map = map;
            this.Map.World.Events.ListenTo<BlocksChangedEvent>(HandleBlocksChanged);
        }
        bool Valid;// = true;
        public void Init()
        {
            if (this.Map.Net is Client)
                return; // if i'm saving the discovered property of cells, it means that i load and send the data to clients. no need for clients to initialize undiscovered areas themselves
            if (!this.Valid)
            {
                var watch = Stopwatch.StartNew();
                this.FloodFill(this.Map);
                watch.Stop();
                $"undiscovered areas initialized in {watch.ElapsedMilliseconds} ms".ToConsole();
            }
            this.Valid = true;
        }
        public void FloodFill(MapBase map)
        {
            var current = new IntVec3(0, 0, MapBase.MaxHeight - 1);
            this.FloodFill(map, current, true);
        }
        public void FloodFill(MapBase map, IntVec3 begin, bool value)
        {
            var current = begin;
            var cell = map.GetCell(current);
            if (cell.Discovered == value)
                throw new Exception(); // was a test
            cell.Discovered = value;
            map.GetChunk(begin).InvalidateSlice(begin.Z);
            if (cell.Block != BlockDefOf.Air)
                return;
            var tohandle = new Queue<IntVec3>();
            tohandle.Enqueue(current);
            while (tohandle.Any())
            {
                current = tohandle.Dequeue();
                foreach(var n in current.GetAdjacentLazy())
                    if (map.TryGetCell(n, out var ncell) && !ncell.Discovered)
                    {
                        ncell.Discovered = true;
                        map.GetChunk(n).InvalidateSlice(n.Z);
                        if(!ncell.IsRoomBorder)
                            tohandle.Enqueue(n);
                    }
            }
        }

        public void Write(BinaryWriter w)
        {
            w.Write(this.Valid);
        }
        public UndiscoveredAreaManager Read(IDataReader r)
        {
            this.Valid = r.ReadBoolean();
            return this;
        }
        public SaveTag Save(string name)
        {
            var tag = new SaveTag(SaveTag.Types.Compound, name);
            this.Valid.Save(tag, "Valid");
            return tag;
        }
        internal void Load(SaveTag tag)
        {
            tag.TryGetTagValueNew("Valid", ref this.Valid);
        }

        //internal void OnGameEvent(GameEvent e)
        //{
        //    switch(e.Type)
        //    {
        //        case Components.Message.Types.BlocksChanged:
        //            foreach (var pos in e.Parameters[1] as IEnumerable<IntVec3>)
        //                Handle(e.Parameters[0] as MapBase, pos);
        //            break;

        //        default:
        //            break;
        //    }
        //}
        void HandleBlocksChanged(BlocksChangedEvent e)
        {
            if(this.Map == e.Map)
                foreach (var pos in e.Positions)
                    Handle(e.Map, pos);
        }

        private void Handle(MapBase map, IntVec3 global)
        {
            if (map.TryGetCell(global, out var gc))
                if (!gc.Discovered)
                    return;
            foreach (var n in global.GetNeighbors())
                if (map.TryGetCell(n, out var nc) && !nc.Discovered)
                    this.FloodFill(map, n, true);
        }
        
        public bool IsUndiscovered(IntVec3 global)
        {
            return !this.Map.GetCell(global).Discovered;
        }
    }
}
