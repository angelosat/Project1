using Project1.Core.AI;
using Project1.Core.Base;
using Project1.Core.Entities.Actors;
using Project1.Core.Helpers;
using Project1.Core.UI;
using Project1.Core;
using Project1.Core.Components;
using Project1.Core.UI;
using Project1.Core.Entities;

namespace Project1.Core.Towns.Labors
{
    class NpcLogUINewNew : GroupBox, ISelectionBound
    {
        private readonly TableScrollableCompact<AILog.Entry> Table = new TableScrollableCompact<AILog.Entry>()
                    .AddColumn(null, "Time", (int)UIManager.Font.MeasureString("HH:mm:ss").X, (e) => new Label(e.Time.ToString("HH:mm:ss")), 0)
                    .AddColumn(null, "Description", 400, (e) => new GroupBox().AddControlsLineWrap(Label.ParseNew(e.Text)), 0);
        public ISelectable CurrentSelection { get; set; }

        public NpcLogUINewNew()
        {
            //this.AutoSize = false;
            var scrollbox = new ScrollableBoxNewNewNew(this.Table.TotalWidth, 300, ScrollModes.Vertical);
            scrollbox.AddControls(this.Table);
            this.AddControls(scrollbox.ToPanel());
            //this.Width = this.Table.TotalWidth;
            //this.Height = 300;
        }

        //public override void OnLayout(int availableWidth, int availableHeight)
        //{
        //    base.OnLayout(availableWidth, availableHeight);
        //    this.Height = 500;
        //    this.Width = this.Table.Width;
        //}
        public void OnBind(ISelectable selectable)
        {
            if (selectable is TargetArgs target &&
                target.Object is Actor actor)
            {
                this.Table.Bind(actor.AI.State.Log.Inner);
                //this.RemoveControls(this.Table);
                //this.AddControls(this.Table);
                //this.Layout(this.Table.Width, 500);
               
            }
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
