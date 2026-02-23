using Microsoft.Xna.Framework;
using Project1.Core.Entities;
using Project1.Core.Entities.Actors;
using Project1.Core.Simulation;
using Project1.Core.Towns;
using Project1.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Project1.Core.AI.Reservations
{
    public class ReservationManager : TownComponent
    {
        readonly List<Reservation> Reservations = [];
        readonly Dictionary<TargetArgs, List<Reservation>> ByTarget = [];
        readonly Dictionary<EntityRefId, List<Reservation>> ByActor = [];
        string _name = "Reservations";
        public override string Name => _name; 
        public ReservationManager(Town town)
        {
            this.Town = town;
            this.Town.Map.Events.ListenTo<EntityForbiddenEvent>(OnEntityForbidden);
            this.Town.Map.Events.ListenTo<EntityDespawnedEvent>(OnEntityDespawned);
            this.Town.Map.Events.ListenTo<CellsInvalidatedEvent>(OnCellInvalidated);
        }
        private void OnEntityDespawned(EntityDespawnedEvent e)
        {
            this.Unreserve(e.Entity);
        }
        private void OnEntityForbidden(EntityForbiddenEvent e)
        {
            this.Unreserve(e.Entity);
        }
        private void OnCellInvalidated(CellsInvalidatedEvent e)
        {
            foreach (var cell in e.Positions)
                this.Unreserve(new TargetArgs(e.Map, cell));
        }
        void AddReservation(Reservation vation)
        {
            this.Reservations.Add(vation);

            if (!this.ByTarget.TryGetValue(vation.Target, out var bytargetlist))
                this.ByTarget[vation.Target] = bytargetlist = [];
            bytargetlist.Add(vation);

            if (!this.ByActor.TryGetValue(vation.Actor, out var byactorlist))
                this.ByActor[vation.Actor] = byactorlist = [];
            byactorlist.Add(vation);
        }
        internal bool Reserve(Actor actor, Plan plan, TargetArgs target, int stackCount = -1)
        {
            if (target.Type == TargetType.Null)
                throw new Exception();

            if (target.Type == TargetType.Cell)
                stackCount = 1;
            else if (target.Type == TargetType.Entity)
                stackCount = (stackCount != -1) ? stackCount : target.Object.StackSize;

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

            var vation = new Reservation(actor, target, stackCount);
          
            if (target.HasObject && stackCount > target.Object.StackSize)
                throw new Exception();

            TryCancelExistingReservations(target, stackCount);
            this.AddReservation(vation);

            return true;
        }
        internal bool ReserveAsManyAsPossible(Actor actor, Plan task, TargetArgs target, int desiredAmount = -1)
        {
            if (target.Type == TargetType.Null || target.Type == TargetType.Cell)
                throw new Exception();
            var unreservedAmount = this.GetUnreservedAmount(target);
            if (unreservedAmount == 0)
                throw new Exception();
            desiredAmount = desiredAmount == -1 ? target.Object.StackMax : desiredAmount;
            var count = Math.Min(desiredAmount, unreservedAmount);
            if (count > target.Object.StackMax)
                throw new Exception();
            var vation = new Reservation(actor, target, count);// { Task = task };
            if (target.HasObject && count > target.Object.StackSize)
                throw new Exception();
            this.AddReservation(vation);
            return true;
        }
        private void TryCancelExistingReservations(TargetArgs target, int stackCount)
        {
            List<Reservation> foundStacks = [];
            int foundAmount = 0;
            for (int i = 0; i < this.Reservations.Count; i++)
            {
                var r = this.Reservations[i];

                if (r.Target.Type != target.Type)
                    continue;
                else if (r.Target.Type == TargetType.Cell && r.Target.Global == target.Global)
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
            var actor = this.Map.World.GetEntity<Actor>(r.Actor);
            var task = actor.CurrentPlan;
            actor.Net.ConsoleBox.Write("cancelling " + actor.Name + "'s task's reservations ");
            task.Cancel();
        }
        internal void Unreserve(Actor actor)
        {
            if (!this.ByActor.TryGetValue(actor.RefId, out var reservationsByActor))
                return;
            foreach (var res in reservationsByActor)
            {
                this.Reservations.Remove(res);
                this.Map.Events.Post(new ReservationInvalidatedEvent(res));
                var listbytarget = this.ByTarget[res.Target];
                listbytarget.Remove(res);
                if (listbytarget.Count == 0)
                    this.ByTarget.Remove(res.Target);
            }
            reservationsByActor.Clear();
            this.ByActor.Remove(actor.RefId);
        }
        void Unreserve(TargetArgs target)
        {
            if (!this.ByTarget.TryGetValue(target, out var reservationsByTarget))
                return;
            HashSet<Actor> actorsToInterrupt = [];
            foreach (var res in reservationsByTarget)
            {
                this.Reservations.Remove(res);
                this.Map.Events.Post(new ReservationInvalidatedEvent(res));
                var listbyactor = this.ByActor[res.Actor];
                listbyactor.Remove(res);
                if (listbyactor.Count == 0)
                    this.ByActor.Remove(res.Actor);
                actorsToInterrupt.Add(target.World.GetEntity<Actor>(res.Actor));
            }
            reservationsByTarget.Clear();
            this.ByTarget.Remove(target);
        }
        internal bool CanReserve(GameObject actor, TargetArgs target, int stackcount = -1, bool ignoreOtherReservations = false)
        {
            if (target.Type == TargetType.Entity && target.Object.Owner == actor)
                return true;
            if (target.Type == TargetType.Cell && stackcount > 1)
                throw new Exception();
            if (target.IsForbidden)
                return false;
            
            if (ignoreOtherReservations)
                return true;

            var unreservedAmount = this.GetUnreservedAmount(target);
            stackcount = stackcount == -1 ? (target.HasObject ? target.Object.StackSize : 1) : stackcount;
            return stackcount <= unreservedAmount;

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
            else if (target.Type == TargetType.Cell)
                amount = 1 - sum;
            else if (target.Type == TargetType.BlockEntity)
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
        }
        public override void Load(SaveTag tag)
        {
            this.Reservations.Clear();
            tag.TryGetTag("Reservations", v =>
            {
                var list = v.Value as List<SaveTag>;
                foreach (var t in list)
                    this.AddReservation(new Reservation(this.Town.Map, t));
            });
        }
    }
}
