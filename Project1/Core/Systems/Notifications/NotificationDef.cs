using Project1.Framework;
using Project1.Framework.Helpers;
using System;

namespace Project1.Core.Systems.Notifications
{
    public sealed class NotificationDef(string name, Type workerType) : Def(name)
    {
        public readonly NotificationWorker Worker = ActivatorSafe<NotificationWorker>.CreateInstance(workerType);
    }

    [EnsureStaticCtorCall]
    static public class NotificationDefOf
    {
        public static readonly NotificationDef NoClerkAssigned = new("NoClerkAssigned", typeof(NotificationNoWorkerAssigned));
        static NotificationDefOf()
        {
            Def.Register(typeof(NotificationDefOf));
        }
    }
}
