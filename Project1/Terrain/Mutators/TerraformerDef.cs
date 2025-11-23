using System;

namespace Start_a_Town_
{
    public class TerraformerDef : Def
    {
        readonly Type TerraformerClass;
        public TerraformerDef(string name, Type terraformerClass) : base(name)
        {
            this.TerraformerClass = terraformerClass;
        }
        public Terraformer Create()
        {
            var instance = (Terraformer)Activator.CreateInstance(this.TerraformerClass);
            instance.Def = this;
            return instance;
        }
    }
}
