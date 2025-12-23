namespace Start_a_Town_
{
    public static class DefHelpers
    {
        public static void SaveDef<T>(this SaveTag tag, string name, T def) where T : Def
        {
            tag.Add(def.Save(name));
        }
    }
}
