namespace Project1.Core.Entities.Stats;

public class DamageDef(string name, DamageCategoryDef damageType) : Def(name)
{
    public readonly DamageCategoryDef DamageType = damageType;
}
