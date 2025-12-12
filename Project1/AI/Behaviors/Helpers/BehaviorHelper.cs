using System;
using System.Collections.Generic;
using System.Linq;
using Start_a_Town_.AI.Behaviors;

namespace Start_a_Town_
{
    class BehaviorHelper
    {
        internal static Behavior SetTarget(TargetIndex a, GameObject value)
        {
            var bhav = new BehaviorCustom();
            bhav.InitAction = () => bhav.Actor.CurrentTask.SetTarget(a, value);
            return bhav;
        }
        internal static Behavior SetTarget(TargetIndex a, IntVec3 value)
        {
            var bhav = new BehaviorCustom();
            bhav.InitAction = () => bhav.Actor.CurrentTask.SetTarget(a, value.At(bhav.Actor.Map));
            return bhav;
        }
        internal static Behavior SetTarget(TargetIndex a, (MapBase map, IntVec3 value) position)
        {
            var bhav = new BehaviorCustom();
            bhav.InitAction = () => bhav.Actor.CurrentTask.SetTarget(a, new TargetArgs(position.map, position.value));
            return bhav;
        }
        internal static Behavior SetTarget(TargetIndex a, Func<TargetArgs> targetGetter)
        {
            var bhav = new BehaviorCustom();
            bhav.InitAction = () => bhav.Actor.CurrentTask.SetTarget(a, targetGetter());
            return bhav;
        }
        internal static Behavior CarryFromInventory(TargetIndex item)
        {
            return new BehaviorInteractionNew(item, () => new InteractionHaul());
        }
        internal static Behavior CarryFromInventoryAndReplaceTarget(TargetIndex item)
        {
            var bhav = new BehaviorCustom();
            bhav.InitAction = () => bhav.Actor.CurrentTask.SetTarget(item, bhav.Actor.Hauled);
            return new BehaviorSequence(
                new BehaviorInteractionNew(item, () => new InteractionHaul()),
                bhav);
        }

        static public BehaviorCustom ExtractNextTargetAmount(TargetIndex index)
        {
            var bhav = new BehaviorCustom();
            bhav.InitAction = () =>
            {
                var actor = bhav.Actor;
                if (!actor.CurrentTask.NextTarget(index))
                    throw new Exception();
                if (!actor.CurrentTask.NextAmount(index))
                    throw new Exception();
            };
            return bhav;
        }
       
        static public BehaviorCustom JumpIfMoreTargets(Behavior jumpBhav, TargetIndex index)
        {
            var bhav = new BehaviorCustom();
            bhav.InitAction = () =>
            {
                var actor = bhav.Actor;
                var task = actor.CurrentTask;
                if (task.GetTargetQueue(index)?.Any() ?? false)
                    actor.AI.State.Behavior.JumpTo(jumpBhav);

                //actor.CurrentTaskBehavior.JumpTo(jumpBhav);

            };
            return bhav;
        }
        static public BehaviorCustom JumpIfTrue(Behavior jumpBhav, Func<bool> predicate)
        {
            var bhav = new BehaviorCustom();
            bhav.InitAction = () =>
            {
                var actor = bhav.Actor;
                if(predicate())
                    //actor.CurrentTaskBehavior.JumpTo(jumpBhav);
                actor.AI.State.Behavior.JumpTo(jumpBhav);

            };
            return bhav;
        }
        static public BehaviorCustom ToolNotRequired()
        {
            var bhav = new BehaviorCustom()
            {
                FailCondition = a => a.CurrentTask.Tool.HasObject,
                Label = "ToolNotRequired"
            };
            return bhav;
        }
       
        static public Behavior JumpIfNextCarryStackable(Behavior jumpBhav, TargetIndex ind)
        {
            var bhav = new BehaviorCustom
            {
                InitAction = () =>
                {
                    var actor = jumpBhav.Actor;
                    var carried = actor.Hauled;
                    var task = actor.CurrentTask;
                    var targets = task.GetTargetQueue(ind);
                    if (!targets?.Any() ?? false)
                        return;
                    var nextTarget = targets[0].Object;
                if (nextTarget.CanAbsorb(carried))
                        //actor.CurrentTaskBehavior.JumpTo(jumpBhav);
                    actor.AI.State.Behavior.JumpTo(jumpBhav);

                }
            };
            return bhav;
        }
        static public Behavior JumpIfNextCarryStackable(Behavior jumpBhav, TargetIndex ind, TargetIndex amountInd)
        {
            var bhav = new BehaviorCustom
            {
                InitAction = () =>
                {
                    var actor = jumpBhav.Actor;
                    var carried = actor.Hauled;
                    var task = actor.CurrentTask;
                    var targets = task.GetTargetQueue(ind);
                    if (!targets?.Any() ?? false)
                        return;
                    var nextTarget = targets[0].Object;
                    var amounts = task.GetAmountQueue(amountInd);
                    var nextAmount = amounts[0];
                    if (carried.CanAbsorb(nextTarget, nextAmount))
                        //actor.CurrentTaskBehavior.JumpTo(jumpBhav);
                    actor.AI.State.Behavior.JumpTo(jumpBhav);

                }
            };
            return bhav;
        }
        [Obsolete]
        static public Behavior StartCarrying(TargetIndex index)
        {
            return new BehaviorInteractionNew(index, () => new InteractionHaul()).FailOnUnavailableTarget(index);
        }
        [Obsolete]
        static public Behavior StartCarrying(TargetIndex index, TargetIndex amountIndex)
        {
            var bhav = new BehaviorInteractionNew(index);
            bhav.InteractionFactory = () => new InteractionHaul(bhav.Actor.CurrentTask.GetAmount(amountIndex));
            return bhav;
        }
        static public Behavior PlaceCarried(TargetIndex index)
        {
            return new BehaviorInteractionNew(index, ()=> new UseHauledOnTarget());
        }

        static public Behavior MoveTo(TargetIndex targetIndex)
        {
            return new BehaviorGetAtNewNew(targetIndex);
        }
        static public Behavior MoveTo(TargetIndex targetIndex, PathEndMode mode)
        {
            return new BehaviorGetAtNewNew(targetIndex, mode);
        }
        
        /// <summary>
        /// Waits until an item that satisfies the conditions exists at the target location, then assigns that item as a task target with the specified index
        /// </summary>
        /// <param name="targetIndex"></param>
        /// <param name="global"></param>
        /// <param name="condition"></param>
        /// <returns></returns>
        static public Behavior WaitForItem(TargetIndex targetIndex, IntVec3 global, Func<GameObject, bool> condition)
        {
            var bhav = new BehaviorWait();
            bhav.EndCondition = () =>
            {
                var actor = bhav.Actor;
                var item = actor.Map.GetObjects(global).FirstOrDefault(condition);
                if (item == null)
                    return false;
                actor.CurrentTask.SetTarget(targetIndex, item);
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
                            new BehaviorGetAtNewNew(itemIndex)),
                        new BehaviorInteractionNew(itemIndex, interactionFactory())
                        )
                }
            };
            return bhav;
        }
    }
}
