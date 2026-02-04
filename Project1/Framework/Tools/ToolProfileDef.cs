using Project1.Core.Entities;
using Project1.Core.Towns;
using Project1.Framework.Base;
using Project1.Framework.Legacy;
using Project1.Framework.Skills;
using Project1.Framework.Stats;
using Start_a_Town_;
using System.Collections.Generic;

namespace Project1.Framework.Tools
{
    public class ToolProfileDef : Def, IItemDefVariator
    {
        public ToolUseDef ToolUse;
        public DamageDef Damage;
        public HashSet<JobDef> AssociatedJobs = new();
        public Sprite SpriteHandle, SpriteHead;
        public SkillDef Skill;
        public string Description;

        public ToolProfileDef(string name) : base(name)
        {
        }
        public ToolProfileDef AssociateJob(params JobDef[] jobs)
        {
            foreach (var j in jobs)
                this.AssociatedJobs.Add(j);
            return this;
        }
        public StorageFilterNewNew GetFilter()
        {
            return new(this.Label, ItemDefOf.Tool, this);
        }
    }
}
