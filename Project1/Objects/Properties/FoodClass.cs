namespace Start_a_Town_
{
    public class FoodClass
    {
        public readonly string Name;

        public FoodClass(string name)
        {
            Name = name;
        }

        static public readonly FoodClass Fruit = new("Fruit");
        static public readonly FoodClass Dish = new("Dish");
    }
}
