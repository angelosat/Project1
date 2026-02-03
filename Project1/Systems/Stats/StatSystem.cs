using Project1.Framework.Base;
using Project1.Framework.Interactions;
using Project1.Framework.Skills;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using static Start_a_Town_.GlobalVars;

namespace Start_a_Town_
{
    [EnsureStaticCtorCall]
    public static class StatSystem
    {
        static readonly Dictionary<SkillDef, ToolUseDef> _skillsToTools = [];
        static readonly Dictionary<ToolUseDef, ToolProfileDef> _useToProfile = [];
        static readonly Dictionary<SkillDef, InteractionDef> _skillToInteraction = [];
        static readonly Dictionary<ToolUseDef, InteractionDef> _toolToInteraction = [];
        static public IReadOnlyDictionary<SkillDef, ToolUseDef> SkillsToTools => _skillsToTools;
        static public IReadOnlyDictionary<ToolUseDef, ToolProfileDef> UseToProfile => _useToProfile;
        static public IReadOnlyDictionary<SkillDef, InteractionDef> SkillToInteraction => _skillToInteraction;
        static public IReadOnlyDictionary<ToolUseDef, InteractionDef> ToolToInteraction => _toolToInteraction;
        static StatSystem()
        {
            var tooldefs = Def.GetDefs<ToolUseDef>();
            foreach (var def in tooldefs)
                if(def.Skill is not null)
                _skillsToTools[def.Skill] = def;

            var toolprofiles = Def.GetDefs<ToolProfileDef>();
            foreach (var profile in toolprofiles)
                if(profile.ToolUse is not null)
                _useToProfile[profile.ToolUse] = profile;

            var interactions = Def.GetDefs<InteractionDef>();
            foreach (var i in interactions)
                if (i.ToolUse is ToolUseDef use)
                {
                    _skillToInteraction[use.Skill] = i;
                    _toolToInteraction[i.ToolUse] = i;
                }
        }
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
    }
}
