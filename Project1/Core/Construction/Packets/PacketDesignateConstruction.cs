using Project1.Core.Construction.Tools;
using Project1.Core.Base;
using Project1.Core.Blocks;
using Project1.Core.Helpers;
using Project1.Core.Materials;
using Project1.Core.Net;
using Project1.Core.Net;

namespace Project1.Core.Construction.Packets
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
            Send(net, a, default);
        }
        
        static public void Send(NetEndpoint net, ToolBlockBuild.Args a, ConstructionDesignationArgs args)
        {
            var w = net.BeginPacketImmediate(p);
            a.Write(w);
            if (!a.Removing)
            {
                w.Write(args.Block);
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
            net.Map.Town.ConstructionsManager.Designate(args, constructionArgs);

            if (net is Server)
                Send(net, args, constructionArgs);
            return;
        }
    }
  
}
