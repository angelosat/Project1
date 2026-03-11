using Microsoft.Xna.Framework;
using Project1.Core.Blocks;
using Project1.Core.Construction.Tools;
using Project1.Core.Helpers;
using Project1.Core.Input;
using Project1.Core.Networking;
using Project1.Core.Screens;
using Project1.Core.Serialization;
using Project1.Core.Systems.Materials;
using Project1.Core.Towns.Constructions;
using Project1.Framework;
using System;
using System.Collections.Generic;

namespace Project1.Core.Construction.Packets
{
    [EnsureStaticCtorCall]
    static class PacketDesignateConstruction
    {
        static readonly PacketId _pDesignate, _pRemoveExplicitly;
        static PacketDesignateConstruction()
        {
            _pDesignate = Registry.PacketHandlers.Register(Receive);
            _pRemoveExplicitly = Registry.PacketHandlers.Register(ReceiveRemoveExplicitly);
            Registry.PlayerInputEventHooks.Register<PlayerCancelledConstructionEvent>(OnPlayerCancelledConstruction);

        }

        private static void ReceiveRemoveExplicitly(NetEndpoint endpoint, Packet packet)
        {
            var r = packet.PacketReader;
            var map = endpoint.Map;
            var selectionType = (SelectionType)r.ReadInt32();
            List<IntVec3> cells;
            switch (selectionType)
            {
                case SelectionType.List:
                    cells = r.ReadListIntVec3();
                    break;

                case SelectionType.Box:
                    var a = r.ReadIntVec3();
                    var b = r.ReadIntVec3();
                    cells = new BoundingBox(a, b).ToListIntVec3();
                    break;

                default:
                    throw new InvalidOperationException();
            }
            if (map.Town.ConstructionsManager.RemoveNew(cells) && endpoint is Server server)
                SendRemoveExplicitly(server, cells);
        }

        private static void OnPlayerCancelledConstruction(PlayerCancelledConstructionEvent e)
        {
            if(Ingame.Net.IsServer)
                Ingame.CurrentMap.Town.ConstructionsManager.RemoveNew(e.Targets);
            SendRemoveExplicitly(Client.Instance, e.Targets);
        }
        static void SendRemoveExplicitly(NetEndpoint net, List<IntVec3> targets)
        {
            net.BeginPacketImmediate(_pRemoveExplicitly)
                .Write((int)SelectionType.List)
                .Write(targets);
        }
        static void SendRemoveExplicitly(NetEndpoint net, IntVec3 begin, IntVec3 end)
        {

        }
        internal static void SendRemove(NetEndpoint net, ToolBlockBuild.Args a)
        {
            Send(net, a, default);
        }
        static public void Send(NetEndpoint net, ToolBlockBuild.Args a, ConstructionDesignationArgs args)
        {
            var w = net.BeginPacketImmediate(_pDesignate);
            a.Write(w);
            if (!a.Removing)
            {
                w.Write(args.BlockDef);
                w.Write(args.Refinement);
                w.Write(args.Material);
                w.Write(args.Orientation);
            }
        }
        static public void Receive(NetEndpoint net, Packet pck)
        {
            var r = pck.PacketReader;
            var args = new ToolBlockBuild.Args(r);
            BlockDef block = null;
            MaterialRefinementDef refinement = null;
            MaterialDef material = null;
            byte orientation;
            if (!args.Removing)
            {
                block = r.ReadDef<BlockDef>();
                refinement = r.ReadDef<MaterialRefinementDef>();
                material = r.ReadDef<MaterialDef>();
                orientation = r.ReadByte();
            }

            var constructionArgs = new ConstructionDesignationArgs(block, refinement, material, (byte)args.Orientation);
            var cells = args.ToolDef.Worker.GetPositions(args.Begin, args.End);
            net.Map.Town.ConstructionsManager.Designate(cells, constructionArgs, args.Removing);

            if (net is Server)
                Send(net, args, constructionArgs);
            return;
        }
    }
  
}
