using SharpDX.MediaFoundation;
using SharpDX.X3DAudio;
using Start_a_Town_.Components;
using Start_a_Town_.Net;
using System;
using System.Data;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Net.Sockets;

namespace Start_a_Town_
{
    public static class NetworkHelper
    {
        public static void Send(this byte[] data, long packetID, PacketType type, SendType sendType, Socket so, EndPoint ip)
        {
            Packet.Create(packetID, type, sendType, data).BeginSendTo(so, ip);
        }
        public static void Send(this byte[] data, long packetID, PacketType type, Socket so, EndPoint ip)
        {
            Packet.Create(packetID, type, data).BeginSendTo(so, ip);
        }
        public static void Translate(this byte[] data, INetPeer objProvider, Action<BinaryReader> reader)
        {
            using var r = new BinaryReader(new MemoryStream(data));
            reader(r);
        }
        public static void Translate(this byte[] data, Action<BinaryReader> reader)
        {
            using var r = new BinaryReader(new MemoryStream(data));
            reader(r);
        }
        
        public static byte[] GetBytes(this Action<BinaryWriter> writer)
        {
            using var w = new BinaryWriter(new MemoryStream());
            writer(w);
            return (w.BaseStream as MemoryStream).ToArray();
        }
        
        public static byte[] Decompress(this byte[] compressed)
        {
            using var compressedStream = new MemoryStream(compressed);
            using var zipStream = new GZipStream(compressedStream, CompressionMode.Decompress);
            using var output = new MemoryStream();
            zipStream.CopyTo(output);
            return output.ToArray();
        }
        public static byte[] Compress(this byte[] data)
        {
            byte[] compressed;
            using (var output = new MemoryStream())
            {
                using (var input = new MemoryStream(data))
                using (var zip = new GZipStream(output, CompressionMode.Compress))
                    input.CopyTo(zip);
                compressed = output.ToArray();
            }
            return compressed;
        }
       
        public static byte[] ToArray(this Action<BinaryWriter> writerHandler)
        {
            using var mem = new MemoryStream();
            using (var output = new BinaryWriter(mem))
                writerHandler(output);
            return mem.ToArray();
        }



        /// <summary>
        /// Returns the packet that exceeded maximum resend attempts
        /// </summary>
        /// <param name="socket"></param>
        /// <param name="ip"></param>
        /// <param name="player"></param>
        /// <returns></returns>
        public static Packet TryResend(Socket socket, EndPoint ip, PlayerData player)
        {
            var waiting = player.WaitingForAck;
            if (waiting.IsEmpty)
                return null;

            var packet = waiting.Values.First();
            if (packet.RTT.ElapsedMilliseconds < Network.RTT)
                return null;
            if (packet.Retries-- <= 0)
                return packet;
            packet.BeginSendTo(socket, ip);
            return null;
        }
    }
}
