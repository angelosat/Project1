using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;

namespace Start_a_Town_
{
    class LiquidFlowProcess
    {
        static readonly List<LiquidFlowProcess> FlowProcesses = new List<LiquidFlowProcess>();
        static readonly float FlowSpeed = Ticks.PerSecond / 2f;
        static float FlowT = FlowSpeed;

        static Random Rand = new(); 
        private Vector3 Source;
        readonly HashSet<Vector3> Handled = new();
        readonly Queue<Vector3> ToHandle = new();
        MapBase Map;


        static public void Add(LiquidFlowProcess proc)
        {
            FlowProcesses.Add(proc);
        }

        public LiquidFlowProcess(MapBase map, Vector3 source, Vector3 current)
        {
            this.Map = map;
            this.Source = source;
            this.ToHandle.Enqueue(current);
        }

        static public void UpdateProcesses()
        {
            FlowT--;
            if (FlowT > 0)
                return;
            FlowT = FlowSpeed;
            foreach (var proc in FlowProcesses.ToList())
                if (proc.Update())
                    FlowProcesses.Remove(proc);
        }
        internal bool Update()
        {
            if (this.ToHandle.Count == 0)
                return true;
            var current = this.ToHandle.Dequeue();
            this.Handled.Add(current);

            var below = current - Vector3.UnitZ;
            var belowBlock = this.Map.GetBlock(below);
            if (belowBlock == BlockDefOf.Air)
            {
                this.Map.SetBlock(below, BlockDefOf.Fluid, MaterialDefOf.Water, 1, Rand.Next(4));
                if (!this.Handled.Contains(below))
                    if (!this.ToHandle.Contains(below))
                        this.ToHandle.Enqueue(below);
                return false;
            }
           
            var east = current + Vector3.UnitX;
            var south = current + Vector3.UnitY;
            var west = current - Vector3.UnitX;
            var north = current - Vector3.UnitY;
            foreach (var n in new List<Vector3>() { east, south, west, north })
            {
                var nblock = this.Map.GetBlock(n);
                if (nblock != BlockDefOf.Air)
                    continue;
                this.Map.SetBlock(n, BlockDefOf.Fluid, MaterialDefOf.Water, 0, Rand.Next(4));
                FlowProcesses.Add(new LiquidFlowProcess(this.Map, this.Source, n));
            }
            return false;
        }
    }
}
