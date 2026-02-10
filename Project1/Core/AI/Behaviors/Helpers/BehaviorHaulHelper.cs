using System;
using Project1.Core.Interactions;
using Project1.Core.Net;
using Project1.Core.AI.Behaviors.Reserve;
using Project1.Core.AI.Behaviors.NodeTypes;

namespace Project1.Core.AI.Behaviors.Helpers
{
    class BehaviorHaulHelper
    {
        [Obsolete]
        static public Behavior StartCarrying(BehaviorExecutePlan source, TargetIndex storageIndex)
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
                        (actor.Net as Server)?.SyncReport($"Reserved amount [{reservedAmount}] different than target amount [{amountFromTask}] in [{actor.Name}]'s [{bhav.GetType()}] behavior");

                    //// the item stacksize might have been increased since the behavior initialization 
                    //if (amountFromTask > reservedAmount)
                    //    throw new Exception("target amount larger than reserved amount");
                    //// do i need to throw this? i do amountToPickUp = reservedAmount immediately below

                    amountToPickUp = reservedAmount;
                    throw new Exception();
                    interaction = null;//  new InteractionHaul(amountToPickUp);
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
        
        
    }
}
