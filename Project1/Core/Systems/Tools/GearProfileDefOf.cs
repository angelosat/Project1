using Project1.Core.Animations;
using Project1.Core.Assets;
using Project1.Core.Entities.Stats;
using Project1.Core.Legacy.Properties;
using Project1.Core.Skills;
using Project1.Core.Systems.Gear;
using Project1.Core.Systems.Materials;
using Project1.Core.Towns.Duties;
using Project1.Framework;
using System.Collections.Generic;
using System.Linq;

namespace Project1.Core.Systems.Tools;


[EnsureStaticCtorCall]
static class GearProfileDefOf
{
    public static readonly GearProfileDef Shovel = new("Shovel", GearRoleDefOf.Tool)
    {
        Description = "Used to dig out grainy material like soil dirt and sand.",
        SpriteHandle = ItemContent.ShovelHandle,
        SpriteHead = ItemContent.ShovelHead,
        ToolUse = ToolUseDefOf.Digging,
        Damage = DamageDefOf.Digging,
        Skill = SkillDefOf.Digging,
        ExampleBone = BoneDefOf.ToolHandle,
        BoneSprites =
        {
            { BoneDefOf.ToolHandle, ItemContent.ShovelHandle},
            { BoneDefOf.ToolHead, ItemContent.ShovelHead }
        },
        BoneMaterials = BoneMaterialSet.ToolDefault
    };

    public static readonly GearProfileDef Hammer = new("Hammer", GearRoleDefOf.Tool)
    {
        Description = "Used for building.",
        SpriteHandle = ItemContent.HammerHandle,
        SpriteHead = ItemContent.HammerHead,
        ToolUse = ToolUseDefOf.Building,
        Damage = DamageDefOf.Blunt,
        Skill = SkillDefOf.Construction,
        BoneSprites =
        {
            { BoneDefOf.ToolHandle, ItemContent.HammerHandle},
            { BoneDefOf.ToolHead, ItemContent.HammerHead }
        },
        BoneMaterials = BoneMaterialSet.ToolDefault
    };
    public static readonly GearProfileDef Pickaxe = new("Pickaxe", GearRoleDefOf.Tool)
    {
        Description = "Used for mining.",
        SpriteHandle = ItemContent.PickaxeHandle,
        SpriteHead = ItemContent.PickaxeHead,
        ToolUse = ToolUseDefOf.Mining,
        Damage = DamageDefOf.Mining,
        Skill = SkillDefOf.Mining,
        BoneSprites =
        {
            { BoneDefOf.ToolHandle, ItemContent.PickaxeHandle},
            { BoneDefOf.ToolHead, ItemContent.PickaxeHead }
        },
        BoneMaterials = BoneMaterialSet.ToolDefault
    };
    public static readonly GearProfileDef Handsaw = new("Handsaw", GearRoleDefOf.Tool)
    {
        Description = "Used for carpentry.",
        SpriteHandle = ItemContent.HandsawHandle,
        SpriteHead = ItemContent.HandsawHead,
        ToolUse = ToolUseDefOf.Carpentry,
        Damage = DamageDefOf.Sawing,
        Skill = SkillDefOf.Carpentry,
        BoneSprites =
        {
            { BoneDefOf.ToolHandle, ItemContent.HandsawHandle},
            { BoneDefOf.ToolHead, ItemContent.HandsawHead }
        },
        BoneMaterials = BoneMaterialSet.ToolDefault
    };
    public static readonly GearProfileDef Hoe = new("Hoe", GearRoleDefOf.Tool)
    {
        Description = "Used to prepare soil for planting by converting it into farmland.",
        SpriteHandle = ItemContent.HoeHandle,
        SpriteHead = ItemContent.HoeHead,
        ToolUse = ToolUseDefOf.Argiculture,
        Damage = DamageDefOf.Tilling,
        Skill = SkillDefOf.Argiculture,
        BoneSprites =
        {
            { BoneDefOf.ToolHandle, ItemContent.HoeHandle},
            { BoneDefOf.ToolHead, ItemContent.HoeHead }
        },
        BoneMaterials = BoneMaterialSet.ToolDefault
    };

    public static readonly GearProfileDef Axe = new("Axe", GearRoleDefOf.Tool)
    {
        Description = "Chops down trees.",
        SpriteHandle = ItemContent.AxeHandle,
        SpriteHead = ItemContent.AxeHead,
        ToolUse = ToolUseDefOf.Chopping,
        Damage = DamageDefOf.Chopping,
        Skill = SkillDefOf.Plantcutting,
        BoneSprites =
        {
            { BoneDefOf.ToolHandle, ItemContent.AxeHandle},
            { BoneDefOf.ToolHead, ItemContent.AxeHead }
        },
        BoneMaterials = BoneMaterialSet.ToolDefault
    };

    public static readonly GearProfileDef Helmet = new("Helmet", GearRoleDefOf.Head)
    {
        BoneSprites =
        {
            { BoneDefOf.Item, ItemContent.HelmetFull}
        },
        BoneMaterials = new BoneMaterialSet().Allow(BoneDefOf.Item, MaterialFilter.Allow(MaterialTypeDefOf.Metal))
    };

    //public static readonly ItemVariantDef AxeNew = new ItemVariantDef(ItemDefOf.Tool, "AxeNew")
    //{ Description = "Chops down trees." }
    //    .AddSpec(new SpriteComp.Spec() { Overrides = [(BoneDefOf.ToolHandle, ItemContent.AxeHandle), (BoneDefOf.ToolHead, ItemContent.AxeHead)] })
    //    .AddSpec(new ToolComp.Spec(ToolUseDefOf.Chopping))
    //    ;

    static GearProfileDefOf()
    {
        Def.Register(typeof(GearProfileDefOf));

        

        //GenerateRecipesNew();
    }

    private static void GenerateRecipesNew()
    {
        var defs = Def.Database.Values.OfType<GearProfileDef>().ToList();
        foreach (var toolDef in defs)
        {
            var reagents = new List<Reaction.Reagent>();

            foreach (var reagent in CraftingProperties.ToolCraftingProperties.Reagents)
                reagents.Add(reagent.Value);

            var reaction = new Reaction(
                $"Craft {toolDef.LabelReadable}",
                Reaction.CanBeMadeAt(IsWorkstation.Types.None, IsWorkstation.Types.Workbench),
                reagents,
                new List<Reaction.Product>() {
                    new Reaction.Product(dic=>ToolSystem.CreateToolOrWeapon(toolDef, dic["Handle"].Body.Material, dic["Head"].Body.Material)) },
                    //new Reaction.Product(dic=>ItemFamilyDefOf.Tool.System.Create(toolDef, new ToolSystem.Args(dic["Handle"].Body.Material, dic["Head"].Body.Material))) },
                SkillDefOf.Crafting,
                DutyDefOf.Craftsman)
            { CreatesUnfinishedItem = true }
                .ModWorkRequiredFromMaterials();

            Def.Register(reaction);
        }
    }
}
