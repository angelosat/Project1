using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;

namespace Start_a_Town_.UI
{
    public class Nameplate : GroupBox, IContextable
    {
        const int VertOffset = 10;
        public bool AlwaysShow;

        static readonly Dictionary<INameplateable, Nameplate> Plates = new();

        public static void Reset()
        {
            ScreenManager.CurrentScreen.WindowManager[UIManager.LayerNameplates].Clear();
        }

        public static Nameplate Create(INameplateable obj)
        {
            var plate = new Nameplate(obj);
            //if (obj is GameObject)
            //    obj.OnNameplateCreated(plate);
            //else
                obj.OnNameplateCreated(plate);
            plate.AlignVertically(Alignment.Horizontal.Center);
            plate.Tag = obj;
            plate.MouseThrough = false; //hmmm
            return plate;
        }

        public void OnFocus()
        {
            this.Show();
        }
        public void OnFocusLost()
        {
            if (this.AlwaysShow)
                return;

            this.Hide();
        }

        public static void Show(GameObject entity)
        {
            GetNameplate(entity)?.OnFocus();
        }
        public static void Hide(GameObject entity)
        {
            GetNameplate(entity)?.OnFocusLost();
        }
        public INameplateable Object;
        Nameplate(INameplateable obj)
        {
            this.Layer = UIManager.LayerNameplates;
            this.Object = obj;
            this.Active = true;
            this.LocationFunc = () =>
            {
                var camera = Ingame.Instance.Camera;
                var rect = this.Object.GetScreenBounds(camera);
                return new Vector2(rect.X + rect.Width / 2, rect.Y - this.Height - VertOffset) - this.Dimensions * new Vector2(.5f, 0);
            };
        }

        public override void OnBeforeDraw(SpriteBatch sb, Rectangle viewport)
        {
            var obj = this.Tag as GameObject;
            var actor = obj.Net.GetPlayer()?.ControllingEntity;
            if (actor is null)
                return;
            if (this.Object is GameObject entity &&
                actor.AttackTarget is GameObject playertarget &&
                entity.RefID == playertarget.RefID)
            {
                var border = this.BoundsScreen;
                border.Inflate(1, 1);
                border.DrawHighlight(sb, Color.Red * 0.5f, Vector2.Zero, 0);
            }
        }
        /// <summary>
        /// need to override this because if i'm drawing nameplates indepentendly of ui scale, i need to re-adjust the positions in the check
        /// </summary>
        /// <param name="viewport"></param>
        /// <returns></returns>
        public override bool HitTest(Rectangle viewport)
        {
            if (this.IsMouseThrough)
                return false;
            var scissor = Rectangle.Intersect(viewport, new((int)(this.BoundsScreen.X / UIManager.Scale), (int)(this.BoundsScreen.Y / UIManager.Scale), (int)( this.BoundsScreen.Width / UIManager.Scale), (int)(this.BoundsScreen.Height / UIManager.Scale)));
            return scissor.Intersects(UIManager.MouseRect);
        }
        public override void OnHitTestPass()
        {
            if (!ToolManager.CurrentTool.TargetOnlyBlocks)
                Controller.Instance.MouseoverNext.Object = this.Object;
        }

        public override void OnMouseEnter()
        {
            this.BackgroundColor = this.Object.GetNameplateColor() * 0.5f;
            base.OnMouseEnter();
        }

        public override void OnMouseLeave()
        {
            this.BackgroundColor = Color.Black * 0.5f;
            base.OnMouseLeave();
        }


        public static Nameplate GetNameplate(INameplateable entity)
        {
            return Plates.GetValueOrDefault(entity);
        }
        static bool Collision2(Nameplate b1, Nameplate toMove, Camera camera)
        {
            if (b1 == toMove)
                return false;
            if (!b1.BoundsScreen.Intersects(toMove.BoundsScreen))
                return false;

            var plateCenter = new Vector2(b1.BoundsScreen.Center.X, b1.BoundsScreen.Center.Y);
            var toMoveCenter = new Vector2(toMove.BoundsScreen.Center.X, toMove.BoundsScreen.Center.Y);
            var d = toMoveCenter - plateCenter;

            if (Math.Abs(d.X) > Math.Abs(d.Y)) // offset on x axis
            {
                if (d.X > 0)
                    toMove.Location.X = b1.Location.X + b1.BoundsScreen.Width + 1;
                else
                    toMove.Location.X = b1.Location.X - toMove.BoundsScreen.Width - 1;
            }
            else // offset on y axis
            {
                if (d.Y < 0)
                    toMove.Location.Y = b1.Location.Y - toMove.BoundsScreen.Height - 1;
                else
                    toMove.Location.Y = b1.Location.Y + b1.BoundsScreen.Height + 1;
            }
            return true;
        }

        public override void GetTooltipInfo(Control tooltip)
        {
            if (this.Object is not ITooltippable tooltippable)
                return;
            tooltippable.GetTooltipInfo(tooltip);
            tooltip.Tag = this.Object;
        }

        public void GetContextActions(GameObject playerEntity, ContextArgs a)
        {
            if (this.Object is not IContextable contextable)
                return;
            contextable.GetContextActions(playerEntity, a);
        }

        public override Control Invalidate(bool invalidateChildren = false)
        {
            return base.Invalidate(invalidateChildren);
        }

        public override void Draw(SpriteBatch sb, Rectangle viewport)
        {
        }
        public override void DrawOnCamera(SpriteBatch sb, Camera camera)
        {
            base.Draw(sb, camera.ViewPort);
            if (SelectionManager.IsSelected(this.Object as GameObject))
                this.BoundsScreen.DrawHighlightBorder(sb, thickness: 2, padding: 2);
        }
    }
}
