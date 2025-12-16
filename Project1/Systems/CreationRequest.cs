using System.Collections.Generic;

namespace Start_a_Town_
{
    public class CreationRequest(Def template, Def stage = null, MaterialDef defaultMaterial = null)
    {
        public readonly Def Template = template, Stage = stage;
        public MaterialDef DefaultMaterial = defaultMaterial;
        public readonly Dictionary<BoneDef, MaterialDef> MaterialBindings = [];

        public CreationRequest Override (BoneDef bone, MaterialDef material)
        {
            this.MaterialBindings.Add(bone, material);
            return this;
        }
        public CreationRequest SetDefaultMaterial(MaterialDef material)
        {
            this.DefaultMaterial = material;
            return this;
        }
    }
}
