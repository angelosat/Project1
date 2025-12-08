using System.IO;
using Start_a_Town_.Net;
using Start_a_Town_.Components.Crafting;
using System.Linq;

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
        internal static void Send(NetEndpoint net, ToolBlockBuild.Args a)
        {
            Send(net, null, a);
        }
        
        static public void Send(NetEndpoint net, ProductMaterialPair item, ToolBlockBuild.Args a)
        {
            var stream = net.BeginPacket(p);
            a.Write(stream);
            if(!a.Removing)
                item.Write(stream);
        }
        static public void Receive(NetEndpoint net, Packet pck)
        {
            var r = pck.PacketReader;
            var args = new ToolBlockBuild.Args(r);
            var product = args.Removing ? null : new ProductMaterialPair(r);
            var positions = args.ToolDef.Worker.GetPositions(args.Begin, args.End).ToList();
            net.Map.Town.ConstructionsManager.Handle(args, product, positions);

            if (net is Server)
                Send(net, product, args);
            return;
        }
    }
}
