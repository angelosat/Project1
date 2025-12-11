using Microsoft.Xna.Framework;

namespace Start_a_Town_
{
    static public class ReservationExtensions
    {
        static public bool Reserve(this Actor obj, AITask task, TargetIndex index)
        {
            //var task = obj.CurrentTask;
            var target = task.GetTarget(index);
            return obj.Reserve(task, target);
        }
        static public bool Reserve(this Actor obj, AITask task, TargetArgs target, int stackcount = -1)
        {
            return obj.Map.Town.ReservationManager.Reserve(obj, task, target, stackcount);
        }
        //static public bool Reserve(this Actor obj, TargetArgs target, int stackcount = -1)
        //{
        //    return obj.Map.Town.ReservationManager.Reserve(obj, target, stackcount);
        //}

        static public bool Reserve(this Actor obj, AITask task, Vector3 target)
        {
            var map = obj.Map;
            return map.Town.ReservationManager.Reserve(obj, task, new TargetArgs(map, target), 1);
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
            obj.Map.Town.ReservationManager.Unreserve(obj);
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
