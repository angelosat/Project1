using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Start_a_Town_.Net;
using Start_a_Town_.UI;

namespace Start_a_Town_
{
    class ToolSelectRectangle : ControlTool
    {
        protected Vector2 Begin;
        protected Rectangle Selection;
        List<GameObject> CurrentSelected;
        public ToolSelectRectangle()
        {

        }
        public ToolSelectRectangle(Vector2 begin)
        {
            this.Begin = begin;
            this.Selection = this.Begin.GetRectangle(this.Begin);
        }

        protected virtual void Select()
        {
            if (this.CurrentSelected != null)
            {
                if (InputState.IsKeyDown(System.Windows.Forms.Keys.LShiftKey))
                    SelectionManager.AddToSelection(this.CurrentSelected);
                else
                    SelectionManager.Select(this.CurrentSelected);
            }
        }
        public override void Update()
        {
            this.Selection = this.Begin.GetRectangle(UIManager.Mouse);
            var cam = Ingame.CurrentMap.Camera;
            this.CurrentSelected = Ingame.Instance.Scene.ObjectsDrawn.Where(o => o.GetScreenBounds(cam).Intersects(this.Selection)).ToList();
        }

        public override ControlTool.Messages MouseLeftUp(System.Windows.Forms.HandledMouseEventArgs e)
        {
            this.Select();
            return Messages.Remove;
        }
        internal override void DrawUI(Microsoft.Xna.Framework.Graphics.SpriteBatch sb, Camera camera)
        {
            this.Selection.DrawHighlight(sb);
            if (this.CurrentSelected != null)
                foreach (var obj in this.CurrentSelected)
                    obj.DrawBorder(sb, camera);
        }

        internal override ControlTool Read(PlayerData player)
        {
            this.Begin = player.MousePosition;
            return base.Read(player);
        }
        internal override void DrawUIRemote(Microsoft.Xna.Framework.Graphics.SpriteBatch sb, Camera camera, Vector2 vector2, TargetArgs targetArgs, Net.PlayerData player)
        {
            PlayerData.GetMousePosition(player.CameraPosition, this.Begin, player.CameraZoom, camera).GetRectangle(vector2).DrawHighlight(sb, Color.Yellow*.5f);
        }
    }
}