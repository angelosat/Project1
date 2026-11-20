using System.Collections.Generic;

namespace Project1.Core.Gear;

public sealed class GearSlotDef(string name) : Def(name)
{
    //public bool CanBeAttackTarget;
    public int AttackTargetWeight;

    public override IEnumerable<string> ConfigErrors()
    {
        if (this.AttackTargetWeight < 0)
            yield return $"{nameof(this.AttackTargetWeight)} cannot be negative.";
    }
}
