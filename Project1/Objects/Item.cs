namespace Start_a_Town_
{
    internal sealed class Item : Entity
    {
        public Item()
        {

        }
        public Item(ItemDef def, int amount) : base(def, amount) { }
   
        //public override GameObject Create()
        //{
        //    return new Item(this.Def);
        //}
    }
}
