namespace Start_a_Town_
{
    public record struct SetBlockArgs(IntVec3 Global, Block Block, MaterialDef Material, byte Data, int Orientation, IntVec3 Source)
    {
        static public SetBlockArgs ReadFrom(IDataReader r)
        {
            var global = r.ReadIntVec3();
            var block = r.ReadDef<BlockDef>().Worker;
            var material = r.ReadDef<MaterialDef>();
            var data = r.ReadByte();
            var orientation = r.ReadInt32();
            var source = r.ReadIntVec3();
            return new(global, block, material, data, orientation, source);
        }
        public readonly SetBlockArgs WriteTo(IDataWriter w)
        {
            w
               .Write(this.Global)
               .Write(this.Block.BlockDef)
               .Write(this.Material)
               .Write(this.Data)
               .Write(this.Orientation)
               .Write(this.Source);
            return this;
        }
    }
}
