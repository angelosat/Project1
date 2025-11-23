using System.Collections.Generic;

namespace Start_a_Town_
{
    public sealed class MaterialTypeDef : Def, IInspectable
    {
        public ReactionClass ReactionClass;
        public readonly MaterialCategory Category;
        public HashSet<MaterialDef> SubTypes = new();
        public float Shininess;
        public JobDef SkillToExtract;

        public MaterialTypeDef(string name, MaterialCategory category)
            : base(name)
        {
            this.Category = category;
        }

        public void AddMaterial(MaterialDef mat)
        {
            mat.Type = this;
            this.SubTypes.Add(mat);
        }
    }
}