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
        static public void Send(INetPeer net, bool remove, List<TargetArgs> targets, DesignationDef designation)
        {
            remove |= designation == null;
            var w = net.GetOutgoingStream();
            w.Write(p);
            w.Write(remove);
            w.Write((int)SelectionType.List);
            w.Write(targets);
            if(!remove)
                designation.Write(w);
        }
        static public void Send(INetPeer net, bool remove, Vector3 begin, Vector3 end, DesignationDef designation)
        {
            remove |= designation == null;
            var w = net.GetOutgoingStream();
            w.Write(p);
            w.Write(remove);
            w.Write((int)SelectionType.Box);
            w.Write(begin);
            w.Write(end);
            if(!remove)
                designation.Write(w);
        }
        static public void Receive(INetPeer net, BinaryReader r)
        {
            var remove = r.ReadBoolean();
            var selectionType = (SelectionType)r.ReadInt32();
            List<IntVec3> positions;
            DesignationDef designation;
            if (selectionType == SelectionType.Box)
            {
                var begin = r.ReadVector3();
                var end = r.ReadVector3();
                positions = new BoundingBox(begin, end).GetBoxIntVec3();
                designation = remove ? null : r.ReadDef<DesignationDef>();
                if (net is Server)
                    Send(net, remove, begin, end, designation);
            }
            else if (selectionType == SelectionType.List)
            {
                var list = r.ReadListTargets();
                positions = list.Select(t => (IntVec3)t.Global).ToList();
                designation = remove ? null : r.ReadDef<DesignationDef>();
                if (net is Server)
                    Send(net, remove, list, designation);
            }
            else
                throw new Exception();

            net.EventOccured(Components.Message.Types.ZoneDesignation, designation, positions, remove);
        }
    }
}
