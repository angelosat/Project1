using System;
using System.Collections.Generic;

namespace Start_a_Town_.UI
{
    public class ContextActionBar : GroupBox
    {
        public class ContextActionBarAction
        {
            public Icon Icon;
            public string Text;
            public Action Action;
            public ContextActionBarAction(Action action, Icon icon = null, string text = "")
            {
                this.Action = action;
                this.Text = text;
                this.Icon = icon;
            }
        }
        public class ContextActionBarArgs
        {
            public TargetArgs Target;
            public List<ContextActionBarAction> Actions = new List<ContextActionBarAction>();
            public Action<Button> ControlInit = btn => { };
            public object[] Parameters;
            public ContextActionBarArgs(TargetArgs target)
            {
                this.Target = target;
            }
        }

        static ContextActionBar _Instance;
        public static ContextActionBar Instance
        {
            get
            {
                if (_Instance is null)
                    _Instance = new ContextActionBar();
                return _Instance;
            }
        }

        static public void Create(TargetArgs t)
        {
            var args = new ContextActionBar.ContextActionBarArgs(t);
            foreach (var comp in Game1.Instance.GameComponents)
                comp.OnContextActionBarCreated(args);
            Instance.Initialize(args);
        }
       
        public void Initialize(ContextActionBarArgs args)
        {
            this.ClearControls();
            foreach(var a in args.Actions)
            {
                var btn = new IconButton();
                btn.BackgroundTexture = UIManager.DefaultIconButtonSprite;
                btn.Icon = a.Icon;
                btn.HoverFunc = () => a.Text;
                btn.LeftClickAction = a.Action;
                this.AddControls(btn);
            }
            this.AlignLeftToRight();
            this.Location = UIManager.Mouse;
            this.Show();
        }
    }
}
