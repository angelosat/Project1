using System;
using System.Linq;
using Start_a_Town_.Net;

namespace Start_a_Town_
{
    class CommandParser
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
                        var old = Block.GetBlock(p[1]);
                        var replace = Block.GetBlock(p[2]);
                        if (replace == BlockDefOf.Air || old == BlockDefOf.Air)
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
                        var toremove = Block.GetBlock(p[1]);
                        if (toremove == BlockDefOf.Air)
                            break;
                        foreach (var ch in net.Map.GetActiveChunks())
                            foreach (var cell in ch.Value.Cells)
                                if (cell.Block == toremove)
                                    net.Map.RemoveBlock(cell.LocalCoords.ToGlobal(ch.Value));
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
