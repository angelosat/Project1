namespace Start_a_Town_
{
    public record struct SetBlockArgs(IntVec3 Global, Block Block, MaterialDef Material, byte Data, int Orientation, IntVec3 Source) : ISerializableNew<SetBlockArgs>
    {
        public static SetBlockArgs Create(IDataReader r)
        {
            return new SetBlockArgs().Read(r);
        }

        //static public SetBlockArgs ReadFrom(IDataReader r)
        //{
        //    var global = r.ReadIntVec3();
        //    var block = r.ReadDef<BlockDef>().Worker;
        //    var material = r.ReadDef<MaterialDef>();
        //    var data = r.ReadByte();
        //    var orientation = r.ReadInt32();
        //    var source = r.ReadIntVec3();
        //    return new(global, block, material, data, orientation, source);
        //}

        public SetBlockArgs Read(IDataReader r)
        {
            this.Global = r.ReadIntVec3();
            this.Block = r.ReadDef<BlockDef>().Worker;
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
