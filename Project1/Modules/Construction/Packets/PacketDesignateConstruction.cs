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
            p = Network.RegisterPacketHandler(Receive);
        }
        internal static void Send(INetEndpoint net, ToolBlockBuild.Args a)
        {
            Send(net, null, a);
        }
        
        static public void Send(INetEndpoint net, ProductMaterialPair item, ToolBlockBuild.Args a)
        {
            var stream = net.GetOutgoingStream();
            stream.Write(p);
            a.Write(stream);
            if(!a.Removing)
                item.Write(stream);
        }
        static public void Receive(INetEndpoint net, BinaryReader r)
        {
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
