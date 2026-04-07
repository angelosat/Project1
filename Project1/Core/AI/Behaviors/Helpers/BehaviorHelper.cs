using Project1.Core.AI.Behaviors.NodeTypes;
using Project1.Core.AI.Behaviors.Pathing;
using Project1.Core.Entities;
using Project1.Core.Helpers;
using Project1.Core.Interactions;
using Project1.Core.Simulation;
using Project1.Core.Towns.Tasks;
using Project1.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Project1.Core.AI.Behaviors.Helpers
{
    class BehaviorHelper
    {
        internal static Behavior SetTarget(TargetIndex a, Entity value)
        {
            var bhav = new BehaviorCustom();
            bhav.InitAction = () => bhav.Actor.CurrentPlan.SetTarget(a, value);
            return bhav;
        }
        internal static Behavior SetTarget(TargetIndex a, IntVec3 value)
        {
            var bhav = new BehaviorCustom();
            bhav.InitAction = () => bhav.Actor.CurrentPlan.SetTarget(a, value.At(bhav.Actor.Map));
            return bhav;
        }
        internal static Behavior SetTarget(TargetIndex a, (MapBase map, IntVec3 value) position)
        {
            var bhav = new BehaviorCustom();
            bhav.InitAction = () => bhav.Actor.CurrentPlan.SetTarget(a, new InteractionTarget(position.map, position.value));
            return bhav;
        }
        internal static Behavior SetTarget(TargetIndex a, Func<InteractionTarget> targetGetter)
        {
            var bhav = new BehaviorCustom();
            bhav.InitAction = () => bhav.Actor.CurrentPlan.SetTarget(a, targetGetter());
            return bhav;
        }
        internal static Behavior CarryFromInventory(TargetIndex item)
        {
            return new BehaviorResolveInteraction(item, () => null);// new InteractionHaul());
        }
        internal static Behavior CarryFromInventoryAndReplaceTarget(TargetIndex item)
        {
            var bhav = new BehaviorCustom();
            bhav.InitAction = () => bhav.Actor.CurrentPlan.SetTarget(item, bhav.Actor.Hauled);
            return new BehaviorSequence(
                new BehaviorResolveInteraction(item, () => null//new InteractionHaul()
                ),
                bhav);
        }

        static public BehaviorCustom ExtractNextTargetAmount(TargetIndex index)
        {
            var bhav = new BehaviorCustom();
            bhav.InitAction = () =>
            {
                var actor = bhav.Actor;
                if (!actor.CurrentPlan.NextTarget(index))
                    throw new Exception();
                if (!actor.CurrentPlan.NextAmount(index))
                    throw new Exception();
            };
            return bhav;
        }
        
        static public Behavior MoveTo(TargetIndex targetIndex)
        {
            return new BehaviorResolvePath(targetIndex);
        }
        static public Behavior MoveTo(TargetIndex targetIndex, PathEndMode mode)
        {
            return new BehaviorResolvePath(targetIndex, mode);
        }
        
        /// <summary>
        /// Waits until an item that satisfies the conditions exists at the target location, then assigns that item as a task target with the specified index
        /// </summary>
        /// <param name="targetIndex"></param>
        /// <param name="global"></param>
        /// <param name="condition"></param>
        /// <returns></returns>
        static public Behavior WaitForItem(TargetIndex targetIndex, IntVec3 global, Func<Entity, bool> condition)
        {
            var bhav = new BehaviorWait();
            bhav.EndCondition = () =>
            {
                var actor = bhav.Actor;
                var item = actor.Map.GetEntitiesAt(global).FirstOrDefault(condition);
                if (item == null)
                    return false;
                actor.CurrentPlan.SetTarget(targetIndex, item);
                return true;
            };
            return bhav;
        }

        static public Behavior InteractInInventoryOrWorld(TargetIndex itemIndex, Func<Interaction> interactionFactory)
        {
            var bhav = new BehaviorSelector
            {
                Children = new List<Behavior>()
                {
                    new BehaviorSequence(
                        new BehaviorSelector(
                            new BehaviorItemIsInInventory(itemIndex),
                            new BehaviorResolvePath(itemIndex)),
                        new BehaviorResolveInteraction(itemIndex, interactionFactory())
                        )
                }
            };
            return bhav;
        }
    }
}
