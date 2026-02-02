using Project1.Framework.Skills;
using System.Collections.Generic;

namespace Start_a_Town_
{
    public sealed class MaterialTypeDef : Def, IInspectable
    {
        public ReactionClass ReactionClass;
        //public readonly MaterialCategory Category;
        public readonly MaterialChemistryDef Chemistry;
        public HashSet<MaterialDef> SubTypes = new();
        public float Shininess;
        public JobDef JobToExtract;
        public SkillDef SkillToRefine;
        public MaterialProcessGraphDef ProductionGraph = MaterialProcessGraphDefOf.Default;

        public MaterialTypeDef(string name, MaterialChemistryDef chemistry)
            : base(name)
        {
            this.Chemistry = chemistry;
        }

        public void AddMaterial(MaterialDef mat)
        {
            mat.Type = this;
            this.SubTypes.Add(mat);
        }
    }
}