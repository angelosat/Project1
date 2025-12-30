using System;

namespace Start_a_Town_
{
    public class BlockDef : Def
    {
        public readonly Type BlockType;
        public readonly Type[] BlockEntityComps;
        public readonly Block Worker;
        public ConstructionProfile ConstructionProfile;
        public BlockEntityComp.Spec[] BlockEntityCompSpecs;

        //public BlockDef()
        //{

        //}
        public BlockDef(string name, Type blockType, Type[] entityComps = null) : base(name)
        {
            this.Worker = ActivatorSafe<Block>.CreateInstance(blockType);
            this.Worker.BlockDef = this;
        }

        public BlockEntity CreateEntity(IntVec3 origin)
        {
            if (this.BlockEntityCompSpecs is null)
                return null;
            var entity = new BlockEntity(origin);
            foreach (var spec in this.BlockEntityCompSpecs)
                entity.AddComp(spec.CreateComp());
            entity.Initialize();
            return entity;
        }
    }
}
