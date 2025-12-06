namespace Start_a_Town_
{
    public class NeedLetDef : Def
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
