namespace Project1.Core.Entities.Stats;

public class DamageDef(string name, DamageTypeDef damageType) : Def(name)
{
    public readonly DamageTypeDef DamageType = damageType;
}
