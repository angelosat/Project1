namespace Start_a_Town_
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

        public override string Label { get; } = typeof(BuildProperties).Name;
    }
}
