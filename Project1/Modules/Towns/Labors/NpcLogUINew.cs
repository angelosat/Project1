using Start_a_Town_.Components;
using Start_a_Town_.UI;

namespace Start_a_Town_.AI
{
    class NpcLogUINew : GroupBox
    {
        Actor Agent;
        public NpcLogUINew()
        {
            this.Name = "History";
        }
        public NpcLogUINew(Actor agent)
        {
            this.Agent = agent;
            Refresh(agent);
        }
        public new void Refresh()
        {
            this.Refresh(this.Agent);
        }
        public void Refresh(Actor agent)
        {
            this.Agent = agent;
            this.Controls.Clear();

            var table = AILog.UI.GetGUI(this.Agent);

            this.Controls.Add(table);
            this.Validate(true);
        }
        
        internal override void OnGameEvent(GameEvent e)
        {
            switch ((Components.Message.Types)e.Type)
            {
                case Message.Types.AILogUpdated:
                    this.Refresh(this.Agent);
                    break;

                default:
                    break;
            }
        }

        static NpcLogUINew Instance;
        internal static Window GetGui(Actor actor)
        {
            Window window;

            if (Instance == null)
            {
                Instance = new NpcLogUINew();
                window = new Window(Instance) { Movable = true, Closable = true };
            }
            else
                window = Instance.GetWindow();
            Instance.Tag = actor;
            window.Title = string.Format("{0} log", actor.Name);
            Instance.Refresh(actor);
            return window;
        }
        internal override void OnSelectedTargetChanged(TargetArgs target)
        {
            var actor = target.Object as Actor;
            if (!actor?.Equals(this.Tag) ?? false)
            {
                GetGui(actor);
            }
        }
    }
}
