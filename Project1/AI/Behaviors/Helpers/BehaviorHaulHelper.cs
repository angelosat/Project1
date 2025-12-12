using System;
using System.Collections.Generic;
using Start_a_Town_.Core;

namespace Start_a_Town_
{
    class BehaviorHaulHelper
    {
        static public Behavior FindNearbyHaulOpportunity(BehaviorPerformTask source, Behavior gotoBhav, TargetIndex itemIndex)
        {
            var bhav = new BehaviorCustom
            {
                InitAction = () =>
                {
                    var actor = gotoBhav.Actor;
                    var hauling = actor.Hauled;
                    //var task = actor.CurrentTask;
                    var task = source.Task;
                    //var task = actor.AI.State.Current.Value.task;
                    //var bhav = actor.AI.State.Current.Value.behavior;
                    var desiredAmount = Math.Min(task.Count, hauling.StackAvailableSpace);
                    if (desiredAmount == 0)
                        return;
                    var map = actor.Map;
                    var item = task.GetTarget(itemIndex).Object;
                    var count = task.Count;

                    var potentialItems = HaulHelper.GetPotentialItemsNew(actor);
                    foreach (var pot in potentialItems)
                    {
                        if (!item.CanAbsorb(pot))
                            continue;
                        var unreservedAmount = actor.GetUnreservedAmount(pot);
                        if (unreservedAmount == 0)
                            continue;
                        desiredAmount = Math.Min(desiredAmount, unreservedAmount);
                        var amount = Math.Min(pot.StackSize, desiredAmount);
                        //actor.Reserve(task, pot, amount);
                        source.Reserve(pot, amount);
                        task.SetTarget(itemIndex, new TargetArgs(pot));
                        //actor.CurrentTaskBehavior.JumpTo(gotoBhav);
                        actor.AI.State.Behavior.JumpTo(gotoBhav);
                        actor.Net.ConsoleBox.Write("found new haul opportunity");
                        return;
                    }
                }
            };
            return bhav;
        }
        static public Behavior StartCarrying(BehaviorPerformTask source, TargetIndex storageIndex)
        {
            var bhav = new BehaviorCustom() { Mode = BehaviorCustom.Modes.Continuous };
            TargetArgs target = null;
            Interaction interaction = null;
            int amountToPickUp = 0;
            bhav.PreInitAction = () =>
            {
                {
                    var actor = bhav.Actor;
                    var task = actor.CurrentTask;
                    target = task.GetTarget(storageIndex);
                    var hauled = actor.Hauled;
                    var item = target.Object;
                    var reservedAmount = actor.GetReservedAmount(item);

                    // this is for testing purposes. i only end up using the reserved amount
                    var amountFromTask = task.GetAmount(storageIndex);
                    amountFromTask = amountFromTask == -1 ? item.StackSize : amountFromTask;
                    if (reservedAmount != amountFromTask)
                        //throw new Exception(); // TODO not sure i should be getting the haul amount from the reservation instead of the amount propert in the task
                        actor.Net.SyncReport($"Reserved amount [{reservedAmount}] different than target amount [{amountFromTask}] in [{actor.Name}]'s [{bhav.GetType()}] behavior");

                    //// the item stacksize might have been increased since the behavior initialization 
                    //if (amountFromTask > reservedAmount)
                    //    throw new Exception("target amount larger than reserved amount");
                    //// do i need to throw this? i do amountToPickUp = reservedAmount immediately below

                    amountToPickUp = reservedAmount;
                    interaction = new InteractionHaul(amountToPickUp);
                    if (amountToPickUp > item.StackSize)
                        throw new Exception();
                    actor.Interact(interaction, target);
                }
            };
            bhav.FailOn(() => interaction.State == Interaction.States.Failed);
            bhav.FailOnUnavailableTarget(storageIndex);
            bhav.SuccessCondition = a =>
            {
                if (interaction.IsFinished)
                {
                    var actor = bhav.Actor;
                    var task = actor.CurrentTask;
                    var hauled = actor.Hauled;
                    task.Count -= amountToPickUp;
                    //actor.Unreserve(target); // UNDONE ??? dont unreserve here because the ai might continue manipulating (placing/carrying) the item during the same behavior

                    if (target.Object != actor.Hauled)
                    {
                        actor.Unreserve(target); // ACTUALLY UNRESERVE SOURCE STACK HERE IN CASE THE HAULED STACK IS SPLIT FROM THE SOURCE ONE
                        //actor.Reserve(task, actor.Hauled);
                        source.Reserve(actor.Hauled);
                        task.SetTarget(storageIndex, actor.Hauled); // replacing task target with combined item because otherwise the behavior will fail since the old item is now disposed
                    }
                    return true;
                }
                return false;
            };
            return bhav;
        }
        static public Behavior DropInStorage(TargetIndex storageIndex)
        {
            var bhav = new BehaviorCustom() { Mode = BehaviorCustom.Modes.Continuous };
            GameObject hauledObj = null;
            bhav.PreInitAction = () =>
            {
                var actor = bhav.Actor;
                var interaction = new UseHauledOnTargetNew();
                hauledObj = actor.Hauled;
                var task = actor.CurrentTask;
                var target = task.GetTarget(storageIndex);
                actor.Interact(interaction, target);
            };
            bhav.AddEndCondition(() =>
            {
                var actor = bhav.Actor;
                var interaction = actor.CurrentInteraction;
                if (interaction.State == Interaction.States.Failed)
                    return BehaviorState.Fail;
                else if (interaction.State == Interaction.States.Finished)
                {
                    actor.Net.ConsoleBox.Write("successfully dropped item");
                    if (actor.Hauled is not Entity hauled)
                        return BehaviorState.Success;
                    if (!hauledObj.IsDisposed)
                        actor.Unreserve(hauledObj);
                    var task = actor.CurrentTask;
                    var target = task.GetTarget(storageIndex);
                    actor.Unreserve(target);
                }
                return BehaviorState.Running;
            });
            return bhav;
        }
        static public Behavior JumpIfNextStorageFound(BehaviorPerformTask source, Behavior gotoBhav, TargetIndex storageIndex)
        {
            var bhav = new BehaviorCustom() { Mode = BehaviorCustom.Modes.Instant };
            Entity hauledObj = null;
            bhav.InitAction = () =>
            {
                var actor = gotoBhav.Actor;
                hauledObj = actor.Hauled as Entity;
                if (hauledObj == null)
                    return;
                var task = actor.CurrentTask;
                var target = task.GetTarget(storageIndex);
                var cell = target.Global.ToCell();
                var targets = GetMoreValidStoragePlaces(actor, hauledObj, cell);

                foreach (var tar in targets)
                {
                    if (tar.HasObject && !tar.Object.CanAbsorb(hauledObj))
                        continue;
                    if (!actor.CanReserve(tar))
                        continue;
                    source.Reserve(tar, 1);
                    task.SetTarget(storageIndex, tar);
                    actor.AI.State.Behavior.JumpTo(gotoBhav);
                    actor.Net.ConsoleBox.Write("found next storage place " + tar.ToString());
                    return;
                }
            };
            return bhav;
        }
        static public IEnumerable<TargetArgs> GetMoreValidStoragePlaces(Actor actor, Entity item, IntVec3 center)
        {
            var storage = item.Map.Town.ZoneManager.GetZoneAt<Stockpile>(center.Below);
            foreach (var spot in storage.GetPotentialHaulTargets(actor, item))
                yield return spot;
        }
    }
}
