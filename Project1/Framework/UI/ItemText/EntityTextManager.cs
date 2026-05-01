using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Project1.Core.Entities;
using Project1.Core.Graphics;
using Project1.Core.Screens;
using Project1.Framework.Helpers;
using Project1.Framework.UI;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Project1.Core.UI;

class EntityTextManager : Control
{
    readonly Dictionary<GameObject, RenderTarget2D> StackSizes = [];
    readonly Queue<GameObject> ToValidate = new();
    readonly Vector2 LabelSize = UIManager.Font.MeasureString("000");
    public override void Update()
    {
        base.Update();
        if (ToValidate.Count != 0)
        {
            var gd = Game1.Instance.GraphicsDevice;
            while (ToValidate.Any())
            {
                var sb = new SpriteBatch(gd);

                var o = ToValidate.Dequeue();
                var rt = new RenderTarget2D(gd, (int)LabelSize.X, (int)LabelSize.Y);
                gd.Clear(Color.Red * .5f);
                sb.Begin();
                UIManager.DrawStringOutlined(sb, o.StackSize.ToString(), Vector2.Zero);
                sb.End();
                StackSizes[o] = rt;
            }
        }
    }
    internal static void Draw(GameObject parent, int stackSize)
    {
        throw new NotImplementedException();
    }
    internal static void DrawStackSize(SpriteBatch sb, MapViewport viewport, GameObject parent)
    {
        var border = 2;
        if (viewport.Camera.Zoom <= 1)
            return;
        if (parent.StackSize <= 1)
            return;
        var text = parent.StackSize.ToString();
        var pos = viewport.GetScreenPosition(parent.Global);
        var textSize = UIManager.Font.MeasureString(text) + Vector2.UnitX * (2 * border);
        var textbg = new Rectangle((int)(pos.X - textSize.X / 2), (int)pos.Y, (int)textSize.X, (int)textSize.Y);
        textbg.DrawHighlight(sb, Color.Black * .5f);
        UIManager.DrawStringOutlined(sb, text, new Vector2((int)(pos.X - textSize.X / 2 + border), (int)pos.Y));// + border));
    }
    //internal static void DrawStackSizeTest(SpriteBatch sb, MapViewport viewport, GameObject parent)
    //{
    //    var border = 2;
    //    var camera = viewport.Camera;
    //    if (camera.Zoom <= 1)
    //        return;
    //    if (parent.StackSize <= 1)
    //        return;
    //    var text = parent.StackSize.ToString();
    //    var pos = camera.GetScreenPosition(parent.Global);
    //    var textSize = UIManager.Font.MeasureString(text) + Vector2.UnitX * (2 * border);
    //    var textbg = new Rectangle((int)(pos.X - textSize.X / 2), (int)pos.Y, (int)textSize.X, (int)textSize.Y);
    //    textbg.DrawHighlight(sb, Color.Black * .5f);
    //    var global = parent.Global + new Vector3(1,1,0);
    //    var entityDepth = camera.GetDrawDepth(parent.Map, global);
    //    var near = viewport.GetNearDepth();
    //    var far = viewport.GetFarDepth();
    //    var depthRange = Math.Abs(near - far);
    //    var depth = 1-(entityDepth / depthRange);
    //    UIManager.DrawStringOutlined(sb, text, new Vector2(pos.X - textSize.X / 2 + border, pos.Y), depth: depth);
    //}
    public override void DrawWorld(MySpriteBatch sb, MapViewport viewport)
    {
        var camera = viewport.Camera;
        if (camera.Zoom <= 1)
            return;
       
        //var objects = Client.Instance.Map.Entities;
        var objects = Ingame.Net.MainViewport.Map.Entities;
        foreach (var o in objects)
        {
            if (o.StackSize <= 1)
                continue;
            if (!StackSizes.TryGetValue(o, out _))
            {
                ToValidate.Enqueue(o);
                continue;
            }
            var rt = StackSizes[o];
            var depth = camera.GetDrawDepth(o);
            sb.Draw(rt, Vector2.Zero, rt.Bounds, 0, Vector2.Zero, Vector2.One, Color.White, Color.White, Color.White, Color.White, Color.Transparent, SpriteEffects.None, depth); 
        }
    }
}
