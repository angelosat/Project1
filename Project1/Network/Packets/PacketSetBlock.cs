using Start_a_Town_.Net;
using System;

namespace Start_a_Town_
{
    public struct SetBlockArgs(IntVec3 global, Block block, MaterialDef material, byte data, int orientation, IntVec3 source)
    {
        public readonly IntVec3 Global = global;
        public readonly Block Block = block;
        public readonly MaterialDef Material = material;
        public readonly byte Data = data;
        public readonly int Orientation = orientation;
        public readonly IntVec3 Source = source;

        static public SetBlockArgs ReadFrom(IDataReader r)
        {
            var global = r.ReadIntVec3();
            var block = r.ReadDef<BlockDef>().Worker;
            var material = r.ReadDef<MaterialDef>();
            var data = r.ReadByte();
            var orientation = r.ReadInt32();
            var source = r.ReadIntVec3();
            return new(global, block, material, data, orientation, source);
        }
    }
    [EnsureStaticCtorCall]
    internal static class PacketSetBlock
    {
        static readonly int _pType;
        static PacketSetBlock()
        {
            _pType = Registry.PacketHandlers.Register(Receive);
        }
        internal static void Send(Server server, SetBlockArgs args)
        {
            server.BeginPacket(_pType)
                .Write(args.Global)
                .Write(args.Block.BlockDef)
                .Write(args.Material)
                .Write(args.Data)
                .Write(args.Orientation)
                .Write(args.Source);
        }
        private static void Receive(NetEndpoint endpoint, Packet packet)
        {
            var client = endpoint as Client;
            var r = packet.PacketReader;
            var args = SetBlockArgs.ReadFrom(r);
            client.Map.SetBlock(args);
        }
    }
}
