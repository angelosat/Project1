namespace Start_a_Town_
{
    public class MaterialCategory
    {
        public readonly string Label;

        public MaterialCategory(string label)
        {
            this.Label = label;
        }
        public override string ToString()
        {
            return this.Label;
        }
        static public readonly MaterialCategory Creature = new("Creature");
        static public readonly MaterialCategory Plant = new("Plant");
        static public readonly MaterialCategory Inorganic = new("Inorganic");
    }
}
