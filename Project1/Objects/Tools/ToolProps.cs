using System.Collections.Generic;
using System.Linq;

namespace Start_a_Town_
{
    public class ToolProps : Def, IItemDefVariator
    {
        public string Description;
        public ToolUseDef ToolUse;
        public HashSet<JobDef> AssociatedJobs = new();
        public Sprite SpriteHandle, SpriteHead;
        internal SkillDef Skill;

        public ToolProps(string name) : base(name)
        {
        }
        public ToolProps AssociateJob(params JobDef[] jobs)
        {
            foreach (var j in jobs)
                this.AssociatedJobs.Add(j);
            return this;
        }

        public GameObject Create()
        {
            var tool = ToolDefs.Tool.CreateNew() as Tool;
            tool.ToolComponent.Props = this;
            tool.Body.Sprite = this.SpriteHandle;
            tool.Body[BoneDefOf.ToolHead].Sprite = this.SpriteHead;
            tool.Name = this.Label;
            return tool;
        }
        public Entity Create(Dictionary<string, Entity> ingredients)
        {
            var tool = Create() as Tool;
            tool.SetMaterials(ingredients.ToDictionary(i => i.Key, i => i.Value.PrimaryMaterial));
            return tool;
        }

        public StorageFilterNewNew GetFilter()
        {
            return new(this.Label, ToolDefs.Tool, this);
        }
    }
}
