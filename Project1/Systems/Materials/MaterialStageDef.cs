namespace Start_a_Town_
{
    public class MaterialStageDef(string name, ItemDef itemDef) : Def(name)
    {
        public ItemDef Item = itemDef;
    }
}
