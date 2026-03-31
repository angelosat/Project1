using Project1.Framework;

namespace Project1.Core.Towns.Reputation;

[EnsureStaticCtorCall]
public static class ReputationSourceDefOf
{
    public static readonly ReputationSourceDef Customer = new("Customer", typeof(ReputationSourceCustomer));
    static ReputationSourceDefOf()
    {
        Def.Register(typeof(ReputationSourceDefOf));
    }
}
