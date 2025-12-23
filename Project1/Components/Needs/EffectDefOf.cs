namespace Start_a_Town_
{
    [EnsureStaticCtorCall]
    static class EffectDefOf
    {
        static public EffectDef Sleeping = new EffectDef("Sleeping", new SleepEffectWorker());
        static public EffectDef Adventuring = new EffectDef("Adventuring", new AdventuringEffectWorker());
        static EffectDefOf()
        {
            Def.Register(typeof(EffectDefOf));
        }
    }
}
