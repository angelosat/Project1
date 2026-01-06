namespace Start_a_Town_
{
    [EnsureStaticCtorCall]
    static class EffectDefOf
    {
        static public EffectDef Sleeping = new("Sleeping", new SleepEffectWorker());
        static public EffectDef Adventuring = new("Adventuring", new AdventuringEffectWorker());
        static EffectDefOf()
        {
            Def.Register(typeof(EffectDefOf));
        }
    }
}
