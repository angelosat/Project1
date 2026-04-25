using Project1.Core.Animations;
using Project1.Core.Blocks;
using Project1.Core.Entities;
using Project1.Core.Entities.Actors;
using Project1.Core.Resources;
using Project1.Core.Skills;
using Project1.Core.Systems.Materials;
using Project1.Framework.Helpers;
using System.Collections.Generic;
using System.Linq;

namespace Project1.Core.Systems.Crafting;

public abstract class WorkstationCapabilityWorker
{
    public abstract WorkstationCapabilityDef CapabilityDef { get; }
    public abstract bool CreatesUnfinished { get; }
    public abstract SkillDef CraftingSkill { get; }
    public virtual (ResourceDef resource, int value) ResourceConsumption { get; }

    public abstract IEnumerable<AddOrderRequest> GetAddOrderRequests(BlockWorkstationComp comp);
    public abstract IEnumerable<CraftingRule> GetCraftingRulesStruct(Def recipe);
    public abstract IEnumerable<BoneDef> GetBoneLayout();

    public Dictionary<BoneDef, Entity> GetIngredientMapping(Def recipe, IEnumerable<Entity> ingredients)
        => this.GetBoneLayout().Zip(ingredients).ToDictionary();
    public Dictionary<BoneDef, MaterialDef> MapBonesToMaterials(Def recipe, IEnumerable<MaterialDef> materials)
        => this.GetBoneLayout().Zip(materials).ToDictionary();

    internal virtual int GetOutputStackSize(Def recipe) => 1;

    internal virtual void PostProcess(Entity product, Actor author, AddOrderRequest parameters) { }

    internal virtual Entity CreateProduct(Actor actor, CraftingOrder order, IEnumerable<Entity> ingredients, QualityDef quality)
    {
        var creationReq = order.GetCreationRequest();
        creationReq.Quality = quality;
        var mapping = order.WorkstationCapability.Worker.GetIngredientMapping(order.ProductDef, ingredients);
        foreach (var (bone, item) in mapping)
        {
            creationReq.OverrideMaterial(bone, item.Body.Material);
            actor.Map.World.DisposeEntity(item);
        }
        var product = creationReq.Create();
        this.PostProcess(product, actor, order.Source);
        return product;
    }
    internal virtual Entity CreateProduct(Actor actor, CraftingOrder order, IEnumerable<Entity> ingredients)
    {
        var quality = order.ProductDef is not null ? CraftingManager.GetCrafingQuality(actor, order) : null;
        return this.CreateProduct(actor, order, ingredients, quality);
    }
}
