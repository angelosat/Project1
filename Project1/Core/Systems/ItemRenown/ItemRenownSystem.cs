using Project1.Core.Entities;
using Project1.Core.Gear;
using Project1.Core.Systems.Stats;
using Project1.Core.Systems.Tools;
using Project1.Core.UI;
using Project1.Framework;
using Project1.Framework.UI;
using System.Collections.Generic;
using System.Linq;

namespace Project1.Core.Systems.ItemRenown;

[EnsureStaticCtorCall]
internal static class ItemRenownSystem
{
    static readonly HashSet<GearSlotDef> RenownEligibleSlots;

    static ItemRenownSystem()
    {
        TooltipManager.RegisterProvider(GetTooltipControls);

        RenownEligibleSlots = [.. 
            Def.Get<GearSlotDef>().Where(slot => slot.AttackTargetWeight > 0), 
            GearSlotDefOf.Mainhand];
    }

    static IEnumerable<Control> GetTooltipControls(ITooltippable item)
    {
        if (item is not Entity typed)
            yield break;
        if (!IsRenownEligible(typed))
            yield break;
        var stat = typed.TrackingStats.GetOrCreateStat(TrackingStatDefOf.TotalDamage);
        yield return new LabelNew(() =>
            $"Renown xp: {stat.Value}"
            )
                .InvalidateOn(stat);
    }

    static bool IsRenownEligible(Entity item)
    {
        if (item.Profile is not GearProfileDef profile)
            return false;
        if (!RenownEligibleSlots.Contains(profile.Role.Slot))
            return false;
        return true;
    }
}
