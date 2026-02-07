using Project1.Core.Base;
using Project1.Core.Entities.Actors;
using Project1.Core.UI;
using SharpDX.Direct3D9;
using Project1.Core;
using Project1.Core.UI;
using Project1.Core.Entities;

namespace Project1.Core.UI
{
    internal class SkillsUINew : GroupBox, ISelectionBound
    {
        readonly ListBoxNoScroll GuiList;
        public ISelectable CurrentSelection { get; set; }
        public SkillsUINew()
        {
            this.GuiList = new();
        }

        public void OnBind(ISelectable selectable)
        {
            if (selectable is TargetArgs target &&
                target.Object is Actor actor)
            {
                Build(actor);
            }
        }

        private void Build(Actor actor)
        {
            this.ClearControls();
            this.GuiList.Clear();
            GuiList.AddItems(actor.Skills.All);
            this.AddControls(this.GuiList);
        }
    }
    internal class SkillsUI : GuiBuilder
    {
        public SkillsUI()
        {
            
        }
        public SkillsUI(Entity entity) : base(entity)
        {
            
        }
        protected override void Build()
        {
            var actor = this.Entity as Actor;
            this.AddControls(actor.Skills.NewGui());
        }

        protected override GuiBuilder BuildFor(Entity entity) => new SkillsUI(entity);
    }
}
