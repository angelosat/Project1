namespace Project1.Core.Gear;

public sealed class GearSlotDef(string name) : Def(name)
{
    //public bool CanBeAttackTarget;
    public int AttackTargetWeight;
}
