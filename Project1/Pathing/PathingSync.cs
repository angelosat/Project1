using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Start_a_Town_.PathFinding;

namespace Start_a_Town_
{
    public partial class PathingSync
    {
        public enum States { Stopped, Working, Finished }
        public States State;

        const float CostStraight = 10, CostDiag = 14;
        public int Ticks = 0;

        readonly PriorityQueue<float, Node> Open = new();
        readonly List<Node> Closed = new();
        readonly HashSet<Vector3> Handled = new();

        Vector3 Goal;
        readonly Dictionary<Vector3, Node> CachedNodes = new();
        public float Range;

        public Vector3 Start, Finish, FinishPrecise;
        public TargetArgs FinishTarget;
        Path PathInProgress;
        MapBase Map;
        Actor Actor;
        static void ConformPathToTerrain(MapBase map, Path path)
        {
            return;
        }

        /// <summary>
        /// Returns the found path and sets the state to stopped
        /// </summary>
        /// <returns></returns>
        public Path GetPath()
        {
            if (this.State != States.Finished)
                throw new Exception();
            this.State = States.Stopped;
            if (this.PathInProgress != null)
            {
                ConformPathToTerrain(this.Map, this.PathInProgress);
                this.PathInProgress.ConformToBlockHeights(this.Map);
            }
            return this.PathInProgress;
        }
        
        public void Begin(Actor actor, Vector3 start, Vector3 finish, float range = 0)
        {
            var map = actor.Map;
            if (!map.Contains(start) || !map.Contains(finish))
                throw new Exception();
            this.Ticks = 0;
            this.Range = range;
            this.Start = start.ToRounded();
            this.Actor = actor;

            this.FinishTarget = new TargetArgs(map, finish);
            this.FinishPrecise = finish;
            this.Finish = finish.ToRounded();

            this.Goal = this.Finish;
            this.Map = map;

            this.State = PathingSync.States.Working;
            this.PathInProgress = new Path();
            var startNode = new Node(this.Map, this.Start, this.Finish) { RegionNodeGlobal = map.GetNodeAt(this.Start.Below()) };
            var finishNode = new Node(this.Map, this.Finish, this.Finish) { RegionNodeGlobal = map.GetNodeAt(this.Goal.Below()) };
            this.CachedNodes.Clear();
            this.Open.Clear();
            this.Closed.Clear();
            this.Handled.Clear();
            this.CachedNodes.Add(this.Start, startNode);
            this.CachedNodes[this.Finish] = finishNode;
            this.Open.Enqueue(startNode.CostToGoal, startNode);
        }

        private void Fail()
        {
            this.State = States.Finished;
            this.PathInProgress = null;
        }
        public void WorkMode(PathEndMode mode)
        {
            do { } while (this.WorkModeStepFaster(mode) && this.Ticks < 100);
            this.Ticks = 0;
        }
        
        public bool WorkModeStepFaster(PathEndMode mode)
        {
            if (this.State != States.Working)
                throw new Exception(); //why is pathfinder called if it's not working?
            this.Ticks++;
            if (this.Open.Count > 0)
            {
                var current = this.Open.Dequeue();
                current.IsQueued = false;
                if (mode.IsFinish(this.Actor, this.Goal, current.Global) && this.Map.IsStandableIn(current.Global))
                {
                    var goalnode = new Node(this.Map, current.Global, this.Finish) { Parent = current.Parent ?? current, CellsToTraverse = current.CellsToTraverse }; // assinging the current node's parent seems to work more correctly, but if the parent is null?
                    this.PathInProgress.Build(goalnode, this.Start);
                    this.State = States.Finished;
                    return false;
                }
                this.Handled.Add(current.Global);
                var currentNode = current.RegionNodeGlobal;
                if (currentNode is null)
                {
                    Log.Warning($"{this.Actor} tried to path from {current.Global} but the {nameof(current.RegionNodeGlobal)} is null");
                    this.Fail();
                    return false;
                }
                var neighbors = currentNode.GetLinksLazy();
                foreach (var n in neighbors) // put visibility check here
                {
                    var nabove = n.Global.Above;
                    if (this.Handled.Contains(nabove))
                        continue;
                    if (!this.CachedNodes.TryGetValue(nabove, out var nnode))
                    {
                        nnode = new Node(this.Map, nabove, this.Finish) { RegionNodeGlobal = n };
                        this.CachedNodes[n.Global.Above] = nnode;

                    }

                    if (!nnode.IsQueued)
                    {
                        nnode.CostFromStart = float.MaxValue;
                        nnode.Parent = null;
                    }
                    UpdateNoPathableCheck(this.Map, current, nnode);
                }
            }

            else
            {
                // no path found
                Fail();
                return false;
            }
            return true;
        }

        static float Heuristic(Vector3 start, Vector3 finish)
        {
            var dx = (float)Math.Abs(finish.X - start.X);
            var dy = (float)Math.Abs(finish.Y - start.Y);
            //var dz = (float)Math.Abs(finish.Z - start.Z);
            var dmin = Math.Min(dx, dy);
            var dmax = dx + dy - dmin;
            //return dx * CostStraight + dy * CostDiag + dz * 100;
            return (dmax - dmin) * CostStraight + dmin * CostDiag;// +dz * 100; // TODO: find correct cost for climbing
        }
        
        void UpdateNoPathableCheck(MapBase map, Node current, Node next)
        {
            var costOld = next.CostFromStart;
            ComputeCostTheta(current, next);

            if (next.CostFromStart < costOld)
            {
                this.Open.Remove(next);
                this.Open.Enqueue(next.CostFromStart + Heuristic(next.Global, this.Goal), next);
                next.IsQueued = true;
            }
        }

        const float BlockBaseCost = 100; //TODO: find best cost for making ai prefer pathways

        void ComputeCostTheta(NodeBase current, NodeBase next)
        {
            var passedcells = new List<Vector3>();
            var los = current.Parent != null && LineOfSight(current.Map, current.Parent.Global, next.Global, out passedcells);
            float cost, costCheck;
            if (los)
            {
                cost = GetCostTheta(current.Parent, next);

                costCheck = current.Parent.CostFromStart + cost;
                if (costCheck < next.CostFromStart)
                {
                    next.Parent = current.Parent;
                    next.CostFromStart = costCheck;
                    next.CellsToTraverse = passedcells;
                }
            }
            else
            {
                cost = GetCostThetaSingle(current, next);

                costCheck = current.CostFromStart + cost;
                if (costCheck < next.CostFromStart)
                {
                    next.Parent = current;
                    next.CostFromStart = costCheck;
                    next.CellsToTraverse = passedcells;
                }
            }
        }
        float GetCostTheta(NodeBase current, NodeBase next)
        {
            // get an 1-wide line between the two positions and sum the block pathing costs below the line
            if(current.Global.Z != next.Global.Z)
                throw new NotImplementedException();
            var z = next.Global.Z;
            var x1 = (int)(current.Global.X);
            var y1 = (int)(current.Global.Y);
            var x2 = (int)(next.Global.X);
            var y2 = (int)(next.Global.Y);

            var positions = LineHelper.Plot2D(x1, y1, x2, y2);
            positions.RemoveAt(0);
            var positionsVec3 = positions.Select(p => new Vector3(p.X, p.Y, z));
            float cost = 0;
            var a = current.Global;
            foreach (var pos in positionsVec3)
            {
                cost += GetCostThetaSingle(a, pos);
                a = pos;
            }
            return cost;
        }
        float GetCostThetaSingle(NodeBase current, NodeBase next)
        {
            return this.GetCostThetaSingle(current.Global, next.Global);
        }
        float GetCostThetaSingle(Vector3 current, Vector3 next)
        {
            var blockCost = Block.GetPathingCost(this.Map, next - Vector3.UnitZ);
            var stepcost = GetStepCost(current, next);
            var cost = stepcost + blockCost * BlockBaseCost;
            return cost;
        }
        float GetStepCost(Vector3 a, Vector3 b)
        {
            var cost = (a.X == b.X || a.Y == b.Y) ? CostStraight : CostDiag;
            if (b.Z - a.Z == 1)
            {
                var blockheight = Block.GetBlockHeight(this.Map, b.Below());
                if (blockheight > .5f)
                    cost += CostStraight * 6f;// 4.5f;// *= 20; 
            }
            // the cost of going around a block should be less than the cost of jumping on it. 
            // the cost of going around a block is coststraight * number of blocks the ai should prefer of walking instead of jumping. 
            // for example entering the area around a block and exiting at the opposite side would be coststraight * 4. 
            // if the ai would go completely around it would be coststraight * 6
            // for now we'll let it prefer jumping if it has to walk more than a semicircle around the block so the cost should be coststraight * 4.5 (???)
            return cost;
        }
        
        [Obsolete]
        static public bool IsPathable2Height(MapBase map, Vector3 global)
        {
            // WARNING! can't check directly below in case the destination is a halbblock with air underneath.
            // so adjust the vector to above().floor() or below().ceiling() or just ceiling() ??
            global = global.CeilingZ();
            if (!map.IsSolid(global.Below())) 
                return false;

            // TODO: check both at the same time          
            if (!map.IsPathable(global))
                return false;
            if (!map.IsPathable(global.Above())) // replace this with height check?
                return false;

            return true;
        }
        static public bool IsPathable(MapBase map, Vector3 global)
        {
            // WARNING! can't check directly below in case the destination is a halbblock with air underneath.
            // so adjust the vector to above().floor() or below().ceiling() or just ceiling() ??
            global = global.CeilingZ();
            if (!map.IsSolid(global.Below()))
                return false;

            // TODO: check both at the same time          
            if (!map.IsPathable(global))
                return false;

            return true;
        }

        static public bool LineOfSight(MapBase map, Vector3 a, Vector3 b, out List<Vector3> cells)
        {
            if (a.Z == b.Z)
                return LineHelper.LineOfSight((int)a.X, (int)a.Y, (int)a.Z, (int)b.X, (int)b.Y, (int)b.Z, p => LosPathableFailCondition(map, p), out cells);
            cells = new List<Vector3>();
            return false;
        }
        public static bool LosPathableFailCondition(MapBase map, Vector3 p)
        {
            var below = p - Vector3.UnitZ;
            var solidbelow = map.IsSolid(below);
            if (!solidbelow)
                return true;

            var unpathablecurrent = !map.IsPathable(p);
            return unpathablecurrent; // i'm testing returning here to make the ai prefer going around single height blocks instead of jumping over them 
        }
    }
}
