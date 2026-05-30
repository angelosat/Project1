using Project1.Core.Entities;
using Project1.Core.Entities.Actors;
using Project1.Core.Systems.ItemRoles;
using System.Collections.Generic;

namespace Project1.Core.Systems.Equipment;

internal class ItemRole_Equipment : ItemRoleWorker
{
    public override int GetInventoryScore(Actor actor, Entity item, ItemRoleDef context)
    {
        if (!item.TryGetComponent<EquipmentComp>(out var comp))
            return 0;
        var value = comp.Armor;
        return value;
    }

    public override int GetSituationalScore(Actor actor, Entity item, ItemRoleDef context)
    {
        var comp = item.GetComponent<EquipmentComp>();
        var profile = (EquipmentDef)item.Profile;
        var armor = comp.Armor;
        var slot = profile.Slot;
        var currentArmor = EquipmentSystem.GetArmor(actor, slot);
        var diff = armor - currentArmor;
        return diff;
    }

    public override IEnumerable<Def> GetValidTargetDefs()
        => Def.Get<EquipmentDef>();
}
