using Project1.Core.Entities;

namespace Project1.Core.Components.Crafting
{
    partial class BlockRecipe
    {
        partial class Product
        {
            public abstract class Modifier
            {
                public string LocalMaterialName { get; set; }
                public Modifier(string localMaterialName)
                {
                    this.LocalMaterialName = localMaterialName;
                }
                public abstract void Apply(GameObject reagent, ref byte data);
            }
        }
    }
}
