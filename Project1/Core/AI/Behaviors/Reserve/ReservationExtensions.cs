using Microsoft.Xna.Framework;
using Project1.Core.Towns.Zones;
using Project1.Core.Entities.Actors;
using Project1.Core.Entities;
using Project1.Framework;
using Project1.Core.Blocks;

namespace Project1.Core.AI.Behaviors.Reserve
{
    static public class ReservationExtensions
    {
        static public bool CanReach(this Actor actor, Zone zone) => actor.CanReach(zone[0]);
        static public bool CanReserve(this Actor obj, BlockEntity target)
        {
            var map = obj.Map;
            return map.Town.ReservationManager.CanReserve(obj, new TargetArgs(target), 1, false);
        }
        static public bool CanReserve(this Actor obj, IntVec3 target)
        {
            var map = obj.Map;
            return map.Town.ReservationManager.CanReserve(obj, new TargetArgs(map, target), 1, false);
        }
        static public bool CanReserve(this Actor obj, Vector3 target, int stackcount = -1, bool force = false)
        {
            var map = obj.Map;
            return map.Town.ReservationManager.CanReserve(obj, new TargetArgs(map, target), stackcount, force);
        }
        static public bool CanReserve(this Actor obj, TargetArgs target, int stackcount = -1, bool force = false)
        {
            return obj.Map.Town.ReservationManager.CanReserve(obj, target, stackcount, force);
        }
        static public bool CanReserve(this Actor obj, GameObject target, int stackcount = -1, bool force = false)
        {
            return obj.Map.Town.ReservationManager.CanReserve(obj, new TargetArgs(target), stackcount, force);
        }
        static public bool CanReserve(this Actor obj, Entity target, int stackcount = -1, bool force = false)
        {
            return obj.Map.Town.ReservationManager.CanReserve(obj, new TargetArgs(target), stackcount, force);
        }
        static public void Unreserve(this Actor obj)
        {
            obj.LastMap.Town.ReservationManager.Unreserve(obj);
        }
        static public void Unreserve(this Actor obj, GameObject tar)
        {
            obj.Map.Town.ReservationManager.Unreserve(obj, new TargetArgs(tar));
        }
        static public void Unreserve(this Actor obj, TargetArgs target)
        {
            obj.Map.Town.ReservationManager.Unreserve(obj, target);
        }

        static public int GetUnreservedAmount(this Actor obj, TargetArgs i)
        {
            return obj.Map.Town.ReservationManager.GetUnreservedAmount(i);
        }
       
        static public bool TryGetUnreservedAmount(this Actor obj, GameObject i, out int amount)
        {
            amount = obj.Map.Town.ReservationManager.GetUnreservedAmount(new TargetArgs(i));
            return amount > 0;
        }
        static public int GetUnreservedAmount(this Actor obj, Vector3 i)
        {
            return obj.Map.Town.ReservationManager.GetUnreservedAmount(new TargetArgs(obj.Map, i));
        }
    }
}
