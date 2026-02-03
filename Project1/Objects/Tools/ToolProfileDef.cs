using Project1.Framework.Base;
using Project1.Framework.Skills;
using System.Collections.Generic;

namespace Start_a_Town_
{
    public class ToolProfileDef : Def, IItemDefVariator
    {
        //public string Description;
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
        //protected override void ApplyTo(Entity item)
        //{
        //    item.ToolComponent.ToolProperties = this;
        //    item.Body.Sprite = this.SpriteHandle;
        //    item.Body[BoneDefOf.ToolHead].Sprite = this.SpriteHead;
        //}
        //protected override Entity ApplyVariantTo(Entity obj)
        //{
        //    //var tool = ItemDefOf.Tool.CreateNew() as Tool;
        //    //tool.ToolComponent.ToolProperties = this;
        //    obj.Body.Sprite = this.SpriteHandle;
        //    obj.Body[BoneDefOf.ToolHead].Sprite = this.SpriteHead;
        //    obj.Name = this.Label;
        //    obj.ToolComponent.ToolUse = this.ToolUse;
        //    return obj;
        //}

        //public Entity Create(Dictionary<string, Entity> ingredients)
        //{
        //    var tool = this.BaseDef.CreateBase(this);
        //    tool.SetMaterials(ingredients.ToDictionary(i => i.Key, i => i.Value.PrimaryMaterial));
        //    return tool;
        //}

        public StorageFilterNewNew GetFilter()
        {
            return new(this.Label, ItemDefOf.Tool, this);
        }
    }
}
