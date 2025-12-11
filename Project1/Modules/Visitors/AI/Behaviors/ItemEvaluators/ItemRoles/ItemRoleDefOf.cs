using System.Linq;

namespace Start_a_Town_
{
    [EnsureStaticCtorCall]
    internal class ItemRoleDefOf
    {
        static ItemRoleDefOf()
        {
            ///solidify list because we modify it within the iteration
            foreach (var contextDef in Def.GetDefs<ItemRoleContextDef>().ToList())
            {
                foreach (var specific in Def.Database.Values.Where(t => t.GetType() == contextDef.Context).ToList()) 
                {
                    var itemroledef = new ItemRoleDef(contextDef, specific);
                    Def.Register(itemroledef);
                }
            }
        }
    }
}
