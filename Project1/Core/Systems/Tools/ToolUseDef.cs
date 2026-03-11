using Project1.Core.Skills;

namespace Project1.Core.Systems.Tools
{
    public class ToolUseDef(string name, string description, SkillDef skill) : Def(name)
    {
        public string Description { get; protected set; } = description;
        public SkillDef Skill = skill;
    }
}
