using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;

namespace Start_a_Town_
{
    public partial class ReservationManager : TownComponent
    {
        string _name = "Reservations";
        public override string Name => _name; 
        public ReservationManager(Town town)
        {
            this.Town = town;
        }
        readonly List<Reservation> Reservations = new();
        static int TaskIDSequence = 0;
        static public int GetNextTaskID()
        {
            return TaskIDSequence++;
        }


        public bool Reserve(Actor actor, TargetArgs target, int stackCount = -1)
        {
            return this.Reserve(actor, actor.CurrentTask, target, stackCount);
        }
        internal bool Reserve(Actor actor, AITask task, TargetArgs target, int stackCount)
        {
            if (target.Type == TargetType.Null)
                throw new Exception();

            /// MOVED THIS HERE FROM BELOW (check comment below)
            if (target.Type == TargetType.Position)
                stackCount = 1;
            else if (target.Type == TargetType.Entity)
                stackCount = (stackCount != -1) ? stackCount : target.Object.StackSize;

            // update existing reservation if it exists
            var existing = this.Reservations.FirstOrDefault(r => r.Target.IsEqual(target) && r.Actor == actor.RefId);
            if (existing != null)
            {
                if (stackCount == existing.Amount)
                    return true;
                var availableAmount = this.GetUnreservedAmount(target) + existing.Amount;
                if (availableAmount < stackCount)
                    return false;
                existing.Amount = stackCount;
                return true;
            }


            /// do i need to check this here? if the behavior has reached the point where it's reserving items, then it should do so, and cancel existing reservations by other actors
            /// because the behavior might have been a result of player forcing a task
            //if (!actor.CanReserve(target, stackCount))
            //    throw new Exception(); // this will probably throw if the canreserve check has been omitted in a taskgiver, or a reservation has been omitted in the initreservations of another behavior
           
            


            /// I MOVED THIS TO THE BEGINNING OF THE FUNCTION because I check the stackCount against any existing reservations which are NEVER -1
            ///if (target.Type == TargetType.Position)
            ///    stackCount = 1;
            ///else if (target.Type == TargetType.Entity)
            ///    stackCount = (stackCount != -1) ? stackCount : target.Object.StackSize;//.StackMax; // UNDONE was there a reason i put stackmax?
            var vation = new Reservation(actor, target, stackCount)
            {
                Task = task
            };
            if (target.HasObject && stackCount > target.Object.StackSize)
                throw new Exception();

            // signal holders of possible existing reservations
            TryCancelExistingReservations(target, stackCount);
            Reservations.Add(vation);
            return true;
        }
        internal bool ReserveAsManyAsPossible(Actor actor, AITask task, TargetArgs target, int desiredAmount = -1)
        {
            if (target.Type == TargetType.Null || target.Type == TargetType.Position)
                throw new Exception();
            var unreservedAmount = this.GetUnreservedAmount(target);
            if (unreservedAmount == 0)
                throw new Exception();
            desiredAmount = desiredAmount == -1 ? target.Object.StackMax : desiredAmount;
            var count = Math.Min(desiredAmount, unreservedAmount);
            if (count > target.Object.StackMax)
                throw new Exception();
            var vation = new Reservation(actor, target, count) { Task = task };
            if (target.HasObject && count > target.Object.StackSize)
                throw new Exception();
            Reservations.Add(vation);
            return true;
        }
        private void TryCancelExistingReservations(TargetArgs target, int stackCount)
        {
            List<Reservation> foundStacks = new();
            int foundAmount = 0;
            for (int i = 0; i < this.Reservations.Count; i++)
            {
                var r = this.Reservations[i];

                if (r.Target.Type != target.Type)
                    continue;
                else if (r.Target.Type == TargetType.Position && r.Target.Global == target.Global)
                {
                    CancelReservation(r);
                }
                else if (r.Target.Type == TargetType.Entity && r.Target.Object != null && r.Target.Object == target.Object)
                {
                    foundStacks.Add(r);
                    foundAmount += r.Amount;
                }
            }
            if (target.HasObject)
            {
                if (foundAmount + stackCount > target.Object.StackMax)
                {
                    for (int i = 0; i < foundStacks.Count; i++)
                    {
                        var r = foundStacks[i];
                        CancelReservation(r);
                    }
                }
            }
        }

        private void CancelReservation(Reservation r)
        {
            var actor = this.Map.Net.GetNetworkObject<Actor>(r.Actor);
            var task = actor.CurrentTask;
            if (task.ID != r.TaskID)
                throw new Exception();
            actor.Net.ConsoleBox.Write("cancelling " + actor.Name + "'s task's reservations ");
            task.Cancel();
        }

        internal void Unreserve(GameObject actor)
        {
            Reservations.RemoveAll(r => r.Actor == actor.RefId);
        }
        
        internal void Unreserve(GameObject actor, TargetArgs target)
        {
            Reservations.RemoveAll(r => r.Actor == actor.RefId && r.Target.IsEqual(target));
        }
        internal bool CanReserve(GameObject actor, TargetArgs target, int stackcount = -1, bool ignoreOtherReservations = false)
        {
            if (target.Type == TargetType.Entity && target.Object.Parent == actor)
                return true;
            if (target.Type == TargetType.Position && stackcount > 1)
                throw new Exception();
            if (target.IsForbidden)
                return false;
            
            if (ignoreOtherReservations)
                return true;

            var unreservedAmount = this.GetUnreservedAmount(target);
            stackcount = stackcount == -1 ? (target.HasObject ? target.Object.StackSize : 1) : stackcount;
            return stackcount <= unreservedAmount;

        }
        internal bool CanReserve(TargetArgs target)
        {
            if (target.IsForbidden)
                return false;
            return Reservations.FirstOrDefault(r =>
            {
                if (r.Target.Type != target.Type)
                    return false;
                if (r.Target.Type == TargetType.Entity && r.Target.Object != null && r.Target.Object == target.Object)
                    return true;
                else if (r.Target.Type == TargetType.Position && r.Target.Global == target.Global)
                    return true;
                return false;
            }) == null;
        }
        internal bool CanReserve(Vector3 global)
        {
            return Reservations.FirstOrDefault(r =>
               r.Target.Type == TargetType.Position && r.Target.Global == global
            ) == null;
        }
        internal int GetUnreservedAmount(GameObject obj)
        {
            return GetUnreservedAmount(new TargetArgs(obj));
        }
        internal int GetUnreservedAmount(TargetArgs target)
        {
            var sum = 0;
            foreach (var r in Reservations) // there might be multiple reservations for the same target, for example an item with stacksize > 1 might be reserved by multiple actors for different tasks
                if (r.Target.IsEqual(target)) 
                {
                    if (r.Amount == -1) // if any of the reservations have amount == -1, it automatically means that the whole stack is reserved, so return 0
                        return 0;
                    sum += r.Amount;
                }
            int amount = 0;

            if (target.Type == TargetType.Entity)
                amount = target.Object.StackSize - sum;
            else if (target.Type == TargetType.Position)
                amount = 1 - sum;

            if (amount < 0)
                throw new Exception(); // CHECK if probably the item was partially reserved for a haul action and it wasn't unreserved after the split stack was picked up

            return amount;
        }
        internal int GetReservedAmount(Actor actor, GameObject item)
        {
            if (item == null)
                throw new Exception();
            return this.Reservations.FirstOrDefault(t => t.Actor == actor.RefId && t.Target.Object == item)?.Amount ?? 0;
        }
        internal bool IsReserved(GameObject gameObject)
        {
            return this.Reservations.Any(t => t.Target.Object == gameObject);
        }
        internal bool IsReserved(IntVec3 global)
        {
            return this.Reservations.Any(t => t.Target.Global == (Vector3)global);
        }
        
        protected override void AddSaveData(SaveTag tag)
        {
            var reservationsTag = new SaveTag(SaveTag.Types.List, "Reservations", SaveTag.Types.Compound);
            foreach (var r in this.Reservations)
                reservationsTag.Add(r.Save());
            tag.Add(reservationsTag);
            tag.Add(TaskIDSequence.Save("TaskIDSequence"));
        }
        public override void Load(SaveTag tag)
        {
            this.Reservations.Clear();
            tag.TryGetTag("Reservations", v =>
            {
                var list = v.Value as List<SaveTag>;
                foreach (var t in list)
                    this.Reservations.Add(new Reservation(this.Town.Map, t));
            });
            tag.TryGetTagValue<int>("TaskIDSequence", out TaskIDSequence);
        }
    }
}
