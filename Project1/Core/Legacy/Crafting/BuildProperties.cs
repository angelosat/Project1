using Project1.Core.Towns.Constructions.Categories;
using Project1.Framework;

namespace Project1.Core.Legacy.Crafting
{
    public class BuildProperties : Inspectable
    {
        public Ingredient Ingredient;
        public float ToolSensitivity;
        public ConstructionCategoryDef Category;
        public int Complexity = 1;
        public int Dimension = 1;
        public BuildProperties()
        {

        }
        public BuildProperties(Ingredient ingredient, float toolContribution)
        {
            this.Ingredient = ingredient;
            this.ToolSensitivity = toolContribution;
        }

        public override string LabelReadable { get; } = typeof(BuildProperties).Name;
    }
}
