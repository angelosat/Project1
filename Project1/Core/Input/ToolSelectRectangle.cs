using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Project1.Core.Entities;
using Project1.Core.Networking;
using Project1.Core.Screens;
using Project1.Core.UI.Hud;
using Project1.Framework.Helpers;
using Project1.Framework.UI;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace Project1.Core.Input;

class ToolSelectRectangle : ControlTool
{
    protected Vector2 Begin;
    protected Rectangle Selection;
    List<Entity> CurrentSelected;
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
        if (this.CurrentSelected is not null)
            Ingame.Instance.Events.Post(new PlayerSelectionRectangleEvent(this.CurrentSelected.Cast<Entity>(), SelectionHelper.GetSelectionOp()));
    }
    public override void Update()
    {
        this.Selection = this.Begin.GetRectangle(UIManager.Mouse);
        this.CurrentSelected = Ingame.Instance.Scene.ObjectsDrawn.Where(o => o.GetScreenBounds(Ingame.MainViewport).Intersects(this.Selection)).ToList();
    }

    public override ControlTool.Messages MouseLeftUp(HandledMouseEventArgs e)
    {
        this.Select();
        return Messages.Remove;
    }
    internal override void DrawUI(SpriteBatch sb, MapViewport viewport)
    {
        this.Selection.DrawHighlight(sb);
        if (this.CurrentSelected != null)
            foreach (var obj in this.CurrentSelected)
                obj.DrawBorder(sb, viewport);
    }

    internal override ControlTool Read(PlayerData player)
    {
        this.Begin = player.MousePosition;
        return base.Read(player);
    }
    internal override void DrawUIRemote(SpriteBatch sb, RenderContext ctx, Vector2 vector2, InteractionTarget targetArgs, PlayerData player)
    {
        PlayerData.GetMousePosition(player.CameraPosition, this.Begin, player.CameraZoom, ctx).GetRectangle(vector2).DrawHighlight(sb, Color.Yellow*.5f);
    }
}