using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using Microsoft.Xna.Framework;
using Start_a_Town_.Net;

namespace Start_a_Town_
{
    [EnsureStaticCtorCall]
    static class PacketEntityDesignation
    {
        const int SelectionRectangle = 0;
        const int SelectionList = 1;
        static int p;
        static PacketEntityDesignation()
        {
            p = Network.RegisterPacketHandler(Receive);
        }
        static public void Send(INetwork net, int type, IEnumerable<TargetArgs> targets, bool value)
        {
            Send(net, type, targets.Select(i => i.Object.RefID).ToList(), value);
        }
        static public void Send(INetwork net, int type, List<int> targets, bool value)
        {
            var strem = net.GetOutgoingStream();
            strem.Write(p);
            strem.Write(type);
            strem.Write(SelectionList);
            strem.Write(targets);
            strem.Write(value);
        }
        static public void Send(INetwork net, int type, Vector3 start, Vector3 end, bool value)
        {
            var strem = net.GetOutgoingStream();
            strem.Write(p);
            strem.Write(type);
            strem.Write(SelectionRectangle);
            strem.Write(start);
            strem.Write(end);
            strem.Write(value);
        }
        static public void Receive(INetwork net, BinaryReader r)
        {
            var manager = net.Map.Town.ChoppingManager;
            var type = r.ReadInt32();
            var selectionType = r.ReadInt32();
            switch(selectionType)
            {
                case SelectionRectangle:
                    {
                    var start = r.ReadVector3();
                    var end = r.ReadVector3();
                    var remove = r.ReadBoolean();
                    manager.Designate(type, start, end, remove);
                    if (net is Server)
                        Send(net, type, start, end, remove);
                    }
                    break;

                case SelectionList:
                    {
                    var ids = r.ReadListInt();
                    var remove = r.ReadBoolean();
                    manager.Designate(type, ids, remove);
                    if (net is Server)
                        Send(net, type, ids, remove);
                    }
                    break;

                default:
                    throw new Exception();
            }
           
        }
    }
}
