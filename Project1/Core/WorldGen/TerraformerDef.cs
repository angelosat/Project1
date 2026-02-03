using Project1.Framework.Base;
using System;

namespace Project1.Core.WorldGen
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
