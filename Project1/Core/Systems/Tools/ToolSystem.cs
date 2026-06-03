using Project1.Core.Animations;
using Project1.Core.Entities;
using Project1.Core.Entities.Actors;
using Project1.Core.Entities.Stats;
using Project1.Core.Legacy.Crafting;
using Project1.Core.Loot;
using Project1.Core.Resources;
using Project1.Core.Stats;
using Project1.Core.Systems.Crafting;
using Project1.Core.Systems.Materials;
using Project1.Framework;
using Project1.Framework.Helpers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Project1.Core.Systems.Tools;

[EnsureStaticCtorCall]
internal static class ToolSystem
{
    public static Dictionary<BoneDef, CraftingRules> Rules = [];
    static HashSet<ToolProfileDef> _allDefs;
    static ToolSystem()
    {
        CreateRuleFor(BoneDefOf.ToolHandle)
            .Allow(MaterialRefinementDefOf.Planks, MaterialRefinementDefOf.Ingots);
        CreateRuleFor(BoneDefOf.ToolHead)
            .Allow(MaterialRefinementDefOf.Ingots, MaterialRefinementDefOf.Planks, MaterialRefinementDefOf.Chunk);

        _allDefs = [.. Def.Get<ToolProfileDef>()];
    }
    public static CraftingRules CreateRuleFor(BoneDef bone)
    {
        var rules = new CraftingRules(bone);
        Rules.Add(bone, rules);
        return rules;
    }
    public static CraftingRules GetRuleFor(BoneDef bone)
    {
        return Rules[bone];
    }
    public static IEnumerable<CraftingRules> GetRules() => Rules.Values;
    static public Entity Create(ToolProfileDef profile, MaterialDef handleMaterial, MaterialDef headMaterial)
    {
        var item = ItemDefOf.Tool.Create();
        item.Profile = profile;
        item.ToolComponent.ToolDef = profile;

        var handle = item.Body.FindBone(BoneDefOf.ToolHandle);
        handle.Sprite = profile.SpriteHandle;
        handle.Material = headMaterial;

        var head = item.Body.FindBone(BoneDefOf.ToolHead);
        head.Sprite = profile.SpriteHead;
        head.Material = handleMaterial;

        item.Name = profile.LabelReadable;

        BakeStats(item);
        item.Initialize();
        return item;
    }
    
    static internal void BakeStats(Entity tool)
    {
        var comp = tool.GetComponent<StatsComp>();
        comp.Bake(StatDefOf.ToolSpeed, BoneDefOf.ToolHandle);
        comp.Bake(StatDefOf.ToolEffectiveness, BoneDefOf.ToolHead);
    }

    internal static Entity Create(EntityCreationRequest req)
    {
        return Create(req.Context as ToolProfileDef, req.MaterialBindings[BoneDefOf.ToolHandle], req.MaterialBindings[BoneDefOf.ToolHead]);
    }

    internal static Entity CreateUnfinishedItem(Actor author, CraftingOrder order, MaterialDef handleMaterial, MaterialDef headMaterial)
    {
        var item = ItemDefOf.UnfinishedItem.Create();
        var profile = order.ProductDef;
        item.Profile = profile;
        var comp = item.GetComponent<UnfinishedItemComp>();
        comp.Initialize(author, order, [handleMaterial ,headMaterial]);

        // temp debugging
        var assembly = item.Resources.ViewOld(ResourceDefOf.Assembly);
        assembly.Value = 0;
        assembly.Max = 110;

        item.Initialize();
        item.SetName($"{profile.LabelReadable} (unfinished)");
        order.UnfinishedItem = item;
        return item;
    }
    internal static void CancelUnfinished(Entity entity)
    {
        if (entity is null) // cancellation has been requested after crafting completion
            return;
        var comp = entity.GetComponent<UnfinishedItemComp>();
        var ingredients = new List<Entity>();
        foreach (var (bone, mat) in comp.MaterialBindings)
        {
            var refinement = GetCorrectRefinementForBoneMaterial(bone, mat);
            var ingredient = MaterialSystem.Create(refinement, mat, 1);
            ingredients.Add(ingredient);
        }
        entity.Map.Events.Post(new LootDropEvent([.. ingredients], entity.Map, entity.Global, entity.Velocity));
        entity.World.DisposeEntity(entity);
    }
    static MaterialRefinementDef GetCorrectRefinementForBoneMaterial(BoneDef bone, MaterialDef material)
    {
        var rule = GetRuleFor(bone);
        var refTypes = rule.Profiles.Cast<MaterialRefinementDef>();
        foreach (var t in refTypes)
            if (t.MaterialType == material.Type)
                return t;
        throw new UnreachableException();
    }

    internal static Entity CreateRandom(Random rand, Tier tier)
    {
        var mats = MaterialSystem.ByTier(tier);

        return Create(_allDefs.SelectRandom(rand), mats.SelectRandom(rand), mats.SelectRandom(rand));
    }
}
