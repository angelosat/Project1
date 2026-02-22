using Project1.Core.Blocks;
using Project1.Core.Helpers;
using Project1.Core.Materials;
using Project1.Framework;
using Project1.Framework.Serialization;

namespace Project1.Core.Networking.Simulation
{
    public record struct SetBlockArgs(IntVec3 Global, Block Block, MaterialDef Material, byte Data, int Orientation, IntVec3 Source) : ISerializableNew<SetBlockArgs>
    {
        public static SetBlockArgs Create(IDataReader r)
        {
            return new SetBlockArgs().Read(r);
        }
        public SetBlockArgs Read(IDataReader r)
        {
            this.Global = r.ReadIntVec3();
            this.Block = r.ReadDef<BlockDef>().Block;
            this.Material = r.ReadDef<MaterialDef>();
            this.Data = r.ReadByte();
            this.Orientation = r.ReadInt32();
            this.Source = r.ReadIntVec3();
            return this;
        }

        public void Write(IDataWriter w)
        {
            w
               .Write(this.Global)
               .Write(this.Block.BlockDef)
               .Write(this.Material)
               .Write(this.Data)
               .Write(this.Orientation)
               .Write(this.Source);
        }

    }
}