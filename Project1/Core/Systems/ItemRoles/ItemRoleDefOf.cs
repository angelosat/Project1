using System.Linq;
using Project1.Framework;

namespace Project1.Core.Systems.ItemRoles
{
    [EnsureStaticCtorCall]
    public class ItemRoleDefOf
    {
        public static readonly ItemRoleDef Nutrition = new(ItemRoleContextDefOf.Nutrition, null);
        static ItemRoleDefOf()
        {
            var contexts = Def.GetDefs<ItemRoleContextDef>();
            ///solidify list because we modify it within the iteration
            foreach (var contextDef in contexts.ToList())
            {
                var specifics = Def.Database.Values.Where(t => t.GetType() == contextDef.Context).ToList();
                if(specifics.Count == 0)
                    Def.Register(new ItemRoleDef(contextDef, null));
                else
                foreach (var specific in specifics)
                {
                    if (specific is null)
                        continue;
                    var itemroledef = new ItemRoleDef(contextDef, specific);
                    Def.Register(itemroledef);
                }
            }
        }
        //static ItemRoleDefOf()
        //{
        //    var contexts = Def.GetDefs<ItemRoleContextDef>();
        //    ///solidify list because we modify it within the iteration
        //    foreach (var contextDef in contexts.ToList())
        //    {
        //        var specifics = Def.Database.Values.Where(t => t.GetType() == contextDef.Context);
        //        foreach (var specific in specifics.ToList()) 
        //        {
        //            if (specific is null)
        //                continue;
        //            var itemroledef = new ItemRoleDef(contextDef, specific);
        //            Def.Register(itemroledef);
        //        }
        //    }
        //}
    }
}
