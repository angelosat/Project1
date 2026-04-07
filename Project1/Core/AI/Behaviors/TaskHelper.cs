using System;
using System.Linq;
using Project1.Core.Towns.Designations;
using Project1.Core.Entities.Actors;
using Project1.Core.Entities;
using Project1.Core.AI.Behaviors.Pathing;
using Project1.Core.AI.Behaviors.NodeTypes;
using Project1.Framework;
using Project1.Core.AI.Reservations;
using Project1.Core.AI.Personality;

namespace Project1.Core.AI.Behaviors
{
    static class TaskHelper
    {
        /// <summary>
        /// Looks in actor's inventory or in map for item that satisfies provided condition
        /// </summary>
        /// <param name="actor"></param>
        /// <param name="condition"></param>
        /// <returns></returns>
        static public InteractionTarget FindItemAnywhere(Actor actor, Func<GameObject, bool> condition)
        {
            var inventoryitem = actor.Inventory.First(condition);
            if (inventoryitem != null)
            {
                var invitemtarget = new InteractionTarget(inventoryitem);
                return invitemtarget;
            }
            else
            {
                var nearbyItems = actor.Map.Entities.OfType<Entity>();
                var item = nearbyItems
                    .Where(i => condition(i) && actor.CanReserve(i))
                    .SortByReachableRegionDistance(actor)
                    .FirstOrDefault();
                    
                if (item == null)
                    return InteractionTarget.Null;
                return new InteractionTarget(item);
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
                var task = bhav.Actor.CurrentPlan;
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
                var task = bhav.Actor.CurrentPlan;
                var t = task.GetTarget(targetInd);
                if (t.Object.IsDisposed)
                    return true;
                if (t.IsForbidden)
                    return true;
                return false;
            });
            return bhav;
        }
        static public Behavior FailOnTargetDespawned(this Behavior bhav, TargetIndex targetInd)
        {
            bhav.FailOn(() =>
            {
                var t = bhav.Actor.CurrentPlan.GetTarget(targetInd);
                return (t.Object.Map != bhav.Actor.Map);
            });
            return bhav;
        }
        static public Behavior FailOnTargetDespawned(this Behavior bhav)
        {
            return bhav.FailOnTargetDespawned(TargetIndex.A);
        }
        static public Behavior FailOnUnavailablePlacedItems(this Behavior bhav)
        {
            bhav.FailOn(() =>
            {
                var task = bhav.Actor.CurrentPlan;
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
        static public Behavior FailOnRanOutOfPatienceWaiting(this BehaviorExecutePlan bhav, Action failAction = null)
        {
            var actor = bhav.Actor;
            var patienceTrait = actor.GetTrait(TraitDefOf.Patience);
            var patienceBase = Ticks.PerSecond * TimeSpan.FromMinutes(1).TotalSeconds;
            var patience = patienceBase * (1 + patienceTrait.Percentage * .5f);
            bhav.FailOn(() =>
            {
                if (bhav.Plan.TicksWaited < patience)
                    return false;
                failAction?.Invoke();
                return true;
            });
            return bhav;
        }
        static public void FailOnNoDesignation(this BehaviorExecutePlan bhav, TargetIndex targetInd, DesignationDef designation)
        {
            bhav.FailOnNoDesignation((int)targetInd, designation);
        }
        static public void FailOnNoDesignation(this BehaviorExecutePlan bhav, int targetInd, DesignationDef designation)
        {
            bhav.FailOn(() =>
            {
                var global = bhav.Plan.GetTarget(targetInd).Global;
                return !bhav.Actor.Town.DesignationManager.IsDesignation(global, designation);
            });
        }
        static public void FailOnCellStandedOn(this BehaviorExecutePlan bhav, TargetIndex targetInd)
        {
            bhav.FailOn(() =>
            {
                var global = bhav.Plan.GetTarget(targetInd).Global;
                var actor = bhav.Actor;
                var objects = actor.Map.GetObjects(global.Above());
                return objects.Any() && (objects.SingleOrDefault(o => o == actor) != actor);
            });
        }
        [Obsolete]
        static public BehaviorCustom NextTargetAmount(Behavior bhavRoot, TargetIndex index)
        {
            throw new NotImplementedException();
            //var bhav = new BehaviorCustom();
            //bhav.InitAction = () =>
            //{
            //    if (bhav.Actor.CurrentTask.NextTarget(index) && bhav.Actor.CurrentTask.NextAmount(index))
            //        if (bhavRoot != null)
            //    bhav.Actor.AI.State.Behavior.JumpTo(bhavRoot);

            //};
            //return bhav;
        }
    }
}
