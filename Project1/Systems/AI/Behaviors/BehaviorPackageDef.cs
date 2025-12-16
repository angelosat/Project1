namespace Start_a_Town_
{
    public class BehaviorPackageDef(string name, Behavior bhav) : Def(name)
    {
        public Behavior Root = bhav;
    }
}
