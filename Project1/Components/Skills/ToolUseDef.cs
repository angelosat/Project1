namespace Start_a_Town_
{
    public class ToolUseDef(string name, string description, SkillDef skill) : Def(name)
    {
        public string Description { get; protected set; } = description;
        public SkillDef Skill = skill;
    }
}
