using Microsoft.Xna.Framework;
using Project1.Core.Base;

namespace Project1.Core.Entities
{
    static class QualityDefOf
    {
        public static readonly QualityDef Trash = new("Trash", Color.Gray, .5f, 5, -5);
        public static readonly QualityDef Common = new("Common", Color.White, 1f, 50);
        public static readonly QualityDef Uncommon = new("Uncommon", Color.Lime, 1.2f, 30, 5);
        public static readonly QualityDef Rare = new("Rare", Color.DodgerBlue, 1.4f, 10, 10);
        public static readonly QualityDef Epic = new("Epic", Color.BlueViolet, 1.6f, 4, 15);
        public static readonly QualityDef Legendary = new("Legendary", Color.DarkOrange, 1.8f, 2, 20);
        public static readonly QualityDef Artifact = new("Artifact", Color.Yellow, 2f, 1, 25);
        public static readonly QualityDef Unique = new("Unique", Color.Yellow, 2f, 0);
        public static readonly QualityDef Cheating = new("Cheating", Color.LightSkyBlue, 100f, 0);

        static QualityDefOf()
        {
            Def.Register(typeof(QualityDefOf));
        }
    }
}
