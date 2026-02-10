using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Project1.Core.Helpers;
using Project1.Core.Net;
using Project1.Framework;
using Project1.Framework.Events;
using Project1.Framework.Serialization;

namespace Project1.Core.Towns.Designations
{
    class PacketDesignation
    {
        enum SelectionType { List, Box }
        static int p;
        static public void Init()
        {
            p = Registry.PacketHandlers.Register(Receive);
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
            var w = net.BeginPacket(p);
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
                var positions = new BoundingBox(begin, end).GetBoxIntVec3();
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
