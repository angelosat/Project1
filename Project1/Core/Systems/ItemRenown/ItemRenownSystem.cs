using Project1.Core.Entities;
using Project1.Core.Systems.Tools;
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
        if (typed.ToolComponent is not ToolComp comp)
            yield break;
        foreach (var stat in comp.GetTrackingStats())
            yield return new LabelNew(() =>
            $"{stat.Def.LabelReadable}: {stat.Value}"
            )
                .InvalidateOn(stat.Notifier);
    }
}
