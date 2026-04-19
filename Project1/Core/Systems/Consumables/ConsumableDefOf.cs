using Project1.Core.Assets;
using Project1.Core.Graphics;
using Project1.Core.Systems.Consumables.Scrolls;
using Project1.Framework;

namespace Project1.Core.Systems.Consumables;

[EnsureStaticCtorCall]
public static class ConsumableDefOf
{
    public static readonly ConsumableDef Pie = new("Pie", "Bake", Sprite.Default, typeof(ConsumableEffect_Food), typeof(ConsumableWorker_Food));
    public static readonly ConsumableDef Scroll = new("Scroll", "Scribe", ItemContent.PageWritten, typeof(ConsumableEffect_TownScroll), typeof(ConsumableWorker_Scroll));
    public static readonly ConsumableDef Potion = new("Potion", "Brew", ItemContent.Potion, typeof(ConsumableEffect_TownScroll), typeof(ConsumableWorker_Potion));
    static ConsumableDefOf()
    {
        Def.Register(typeof(ConsumableDefOf));
    }
}
