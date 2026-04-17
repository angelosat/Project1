using Project1.Core.Effects;
using Project1.Core.Legacy.Crafting.Defs;
using Project1.Core.Skills;
using Project1.Core.Towns.Duties;
using Project1.Framework;
using System.Collections.Generic;

#nullable enable

namespace Project1.Core.Systems.Materials;

public sealed class MaterialTypeDef : Def, IInspectable
{
    public ReactionClass ReactionClass;
    public readonly MaterialChemistryDef Chemistry;
    public HashSet<MaterialDef> SubTypes = [];
    public float Shininess;
    public DutyDef JobToExtract;
    public SkillDef SkillToRefine;
    public SkillDef GatheringSkill;
    public MaterialProcessGraphDef ProductionGraph = MaterialProcessGraphDefOf.Default;
    public EffectDef? AlchemyEffect;

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