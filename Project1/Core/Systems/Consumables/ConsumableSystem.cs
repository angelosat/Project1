using Project1.Core.Animations;
using Project1.Core.Entities;
using Project1.Core.Systems.Magic;
using Project1.Core.Systems.Materials;
using Project1.Core.Systems.Tools;
using Project1.Core.Systems.Quality;
using Project1.Framework;
using System.Collections.Generic;

namespace Project1.Core.Systems.Consumables;

[EnsureStaticCtorCall]
internal static class ConsumableSystem
{
    public static Dictionary<BoneDef, CraftingRules> Rules = [];
    static ConsumableSystem()
    {
    }

    public static Entity CreateScroll(SpellDef spell, MaterialDef material, QualityDef quality)
    {
        var item = ItemDefOf.Consumable.Create(profile: ConsumableDefOf.Scroll);
        item.Body.Sprite = ConsumableDefOf.Scroll.Sprite;
        item.Body.Material = material;
        item.Consumable.Spell = spell;
        //item.QualityComp.SetLevel(quality);
        item.QualityComp.Tier = quality;
        ConsumableDefOf.Scroll.Worker.PostProcess(item);
        item.Initialize();
        return item;
    }
    public static Entity Create(ConsumableDef profile, MaterialDef material, QualityDef quality, int stackSize = -1)
    {
        var item = ItemDefOf.Consumable.Create(profile: profile, amount: stackSize);
        item.Body.Sprite = profile.Sprite;
        item.Body.Material = material;
        item.QualityComp.Tier = quality;
        item.Name = $"{material.LabelReadable} {profile.LabelReadable}";
        item.Initialize();
        profile.Worker.PostProcess(item);
        return item;
    }

    internal static Entity Create(EntityCreationRequest req)
    {
        return Create((ConsumableDef)req.Context, req.MaterialBindings[BoneDefOf.Item], req.Quality, req.StackSize);
    }

    extension(Entity item)
    {
        public bool IsConsumable => item.Consumable is not null;
        public ConsumableComp Consumable => item.TryGetComponent<ConsumableComp>(out var comp) ? comp : null;
    }
}
