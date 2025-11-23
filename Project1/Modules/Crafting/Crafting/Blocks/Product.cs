using System.Collections.Generic;

namespace Start_a_Town_.Components.Crafting
{
    partial class BlockRecipe
    {
        public partial class Product
        {
            public Block Block;
            public List<Modifier> Modifiers;
            public Product(Block block, params Modifier[] modifiers)
            {
                this.Block = block;
                this.Modifiers = new List<Modifier>(modifiers);
            }
        }
    }
}
