using Microsoft.Xna.Framework;

namespace Start_a_Town_
{
    static class QualityDefOf
    {
        public static readonly Quality Trash = new("Trash", Color.Gray, .5f, 5, -5);
        public static readonly Quality Common = new("Common", Color.White, 1f, 50);
        public static readonly Quality Uncommon = new("Uncommon", Color.Lime, 1.2f, 30, 5);
        public static readonly Quality Rare = new("Rare", Color.DodgerBlue, 1.4f, 10, 10);
        public static readonly Quality Epic = new("Epic", Color.BlueViolet, 1.6f, 4, 15);
        public static readonly Quality Legendary = new("Legendary", Color.DarkOrange, 1.8f, 1, 20);
        public static readonly Quality Unique = new("Unique", Color.Yellow, 2f, 0);
        public static readonly Quality Cheating = new("Cheating", Color.LightSkyBlue, 100f, 0);

        static QualityDefOf()
        {
            Def.Register(typeof(QualityDefOf));
        }
    }
}
