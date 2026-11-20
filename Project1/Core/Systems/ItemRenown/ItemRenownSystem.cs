using Project1.Core.Entities;
using Project1.Core.Systems.Stats;
using Project1.Core.UI;
using Project1.Framework;
using Project1.Framework.UI;
using System.Collections.Generic;

namespace Project1.Core.Systems.ItemRenown;

[EnsureStaticCtorCall]
internal static class ItemRenownSystem
{
    static ItemRenownSystem()
    {
        TooltipManager.RegisterProvider(GetTooltipControls);
    }

    static IEnumerable<Control> GetTooltipControls(ITooltippable item)
    {
        if (item is not Entity typed)
            yield break;

        var stat = typed.TrackingStats.GetOrCreateStat(TrackingStatDefOf.TotalPreventedDamage);
        yield return new LabelNew(() =>
            $"Renown xp: {stat.Value}"
            )
                .InvalidateOn(stat);
    }
}
