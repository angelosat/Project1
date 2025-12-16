using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Start_a_Town_
{
    public class EntityCreationRequest(Def template, Def stage, MaterialDef defaultMaterial = null)
    {
        public readonly Def Template = template;
        public readonly Def Stage = stage;
        public MaterialDef DefaultMaterial = defaultMaterial;
        public readonly Dictionary<BoneDef, MaterialDef> MaterialBindings = [];

        public EntityCreationRequest Override (BoneDef bone, MaterialDef material)
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
