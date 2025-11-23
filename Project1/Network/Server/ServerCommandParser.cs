using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace Start_a_Town_.Net
{
    public partial class Server
    {
        class ServerCommandParser
        {
            readonly Server Server;
            public ServerCommandParser(Server server)
            {
                this.Server = server;
            }
                public void Command(string command)
            {
                var queue = new Queue<string>(command.Split(' '));
                try
                {
                    switch (queue.Dequeue())
                    {
                        case "hello":
                            Server.ConsoleBox.Write("SERVER", "how are you?");
                            break;

                        case "loadworld":
                            string worldName = queue.Dequeue();
                            break;

                        case "unloadworld":
                            UnloadWorld();
                            break;

                        case "broadcast":
                            string message = queue.Dequeue();
                            byte[] data = Network.Serialize(w => w.WriteASCII(message));
                            foreach (var p in Instance.Players.GetList())
                                Instance.Enqueue(p, Packet.Create(p, PacketType.ServerBroadcast, data));

                            Server.ConsoleBox.Write(Color.Orange, "SERVER", message);
                            break;

                        case "kick":
                            int plid;
                            if (int.TryParse(queue.Peek(), out plid))
                                KickPlayer(plid);
                            break;

                        case "acks":
                        case "ack":
                            if (!Instance.ConsoleBox.Filters.Remove(UI.ConsoleMessageTypes.Acks))
                            {
                                Server.ConsoleBox.Write("SERVER", "ACK reporting on");
                                Instance.ConsoleBox.Filters.Add(UI.ConsoleMessageTypes.Acks);
                            }
                            else
                                Server.ConsoleBox.Write("SERVER", "ACK reporting off");
                            break;

                        case "savechunk":
                            try
                            {
                                int x = int.Parse(queue.Dequeue());
                                int y = int.Parse(queue.Dequeue());
                                var pos = new Vector2(x, y);
                                if (!Instance.Map.GetActiveChunks().TryGetValue(pos, out Chunk chunk))
                                {
                                    Server.ConsoleBox.Write("SERVER", "Chunk " + pos.ToString() + " doesn't exist");
                                    break;
                                }
                                Server.ConsoleBox.Write("SERVER", "Saving chunk " + pos.ToString());
                                chunk.SaveToFile();
                            }
                            catch (Exception) { Server.ConsoleBox.Write("SERVER", "Syntax error in: " + command); }
                            break;

                        case "savechunks":

                            Server.ConsoleBox.Write("SERVER", "Saving all active chunks");
                            foreach (var ch in Instance.Map.GetActiveChunks().Values)
                                ch.SaveToFile();
                            break;

                        case "savethumb":
                            Instance.Map.GenerateThumbnails();
                            break;

                        default:
                            Server.ConsoleBox.Write("SERVER", "Unknown command " + command);
                            break;
                    }
                }
                catch (Exception) { Server.ConsoleBox.Write("SERVER", "Syntax error in: " + command); }
            }
        }
    }
}
