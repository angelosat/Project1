using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Project1.Core.Networking;
using Project1.Core.Simulation;
using Project1.Core.UI;

namespace Project1.Core.Networking
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
                        this.Server.ConsoleBox.Write("SERVER", "how are you?");
                        break;

                    case "loadworld":
                        string worldName = queue.Dequeue();
                        break;

                    case "unloadworld":
                        this.Server.UnloadWorld();
                        break;

                    case "broadcast":
                        string message = queue.Dequeue();
                        byte[] data = Network.Serialize(w => w.WriteASCII(message));
                        foreach (var p in this.Server.Players.GetList())
                            this.Server.Enqueue(p, Packet.Create(p, PacketType.ServerBroadcast, data));

                        this.Server.ConsoleBox.Write(Color.Orange, "SERVER", message);
                        break;

                    case "kick":
                        int plid;
                        if (int.TryParse(queue.Peek(), out plid))
                            this.Server.KickPlayer(plid);
                        break;

                    case "acks":
                    case "ack":
                        if (!this.Server.ConsoleBox.Filters.Remove(ConsoleMessageTypes.Acks))
                        {
                            this.Server.ConsoleBox.Write("SERVER", "ACK reporting on");
                            this.Server.ConsoleBox.Filters.Add(ConsoleMessageTypes.Acks);
                        }
                        else
                            this.Server.ConsoleBox.Write("SERVER", "ACK reporting off");
                        break;

                    case "savechunk":
                        try
                        {
                            int x = int.Parse(queue.Dequeue());
                            int y = int.Parse(queue.Dequeue());
                            var pos = new Vector2(x, y);
                            if (!this.Server.Map.GetActiveChunks().TryGetValue(pos, out Chunk chunk))
                            {
                                this.Server.ConsoleBox.Write("SERVER", "Chunk " + pos.ToString() + " doesn't exist");
                                break;
                            }
                            this.Server.ConsoleBox.Write("SERVER", "Saving chunk " + pos.ToString());
                            chunk.SaveToFile();
                        }
                        catch (Exception) { this.Server.ConsoleBox.Write("SERVER", "Syntax error in: " + command); }
                        break;

                    case "savechunks":

                        this.Server.ConsoleBox.Write("SERVER", "Saving all active chunks");
                        foreach (var ch in this.Server.Map.GetActiveChunks().Values)
                            ch.SaveToFile();
                        break;

                    case "savethumb":
                        this.Server.Map.GenerateThumbnails();
                        break;

                    default:
                        this.Server.ConsoleBox.Write("SERVER", "Unknown command " + command);
                        break;
                }
            }
            catch (Exception) { this.Server.ConsoleBox.Write("SERVER", "Syntax error in: " + command); }
        }
    }
}