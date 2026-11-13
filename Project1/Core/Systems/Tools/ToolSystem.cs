using Project1.Core.Animations;
using Project1.Core.Entities;
using Project1.Core.Entities.Actors;
using Project1.Core.Entities.Stats;
using Project1.Core.Gear;
using Project1.Core.Legacy.Crafting;
using Project1.Core.Loot;
using Project1.Core.Networking;
using Project1.Core.Resources;
using Project1.Core.Stats;
using Project1.Core.Systems.Crafting;
using Project1.Core.Systems.Gear;
using Project1.Core.Systems.Materials;
using Project1.Core.Systems.Quality;
using Project1.Framework;
using Project1.Framework.Helpers;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Project1.Core.Systems.Tools;

[EnsureStaticCtorCall]
internal static class ToolSystem
{
    static readonly StatBonusDef[] AllBonusDefs;

    static readonly StatDef[] AllStatDefs;

    static readonly StatBonusDef[] AllSuffixDefs, AllPrefixDefs;

    static readonly Random _r = new();

    public static Dictionary<BoneDef, CraftingRules> Rules = [];

    static readonly HashSet<GearProfileDef> _allDefs;

    static readonly Dictionary<GearSlotDef, List<GearProfileDef>> _defsBySlot;

    static ToolSystem()
    {
        CreateRuleFor(BoneDefOf.ToolHandle)
            .Allow(MaterialRefinementDefOf.Planks, MaterialRefinementDefOf.Ingots);
        CreateRuleFor(BoneDefOf.ToolHead)
            .Allow(MaterialRefinementDefOf.Ingots, MaterialRefinementDefOf.Planks, MaterialRefinementDefOf.Chunk);

        _allDefs = [.. Def.Get<GearProfileDef>()];

        _defsBySlot = _allDefs
            .GroupBy(d => d.Role.Slot)
            .ToDictionary(d => d.Key, d => d.ToList());

        AllBonusDefs = [.. Def.Get<StatBonusDef>()];
        AllPrefixDefs = [.. AllBonusDefs.Where(b => b.Worker is StatBonusWorker_Item)];
        AllSuffixDefs = [.. AllBonusDefs.Where(b => b.Worker is StatBonusWorker_Actor)];
        AllStatDefs = [.. Def.Get<StatDef>()];
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

    public static IEnumerable<CraftingRule> GetCraftingRulesStruct(Def recipe)
    {
        foreach (var rule in GetRules())
            yield return new(rule.Bone, ItemDefOf.Ingredient, rule.Profiles, [.. rule.Profiles.Select(mr => ((MaterialRefinementDef)mr).MaterialType)], 1);
    }

    static public Entity CreateToolOrWeapon(GearProfileDef profile, MaterialDef handleMaterial, MaterialDef headMaterial)
    {
        var item = ItemDefOf.Gear.Create();
        item.Profile = profile;
        item.ToolComponent.ToolDef = profile;

        var handle = item.Body.FindBone(BoneDefOf.ToolHandle);
        handle.Sprite = profile.SpriteHandle;
        handle.Material = handleMaterial;

        var head = item.Body.FindBone(BoneDefOf.ToolHead);
        head.Sprite = profile.SpriteHead;
        head.Material = headMaterial;

        item.Name = profile.LabelReadable;

        BakeStats(item);
        item.Initialize();
        return item;
    }

    static public Entity CreateArmor(GearProfileDef profile, MaterialDef material)
    {
        var item = ItemDefOf.Gear.Create();
        item.Profile = profile;

        item.Body.SetMaterial(material);

        item.Name = profile.LabelReadable;

        BakeStats(item);
        item.Initialize();
        return item;
    }

    static public Entity Create(GearProfileDef profile, params (BoneDef bone, MaterialDef)[] materialBindings)
    {
        var item = ItemDefOf.Gear.Create();
        item.Profile = profile;

        item.SpriteComp.Body = profile.Role.Bones.Body.Clone();
        item.SpriteComp.BodyDef = profile.Role.Bones;

        foreach (var (bonedef, material) in materialBindings)
        {
            //item.Body.FindBone(bone).SetMaterial(material);
            var bone = item.Body.FindBone(bonedef);
            bone.SetMaterial(material);
            bone.SetSprite(profile.BoneSprites[bonedef]);
        }

        item.Name = profile.LabelReadable;

        BakeStats(item);
        item.Initialize();
        return item;
    }

    static internal void BakeStats(Entity tool)
    {
        var comp = tool.GetComponent<StatsComp>();
        comp.Bake(StatDefOf.ItemSpeed, BoneDefOf.ToolHandle);
        comp.Bake(StatDefOf.ItemPower, BoneDefOf.ToolHead);
    }

    internal static Entity Create(EntityCreationRequest req)
    {
        return CreateToolOrWeapon(req.Context as GearProfileDef, req.MaterialBindings[BoneDefOf.ToolHandle], req.MaterialBindings[BoneDefOf.ToolHead]);
    }

    internal static Entity CreateUnfinishedItem(Actor author, CraftingOrder order, MaterialDef handleMaterial, MaterialDef headMaterial)
    {
        var item = ItemDefOf.UnfinishedItem.Create();
        var profile = order.ProductDef;
        item.Profile = profile;
        var comp = item.GetComponent<UnfinishedItemComp>();
        comp.Initialize(author, order, [handleMaterial ,headMaterial]);

        // temp debugging
        var assembly = item.Resources.View(ResourceDefOf.Assembly);
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
        return CreateToolOrWeapon(_allDefs.SelectRandom(rand), mats.SelectRandom(rand), mats.SelectRandom(rand));
    }

    internal static Entity? CreateRandomNew(Random rand)
    {
        var slot = _defsBySlot.Keys.SelectRandom(rand);
        var item = CreateRandom(slot, rand);
        return item;
        //var item = ItemDefOf.Gear.Create();
        //var profile = _allDefs.SelectRandom(rand);
        //item.Profile = profile;
        //item.ToolComponent.ToolDef = profile;
        //item.SpriteComp.Body = profile.Role.Bones.Body.Clone();
        //item.SpriteComp.BodyDef = profile.Role.Bones;
        //item.Randomize(Server.Random);
        //return item;
    }
    internal static Entity? CreateRandom(GearSlotDef slot, Random random)
    {
        if (!_defsBySlot.ContainsKey(slot))
            return null;
        var profiles = _defsBySlot[slot];
        var profile = profiles.SelectRandom(random);
        var item = Create(profile, random);
        return item;
    }
    internal static Entity Create(GearProfileDef profile, Random random)
    {
        //var profiles = _defsBySlot[slot];
        //var profile = profiles.SelectRandom(random);
        var item = ItemDefOf.Gear.Create();
        item.Profile = profile;
        item.ToolComponent.ToolDef = profile;
        item.SpriteComp.Body = profile.Role.Bones.Body.Clone();
        item.SpriteComp.BodyDef = profile.Role.Bones;
        //item.Randomize(Server.Random);
        Randomize(item, random);
        return item;
    }

    internal static IEnumerable<GameObject> GenerateTemplates()
    {
        foreach (var toolProp in Def.Get<GearProfileDef>())
        {
            //var obj = CreateToolOrWeapon(toolProp, MaterialDefOf.LightWood, MaterialDefOf.LightWood);
            var bones = toolProp.Role.Bones.Bones;
            var bonesMats = bones
                .Select(b => (b, MaterialDefOf.LightWood))
                .ToArray();
            var obj = Create(toolProp, bonesMats);
            yield return obj;
        }
    }

    public static void Randomize(Entity item, Random r)
    {
        QualitySystem.Randomize(item);
        int durabilityMax = 0;
        var profile = (GearProfileDef)item.Profile;
        var role = profile.Role;
        var boneMats = profile.BoneMaterials;
        foreach (var set in boneMats.Inner)
        {
            var bone = set.Key;
            var filter = set.Value;
            var entityBone = item.Body.FindBone(bone);
            var mats = filter.MaterialTypes.SelectMany(t => MaterialSystem.MaterialsByType[t]).ToArray();
            var mat = mats.SelectRandom(r);
            entityBone.SetMaterial(mat);
            durabilityMax += mat.Density;
        }
        var durability = item.Resources.View(ResourceDefOf.Durability);
        durability.Value = durability.Max = durabilityMax;
        var comp = item.ToolComponent;
        var craftsmanship = QualitySystem.GetRandomCraftsmahship(r);
        comp.Craftsmanship = craftsmanship;
        ApplyName(item);
        var bonusCount = item.Quality.Rank - QualityDefOf.Common.Rank + 5;
        var selectedBonuses = StatSystem.RollBonuses(ActorDnaDefOf.Npc, role, bonusCount);
        foreach (var bonus in selectedBonuses)
            item.ToolComponent.Modifiers[bonus] = 0;
        CalculateEnhancements(comp);
        role.Worker.InitIntrinsicStats(item);
    }

    public static void Randomize(Entity item)
    {
        Randomize(item, _r);
    }

    public static void CalculateEnhancements(ToolComp comp)
    {
        var random = new Random(GetBonusSeed(comp.Owner.RefId, 1));
        var temp = comp.Modifiers.Count;
        foreach (var mod in comp.Modifiers.Keys)
            comp.Modifiers[mod] = RollEnhancement(random, mod, temp);
    }

    public static void PostLoad(Entity item)
    {
        var profile = (GearProfileDef)item.Profile;
        var role = profile.Role;
        role.Worker.InitIntrinsicStats(item);
        CalculateEnhancements(item.ToolComponent);
    }

    static float RollEnhancement(Random rand, StatDef stat, int magnitude)
    {
        return 1 + rand.Next(magnitude);
    }

    static int GetBonusSeed(int itemId, int version)
    {
        unchecked
        {
            uint x = (uint)itemId;
            x ^= (uint)version * 0x9E3779B9u;
            x ^= x >> 16;
            x *= 0x7FEB352Du;
            x ^= x >> 15;
            x *= 0x846CA68Bu;
            x ^= x >> 16;
            return (int)x;
        }
    }

    public static float? CalculateStat(Entity item, StatDef stat)
    {
        //var profile = (GearProfileDef)item.Profile;
        //if (!profile.Role.Stats.Contains(stat))
        //    return null;
        //return stat.CalculateFor(item);
        var comp = item.ToolComponent;
        var stats = comp.IntrinsicStats;
        if (!stats.TryGetValue(stat, out var runtime))
            return null;
        return runtime.Value;
    }

    public static IEnumerable<StatDef> GetSupportedStats(Entity item)
    {
        var gearProf = item.Profile as GearProfileDef;
        var gearRole = gearProf.Role;
        return gearRole.Stats;
    }

    public static IEnumerable<(StatDef def, float value)> GetStats(Entity item)
    {
        var gearProf = item.Profile as GearProfileDef;
        var gearRole = gearProf.Role;
        foreach(var stat in gearRole.Stats)
        {
            var value = CalculateStat(item, stat).Value;
            yield return (stat, value);
        }
    }

    public static IEnumerable<(Def def, float value)> GetStatsNew(Entity item)
    {
        var gearProf = item.Profile as GearProfileDef;
        var gearRole = gearProf.Role;
        foreach (var stat in gearRole.Stats)
        {
            var value = CalculateStat(item, stat).Value;
            yield return (stat, value);
        }
        foreach (var (bonusDef, bonusVal) in item.ToolComponent.ActorEnhancements)
            yield return (bonusDef, bonusVal);
    }

    internal static void ApplyName(Entity item)
    {
        var mat = item.Body.Material;
        var prof = item.Profile;
        item.SetName($"{item.ToolComponent.Craftsmanship.LabelReadable} {mat.LabelReadable} {prof.LabelReadable}");
    }

    public static float GetFinalValue(Entity item, ToolUseDef capability)
    {
        var comp = item.ToolComponent;
        var profile = item.Profile as GearProfileDef;
        var stats = comp.IntrinsicStats;
        var magnitude = stats[StatDefOf.ItemPower].Value;
        var profileMod = profile.ToolUsesModifiers[capability];
        return magnitude * profileMod;
    }
}
