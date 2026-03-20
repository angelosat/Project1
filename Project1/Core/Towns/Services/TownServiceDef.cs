using Project1.Framework;
using Project1.Framework.Helpers;
using System;

namespace Project1.Core.Towns.Services
{
    [EnsureStaticCtorCall]
    public static class TownServiceDefOf
    {
        static public readonly TownServiceDef Selling = new("Selling", typeof(TownServiceSelling));
        static public readonly TownServiceDef Buying = new("Buying", typeof(TownServiceBuying));
        static public readonly TownServiceDef Repairing = new("Repairing", typeof(TownServiceRepairing));
        static public readonly TownServiceDef Lodging = new("Lodging", typeof(TownServiceLodging));
    }
    public sealed class TownServiceDef(string name, Type runtimeType) : Def(name)
    {
        public readonly Type RuntimeType = runtimeType;
        public TownServiceRuntime CreateRuntime() => ActivatorSafe<TownServiceRuntime>.CreateInstance(this.RuntimeType);
    }

    public abstract class TownServiceRuntime { }

    public sealed class TownServiceSelling : TownServiceRuntime { }
    public sealed class TownServiceBuying : TownServiceRuntime { }
    public sealed class TownServiceRepairing : TownServiceRuntime { }
    public sealed class TownServiceLodging : TownServiceRuntime { }

    static class TownServiceExtensions
    {
        extension(TownServicesComp comp)
        {
            public TownServiceSelling Selling => comp.GetService<TownServiceSelling>();
            public TownServiceBuying Buying => comp.GetService<TownServiceBuying>();
            public TownServiceRepairing Repairing => comp.GetService<TownServiceRepairing>();
            public TownServiceLodging Lodging => comp.GetService<TownServiceLodging>();
        }
    }

}
