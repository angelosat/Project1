using System.Linq;

namespace Start_a_Town_
{
    class TaskGiverVisitorRentRoom : TaskGiver
    {
        protected override AITask TryAssignTask(Actor actor)
        {
            if (actor.Possessions.Has(RoomRoleDefOf.Bedroom))
                return null;
            return SearchByTavern(actor);
        }

        private static AITask SearchByBed(Actor actor)
        {
            var map = actor.Map;
            var town = map.Town;
            var visitorbeds = town.GetUtilities(Utility.Types.Sleeping).Select(g => (global: g, bed: BlockBed.GetEntity(map, g))).Where(e => e.bed.Type == BlockBedEntity.Types.Visitor);
            // iterate inns or beds?
            // TODO check already claimed bed
            // check if owning a bedroom? check if owning a room containing a bed? or check ownership of an actual bed?
            foreach (var (bedglobal, bed) in visitorbeds)
            {
                // TODO if this taskgiver's purpose is to claim/rent a bed(room),
                // then it maybe doesnt make sense to check canreserve and canoperate here
                if (!actor.CanReserve(bedglobal))
                    continue;
                if (!actor.CanOperate(bedglobal, out var operatingPos))
                    continue;
                var room = town.RoomManager.GetRoomAt(bedglobal);
                if (room.Workplace is not Tavern tavern)
                    continue;
                if (!tavern.CanOfferBed)
                    continue;
                var customer = tavern.AddCustomer(actor, town.RoomManager.GetRoomAt(bedglobal));
                return new AITask(typeof(TaskBehaviorSleepingNew), (map, bedglobal), (map, operatingPos)) { ShopID = tavern.ID, CustomerProps = customer };
            }
            return null;
        }
        static AITask SearchByTavern(Actor actor)
        {
            var map = actor.Map;
            var town = map.Town;
            var actorMoney = actor.GetMoneyTotal();
            var taverns = town.ShopManager.GetShops<Tavern>();
            var visitorProps = actor.GetVisitorProperties();
            foreach(var tavern in taverns)
            {
                if (visitorProps.IsBlacklisted(tavern))
                    continue;
                var freeBedroom = tavern.GetFreeBedroom(actorMoney);
                if (freeBedroom is null)
                    continue;
                tavern.AddCustomer(actor, freeBedroom);
                return new AITask(typeof(TaskBehaviorVisitorRentBed)) { ShopID = tavern.ID };
            }
            return null;
        }
    }
}
