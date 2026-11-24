using Project1.Core.Systems.Effects;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Project1.Core.Systems.Magic;

[Flags]
public enum SpellSubject { Self = 0x1, Other = 0x2, Any = Self | Other }

public sealed class SpellDef(string name,
                             TargetType targetType,
                             SpellSubject subject,
                             SpellSchoolDef school,
                             int manaCost,
                             int castTimeInSecs,
                             EntityEffectSpec[] effects) 
    : Def(name)
{
    public readonly SpellSchoolDef School = school;

    public readonly int CastTimeSeconds = castTimeInSecs;

    public readonly int ManaCost = manaCost;

    public readonly OrderedDictionary<EntityEffectKey, int> Effects =
        new(effects.Select(e =>
            KeyValuePair.Create(
                new EntityEffectKey(e.Def, e.Target),
                e.Magnitude)));

    public readonly TargetType TargetType = targetType;

    public readonly SpellSubject Subject = subject;
}
