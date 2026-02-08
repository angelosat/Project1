using Project1.Core.Entities;
using Project1.Core.Entities.Stats;
using Project1.Core.Towns;
using Project1.Core.Base;
using Project1.Core.Graphics;
using Project1.Core.Legacy;
using Project1.Core.Legacy.Storage.New;
using Project1.Core.Skills;
using System.Collections.Generic;

namespace Project1.Core.Tools
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
            return new(this.LabelReadable, ItemDefOf.Tool, this);
        }
    }
}
