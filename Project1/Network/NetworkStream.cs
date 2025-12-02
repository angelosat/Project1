using System;
using System.IO;

namespace Start_a_Town_.Net
{

    //public partial class Client
    //{
    //Dictionary<ReliabilityType, BinaryWriter> Streams = new() { { ReliabilityType.Unreliable, new BinaryWriter(new MemoryStream()) }, { ReliabilityType.Reliable, new BinaryWriter(new MemoryStream()) }, { ReliabilityType.OrderedReliable, new BinaryWriter(new MemoryStream()) } };
    //private void SendOutgoingStreams()
    //{
    //    foreach (var i in this.Streams)
    //        if (i.Value.BaseStream.Position > 0)
    //        {
    //            var data = ((MemoryStream)i.Value.BaseStream).ToArray();
    //            if (data.Length > 0)
    //                this.Send(PacketType.MergedPackets, data, i.Key);
    //        }
    //}
    public class NetworkStream
    {
        public readonly ReliabilityType Reliability;
        readonly MemoryStream Memory = new();
        public readonly BinaryWriter Writer;
        public NetworkStream(ReliabilityType reliability)
        {
            this.Reliability = reliability;
            this.Writer = new BinaryWriter(this.Memory);
        }
        public void Reset() => this.Writer.BaseStream.SetLength(0);
        public ArraySegment<byte> GetBuffer()
        {
            return new ArraySegment<byte>(this.Memory.GetBuffer(), 0, (int)this.Memory.Position);
        }
        public byte[] GetBytes(MemoryStream append)
        {
            this.Memory.Write(append.GetBuffer(), 0, (int)append.Length);
            append.SetLength(0);
            return this.Memory.ToArray();
        }
        public byte[] GetBytes()
        {
            return this.Memory.ToArray();

            // Return a new array of the exact length needed
            int length = (int)this.Memory.Position;
            byte[] data = new byte[length];
            Array.Copy(this.Memory.GetBuffer(), data, length);
            return data;
        }
    }
}