using Project1.Core.Entities;
using Project1.Core.Entities.Actors;
using Project1.Core.Gear;
using Project1.Core.Systems.Quality;
using Project1.Core.Systems.Stats;
using Project1.Core.Systems.Tools;
using Project1.Core.UI;
using Project1.Core.World.Combat;
using Project1.Framework;
using Project1.Framework.UI;
using System.Collections.Generic;
using System.Linq;

namespace Project1.Core.Systems.ItemRenown;

[EnsureStaticCtorCall]
internal static class ItemRenownSystem
{
    static readonly HashSet<GearSlotDef> RenownEligibleSlots;

    static ItemRenownSystem()
    {
        Registry.WorldEventHooksServer.Register<WorldCombatEvent>(OnWorldCombatEvent);

        TooltipManager.RegisterProvider(GetTooltipControls);

        RenownEligibleSlots = [.. 
            Def.Get<GearSlotDef>().Where(slot => slot.AttackTargetWeight > 0), 
            GearSlotDefOf.Mainhand];
    }

    private static void OnWorldCombatEvent(WorldCombatEvent e)
    {
        if (!e.Result.Connected)
            return;
        var world = e.World;
        var attacker = world.Resolve<Actor>(e.AttackerId);
        var combat = world.Combat.Get(attacker);
        var defender = combat.GetOther(attacker);
        if (attacker.Gear.Weapon is Entity weapon)
            UpdateRenown(weapon);
        if(e.Result.BodyPart is GearSlotDef slot && defender.Gear.GetGear(slot) is Entity armor)
            UpdateRenown(armor);
    }

    static void UpdateRenown(Entity item)
    {
        var current = item.Quality;
        var currentLevel = (int)current.RankEffective;
        var renownXp = GetRenownXp(item).Value;
        while (renownXp >= XPForLevel(currentLevel + 1))
        {
            QualitySystem.Upgrade(item);
            currentLevel++;
        }
    }

    static int XPForLevel(int level)
        => 5 * level * level;

    static TrackingStat GetRenownXp(Entity item)
        => item.TrackingStats.GetOrCreateStat(TrackingStatDefOf.TotalDamage);

    static IEnumerable<Control> GetTooltipControls(ITooltippable item)
    {
        if (item is not Entity typed)
            yield break;
        if (!IsRenownEligible(typed))
            yield break;
        //var stat = typed.TrackingStats.GetOrCreateStat(TrackingStatDefOf.TotalDamage);
        var xp = GetRenownXp(typed);
        yield return new LabelNew(() =>
            $"Renown xp: {xp.Value}"// {stat.Value}"
            )
                .InvalidateOn(xp);
    }

    static bool IsRenownEligible(Entity item)
    {
        if (item.Profile is not GearProfileDef profile)
            return false;
        if (!RenownEligibleSlots.Contains(profile.Role.Slot))
            return false;
        return true;
    }
}
