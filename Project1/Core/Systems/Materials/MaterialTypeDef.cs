using Project1.Core.Legacy.Crafting.Defs;
using Project1.Core.Skills;
using Project1.Core.Towns.Duties;
using Project1.Framework;
using System.Collections.Generic;

namespace Project1.Core.Systems.Materials
{
    public sealed class MaterialTypeDef : Def, IInspectable
    {
        public ReactionClass ReactionClass;
        public readonly MaterialChemistryDef Chemistry;
        public HashSet<MaterialDef> SubTypes = [];
        public float Shininess;
        public DutyDef JobToExtract;
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