namespace Start_a_Town_
{
    class NeedLetDef : Def
    {
        public NeedLetDef(string name
            ):base(name)
        {
        }

        static public void Init()
        {
            Register(NeedLetDefOf.Sleeping);
        }
    }
}
