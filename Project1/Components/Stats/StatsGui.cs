using Start_a_Town_.Components;
using Start_a_Town_.UI;

namespace Start_a_Town_
{
    class StatsGui : GroupBox
    {
        readonly PanelLabeledNew PanelAttributes;
        readonly PanelLabeledNew PanelStats;
        public StatsGui()
        {
            this.Name = "Stats";
            this.PanelAttributes = new PanelLabeledNew("Attributes") { AutoSize = true };
            this.PanelStats = new PanelLabeledNew("Stats") { AutoSize = true };
        }
        
        public Control Refresh(Actor actor)
        {
            var comp = actor.GetComponent<StatsComponent>();
            this.ClearControls();
          
            this.PanelAttributes.Client.ClearControls();
            PanelAttributes.Client.AddControls(actor.Attributes.GetGui());
            this.AddControlsTopRight(this.PanelAttributes);

            this.PanelStats.Client.ClearControls();
            comp.GetInterface(actor, this.PanelStats.Client);
            this.AddControlsBottomLeft(this.PanelStats);
            this.Validate(true);
            return this;
        }
        static StatsGui Instance;
        internal static Window GetGui(Actor actor)
        {
            Window window;

            if (Instance is null)
            {
                Instance = new StatsGui();
                window = new Window(Instance) { Movable = true, Closable = true };
            }
            else
                window = Instance.GetWindow();
            Instance.Tag = actor;
            window.Title = $"{actor.Name} stats";
            Instance.Refresh(actor);
            return window;
        }
        internal override void OnSelectedTargetChanged(TargetArgs target)
        {
            this.Refresh(target.Object as Actor);
        }
    }
}
