using Project1.Core.Entities;
using Project1.Core.Helpers.Structs;
using Project1.Core.Simulation;
using Project1.Framework;
using Project1.Framework.Serialization;
using System.Collections.Generic;

namespace Project1.Core.Helpers
{
    internal static class IOHelpers
    {
        public static int ReadEntityRefId(this IDataReader r) => new EntityRefId(r.ReadInt32());
        public static T ReadDef<T>(this IDataReader r) where T : Def => Def.GetDef<T>(r.ReadString());
        public static Def ReadDef(this IDataReader r) => Def.GetDef(r.ReadString());
        public static T TryReadDef<T>(this IDataReader r) where T : Def => r.ReadString() is string defName && !defName.IsNullEmptyOrWhiteSpace() ? Def.GetDef<T>(defName) : null!;
        public static TargetArgs ReadTarget(this IDataReader r, MapBase map) => TargetArgs.Read(map, r);

        public static IDataWriter Write(this IDataWriter w, List<EntityRefId> v)
        {
            w.Write(v.Count);
            foreach (var i in v)
            {
                if (i == 0)
                    throw new System.Exception();
                w.Write(i);
            }
            return w;
        }
        public static IDataWriter Write(this IDataWriter w, Def def) { w.Write(def.Name); return w; }
        public static IDataWriter Write(this IDataWriter w, ICollection<TargetArgs> list)
        {
            w.Write(list.Count);
            foreach (var i in list)
                i.Write(w);
            return w;
        }
        public static IDataWriter Write(this IDataWriter w, EntityComp comp) { comp.Write(w); return w; }
        public static IDataWriter Write(this IDataWriter w, TargetArgs target) { target.Write(w); return w; }


    }
}
