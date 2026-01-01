using System.Collections.Generic;
namespace Start_a_Town_
{
    public class EntityCreationRequest(Def context, Def stage, MaterialDef defaultMaterial = null, int stackSize = -1)
    {
        public readonly Def Context = context;
        public readonly Def Stage = stage;
        public MaterialDef DefaultMaterial = defaultMaterial;
        public readonly Dictionary<BoneDef, MaterialDef> MaterialBindings = [];
        public readonly int StackSize = stackSize;

        public EntityCreationRequest Override(BoneDef bone, MaterialDef material)
        {
            this.MaterialBindings.Add(bone, material);
            return this;
        }
        public EntityCreationRequest SetDefaultMaterial(MaterialDef material)
        {
            this.DefaultMaterial = material;
            return this;
        }
        //public EntityCreationRequest SetStage(Def stage)
        //{
        //    this.Stage = stage;
        //    return this;
        //}

        public Entity Create()
        {
            return EntityFactory.Create(this);
        }
    }
}
