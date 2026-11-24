using Project1.Core.Animations;
using Project1.Core.Entities;
using Project1.Core.Entities.Actors;
using Project1.Core.Resources;
using Project1.Core.Simulation;
using Project1.Core.Systems.Effects;
using Project1.Core.Systems.Magic;
using Project1.Core.Systems.Materials;
using Project1.Core.Systems.Quality;
using Project1.Core.Systems.Tools;
using Project1.Core.Towns.Services.Spells;
using Project1.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

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
    public static Entity Create(ConsumableDef profile, SpellDef spell, QualityDef quality, int stackSize = -1)
    {
        var item = ItemDefOf.Consumable.Create(profile: profile, amount: stackSize);
        item.Body.Sprite = profile.Sprite;
        item.Body.Material = null;
        item.QualityComp.Tier = quality;
        item.Initialize();
        item.Consumable.Spell = spell;
        //var (effect, target, magnitude) = spell.Effects[0];
        var (effect, target) = spell.Effects.First().Key;
        item.Name = $"{ConsumableDefOf.Potion.LabelReadable} of {effect.Verb} {target.LabelReadable}";
        return item;
    }

    internal static Entity Create(EntityCreationRequest req)
    {
        return Create((ConsumableDef)req.Context, req.MaterialBindings[BoneDefOf.Item], req.Quality, req.StackSize);
    }

    internal static void Activate(Entity potion, Actor actor)
    {
        Activate(potion, actor, actor);
    }

    internal static void Activate(Entity item, Actor actor, Actor target)
    {
        var family = item.GetProfile<ConsumableDef>();

        if (family.TriggersCooldown &&
            target.Effects.Any(EffectDefOf.ConsumableCooldown, family))
            throw new InvalidOperationException(
                $"Tried to activate {item} while {family} cooldown is active.");

        item.Consume(1);
        var world = (StaticWorld)item.World;
        //item.Consumable.ApplyEffects(target);
        foreach (var (key, mag) in item.Consumable.Spell.Effects)
            world.Events.Post(new ActorEffectAppliedNewEvent(world, target.RefId, key.Def, key.Target, mag));

        if (family.TriggersCooldown)
        //    target.Effects.Apply(
        //        EffectDefOf.ConsumableCooldown.Worker.CreateWrapper(family, 30));
        world.Events.Post(new ActorEffectAppliedNewEvent(world, target.RefId, EffectDefOf.ConsumableCooldown, family, 30));
    }

    internal static bool CanUsePotion(Actor actor)
        => CanUseConsumable(actor, ConsumableDefOf.Potion);

    internal static bool CanUseConsumable(Actor actor, ConsumableDef family)
        => !actor.Effects.Any(EffectDefOf.ConsumableCooldown, family);

    internal static Entity FindInInventory(Actor actor, ResourceDef restore)
        //=> actor.Inventory.First(i => i.Profile == ConsumableDefOf.Potion && i.Consumable.HasEffect(EffectDefOf.RestoreResource, restore));
        => actor.Inventory.First(i => i.Profile == ConsumableDefOf.Potion && i.Consumable.HasEffect(EffectDefOf.RestoreResource, restore));

    extension(Entity item)
    {
        public bool IsConsumable => item.Consumable is not null;
        public ConsumableComp Consumable => item.TryGetComponent<ConsumableComp>(out var comp) ? comp : null;
    }
}
