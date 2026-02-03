using Project1.Framework.Base;

namespace Start_a_Town_
{
    public static class DefHelpers
    {
        public static void SaveDef<T>(this SaveTag tag, string name, T def) where T : Def
        {
            tag.Add(def.Save(name));
        }
        //public static void Save(this SaveTag tag, string name, Def def)
        //{
        //    tag.Add(def.Save(name));
        //}
    }
}
