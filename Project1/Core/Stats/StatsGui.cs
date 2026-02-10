using Project1.Core.Entities.Actors;
using Project1.Core.UI;
using Project1.Framework.UI;

namespace Project1.Core.Entities.Stats
{
    class StatsGuiNew : GroupBox, ISelectionBound
    {
        PanelLabeledNew PanelAttributes;
        PanelLabeledNew PanelStats;

        public ISelectable CurrentSelection { get; set; }

        public void OnBind(ISelectable selectable)
        {
            if (selectable is TargetArgs target && target.Object is Actor actor)
                this.Build(actor);
        }

        protected void Build(Actor actor)
        {
            this.Name = "Stats";

            this.PanelAttributes = new PanelLabeledNew("Attributes") { AutoSize = true };
            this.PanelStats = new PanelLabeledNew("Stats") { AutoSize = true };
            var comp = actor.GetComponent<StatsComponent>();
            this.ClearControls();

            this.PanelAttributes.Client.ClearControls();
            PanelAttributes.Client.AddControls(actor.Attributes.GetGui());
            this.AddControlsTopRight(this.PanelAttributes);

            this.PanelStats.Client.ClearControls();
            comp.GetInterface(actor, this.PanelStats.Client);
            this.AddControlsBottomLeft(this.PanelStats);
        }
    }
    class StatsGui : GuiBuilder
    {
        PanelLabeledNew PanelAttributes;
        PanelLabeledNew PanelStats;
        public StatsGui()
        {
           
        }
        public StatsGui(Entity entity) : base(entity)
        {
                
        }
        
        protected override void Build()
        {
            this.Name = "Stats";

            this.PanelAttributes = new PanelLabeledNew("Attributes") { AutoSize = true };
            this.PanelStats = new PanelLabeledNew("Stats") { AutoSize = true };
            var actor = this.Entity as Actor;
            var comp = actor.GetComponent<StatsComponent>();
            this.ClearControls();

            this.PanelAttributes.Client.ClearControls();
            PanelAttributes.Client.AddControls(actor.Attributes.GetGui());
            this.AddControlsTopRight(this.PanelAttributes);

            this.PanelStats.Client.ClearControls();
            comp.GetInterface(actor, this.PanelStats.Client);
            this.AddControlsBottomLeft(this.PanelStats);
        }

        protected override GuiBuilder BuildFor(Entity entity) => new StatsGui(entity);
    }
    
}
