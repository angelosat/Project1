using System.Collections.Generic;
using System.Linq;

namespace Start_a_Town_.Components.Crafting
{
    class IsOfSubType : Reaction.Reagent.ReagentFilter
    {
        readonly List<ItemSubType> ValidTypes;

        public override string Name => "Is of (any) subtype";
       
        public IsOfSubType(params ItemSubType[] types)
        {
            this.ValidTypes = types.ToList(); ;
        }
        public override bool Condition(Entity obj)
        {
            return this.ValidTypes.Contains(obj.GetInfo().ItemSubType);
        }
        public override string ToString()
        {
            string txt = Name + ": ";
            foreach (var type in this.ValidTypes)
                txt += type.Name + ", ";
            return txt.TrimEnd(',');
        }
    }
}
