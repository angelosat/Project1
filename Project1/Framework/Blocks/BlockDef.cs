using Start_a_Town_;
using System;
using System.Linq;

namespace Project1.Framework.Blocks
{
    public class BlockDef : Def
    {
        public readonly Type BlockType;
        public readonly Type[] BlockEntityComps;
        public readonly Block Worker;
        public Def Profile;
        public ConstructionProfile ConstructionProfile;
        public BlockEntityComp.Spec[] BlockEntityCompSpecs;
        internal MaterialDef DefaultMaterial;

        public T GetSpec<T>() where T: BlockEntityComp.Spec
        {
            return this.BlockEntityCompSpecs.OfType<T>().SingleOrDefault();
        }

        public T GetProfile<T>() where T : Def => (T)this.Profile;
        
      
        public BlockDef(string name, Type blockType, Type[] entityComps = null) : base(name)
        {
            this.Worker = ActivatorSafe<Block>.CreateInstance(blockType);
            this.Worker.BlockDef = this;
        }

        public BlockEntity CreateEntity(IntVec3 origin)
        {
            if (this.BlockEntityCompSpecs is null)
                return null;
            var entity = new BlockEntity(this, origin);
            foreach (var spec in this.BlockEntityCompSpecs)
                entity.AddComp(spec.CreateComp());
            entity.Initialize();
            return entity;
        }
        public BlockEntity CreateEntity()
        {
            if (this.BlockEntityCompSpecs is null)
                return null;
            var entity = new BlockEntity(this);
            foreach (var spec in this.BlockEntityCompSpecs)
                entity.AddComp(spec.CreateComp());
            entity.Initialize();
            return entity;
        }

    }
}
