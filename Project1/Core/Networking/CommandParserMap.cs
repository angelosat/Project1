using System;
using System.Linq;
using Project1.Core.Blocks;

namespace Project1.Core.Networking
{
    class CommandParserMap
    {
        public void Execute(INetEndpoint net, string command)
        {
            try
            {
                var p = command.Split(' ');
                var type = p[0];
                switch (type)
                {
                    case "set":
                        switch (p[1])
                        {
                            case "time":
                            case "hour":
                                int t = int.Parse(p[2]);
                                if (net is Server)
                                    (net as Server).Enqueue(PacketType.PlayerServerCommand, Network.Serialize(w => w.WriteASCII(command)), ReliabilityType.OrderedReliable);
                                break;

                            default:
                                break;
                        }
                        break;

                    case "replace":
                        var old = Def.GetDef<BlockDef>(p[1]).Block;
                        var replace = Def.GetDef<BlockDef>(p[2]).Block;
                        if (replace == BlockDefOf.Air.Block || old == BlockDefOf.Air.Block)
                            break;
                        foreach (var ch in net.Map.GetActiveChunks())
                            foreach (var cell in ch.Value.Cells)
                                if (cell.Block == old)
                                {
                                    cell.Block = replace;
                                    var rest = p.Skip(3);
                                    string data = "";
                                    foreach(var s in rest)
                                    {
                                        data += s + " ";
                                    }
                                    data = data.TrimEnd(' ');
                                    if (p.Length > 3)
                                        cell.BlockData = replace.ParseData(data);
                                }
                        if (net is Server)
                            (net as Server).Enqueue(PacketType.PlayerServerCommand, Network.Serialize(w => w.WriteASCII(command)), ReliabilityType.OrderedReliable);
                        break;

                    case "remove":
                        var toremove = Def.GetDef<BlockDef>(p[1]).Block;
                        if (toremove == BlockDefOf.Air.Block)
                            break;
                        foreach (var ch in net.Map.GetActiveChunks())
                            foreach (var cell in ch.Value.Cells)
                                if (cell.Block == toremove)
                                    net.Map.RemoveBlock(cell.LocalCoords);
                        if (net is Server)
                            (net as Server).Enqueue(PacketType.PlayerServerCommand, Network.Serialize(w => w.WriteASCII(command)), ReliabilityType.OrderedReliable);
                        break;

                    default:
                        break;
                }
            }
            catch (Exception) { net.ConsoleBox.Write("Invalid command"); }
        }
    }
}
