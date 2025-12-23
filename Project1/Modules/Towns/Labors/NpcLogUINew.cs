using Start_a_Town_.Components;
using Start_a_Town_.UI;

namespace Start_a_Town_.AI
{
    class NpcLogUINewNew : GroupBox, ISelectionBound
    {
        private readonly TableScrollableCompact<AILog.Entry> Table = new TableScrollableCompact<AILog.Entry>()
                    .AddColumn(null, "Time", (int)UIManager.Font.MeasureString("HH:mm:ss").X, (e) => new Label(e.Time.ToString("HH:mm:ss")), 0)
                    .AddColumn(null, "Description", 400, (e) => new GroupBox().AddControlsLineWrap(Label.ParseNew(e.Text)), 0);
        public ISelectable CurrentSelection { get; set; }

        public NpcLogUINewNew()
        {
            this.AddControls(this.Table);
        }

        public void OnBind(ISelectable selectable)
        {
            if (selectable is TargetArgs target &&
                target.Object is Actor actor)
                this.Table.Bind(actor.AI.State.History.Inner);
        }
    }
    class NpcLogUINew : GuiBuilder
    {
        //Actor Agent;
        public NpcLogUINew() : base()
        {
            this.Name = "History";
        }
        public NpcLogUINew(Entity entity) : base(entity)
        {
        }
        
        internal override void OnGameEvent(GameEvent e)
        {
            switch ((Components.Message.Types)e.Type)
            {
                case Message.Types.AILogUpdated:
                    throw new System.Exception();
                    //this.Refresh(this.Entity);
                    break;

                default:
                    break;
            }
        }


        protected override void Build()
        {
            //this.Agent = agent;
            this.Controls.Clear();

            var table = AILog.UI.GetGUI(this.Entity as Actor);

            this.Controls.Add(table);
        }

        protected override GuiBuilder BuildFor(Entity entity) => new NpcLogUINew(entity);
    }
}
