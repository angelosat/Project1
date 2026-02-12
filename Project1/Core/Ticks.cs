namespace Project1.Core
{
    static class Ticks
    {
        public const int IngameMillisecondsPerTick = 1000;// 1440; // one tick is 1.44 ingame seconds
        public const int PerSecond = 60;
        public const int PerGameSecond = PerSecond * IngameMillisecondsPerTick / 1000;
        public const int PerGameMinute = PerGameSecond;
        public const int PerGameHour = 60 * PerGameMinute;
        public const int PerGameDay = 24 * PerGameHour;

        public static float FromSeconds(float seconds)
        {
            return PerSecond * seconds;
        }
        public static int FromMinutes(int minutes)
        {
            return PerGameMinute * minutes;
        }
        public static int FromHours(int hours)
        {
            return PerGameHour * hours;
        }
        public static int FromDays(int days)
        {
            return PerGameDay * days;
        }
        public static int From( int days, int hours, int minutes)
        {
            return FromDays(days) * FromHours(hours) * FromMinutes(minutes);
        }
    }
}
