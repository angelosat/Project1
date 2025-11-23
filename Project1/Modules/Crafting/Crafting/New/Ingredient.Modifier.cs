using System;
using System.Collections.Generic;

namespace Start_a_Town_
{
    public partial class Ingredient
    {
        class Modifier
        {
            public string Label;
            Predicate<ItemDef> Condition;

            public Modifier(string label, Predicate<ItemDef> condition)
            {
                Label = label;
                Condition = condition;
            }
            public bool Evaluate(ItemDef def)
            {
                return this.Condition(def);
            }
        }

    }
}
