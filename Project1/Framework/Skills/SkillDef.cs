using System;
using System.Collections.Generic;
using Project1.Framework.Base;
using Project1.Framework.Interactions;
using Project1.Framework.Stats;
using Project1.Framework.Tools;
using Start_a_Town_.UI;

namespace Project1.Framework.Skills
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
