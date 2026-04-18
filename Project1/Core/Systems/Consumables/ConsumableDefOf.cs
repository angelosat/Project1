using Project1.Core.Assets;
using Project1.Core.Graphics;
using Project1.Core.Systems.Consumables.Scrolls;
using Project1.Framework;

namespace Project1.Core.Systems.Consumables;

[EnsureStaticCtorCall]
public static class ConsumableDefOf
{
    public static ConsumableDef Pie = new("Pie", "Bake", Sprite.Default, typeof(ConsumableEffect_Food));
    public static ConsumableDef TownScroll = new("TownScroll", "Scribe", ItemContent.PageWritten, typeof(ConsumableEffect_TownScroll));
    public static ConsumableDef Potion = new("Potion", "Brew", ItemContent.Potion, typeof(ConsumableEffect_TownScroll));
    static ConsumableDefOf()
    {
        Def.Register(typeof(ConsumableDefOf));
    }
}
