using System;
using Microsoft.Xna.Framework.Graphics;
using Start_a_Town_.UI;

namespace Start_a_Town_
{
    public enum DragDropEffects { None = 0, Copy = 1, Move = 2, Link = 4 }

    class DragDropManager : InputHandler, IKeyEventHandler
    {
        public DragEventArgs Action;
            
        static DragDropManager _instance;
        public static DragDropManager Instance => _instance ??= new DragDropManager();
       
        public Control Control;
        Action CreateAction;

        public void Update()
        {
            if(this.Control is not null)
            {
                if(!this.Control.HitTest())
                {
                    this.Control = null;
                    this.CreateAction();
                }
            }
        }

        static public void Start(Control control, Action createAction)
        {
            Instance.Control = control;
            Instance.CreateAction = createAction;
        }

        DragDropManager() 
        {
        }

        public override void HandleRButtonDown(System.Windows.Forms.HandledMouseEventArgs e)
        {
            if (this.Clear())
                e.Handled = true;
        }
        public override void HandleLButtonDown(System.Windows.Forms.HandledMouseEventArgs e)
        {
            if (this.Action is null)
                return;
            if (Controller.Instance.Mouseover.Object is not IDropTarget target)
                return;

            e.Handled = true;
            DragDropEffects result = target.Drop(Action);

            switch (result)
            {
                case DragDropEffects.None:
                    break;
                case DragDropEffects.Move:
                    this.Action = null;
                    break;
                    // TODO: put a bool to check wether the dragdrop stops after dropping
                case DragDropEffects.Link:
                    this.Action = null;
                    break;

            }
        }
        public override void HandleLButtonUp(System.Windows.Forms.HandledMouseEventArgs e)
        {
            this.Control = null;
            base.HandleLButtonUp(e);
        }
      
        static public void Create(DragEventArgs action)
        {
            Instance.Action = action;
        }

        public void Draw(SpriteBatch sb)
        {
            Action?.Draw(sb);
        }

        public bool Clear()
        {
            if (this.Action is null)
                return false;
            this.Action = null;
            return true;
        }
    }
}
