using Start_a_Town_;

namespace Project1.Framework.Skills
{
    public class ToolUseDef(string name, string description, SkillDef skill) : Def(name)
    {
        public string Description { get; protected set; } = description;
        public SkillDef Skill = skill;
    }
}
