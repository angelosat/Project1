using Project1.Core.Entities.Actors;
using Project1.Framework.Events;

namespace Project1.Core.Skills
{
    internal record struct SkillAdjustedEvent(Actor Actor, SkillRuntime Skill) : IEventPayload { }
    internal record struct SkillLevelUpEvent(Actor Actor, SkillRuntime Skill) : IEventPayload { }

}