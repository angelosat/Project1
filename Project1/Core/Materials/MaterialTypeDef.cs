using Project1.Core.Towns;
using Project1.Core.Legacy.Crafting.Defs;
using Project1.Core.Skills;
using System.Collections.Generic;
using Project1.Framework;

namespace Project1.Core.Materials
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