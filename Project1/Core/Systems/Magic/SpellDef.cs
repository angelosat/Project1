using Project1.Core.Systems.Effects;
using Project1.Framework.Helpers;
using System;

namespace Project1.Core.Systems.Magic;

[Flags]
public enum SpellSubject { Self = 0x1, Other = 0x2, Any = Self | Other }
public sealed class SpellDef(string name, TargetType targetType, SpellSubject subject, SpellSchoolDef school, /*Type workerType, */int manaCost, int castTimeInSecs, /*int effectDuration,*/ (EffectDef, Def)[] effects) : Def(name)
{
    public readonly SpellSchoolDef School = school;
    public readonly int CastTimeSeconds = castTimeInSecs;
    //public readonly int EffectDuration = effectDuration;
    public readonly int ManaCost = manaCost;
    //[Obsolete]
    //public readonly SpellWorker Worker = ActivatorSafe<SpellWorker>.CreateInstance(workerType);
    public readonly (EffectDef effect, Def target)[] Effects = effects;
    public readonly TargetType TargetType = targetType;
    public readonly SpellSubject Subject = subject;
}
