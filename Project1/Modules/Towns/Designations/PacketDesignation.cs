using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using Microsoft.Xna.Framework;
using Start_a_Town_.Net;

namespace Start_a_Town_
{
    class PacketDesignation
    {
        enum SelectionType { List, Box }
        static int p;
        static public void Init()
        {
            p = Network.RegisterPacketHandler(Receive);
        }
        static public void Send(INetEndpoint net, bool remove, IEnumerable<TargetArgs> targets, DesignationDef designation)
        {
            remove |= designation == null;
            var w = net.GetOutgoingStreamOrderedReliable();
            w.Write(p);
            w.Write(remove);
            w.Write((int)SelectionType.List);
            w.Write(targets);
            if(!remove)
                designation.Write(w);
        }
        static public void Send(INetEndpoint net, bool remove, IntVec3 begin, IntVec3 end, DesignationDef designation)
        {
            remove |= designation == null;
            var w = net.GetOutgoingStreamOrderedReliable();
            w.Write(p);
            w.Write(remove);
            w.Write((int)SelectionType.Box);
            w.Write(begin);
            w.Write(end);
            if(!remove)
                designation.Write(w);
        }
        static public void Receive(INetEndpoint net, BinaryReader r)
        {
            var remove = r.ReadBoolean();
            var selectionType = (SelectionType)r.ReadInt32();
            IEnumerable<TargetArgs> targetList;
            DesignationDef designation;
            if (selectionType == SelectionType.Box)
            {
                var begin = r.ReadVector3();
                var end = r.ReadVector3();
                var positions = new BoundingBox(begin, end).GetBoxIntVec3();
                designation = remove ? null : r.ReadDef<DesignationDef>();
                if (net is Server)
                    Send(net, remove, begin, end, designation);
                targetList = positions.Select(p => new TargetArgs(net.Map, p));
            }
            else if (selectionType == SelectionType.List)
            {
                targetList = r.ReadListTargets(net);
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
