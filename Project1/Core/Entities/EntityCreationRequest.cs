using System.Collections.Generic;
using Project1.Core.Animations;
using Project1.Core.Systems.Materials;

namespace Project1.Core.Entities;

public class EntityCreationRequest(Def context, Def stage, MaterialDef defaultMaterial = null, int stackSize = -1)
{
    public readonly Def Context = context;
    public readonly Def Stage = stage;
    public MaterialDef DefaultMaterial = defaultMaterial;
    public readonly Dictionary<BoneDef, MaterialDef> MaterialBindings = [];
    public readonly int StackSize = stackSize;

    public EntityCreationRequest OverrideMaterial(BoneDef bone, MaterialDef material)
    {
        this.MaterialBindings.Add(bone, material);
        return this;
    }
    public EntityCreationRequest SetDefaultMaterial(MaterialDef material)
    {
        this.DefaultMaterial = material;
        return this;
    }

    public Entity Create()
    {
        return EntityFactory.Create(this);
    }
}
