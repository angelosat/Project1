using Project1.Core.Towns.Healing;
using Project1.Core.Towns.Inns;
using Project1.Core.Towns.Shops;
using Project1.Framework;
using Project1.Framework.Helpers;
using System;

namespace Project1.Core.Towns.Services
{
    [EnsureStaticCtorCall]
    public static class TownServiceDefOf
    {
        //static public readonly TownServiceDef Selling = new("Selling", typeof(TownServiceSelling));
        //static public readonly TownServiceDef Buying = new("Buying", typeof(TownServiceBuying));
        //static public readonly TownServiceDef Repairing = new("Repairing", typeof(TownServiceRepairing));
        //static public readonly TownServiceDef Lodging = new("Lodging", typeof(TownServiceLodging));
        //static public readonly TownServiceDef Healing = new("Healing", typeof(TownServiceHealing)); //, typeof(SpellRequest)

        static public readonly TownServiceDef Buying = new("Buying", typeof(ShopTransaction));
        static public readonly TownServiceDef Lodging = new("Lodging", typeof(InnTransaction));
        static public readonly TownServiceDef Healing = new("Healing", typeof(SpellRequest));
    }
    public sealed class TownServiceDef(string name, Type runtimeType) : Def(name)
    {
        public readonly Type RuntimeType = runtimeType;
        public readonly TownServiceWorker Worker;

        public T CreateRuntime<T>() where T : TownServiceRequest => ActivatorSafe<T>.CreateInstance(this.RuntimeType);

        //public TownServiceRuntime CreateRuntime() => ActivatorSafe<TownServiceRuntime>.CreateInstance(this.RuntimeType);

    }

    public abstract class TownServiceWorker
    {
        public abstract TownServiceRequest CreateRuntime();
    }

    //public sealed class TownServiceWorker_Selling : TownServiceWorker { }
    //public sealed class TownServiceWorker_Repairing : TownServiceWorker { }
    public sealed class TownServiceWorker_Lodging : TownServiceWorker
    {
        public override InnTransaction CreateRuntime() => new();
    }
    public sealed class TownServiceWorker_Healing : TownServiceWorker
    {
        public override SpellRequest CreateRuntime() => new();

    }
    public sealed class TownServiceWorker_Buying : TownServiceWorker
    {
        public override ShopTransaction CreateRuntime() => new();
    }

    //static class TownServiceExtensions
    //{
    //    extension(TownServicesComp comp)
    //    {
    //        //public TownServiceWorker_Selling Selling => comp.GetService<TownServiceWorker_Selling>();
    //        public TownServiceWorker_Buying Buying => comp.GetService<TownServiceWorker_Buying>();
    //        //public TownServiceWorker_Repairing Repairing => comp.GetService<TownServiceWorker_Repairing>();
    //        public TownServiceWorker_Lodging Lodging => comp.GetService<TownServiceWorker_Lodging>();
    //    }
    //}

}
