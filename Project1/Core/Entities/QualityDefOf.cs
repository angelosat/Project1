using Microsoft.Xna.Framework;
using Project1.Framework;

namespace Project1.Core.Entities;

[EnsureStaticCtorCall]
static class QualityDefOf
{
    public static readonly QualityDef Trash = new("Trash", 1, Color.Gray, .5f, 5, threshold: 5, -5);
    public static readonly QualityDef Common = new("Common", 2, Color.White, 1f, 50, threshold: 50);
    public static readonly QualityDef Uncommon = new("Uncommon", 3, Color.Lime, 1.2f, 30, threshold: 75, 5);
    public static readonly QualityDef Rare = new("Rare", 4, Color.DodgerBlue, 1.4f, 10, threshold: 90, 10);
    public static readonly QualityDef Epic = new("Epic", 5, Color.BlueViolet, 1.6f, 4, threshold: 95, 15);
    public static readonly QualityDef Legendary = new("Legendary", 6, Color.DarkOrange, 1.8f, 2, threshold: 100, 20);
    public static readonly QualityDef Artifact = new("Artifact", 7, Color.Yellow, 2f, 1, threshold: 105, 25);
    public static readonly QualityDef Unique = new("Unique", 8, Color.Yellow, 2f, 0, threshold: null);
    public static readonly QualityDef Cheating = new("Cheating", -1, Color.LightSkyBlue, 100f, 0, threshold: null);

    static QualityDefOf()
    {
        Def.Register(typeof(QualityDefOf));
    }
}
