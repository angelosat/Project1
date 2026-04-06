using System;
using System.Linq;

namespace Project1.Core.Systems.Presentation
{
    internal static class PresentationSystem
    {
        internal static void Init()
        {
            //foreach (var type in AppDomain.CurrentDomain.GetAssemblies().SelectMany(a => a.GetTypes()))
            //{
            //    if (typeof(IPresentationSystem).IsAssignableFrom(type) && !type.IsAbstract)
            //        ((IPresentationSystem)Activator.CreateInstance(type)).Init();
            //}

            //foreach (var type in AppDomain.CurrentDomain.GetAssemblies().SelectMany(a => a.GetTypes()))
            //{
            //    if (typeof(IPresentationWorker).IsAssignableFrom(type) && !type.IsInterface)
            //        ((IPresentationWorker)Activator.CreateInstance(type)).Register();
            //}
            var types = AppDomain.CurrentDomain.GetAssemblies().SelectMany(a => a.GetTypes()).Where(type => typeof(IPresentationWorker).IsAssignableFrom(type));
            foreach (var type in types)
            {
                if (typeof(IPresentationWorker).IsAssignableFrom(type) && !type.IsInterface)
                    ((IPresentationWorker)Activator.CreateInstance(type)).Register();
            }
        }
    }
}
