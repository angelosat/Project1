using System;
using System.Collections.Generic;
using Start_a_Town_.UI;

namespace Start_a_Town_
{
    public sealed class SkillDef(string name) : Def(name)
    {
        public string Description;
        public Icon Icon;
        public Func<Interaction> WorkFactory;
        public List<StatDef> AffectedStats = [];
        public ToolProfileDef RelevantTool;
        public ToolUseDef RelevantWorkType;
    }
}
