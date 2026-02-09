namespace Project1.Core.Legacy.Crafting.Defs
{
    public class ReactionClass : Def
    {
        public ReactionClass(string name):base(name)
        {

        }
        static public readonly ReactionClass Tools = new("Tools");
        static public readonly ReactionClass Protein = new("Protein");
    }
}
