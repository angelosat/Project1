using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Start_a_Town_.UI;
using System.Linq;

namespace Start_a_Town_
{
    class UINpcFrame : ButtonBase
    {
        readonly GroupBox FrameContainer, Frame;
        readonly PictureBox Sprite;
        readonly Label Label;
        readonly GameObject Npc;

        public UINpcFrame(Actor actor)
        {
            this.Active = true;
            this.MouseThrough = false;
            var padding = 8;// 5;
            this.AutoSize = true;
            this.FrameContainer = new GroupBox(64) 
            {
                //Location = new(padding),
                MouseThrough = true, 
                //BackgroundColorFunc = () => Color.Lerp(Color.Red * .5f, Color.Lime * .5f, actor.MoodValue / 100f) 
            };

            this.Frame = new GroupBox(this.FrameContainer.Width - padding - padding)
            {
                MouseThrough = true,
                Location = new(padding),
                BackgroundColorFunc = () => Color.Lerp(Color.Red * .5f, Color.Lime * .5f, actor.MoodValue / 100f) 
            };
            this.FrameContainer.AddControls(this.Frame);

            this.Sprite = new PictureBox(actor.Body.RenderIcon(actor, 1)) { LocationFunc = () => this.FrameContainer.Center, Anchor = Vector2.One * .5f };
            this.Sprite.MouseThrough = true;

            this.Npc = actor;
            this.Tag = actor;
            this.LeftClickAction = () =>
            {
                if (InputState.IsKeyDown(System.Windows.Forms.Keys.LShiftKey))
                    SelectionManager.AddToSelection(new TargetArgs(actor));
                else
                    SelectionManager.Select(new TargetArgs(actor));
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
            if (SelectionManager.IsSelected(this.Npc))
                this.Frame.DrawHighlightBorder(sb);
        }
    }

    class UINpcFrameOld : ButtonBase
    {
        readonly GroupBox PictureBoxBox;
        readonly PictureBox Sprite;
        readonly Label Label;
        readonly GameObject Npc;
        public UINpcFrameOld(Actor actor)
        {
            this.MouseThrough = false;
            var padding = 5;
            this.AutoSize = true;
            this.PictureBoxBox = new GroupBox() { AutoSize = false };

            this.Sprite = new PictureBox(actor.Body.RenderIcon(actor, 1)) { LocationFunc = () => this.PictureBoxBox.Center, Anchor = Vector2.One * .5f };
            this.Sprite.MouseThrough = true;

            this.PictureBoxBox.BackgroundColorFunc = () => Color.Lerp(Color.Red * .5f, Color.Lime * .5f, actor.MoodValue / 100f);
            this.PictureBoxBox.Size = new Rectangle(0, 0, this.Sprite.Width * 2 + padding, this.Sprite.Width * 2 + padding);
            this.PictureBoxBox.AddControls(this.Sprite);

            this.Npc = actor;
            this.LeftClickAction = () =>
            {
                if (InputState.IsKeyDown(System.Windows.Forms.Keys.LShiftKey))
                    SelectionManager.AddToSelection(new TargetArgs(actor));
                else
                    SelectionManager.Select(new TargetArgs(actor));
            };
            this.Label = new Label()
            {
                LocationFunc = () =>
                new Vector2(this.PictureBoxBox.Width / 2f, this.PictureBoxBox.Height),
                Anchor = new Vector2(.5f, .5f),
                TextFunc = () => actor.Name.Split(' ').First(),
            };
            this.AddControls(
                this.PictureBoxBox
                , this.Label
                );
        }
        public override void Draw(SpriteBatch sb, Rectangle viewport)
        {
            base.Draw(sb, viewport);
            if (SelectionManager.IsSelected(this.Npc))
                this.BoundsScreen.DrawHighlightBorder(sb, .5f, 1);
        }
    }
}
