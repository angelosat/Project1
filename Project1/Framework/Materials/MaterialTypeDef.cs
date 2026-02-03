using Project1.Core.Materials;
using Project1.Framework.Base;
using Project1.Framework.Skills;
using Start_a_Town_;
using System.Collections.Generic;

namespace Project1.Framework.Materials
{
    public sealed class MaterialTypeDef : Def, IInspectable
    {
        public ReactionClass ReactionClass;
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