using Project1.Core.Entities.Actors;
using Project1.Core.Resources;
using Project1.Core.Systems.Biology;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Project1.Core.Systems.CombatOverworld;

internal sealed class CombatRuntime(int id, Actor first, Actor second)
{
    internal readonly int Id = id;

    internal readonly Actor First = first, Second = second;

    readonly List<AttackResult> Log = [];

    readonly static int BaseCooldown = Ticks.FromMinutes(10);

    int CooldownFirst = BaseCooldown, CooldownSecond = BaseCooldown;

    internal bool IsResolved { get; private set; }

    internal void Tick()
    {
        if (this.IsResolved)
            throw new InvalidOperationException("Ticked a resolved combat encounter");

        if (CooldownFirst-- <= 0)
        {
            CooldownFirst = BaseCooldown;
            PerformAttack(First, Second);
        }

        if (CooldownSecond-- <= 0)
        {
            CooldownSecond = BaseCooldown;
            PerformAttack(Second, First);
        }
    }

    private void PerformAttack(Actor attacker, Actor defender)
    {
        var result = CombatSystem.ResolveHit(attacker, defender);
        switch (result.Type)
        {
            case AttackResultTypes.Miss:
            case AttackResultTypes.Dodge:
            case AttackResultTypes.Parry:
                break;
            case AttackResultTypes.Hit:
                var damage = result.Damage;
                defender.Resources.ApplyDelta(ResourceDefOf.Health, -damage.Value);
                if(defender.Biology.IsIncapacitated)
                    this.IsResolved = true;
                break;
            default:
                throw new UnreachableException();
        }
        Log.Add(result);
    }
}
