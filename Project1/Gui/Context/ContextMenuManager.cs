using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Start_a_Town_.UI;

namespace Start_a_Town_
{
    public class ContextArgs : EventArgs
    {
        public List<ContextAction> Actions;
        public object[] Parameters;

        public ContextArgs()
        {
            this.Actions = new List<ContextAction>();
        }
        public ContextArgs(params ContextAction[] actions)
        {
            this.Actions = new List<ContextAction>(actions);
        }
    }
  
    public interface IContextable
    {
        void GetContextActions(GameObject playerEntity, ContextArgs a);
    }

    class ContextMenuManager : InputHandler, IKeyEventHandler
    {
        static ContextMenuManager _Instance;
        public static ContextMenuManager Instance => _Instance ??= new ContextMenuManager();
        static IContextable Object;
        static float DelayInterval = Ticks.PerSecond / 2f;
        static float Delay;

        static public void PopUp(params (string, Action)[] a)
        {
            PopUp(a.Select(i => new ContextAction(i)).ToArray());
        }
        static public void PopUp(params ContextAction[] a)
        {
            ContextMenu2.Instance.Initialize(new ContextArgs(a));
        }

        public override void HandleRButtonDown(System.Windows.Forms.HandledMouseEventArgs e)
        {
            if (ContextMenu2.Instance.Hide())
            {
                e.Handled = true;
                return;
            }
            Object = Controller.Instance.Mouseover.Object as IContextable;
            Delay = DelayInterval;
        }

        public override void HandleRButtonUp(System.Windows.Forms.HandledMouseEventArgs e)
        {
            Object = null;
        }

        public static Window CreateContextSubMenu(string title, IEnumerable<(string, Action)> items)
        {
            var box = new ListBoxNoScroll<(string, Action), Button>(createButton, 0).AddItems(items);
            box.BackgroundColor = Microsoft.Xna.Framework.Color.Black * .5f;
            var win = box.ToWindow(title);//.Transparent();
            win.Location = Controller.Instance.MouseLocation;
            return win;

            static Button createButton((string label, Action action) item)
            {
                var btn = new Button(item.label, item.action, 96);
                //btn.IsToggledFunc = () => ToolManager.Instance.ActiveTool is ToolDigging tool && btn.Tag == tool.DesignationDef;
                return btn;
            }
        }
    }
}
