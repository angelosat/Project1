using Microsoft.Xna.Framework;
using Project1.Core.Entities;
using Project1.Core.Helpers;
using Project1.Core.Input;
using Project1.Core.Networking;
using Project1.Core.Screens;
using Project1.Core.Serialization;
using Project1.Framework;
using Project1.Framework.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Project1.Core.Towns.Designations
{
    [EnsureStaticCtorCall]
    class PacketsDesignations
    {
        static readonly PacketId p, _pPlayerDesignation, _pPlayerDesignationEntities;
        static PacketsDesignations()
        {
            _pPlayerDesignation = Registry.PacketHandlers.Register(ReceiveCells);
            _pPlayerDesignationEntities = Registry.PacketHandlers.Register(ReceiveEntities);
            Registry.PlayerInputEventHooks.Register<PlayerDesignationCellsEvent>(OnPlayerDesignation);
            Registry.PlayerInputEventHooks.Register<PlayerDesignationEntitiesEvent>(OnPlayerDesignationEntities);
        }
        private static void OnPlayerDesignationEntities(PlayerDesignationEntitiesEvent e)
        {
            if (Ingame.Net.IsServer)
                Ingame.CurrentMap.Town.DesignationManager.AddEntities(e.Designation, e.Entities, e.IsRemoval);
            Send(Ingame.Net, e.Entities, e.Designation, e.IsRemoval);
        }
        static public void Send(NetEndpoint endpoint, IReadOnlyCollection<Entity> entities, DesignationDef def, bool isRemoval)
        {
            endpoint.BeginPacketImmediate(_pPlayerDesignationEntities)
                .Write(isRemoval)
                .Write(def)
                .Write(entities.Select(e => e.RefId).ToList());
        }
        private static void ReceiveEntities(NetEndpoint endpoint, Packet packet)
        {
            var map = endpoint.Map;
            var r = packet.PacketReader;
            var isRemoval = r.ReadBoolean();
            var def = r.ReadDef<DesignationDef>();
            var entities = map.World.GetEntities(r.ReadListEntityRefId()).ToArray();
            map.Town.DesignationManager.Edit(def, entities, isRemoval);
            if (endpoint.IsServer)
                Send(endpoint, entities, def, isRemoval);
        }
        private static void OnPlayerDesignation(PlayerDesignationCellsEvent e)
        {
            if (Ingame.Net.IsServer)
                Ingame.CurrentMap.Town.DesignationManager.Edit(e.Designation, e.Begin, e.End, e.IsRemoval);
            Send(Ingame.Net, e.Begin, e.End, e.Designation, e.IsRemoval);
        }
        static public void Send(NetEndpoint endpoint, IntVec3 begin, IntVec3 end, DesignationDef def, bool isRemoval)
        {
            endpoint.BeginPacketImmediate(_pPlayerDesignation)
                .Write(isRemoval)
                .Write(def)
                .Write(begin)
                .Write(end);
        }
        private static void ReceiveCells(NetEndpoint endpoint, Packet packet)
        {
            var map = endpoint.Map;
            var r = packet.PacketReader;
            var isRemoval = r.ReadBoolean();
            var def = r.ReadDef<DesignationDef>();
            var begin = r.ReadIntVec3();
            var end = r.ReadIntVec3();
            map.Town.DesignationManager.Edit(def, begin, end, isRemoval);
            if (endpoint.IsServer)
                Send(endpoint, begin, end, def, isRemoval);
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
        //static public void Receive(NetEndpoint net, Packet pck)
        //{
        //    var r = pck.PacketReader;
        //    var isRemoval = r.ReadBoolean();
        //    var selectionType = (SelectionType)r.ReadInt32();
        //    IEnumerable<TargetArgs> targetList;
        //    DesignationDef designation;
        //    if (selectionType == SelectionType.Box)
        //    {
        //        var begin = r.ReadIntVec3();
        //        var end = r.ReadIntVec3();
        //        var positions = new BoundingBox(begin, end).ToListIntVec3();
        //        designation = isRemoval ? null : r.ReadDef<DesignationDef>();
        //        if (net is Server)
        //            Send(net, isRemoval, begin, end, designation);
        //        targetList = positions.Select(p => new TargetArgs(net.Map, p));
        //    }
        //    else if (selectionType == SelectionType.List)
        //    {
        //        targetList = r.ReadListTargets(net);
        //        foreach (var t in targetList)
        //            t.Map = net.Map;
        //        designation = isRemoval ? null : r.ReadDef<DesignationDef>();
        //        if (net is Server)
        //            Send(net, isRemoval, targetList, designation);
        //    }
        //    else
        //        throw new Exception();
        //    net.Map.Town.DesignationManager.Add(designation, targetList, isRemoval);
        //}
    }
}
