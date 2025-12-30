using Start_a_Town_.Net;
using Start_a_Town_.Components.Crafting;

namespace Start_a_Town_
{
    [EnsureStaticCtorCall]
    static class PacketDesignateConstruction
    {
        static readonly int p;
        static PacketDesignateConstruction()
        {
            p = Registry.PacketHandlers.Register(Receive);
        }
        internal static void SendRemove(NetEndpoint net, ToolBlockBuild.Args a)
        {
            //Send(net, null, a);
            Send(net, a, default);
        }
        
        static public void Send(NetEndpoint net, ProductMaterialPair item, ToolBlockBuild.Args a)
        {
            //var stream = net.BeginPacket(p);
            var server = net as Server;
            var stream = server.BeginPacket(p);
            a.Write(stream);
            //if(!a.Removing)
            //    item.Write(stream);
        }
        static public void Send(NetEndpoint net, ToolBlockBuild.Args a, ConstructionDesignationArgs args)
        {
            //IDataWriter w;
            //if (net is Server server)
            //    w = server.BeginUntimestamped(p);
            //else
            //var w = net.BeginPacket(p);
            //var w = net is Server server ? server.BeginPacketPlayerCommand(p) : net.BeginPacket(p);
            var w = net.BeginPacketImmediate(p);
            a.Write(w);
            if (!a.Removing)
            {
                w.Write(args.Block.BaseID);
                w.Write(args.Refinement);
                w.Write(args.Material);
                w.Write(args.Orientation);
            }
        }
        
        static public void Receive(NetEndpoint net, Packet pck)
        {
            var r = pck.PacketReader;
            var args = new ToolBlockBuild.Args(r);
            Block block = null;
            MaterialRefinementDef refinement = null;
            MaterialDef material = null;
            byte orientation;
            if(!args.Removing)
            {
                block = Block.GetBlock(r.ReadInt32());
                refinement = r.ReadDef<MaterialRefinementDef>();
                material = r.ReadDef<MaterialDef>();
                orientation = r.ReadByte();
            }
     
            var constructionArgs = new ConstructionDesignationArgs(block, refinement, material, (byte)args.Orientation);
            net.Map.Town.ConstructionsManager.Designate(args, constructionArgs);

            if (net is Server)
                Send(net, args, constructionArgs);
            return;
        }

        
    }
    public struct ConstructionDesignationArgs(Block block, MaterialRefinementDef refinement, MaterialDef material, int amount, byte orientation = 0)
    {
        //public List<IntVec3> Positions = positions;
        public Block Block = block;
        public MaterialRefinementDef Refinement = refinement;
        public MaterialDef Material = material;
        public int Amount = amount;
        public byte Orientation = orientation;
        public override string ToString() => $"{this.Material.Label} {this.Refinement.Label} x{this.Amount}";
    }
}
