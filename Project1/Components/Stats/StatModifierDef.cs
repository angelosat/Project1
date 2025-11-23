using System;

namespace Start_a_Town_
{
    public enum DurationTypes { Permanent, Finite }
    class StatModifierDef : Def
    {
        public StatDef Source;
        public Func<GameObject, float, float> Mod;
        public DurationTypes DurationType;
        public int BaseDurationInTicks = 0;
        public StatModifierDef(string name, StatDef source) : base(name)
        {
         
            this.Source = source;
        }

        static public readonly StatModifierDef WalkSpeedHaulingWeight = new StatModifierDef("WalkSpeedHaulingWeight", StatDefOf.WalkSpeed)
        {
            DurationType = DurationTypes.Permanent,
            Mod = (a,v)=>
            {
                var encumberance = StatDefOf.Encumberance.GetValue(a);
                var carryingw = a.Hauled?.TotalWeight ?? 0;
                if (carryingw == 0)
                    return v;
                return .5f + .5f * (1 - encumberance);
            }
        };
    }
}
