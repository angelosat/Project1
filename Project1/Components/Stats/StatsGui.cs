using Start_a_Town_.Components;
using Start_a_Town_.UI;

namespace Start_a_Town_
{
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
