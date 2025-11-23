using System;
using Start_a_Town_.UI;

namespace Start_a_Town_
{
    public sealed class SkillDef : Def
    {
        public string Description;
        public Icon Icon;
        public Func<Interaction> WorkFactory;
        public SkillDef(string name) : base(name)
        {
        }
    }
}
