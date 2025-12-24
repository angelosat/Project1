using System;

namespace Start_a_Town_
{
    public static class BlockEntityFactory
    {
        [Obsolete($"use {nameof(block.CreateEntity)}")]
        public static BlockEntity Create(Block block, IntVec3 originGlobal)
        {
            return block.CreateEntity(originGlobal);
            if (block.BlockEntityCompSpecs is null)
                return null;
            var entity = new BlockEntity(originGlobal);
            //foreach (var comp in block.BlockEntityComps)
            //    entity.AddComp((BlockEntityComp)Activator.CreateInstance(comp));
            foreach (var spec in block.BlockEntityCompSpecs)
                entity.AddComp(spec.CreateComp());
            entity.Initialize();
            return entity;
        }
    }
}
