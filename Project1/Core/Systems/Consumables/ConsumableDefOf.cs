using Project1.Core.Graphics;
using Project1.Framework;

namespace Project1.Core.Systems.Consumables
{
    [EnsureStaticCtorCall]
    public static class ConsumableDefOf
    {
        public static ConsumableDef Pie = new("Pie", "Bake", Sprite.Default);
        static ConsumableDefOf()
        {
            Def.Register(typeof(ConsumableDefOf));
        }
    }
}
