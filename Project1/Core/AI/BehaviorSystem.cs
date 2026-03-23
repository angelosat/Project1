using Project1.Core.Entities.Actors;
using Project1.Core.Simulation;

namespace Project1.Core.AI
{
    internal class BehaviorSystem : SimulationSystem
    {
        public BehaviorSystem(MapBase map) : base(map)
        {
            map.Events.ListenTo<ReservationInvalidatedEvent>(OnReservationInvalidated);
        }
        private void OnReservationInvalidated(ReservationInvalidatedEvent e)
        {
            var actor = this.Map.World.Get<Actor>(e.Reservation.Actor);
            actor.CurrentPlan.Cancel();
        }
    }
}
