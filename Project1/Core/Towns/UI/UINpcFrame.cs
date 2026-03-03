using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Project1.Core.Entities.Actors;
using Project1.Core.Input;
using Project1.Core.Screens;
using Project1.Core.UI.Hud;
using Project1.Framework.Input;
using Project1.Framework.UI;
using System.Linq;

namespace Project1.Core.Towns.UI
{
    class UINpcFrame : ButtonBase
    {
        readonly GroupBox FrameContainer, Frame;
        readonly PictureBox Sprite;
        readonly Label Label;
        readonly Actor Npc;

        public UINpcFrame(Actor actor)
        {
            this.Active = true;
            this.MouseThrough = false;
            var padding = 8;// 5;
            this.AutoSize = true;
            this.FrameContainer = new GroupBox(64) 
            {
                MouseThrough = true, 
            };

            this.Frame = new GroupBox(this.FrameContainer.Width - padding - padding)
            {
                MouseThrough = true,
                Location = new(padding),
                BackgroundColorFunc = () => Color.Lerp(Color.Red * .5f, Color.Lime * .5f, actor.MoodValue / 100f) 
            };
            this.FrameContainer.AddControls(this.Frame);

            this.Sprite = new PictureBox(actor.Body.RenderIcon(actor, 1))
            {
                LocationFunc = () => this.FrameContainer.Center,
                Anchor = Vector2.One * .5f,
                MouseThrough = true
            };

            this.Npc = actor;
            this.Tag = actor;
            this.LeftClickAction = () =>
            {
                Ingame.Instance.Events.Post(new PlayerSelectionRectangleEvent(
                    [actor], 
                    InputState.IsKeyDown(System.Windows.Forms.Keys.LShiftKey) ? SelectionOp.Add : SelectionOp.Clear));
            };
            this.Label = new Label()
            {
                MouseThrough = true,
                LocationFunc = () =>
                new Vector2(this.FrameContainer.Width / 2f, this.FrameContainer.Height),
                Anchor = new Vector2(.5f, .5f),
                TextFunc = () => actor.Name.Split(' ').First(),
            };
            this.AddControls(
                this.FrameContainer,
                this.Sprite,
                this.Label
                );
        }
        public override void Draw(SpriteBatch sb, Rectangle viewport)
        {
            base.Draw(sb, viewport);
            if (SelectionManager.Instance.CurrentSelections.Contains(this.Npc))
                this.Frame.DrawHighlightBorder(sb);
        }
    }
}
