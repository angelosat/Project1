using Project1.Core.Animations;
using Project1.Core.Assets;
using Project1.Core.Skills;
using Project1.Core.Systems.Gear;
using Project1.Core.Systems.Materials;
using Project1.Core.Systems.Stats;
using Project1.Framework;

namespace Project1.Core.Systems.Tools;

[EnsureStaticCtorCall]
static class GearProfileDefOf
{
    public static readonly GearProfileDef Shovel = new("Shovel", GearRoleDefOf.MainHand)
    {
        Description = "Used to dig out grainy material like soil dirt and sand.",
        SpriteHandle = ItemContent.ShovelHandle,
        SpriteHead = ItemContent.ShovelHead,
        ToolUse = ToolUseDefOf.Digging,
        ToolUses = [ToolUseDefOf.Digging],
        Damage = DamageDefOf.Blunt,
        BaseSpeed = 1.3f,
        Skill = SkillDefOf.Digging,
        ExampleBone = BoneDefOf.ToolHandle,
        BoneSprites =
        {
            { BoneDefOf.ToolHandle, ItemContent.ShovelHandle },
            { BoneDefOf.ToolHead, ItemContent.ShovelHead }
        },
        BoneMaterials = BoneMaterialSet.ToolDefault,
        ToolUsesModifiers = { { ToolUseDefOf.Digging, 1f } }
    };

    public static readonly GearProfileDef Hammer = new("Hammer", GearRoleDefOf.MainHand)
    {
        Description = "Used for building.",
        SpriteHandle = ItemContent.HammerHandle,
        SpriteHead = ItemContent.HammerHead,
        ToolUse = ToolUseDefOf.Building,
        ToolUses = [ToolUseDefOf.Building],
        Damage = DamageDefOf.Blunt,
        Skill = SkillDefOf.Construction,
        BaseSpeed = 1.1f,
        BoneSprites =
        {
            { BoneDefOf.ToolHandle, ItemContent.HammerHandle },
            { BoneDefOf.ToolHead, ItemContent.HammerHead }
        },
        BoneMaterials = BoneMaterialSet.ToolDefault,
        ToolUsesModifiers = { { ToolUseDefOf.Building, 1f } }
    };
    public static readonly GearProfileDef Pickaxe = new("Pickaxe", GearRoleDefOf.MainHand)
    {
        Description = "Used for mining.",
        SpriteHandle = ItemContent.PickaxeHandle,
        SpriteHead = ItemContent.PickaxeHead,
        ToolUse = ToolUseDefOf.Mining,
        ToolUses = [ToolUseDefOf.Mining],
        Damage = DamageDefOf.Piercing,
        Skill = SkillDefOf.Mining,
        BaseSpeed = 1.6f,
        BoneSprites =
        {
            { BoneDefOf.ToolHandle, ItemContent.PickaxeHandle },
            { BoneDefOf.ToolHead, ItemContent.PickaxeHead }
        },
        BoneMaterials = BoneMaterialSet.ToolDefault,
        ToolUsesModifiers = { { ToolUseDefOf.Mining, 1f } }
    };
    public static readonly GearProfileDef Handsaw = new("Handsaw", GearRoleDefOf.MainHand)
    {
        Description = "Used for carpentry.",
        SpriteHandle = ItemContent.HandsawHandle,
        SpriteHead = ItemContent.HandsawHead,
        ToolUse = ToolUseDefOf.Carpentry,
        ToolUses = [ToolUseDefOf.Carpentry],
        Damage = DamageDefOf.Slashing,
        Skill = SkillDefOf.Carpentry,
        BaseSpeed = 1.1f,
        BoneSprites =
        {
            { BoneDefOf.ToolHandle, ItemContent.HandsawHandle },
            { BoneDefOf.ToolHead, ItemContent.HandsawHead }
        },
        BoneMaterials = BoneMaterialSet.ToolDefault,
        ToolUsesModifiers = { { ToolUseDefOf.Carpentry, 1f } }

    };
    public static readonly GearProfileDef Hoe = new("Hoe", GearRoleDefOf.MainHand)
    {
        Description = "Used to prepare soil for planting by converting it into farmland.",
        SpriteHandle = ItemContent.HoeHandle,
        SpriteHead = ItemContent.HoeHead,
        ToolUse = ToolUseDefOf.Argiculture,
        ToolUses = [ToolUseDefOf.Argiculture],
        Damage = DamageDefOf.Blunt,
        Skill = SkillDefOf.Argiculture,
        BaseSpeed = 1.5f,
        BoneSprites =
        {
            { BoneDefOf.ToolHandle, ItemContent.HoeHandle },
            { BoneDefOf.ToolHead, ItemContent.HoeHead }
        },
        BoneMaterials = BoneMaterialSet.ToolDefault,
        ToolUsesModifiers = { { ToolUseDefOf.Argiculture, 1f } }
    };

    public static readonly GearProfileDef Axe = new("Axe", GearRoleDefOf.MainHand)
    {
        Description = "Chops down trees.",
        SpriteHandle = ItemContent.AxeHandle,
        SpriteHead = ItemContent.AxeHead,
        ToolUse = ToolUseDefOf.Chopping,
        ToolUses = [ToolUseDefOf.Chopping],
        Damage = DamageDefOf.Slashing,
        Skill = SkillDefOf.Plantcutting,
        BaseSpeed = 1.2f,
        BoneSprites =
        {
            { BoneDefOf.ToolHandle, ItemContent.AxeHandle },
            { BoneDefOf.ToolHead, ItemContent.AxeHead }
        },
        BoneMaterials = BoneMaterialSet.ToolDefault,
        ToolUsesModifiers = { 
            { ToolUseDefOf.Chopping, 1f },
        }
    };

    public static readonly GearProfileDef Sword = new("Sword", GearRoleDefOf.MainHand)
    {
        Description = "Slashes enemies.",
        SpriteHandle = ItemContent.AxeHandle,
        SpriteHead = ItemContent.AxeHead,
        ToolUse = ToolUseDefOf.Chopping,
        ToolUses = [ToolUseDefOf.Chopping, ToolUseDefOf.Argiculture],
        Damage = DamageDefOf.Slashing,
        Skill = SkillDefOf.Melee,
        BaseSpeed = 1f,
        BoneSprites =
        {
            { BoneDefOf.ToolHandle, ItemContent.SwordHandle },
            { BoneDefOf.ToolHead, ItemContent.SwordHead }
        },
        BoneMaterials = BoneMaterialSet.ToolDefault,
        ToolUsesModifiers = { 
            { ToolUseDefOf.Chopping, .5f },
            { ToolUseDefOf.Argiculture, .2f }
        }
    };
    public static readonly GearProfileDef Dagger = new("Dagger", GearRoleDefOf.MainHand)
    {
        Description = "Slashes enemies.",
        SpriteHandle = ItemContent.AxeHandle,
        SpriteHead = ItemContent.AxeHead,
        Damage = DamageDefOf.Piercing,
        Skill = SkillDefOf.Melee,
        BaseSpeed = .6f,
        BoneSprites =
        {
            { BoneDefOf.ToolHandle, ItemContent.SwordHandle },
            { BoneDefOf.ToolHead, ItemContent.SwordHead }
        },
        BoneMaterials = BoneMaterialSet.ToolDefault
    };
    public static readonly GearProfileDef Mace = new("Mace", GearRoleDefOf.MainHand)
    {
        Description = "Slashes enemies.",
        SpriteHandle = ItemContent.AxeHandle,
        SpriteHead = ItemContent.AxeHead,
        Damage = DamageDefOf.Blunt,
        Skill = SkillDefOf.Melee,
        BaseSpeed = 1.3f,
        BoneSprites =
        {
            { BoneDefOf.ToolHandle, ItemContent.SwordHandle },
            { BoneDefOf.ToolHead, ItemContent.SwordHead }
        },
        BoneMaterials = BoneMaterialSet.ToolDefault
    };
    public static readonly GearProfileDef Helmet = new("Helmet", GearRoleDefOf.Head)
    {
        BoneSprites =
        {
            { BoneDefOf.Item, ItemContent.HelmetFull }
        },
        BoneMaterials = new BoneMaterialSet().Allow(BoneDefOf.Item, MaterialFilter.Allow(MaterialTypeDefOf.Metal)),
        ArmorModifier = 1
    };

    public static readonly GearProfileDef Boots = new("Boots", GearRoleDefOf.Feet)
    {
        BoneSprites =
        {
            { BoneDefOf.Item, ItemContent.HelmetFull }
        },
        BoneMaterials = new BoneMaterialSet().Allow(BoneDefOf.Item, MaterialFilter.Allow(MaterialTypeDefOf.Metal)),
        ArmorModifier = 1
    };

    public static readonly GearProfileDef Shoes = new("Shoes", GearRoleDefOf.Feet)
    {
        BoneSprites =
        {
            { BoneDefOf.Item, ItemContent.HelmetFull }
        },
        BoneMaterials = new BoneMaterialSet().Allow(BoneDefOf.Item, MaterialFilter.Allow(MaterialTypeDefOf.Metal)),
        ArmorModifier = 1
    };

    public static readonly GearProfileDef Shirt = new("Shirt", GearRoleDefOf.Torso)
    {
        BoneSprites =
        {
            { BoneDefOf.Item, ItemContent.HelmetFull }
        },
        BoneMaterials = new BoneMaterialSet().Allow(BoneDefOf.Item, MaterialFilter.Allow(MaterialTypeDefOf.Metal)), 
        ArmorModifier = 1
    };

    public static readonly GearProfileDef Pants = new("Pants", GearRoleDefOf.Legs)
    {
        BoneSprites =
        {
            { BoneDefOf.Item, ItemContent.HelmetFull }
        },
        BoneMaterials = new BoneMaterialSet().Allow(BoneDefOf.Item, MaterialFilter.Allow(MaterialTypeDefOf.Metal)),
        ArmorModifier = 1
    };

    static GearProfileDefOf()
    {
        Def.Register(typeof(GearProfileDefOf));
    }

    //private static void GenerateRecipesNew()
    //{
    //    var defs = Def.Database.Values.OfType<GearProfileDef>().ToList();
    //    foreach (var toolDef in defs)
    //    {
    //        var reagents = new List<Reaction.Reagent>();

    //        foreach (var reagent in CraftingProperties.ToolCraftingProperties.Reagents)
    //            reagents.Add(reagent.Value);

    //        var reaction = new Reaction(
    //            $"Craft {toolDef.LabelReadable}",
    //            Reaction.CanBeMadeAt(IsWorkstation.Types.None, IsWorkstation.Types.Workbench),
    //            reagents,
    //            new List<Reaction.Product>() {
    //                new Reaction.Product(dic=>ToolSystem.CreateToolOrWeapon(toolDef, dic["Handle"].Body.Material, dic["Head"].Body.Material)) },
    //                //new Reaction.Product(dic=>ItemFamilyDefOf.Tool.System.Create(toolDef, new ToolSystem.Args(dic["Handle"].Body.Material, dic["Head"].Body.Material))) },
    //            SkillDefOf.Crafting,
    //            DutyDefOf.Craftsman)
    //        { CreatesUnfinishedItem = true }
    //            .ModWorkRequiredFromMaterials();

    //        Def.Register(reaction);
    //    }
    //}
}
