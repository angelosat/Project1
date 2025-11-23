using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;

namespace Start_a_Town_
{
    static class TaskHelper
    {
        public static bool TryClearArea(Actor actor, IntVec3 global, IEnumerable<GameObject> exclude, out AITask clearTask)
        {
            var exclusions = new HashSet<GameObject>(exclude);
            clearTask = null;
            var items = actor.Map.GetObjectsOccupyingCell(global).Except(exclusions);
            if (!items.Any()) 
                return true;
            if (items.HasSingle(out var single) && single == actor)
                return true;
            foreach (var i in items)
            {
                if (i is not Entity ientity)
                    continue;
                if (i is Plant && actor.CanReserve(i))
                {
                    var plantCutTask = new AITask(TaskDefOf.Chopping, i)
                    {
                        Tool = TaskGiver.FindTool(actor, JobDefOf.Lumberjack)
                    };
                    clearTask = plantCutTask;
                    return false;
                }
                var haulAsideTask = TryHaulAside(actor, ientity);
                if (haulAsideTask != null)
                {
                    clearTask = haulAsideTask;
                    return false;
                }
            }
            return false;
        }
        static public AITask TryHaulAside(Actor actor, Entity item)
        {
            if (!item.IsHaulable)
                return null;
            if (!actor.TryGetUnreservedAmount(item, out var unreservedCount))
                return null;
            if (!HaulHelper.TryFindNearbyPlace(actor, item, out var place))
                return null;

            var count = Math.Min(unreservedCount, actor.GetHaulStackLimitFromEndurance(item));

            return new AITask(TaskDefOf.HaulAside, new TargetArgs(item), place) { Count = count };
        }
        static public bool TryHaulAside(Actor actor, Vector3 global, out AITask task)
        {
            task = null;
            var map = actor.Map;
            var objectsAbove = map.GetObjects(global);
            if (!objectsAbove.Any())
                return true;
            if (objectsAbove.SingleOrDefault(o => o == actor) != null)
                return true;
            foreach (var objAbove in objectsAbove)
            {
                if (objAbove == actor)
                    continue;
                var haulAsideTask = TaskHelper.TryHaulAside(actor, objAbove as Entity);
                if (haulAsideTask != null)
                {
                    task = haulAsideTask;
                    return true;
                }
            }
            return false; // if there are any items above the cell and a haulasidetask hasn't been returned, it means that there are immovable objects
        }
        /// <summary>
        /// Looks in actor's inventory or in map for item that satisfies provided condition
        /// </summary>
        /// <param name="actor"></param>
        /// <param name="condition"></param>
        /// <returns></returns>
        static public TargetArgs FindItemAnywhere(Actor actor, Func<GameObject, bool> condition)
        {
            var inventoryitem = actor.Inventory.First(condition);
            if (inventoryitem != null)
            {
                var invitemtarget = new TargetArgs(inventoryitem);
                return invitemtarget;
            }
            else
            {
                var nearbyItems = actor.Map.GetObjects().OfType<Entity>();
                var item = nearbyItems
                    .Where(i => condition(i) && actor.CanReserve(i))
                    .OrderByReachableRegionDistance(actor)
                    .FirstOrDefault();
                    
                if (item == null)
                    return TargetArgs.Null;
                return new TargetArgs(item);
            }
        }
        static public Behavior FailOnNotCarrying(this Behavior bhav)
        {
            bhav.FailOn(() => bhav.Actor.Hauled == null);
            return bhav;
        }
        static public Behavior FailOnForbidden(this Behavior bhav, TargetIndex targetInd)
        {
            bhav.FailOn(() =>
            {
                var task = bhav.Actor.CurrentTask;
                if (task.GetTarget(targetInd).IsForbidden)
                    return true;

                return false;
            });
            return bhav;
        }
        static public Behavior FailOnUnavailableTarget(this Behavior bhav, TargetIndex targetInd)
        {
            bhav.FailOn(() =>
            {
                var task = bhav.Actor.CurrentTask;
                var t = task.GetTarget(targetInd);
                if (t.Object.IsDisposed)
                    return true;
                if (t.IsForbidden)
                    return true;
                return false;
            });
            return bhav;
        }
        static public Behavior FailOnUnavailablePlacedItems(this Behavior bhav)
        {
            bhav.FailOn(() =>
            {
                var task = bhav.Actor.CurrentTask;
                foreach (var t in task.PlacedObjects)
                {
                    if (t.Object.IsDisposed)
                        return true;
                    if (t.Object.IsForbidden)
                        return true;
                }
                return false;
            });
            return bhav;
        }
        static public Behavior FailOnRanOutOfPatienceWaiting(this BehaviorPerformTask bhav, Action failAction = null)
        {
            var actor = bhav.Actor;
            var patienceTrait = actor.GetTrait(TraitDefOf.Patience);
            var patienceBase = Ticks.PerSecond * TimeSpan.FromMinutes(1).TotalSeconds;
            var patience = patienceBase * (1 + patienceTrait.Percentage * .5f);
            bhav.FailOn(() =>
            {
                if (bhav.Task.TicksWaited < patience)
                    return false;
                failAction?.Invoke();
                return true;
            });
            return bhav;
        }
        static public void FailOnNoDesignation(this BehaviorPerformTask bhav, TargetIndex targetInd, DesignationDef designation)
        {
            bhav.FailOnNoDesignation((int)targetInd, designation);
        }
        static public void FailOnNoDesignation(this BehaviorPerformTask bhav, int targetInd, DesignationDef designation)
        {
            bhav.FailOn(() =>
            {
                var global = bhav.Task.GetTarget(targetInd).Global;
                return !bhav.Actor.Town.DesignationManager.IsDesignation(global, designation);
            });
        }
        static public void FailOnCellStandedOn(this BehaviorPerformTask bhav, TargetIndex targetInd)
        {
            bhav.FailOn(() =>
            {
                var global = bhav.Task.GetTarget(targetInd).Global;
                var actor = bhav.Actor;
                var objects = actor.Map.GetObjects(global.Above());
                return objects.Any() && (objects.SingleOrDefault(o => o == actor) != actor);
            });
        }
        static public BehaviorCustom NextTargetAmount(Behavior bhavRoot, TargetIndex index)
        {
            var bhav = new BehaviorCustom();
            bhav.InitAction = () =>
            {
                if (bhav.Actor.CurrentTask.NextTarget(index) && bhav.Actor.CurrentTask.NextAmount(index))
                    if (bhavRoot != null)
                        bhav.Actor.CurrentTaskBehavior.JumpTo(bhavRoot);
            };
            return bhav;
        }
    }
}
