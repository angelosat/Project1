using System.Collections.Generic;
using System.Linq;

namespace Start_a_Town_
{
    public sealed class ToolProps : VariantProps, IItemDefVariator
    {
        public string Description;
        public ToolUseDef ToolUse;
        public HashSet<JobDef> AssociatedJobs = new();
        public Sprite SpriteHandle, SpriteHead;
        internal SkillDef Skill;

        public ToolProps(string name) : base(ItemDefOf.Tool, name)
        {
        }
        public ToolProps AssociateJob(params JobDef[] jobs)
        {
            foreach (var j in jobs)
                this.AssociatedJobs.Add(j);
            return this;
        }
        //protected override void ApplyTo(Entity item)
        //{
        //    item.ToolComponent.ToolProperties = this;
        //    item.Body.Sprite = this.SpriteHandle;
        //    item.Body[BoneDefOf.ToolHead].Sprite = this.SpriteHead;
        //}
        protected override Entity ApplyTo(Entity obj)
        {
            //var tool = ItemDefOf.Tool.CreateNew() as Tool;
            //tool.ToolComponent.ToolProperties = this;
            obj.Body.Sprite = this.SpriteHandle;
            obj.Body[BoneDefOf.ToolHead].Sprite = this.SpriteHead;
            obj.Name = this.Label;
            obj.ToolComponent.ToolUse = this.ToolUse;
            return obj;
        }

        public Entity Create(Dictionary<string, Entity> ingredients)
        {
            var tool = this.BaseDef.CreateNew();// Create() as Tool;
            tool.SetMaterials(ingredients.ToDictionary(i => i.Key, i => i.Value.PrimaryMaterial));
            return tool;
        }

        public StorageFilterNewNew GetFilter()
        {
            return new(this.Label, ItemDefOf.Tool, this);
        }
    }
}
