using System.Linq;
using Project1.Framework;
using Project1.Core.Blocks;
using Project1.Core.Helpers;
using Project1.Core.Networking;
using Project1.Core.Simulation;
using Project1.Core.Systems.Materials;

namespace Project1.Core.Networking.Simulation
{
    [EnsureStaticCtorCall]
    internal static class PacketsMapEdit
    {
        static readonly PacketId _pMapEdit;
        static PacketsMapEdit()
        {
            _pMapEdit = Registry.PacketHandlers.Register(OnMapEdit);
            Registry.MapEventHooksServer.Register<MapEditEvent>(HandleMapEditEvent);
        }

        private static void HandleMapEditEvent(MapEditEvent e)
        {
            var w = e.Context == MapEditContext.Player ? 
                Server.Instance.BeginPacketImmediate(_pMapEdit) : 
                Server.Instance.BeginPacket(_pMapEdit);
            w.Write((int)e.Context);
            w.Write((int)e.Type);
            w.Write(e.Map.ID);
            w.Write(e.Targets);
            w.Write(e.Block.BlockDef);
            w.Write(e.Material);
            w.Write(e.Data);
            w.Write(e.Variation);
            w.Write(e.Orientation);
        }

        private static void OnMapEdit(NetEndpoint endpoint, Packet packet)
        {
            var client = endpoint as Client;
            var r = packet.PacketReader;
            var context = (MapEditContext)r.ReadInt32();
            var type = (MapEditType)r.ReadInt32();
            _ = r.ReadInt32();
            var targets = r.ReadListIntVec3().ToHashSet();
            var blockdef = r.ReadDef<BlockDef>();
            var materialdef = r.ReadDef<MaterialDef>();
            var data = r.ReadByte();
            var variation = r.ReadInt32();
            var orientation = r.ReadInt32();
            if (type == MapEditType.Create)
                MapEdit.Paint(context, client.Map, targets, blockdef.Block, materialdef, data, variation, orientation);
            else if (type == MapEditType.Replace)
                MapEdit.PaintWithOrigin(context, client.Map, targets, blockdef.Block, materialdef, data, variation, orientation);
        }
    }
}
