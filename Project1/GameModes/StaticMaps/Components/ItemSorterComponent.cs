using System;
using System.Collections.Generic;

namespace Start_a_Town_
{
    internal class ItemSorterComponent : MapComponent
    {
        static List<ItemRoleDef> _flatItemRolesListLazy;
        static List<ItemRoleDef> _flatItemRolesList => _flatItemRolesListLazy ??= GenerateItemRolesAll();
        static Dictionary<ItemRoleContextDef, List<ItemRoleDef>> _contextToItemRolesMap = [];

        static List<ItemRoleDef> GenerateItemRolesAll()
        {
            var flat = new List<ItemRoleDef>();
            foreach (var rDef in Def.GetDefs<ItemRoleDef>())
            {
                if (!_contextToItemRolesMap.TryGetValue(rDef.Context, out var list))
                    _contextToItemRolesMap[rDef.Context] = list = [];
                list.Add(rDef);
                flat.Add(rDef);
            }
            return flat;
        }

        Dictionary<ItemRoleDef, List<Entity>> _byRole = [];
        Queue<Entity> _notScannedYet;

        public override void Tick()
        {
            this.ScanOne();
        }

        void ScanOne()
        {
            if (this._notScannedYet.Count == 0)
                return;
            var current = this._notScannedYet.Dequeue();

        }

    }
}
