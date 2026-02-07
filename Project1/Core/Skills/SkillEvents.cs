using Project1.Core.Base;
using Project1.Core.Entities.Actors;
using Project1.Core.Skills;

namespace Project1.Core.Skills
{
    internal record struct SkillAdjustedEvent(Actor Actor, Skill Skill) : IEventPayload { }
    internal record struct SkillLevelUpEvent(Actor Actor, Skill Skill) : IEventPayload { }

}