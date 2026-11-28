using System;
using System.Collections.Generic;

namespace Project1.Core;

static class Ticks
{
    public const int IngameMillisecondsPerTick = 1000;// 1440; // one tick is 1.44 ingame seconds

    public const int PerSecond = 60;
    //public const int PerGameSecond = PerSecond * IngameMillisecondsPerTick / 1000;
    //public const int PerGameMinute = PerGameSecond;

    public const int PerGameSecond = IngameMillisecondsPerTick / 1000;

    public const int PerGameMinute = 60 * PerGameSecond;

    public const int PerGameHour = 60 * PerGameMinute;

    public const int PerGameDay = 24 * PerGameHour;

    public static float FromSeconds(float seconds)
        => PerSecond * seconds;
    
    public static int FromMinutes(int minutes)
        => PerGameMinute * minutes;
    
    public static int FromHours(int hours)
        => PerGameHour * hours;
    
    public static int FromDays(int days)
        => PerGameDay * days;
    
    public static int From( int days, int hours, int minutes)
        => FromDays(days) + FromHours(hours) + FromMinutes(minutes);


    public static TimeSpan ToTimespan(int ingameTicks)
        => TimeSpan.FromSeconds(PerGameSecond * ingameTicks);

    public static string ToString(int ingameTicks)
    {
        var span = ToTimespan(ingameTicks);
        return span.Format();
        //List<string> final = [];

        //if (span.Days > 0) final.Add($"{span.Days} d");
        //if (span.Hours > 0) final.Add($"{span.Hours} h");
        //if (span.Minutes > 0) final.Add($"{span.Minutes} m");
        //if (span.Seconds > 0) final.Add($"{span.Seconds} s");

        //return string.Join(" ", final);
    }

    extension(TimeSpan span)
    {
        public string Format()
        {
            List<string> final = [];

            if (span.Days > 0) final.Add($"{span.Days} d");
            if (span.Hours > 0) final.Add($"{span.Hours} h");
            if (span.Minutes > 0) final.Add($"{span.Minutes} m");
            if (span.Seconds > 0) final.Add($"{span.Seconds} s");

            return string.Join(" ", final);
        }
    }
}
