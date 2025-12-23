using Start_a_Town_.Components;
using Start_a_Town_.UI;

namespace Start_a_Town_.AI
{
    class NpcLogUINew : GuiBuilder
    {
        //Actor Agent;
        public NpcLogUINew() : base()
        {
            this.Name = "History";
        }
        public NpcLogUINew(Entity entity) : base(entity)
        {
            //this.Agent = entity;
            //Refresh(entity
        }
        //public new void Refresh()
        //{
        //    this.Refresh(this.Agent);
        //}
        //public void Refresh(Actor agent)
        //{
        //    this.Agent = agent;
        //    this.Controls.Clear();

        //    var table = AILog.UI.GetGUI(this.Agent);

        //    this.Controls.Add(table);
        //    this.Validate(true);
        //}

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
