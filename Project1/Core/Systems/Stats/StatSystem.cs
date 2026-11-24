using Project1.Core.Entities.Actors;
using Project1.Core.Interactions;
using Project1.Core.Skills;
using Project1.Core.Systems.Effects;
using Project1.Core.Systems.Tools;
using Project1.Framework;
using Project1.Framework.Helpers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Project1.Core.Entities.Stats;

[EnsureStaticCtorCall]
public static class StatSystem
{
    static readonly Random r = new();

    static readonly Dictionary<SkillDef, ToolUseDef> _skillsToTools = [];

    static readonly Dictionary<ToolUseDef, GearProfileDef> _useToProfile = [];

    static readonly Dictionary<SkillDef, InteractionDef> _skillToInteraction = [];

    static readonly Dictionary<ToolUseDef, InteractionDef> _toolToInteraction = [];

    static readonly Dictionary<Def, StatDef> DerivedStats = [];

    static readonly Dictionary<GearRoleDef, List<StatDef>> BonusRolesEligibility = [];

    static readonly Dictionary<Type, List<StatDef>> _statsByCompType = [];

    static public IReadOnlyDictionary<SkillDef, ToolUseDef> SkillsToTools => _skillsToTools;

    static public IReadOnlyDictionary<ToolUseDef, GearProfileDef> UseToProfile => _useToProfile;

    static public IReadOnlyDictionary<SkillDef, InteractionDef> SkillToInteraction => _skillToInteraction;

    static public IReadOnlyDictionary<ToolUseDef, InteractionDef> ToolToInteraction => _toolToInteraction;

    static public readonly StatDef[] AllStats, ActorStats;

    static StatSystem()
    {
        var tooldefs = Def.Get<ToolUseDef>();
        foreach (var def in tooldefs)
            if (def.Skill is not null)
                _skillsToTools[def.Skill] = def;

        var toolprofiles = Def.Get<GearProfileDef>();
        foreach (var profile in toolprofiles)
            if (profile.ToolUse is not null)
                _useToProfile[profile.ToolUse] = profile;

        var interactions = Def.Get<InteractionDef>();
        foreach (var i in interactions)
            if (i.ToolUse is ToolUseDef use)
            {
                _skillToInteraction[use.Skill] = i;
                _toolToInteraction[i.ToolUse] = i;
            }

        var statdefs = Def.Get<StatDef>();
        foreach (var stat in statdefs)
        {
            var comptype = stat.CompRequired;
            if (comptype is null)
                continue;
            if (!_statsByCompType.TryGetValue(comptype, out var list))
                _statsByCompType[comptype] = list = [];
            list.Add(stat);
        }

        AllStats = [.. statdefs];

        ActorStats = [.. AllStats.Where(s => s.Worker.IsApplicable(s, ActorDnaDefOf.Npc))];

        foreach (var stat in statdefs.Where(s => s.TargetDef is not null))
            DerivedStats[stat.TargetDef] = stat;

        foreach (var role in Def.Get<GearRoleDef>())
            BonusRolesEligibility.Add(role, []);

        foreach(var stat in ActorStats)
        {
            if (stat.EligibleBonusTargets is null)
                foreach (var list in BonusRolesEligibility.Values)
                    list.Add(stat);
            else
                foreach (var role in stat.EligibleBonusTargets)
                    BonusRolesEligibility[role].Add(stat);
        }

        //Registry.WorldEventHooksClient.Register<ActorStatBonusModifiedEvent>(OnActorStatBonusModified);
        //Registry.WorldEventHooksServer.Register<ActorStatBonusModifiedEvent>(OnActorStatBonusModified);

        //Registry.WorldEventHooksClient.Register<ActorEffectAppliedEvent>(OnActorEffectApplied);
        //Registry.WorldEventHooksServer.Register<ActorEffectAppliedEvent>(OnActorEffectApplied);

        Registry.WorldEventHooksClient.Register<ActorEffectAppliedNewEvent>(OnActorEffectApplied);
        Registry.WorldEventHooksServer.Register<ActorEffectAppliedNewEvent>(OnActorEffectApplied);
        Registry.WorldEventHooksClient.Register<ActorEffectRemovedEvent>(OnActorEffectRemoved);
        Registry.WorldEventHooksServer.Register<ActorEffectRemovedEvent>(OnActorEffectRemoved);
    }

    static public StatDef GetDerivedStat(Def def)
        => DerivedStats[def];

    static public IEnumerable<ToolUseDef> GetToolUsesFor(SkillDef skill)
    {
        if (SkillsToTools.TryGetValue(skill, out var result))
            yield return result;
    }

    static public IEnumerable<InteractionDef> GetInteractionsFor(ToolUseDef toolUse)
    {
        if (ToolToInteraction.TryGetValue(toolUse, out var result))
            yield return result;
    }

    static public IEnumerable<InteractionDef> GetAffectedInteractionsFor(SkillDef skill)
    {
        foreach (var result in GetToolUsesFor(skill))
            foreach (var r in GetInteractionsFor(result))
                yield return r;
    }

    static public string Format(StatDef def, float value)
    {
        if (def.Type == StatDef.Types.Scalar)
            return $"{value}";
        else if (def.Type == StatDef.Types.Percentile)
            return $"{value:0.##}%";
        throw new UnreachableException();
    }

    internal static IEnumerable<StatDef> RollBonuses(ActorDnaDef species, GearRoleDef role, int count)
    {
        var collection = BonusRolesEligibility[role];
        var indices = new int[collection.Count];
        for (int i = 0; i < collection.Count; i++)
            indices[i] = i;
        indices = [.. indices.Shuffle()];
        for (int i = 0; i < count; i++)
            yield return collection[indices[i]];
    }

    internal static IEnumerable<StatDef> RollBonuses(GearRoleDef role, int count, Random random)
    {
        var eligible = BonusRolesEligibility[role];
        var shuffled = eligible.Shuffle(random).ToArray();
        for (int i = 0; i < count && i < eligible.Count; i++)
            yield return shuffled[i]; 
        // TODO cache a shuffled array, store the current index, and reshuffle it when the index reaches max ?
    }

    internal static IReadOnlyCollection<StatDef> GetEligibleBonuses(GearRoleDef role)
        => BonusRolesEligibility[role];

    //private static void OnActorStatBonusModified(ActorStatBonusModifiedEvent e)
    //{
    //    var actor = e.World.Resolve<Actor>(e.ActorId);
    //    actor.Stats.ModifyBonus(e.StatDef, e.Delta);
    //}

    //private static void OnActorEffectApplied(ActorEffectAppliedEvent e)
    //{
    //    var actor = e.Actor;
    //    var effect = e.Effect;
    //    InvalidateStats(actor, effect);
    //}
    private static void OnActorEffectApplied(ActorEffectAppliedNewEvent e)
    {
        var actor = e.World.Resolve<Actor>(e.ActorId);
        var effect = e.EffectDef;
        InvalidateStats(actor, effect);
    }
    private static void OnActorEffectRemoved(ActorEffectRemovedEvent e)
    {
        var actor = e.World.Resolve<Actor>(e.ActorId); ;
        var effectDef = e.EffectDef;
        var wrapper = actor.Effects.GetEffect(effectDef);
        InvalidateStats(actor, wrapper);
    }

    private static void InvalidateStats(Actor actor, EntityEffectWrapper effect)
    {
        var bonuses = effect.GetStatModifiers(actor);
        foreach (var (statDef, _) in bonuses)
            actor.Stats.MarkDirty(statDef);
    }

    private static void InvalidateStats(Actor actor, EffectDef effect)
    {
        var bonuses = effect.Worker.GetStatModifiers(actor);
        foreach (var (statDef, _) in bonuses)
            actor.Stats.MarkDirty(statDef);
    }
}
