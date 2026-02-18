using Microsoft.Xna.Framework;
using Project1.Core.Helpers;
using Project1.Core.Input;
using Project1.Core.Networking;
using Project1.Core.Serialization;
using Project1.Framework;
using Project1.Framework.Events;
using Project1.Framework.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Project1.Core.Towns.Designations
{
    [EnsureStaticCtorCall]
    class PacketsDesignations
    {
        static int p;
        static PacketsDesignations()
        {

            p = Registry.PacketHandlers.Register(Receive);

            Registry.PlayerInputEventHooks.Register<PlayerDesignationEvent>(OnPlayerDesignation);
        }

        private static void OnPlayerDesignation(PlayerDesignationEvent e)
        {
            throw new NotImplementedException();
            //Send(Client.Instance, e.Removal, e.Targets, e.Designation);
        }

        static public void Send(NetEndpoint net, bool remove, IEnumerable<TargetArgs> targets, DesignationDef designation)
        {
            remove |= designation == null;
            var w = net.BeginPacketImmediate(p);

            w.Write(remove);
            w.Write((int)SelectionType.List);
            w.Write(targets.ToList());
            if(!remove)
                designation.Write(w);
        }
        static public void Send(NetEndpoint net, bool remove, IntVec3 begin, IntVec3 end, DesignationDef designation)
        {
            remove |= designation == null;
            var w = net.BeginPacketImmediate(p);
            w.Write(remove);
            w.Write((int)SelectionType.Box);
            w.Write(begin);
            w.Write(end);
            if(!remove)
                designation.Write(w);
        }
        static public void Receive(NetEndpoint net, Packet pck)
        {
            var r = pck.PacketReader;
            var remove = r.ReadBoolean();
            var selectionType = (SelectionType)r.ReadInt32();
            IEnumerable<TargetArgs> targetList;
            DesignationDef designation;
            if (selectionType == SelectionType.Box)
            {
                var begin = r.ReadIntVec3();
                var end = r.ReadIntVec3();
                var positions = new BoundingBox(begin, end).ToListIntVec3();
                designation = remove ? null : r.ReadDef<DesignationDef>();
                if (net is Server)
                    Send(net, remove, begin, end, designation);
                targetList = positions.Select(p => new TargetArgs(net.Map, p));
            }
            else if (selectionType == SelectionType.List)
            {
                targetList = r.ReadListTargets(net);
                foreach (var t in targetList)
                    t.Map = net.Map;
                designation = remove ? null : r.ReadDef<DesignationDef>();
                if (net is Server)
                    Send(net, remove, targetList, designation);
            }
            else
                throw new Exception();
            net.Map.Town.DesignationManager.Add(designation, targetList, remove);
        }
    }
}
