using Project1.Core.Blocks.Comps;
using Project1.Core.Rooms;
using Project1.Core.Systems.Crafting;
using Project1.Core.Systems.Materials;
using Project1.Framework;
using Project1.Framework.Helpers;
using System;
using System.Linq;

namespace Project1.Core.Blocks
{
    public class BlockDef : Def
    {
        public readonly Type BlockType;
        public readonly Type[] BlockEntityComps;
        public readonly Block Block;
        public Def Profile;
        public Def BreakProduct;
        public ConstructionProfile ConstructionProfile;
        public BlockComp.Spec[] BlockEntityCompSpecs;
        public BlockCompDef[] Comps;
        internal MaterialDef DefaultMaterial;
        public FurnitureDef? Furniture;

        public T GetSpec<T>() where T: BlockComp.Spec
        {
            return this.BlockEntityCompSpecs.OfType<T>().SingleOrDefault();
        }

        public T GetProfile<T>() where T : Def => (T)this.Profile;
        
      
        public BlockDef(string name, Type blockType, Type[] entityComps = null) : base(name)
        {
            this.Block = ActivatorSafe<Block>.CreateInstance(blockType);
            this.Block.BlockDef = this;
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
