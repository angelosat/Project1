using System;
using System.Collections.Generic;
using Project1.Core.Entities.Stats;
using Project1.Core.Base;
using Project1.Core.Interactions;
using Project1.Core.Tools;
using Project1.Core.UI;

namespace Project1.Core.Skills
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
