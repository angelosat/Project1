using System;
using Start_a_Town_.UI;

namespace Start_a_Town_
{
    public class ContextAction
    {
        public Func<string> Name;
        public Func<bool> Action;
        public Func<bool> Available = () => true;
        public Action<ContextAction, Button> ControlInit = (act, btn) => { };

        public ContextAction(string name, Func<bool> action)
        {
            this.Name = () => name;
            this.Action = action;
        }
        public ContextAction(Func<string> name, Func<bool> action)
        {
            this.Name = name;
            this.Action = action;
        }
        public ContextAction(Func<string> name, Action action)
        {
            this.Name = name;
            this.Action = () => { action(); return true; };
        }
        public ContextAction(string name, Action action)
        {
            this.Name = () => name;
            this.Action = () => { action(); return true; };
        }
        public ContextAction((string name, Action action) args)
        {
            this.Name = () => args.name;
            this.Action = () => { args.action(); return true; };
        }
        public ContextAction(Interaction interaction)
        {
            this.Name = () => interaction.Name;
        }
    }
}
