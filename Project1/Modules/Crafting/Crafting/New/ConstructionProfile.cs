namespace Start_a_Town_
{
    public class ConstructionProfile(MaterialRefinementDef[] refinements) : Inspectable
    {
        public readonly MaterialRefinementDef[] Refinements = refinements;
        public int Dimension = 1;
    }
}
