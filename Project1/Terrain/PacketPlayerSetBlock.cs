using Start_a_Town_.Net;

namespace Start_a_Town_
{
    [EnsureStaticCtorCall]
    static class PacketPlayerSetBlock
    {
        static readonly int p;
        static PacketPlayerSetBlock()
        {
            p = Registry.PacketHandlers.Register(Receive);
        }
        public static void Send(NetEndpoint net, PlayerData player, IntVec3 global, Block block, MaterialDef material, byte data = 0, int variation = 0, int orientation = 0)
        {
            //if (net is Server)
            //    Perform(net.Map, global, block, material, data, variation, orientation);

            //var w = net.BeginPacketOld(p);
            var w = net.BeginPacket(p);
            w.Write(player.ID);
            w.Write(global);
            w.Write(block.BlockDef);
            material.Write(w);
            w.Write(data);
            w.Write(variation);
            w.Write(orientation);
        }
        private static void Receive(NetEndpoint net, Packet pck)
        {
            var r = pck.PacketReader;
            var player = net.GetPlayer(r.ReadInt32());
            var global = r.ReadIntVec3();
            //var block = Block.GetBlock(r);
            var block = r.ReadDef<BlockDef>().Worker;//.GetBlock(r);
            var material = Def.GetDef<MaterialDef>(r);
            var data = r.ReadByte();
            var variation = r.ReadInt32();
            var orientation = r.ReadInt32();

            //if (net is Server)
            //    Send(net, player, global, block, material, data, variation, orientation);
            
            Perform(net.Map, global, block, material, data, variation, orientation);
        }

        private static void Perform(MapBase map, IntVec3 global, Block block, MaterialDef material, byte data, int variation, int orientation)
        {
            if (!map.IsInBounds(global))
                return;
            // DONT CALL PREVIOUS BLOCK'S REMOVE METHOD
            // when in block editing mode, we don't want to call block's remove method, so for example they don't pop out their contents or have any other effects to the world
            // HOWEVER we want to dispose their contents (gameobjects) if any! 
            // so 1) query their contents and dispose them here? 
            //    2) call something like dispose() on them and let them dispose them themselves?
            // TODO: DECIDE!


            map.RemoveBlock(global);
            //if (block != BlockDefOf.Air)
            if (block != BlockDefOf.Air.Worker)
                //Block.Place(block, map, global, material, data, variation, orientation);
                map.SetBlock(global, block, material, data, variation, orientation);

        }
    }
}
