namespace Project1.Core.Systems.Effects;

internal static class EffectsUtils
{
    internal static string GetString(EffectDef effect, Def target, float multiplier = 1)
    {
        var basestring = $"{effect.Verb} {(int)(effect.BaseMagnitude * multiplier)} {target?.LabelReadable}";
        if (effect.BaseDuration > 0)
            //basestring += $" for {TimeSpan.FromMinutes(Ticks.PerGameMinute * effect.BaseDuration)}";
            basestring += $" for {Ticks.ToString(effect.BaseDuration)}";
        return basestring;
    }
    internal static string GetStringNew(EffectDef effect, Def target, int magnitude)
    {
        var basestring = $"{effect.Verb}";// {magnitude} {target?.LabelReadable}";
        //List<string> parts = [effect.Verb];
        if (magnitude != 0)
            basestring += $" {magnitude}";
        if (target is not null)
            basestring += $" {target.LabelReadable}";
        if (effect.BaseDuration > 0)
            //basestring += $" for {TimeSpan.FromMinutes(Ticks.PerGameMinute * effect.BaseDuration)}";
            basestring += $" for {Ticks.ToString(effect.BaseDuration)}";
        return basestring;
    }
}
