namespace Start_a_Town_
{
    public class MaterialFormDef(string name, ItemDef itemDef) : Def(name)
    {
        public ItemDef Item = itemDef;
    }
}
