using Project1.Core.Blocks;
using Project1.Core.Helpers;
using Project1.Core.Materials;
using Project1.Core.Networking;
using Project1.Framework;
using System;

namespace Project1.Core.Simulation
{
    [EnsureStaticCtorCall]
    static class PacketsCells
    {
        readonly static PacketId _pCellEdit = Registry.PacketHandlers.Register(ReceiveCellEdit);

        static PacketsCells()
        {
            Registry.MapEventHooksServer.Register<CellEditEvent>(HandleCellMutated);
        }

        private static void HandleCellMutated(CellEditEvent e)
        {
            SendCellEdit(e.Edit);
        }
        private static void SendCellEdit(CellQuery cell)
        {
            var net = cell.Map.Net;
            net.BeginPacket(_pCellEdit)
                .Write(cell.Map.ID)
                .Write(cell.GetGlobal())
                .Write(cell.Block.BlockDef)
                .Write(cell.Material)
                .Write(cell.Data);
        }
        private static void ReceiveCellEdit(NetEndpoint endpoint, Packet packet)
        {
            if (endpoint.IsServer)
                throw new Exception();
            var r = packet.PacketReader;
            var mapid = r.ReadInt32();
            var global = r.ReadIntVec3();
            var block = r.ReadDef<BlockDef>();
            var material = r.ReadDef<MaterialDef>();
            var data = r.ReadInt32();
            var edit = new CellQuery(endpoint.Map, global);
            edit.Block = block.Block;
            edit.Material = material;
            edit.Data = data;
        }
    }
}
