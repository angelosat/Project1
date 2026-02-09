using Project1.Core.Base;
using Project1.Core.Blocks;
using Project1.Core.Helpers;
using Project1.Core.Materials;
using Project1.Core.Net;
using Project1.Core.Simulation;
using Project1.Framework;

namespace Project1.Core.Networking.Simulation
{
    [EnsureStaticCtorCall]
    static class PacketPlayerSetBlock
    {
        static readonly int p;
        static PacketPlayerSetBlock()
        {
            p = Registry.PacketHandlers.Register(Receive);
            Registry.PlayerInputEventHooks.Register<PlayerPaintedBlockEvent>(HandlePlayerPaintedBlock);
        }

        private static void HandlePlayerPaintedBlock(PlayerPaintedBlockEvent e)
        {
            Send(Client.Instance, Client.Instance.GetPlayer(), e.Global, e.Block, e.Material, e.State, e.Variation, e.Orientation);
        }

        public static void Send(NetEndpoint net, PlayerData player, IntVec3 global, Block block, MaterialDef material, byte data = 0, int variation = 0, int orientation = 0)
        {
            net.BeginPacketImmediate(p)
               .Write(player.ID)
               .Write(global)
               .Write(block.BlockDef)
               .Write(material)
               .Write(data)
               .Write(variation)
               .Write(orientation);
        }
        private static void Receive(NetEndpoint net, Packet pck)
        {
            var r = pck.PacketReader;
            var player = net.GetPlayer(r.ReadInt32());
            var global = r.ReadIntVec3();
            var block = r.ReadDef<BlockDef>().Worker;
            var material = Def.GetDef<MaterialDef>(r);
            var data = r.ReadByte();
            var variation = r.ReadInt32();
            var orientation = r.ReadInt32();
            
            Perform(net.Map, global, block, material, data, variation, orientation);
        }

        private static void Perform(MapBase map, IntVec3 global, Block block, MaterialDef material, byte data, int variation, int orientation)
        {
            if (!map.IsInBounds(global))
                return;
            MapEdit.Paint(MapEditContext.Player, map, [global], block, material, data, variation, orientation);
        }
    }
}
