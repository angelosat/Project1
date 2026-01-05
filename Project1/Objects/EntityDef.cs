using System;

namespace Start_a_Town_
{
    public class EntityDef : Def
    {
        public readonly Type ItemClass;
        public string Description;
        public float Height = 1;
        public float Weight = 1;
        public bool Haulable = true;

        public EntityDef(string name, Type itemClass) : base(name)
        {
            this.ItemClass = itemClass;
        }
    }
}
