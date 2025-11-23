using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Start_a_Town_.UI;

namespace Start_a_Town_
{
    class CameraWidget : GroupBox
    {
        readonly GroupBox BoxRotation = new();
        readonly GroupBox BoxZoom = new();
        readonly IconButton BtnSettings;
        readonly UICameraSettings Widget;
        public CameraWidget(Camera camera)
        {
            this.BackgroundColorFunc = () => Color.Black * .5f;
            //IconButton
            //    btn_rotleft = new('↺', camera.RotateCounterClockwise) { HoverText = "Rotate camera counterclockwise" },
            //    btn_rotright = new('↻', camera.RotateClockwise) { HoverText = "Rotate camera clockwise" },
            //    btn_rotreset = new('↶', camera.RotationReset) { HoverText = "Reset rotation" },
            //    btn_zoomin = new('+', camera.ZoomIncrease) { HoverText = "Zoom in" },
            //    btn_zoomreset = new('●', camera.ZoomReset) { HoverText = "Reset zoom" },
            //    btn_zoomout = new('-', camera.ZoomDecrease) { HoverText = "Zoom out" };


            var btn_rotleft = ButtonNew.CreateMedium("↺", camera.RotateCounterClockwise);
            btn_rotleft.HoverText = "Rotate camera counterclockwise";

            var btn_rotright = ButtonNew.CreateMedium("↻", camera.RotateClockwise);
            btn_rotright.HoverText = "Rotate camera clockwise";

            var btn_rotreset = ButtonNew.CreateMedium("↶", camera.RotationReset);
            btn_rotreset.HoverText = "Reset rotation";

            var btn_zoomin = ButtonNew.CreateMedium("+", camera.ZoomIncrease);
            btn_zoomin.HoverText = "Zoom in";

            var btn_zoomreset = ButtonNew.CreateMedium("●", camera.ZoomReset);
            btn_zoomreset.HoverText = "Reset zoom";

            var btn_zoomout = ButtonNew.CreateMedium("-", camera.ZoomDecrease);
            btn_zoomout.HoverText = "Zoom out";

            //↶⊘
            this.BoxRotation.AddControlsHorizontally(1, btn_rotleft, btn_rotreset, btn_rotright);
            this.BoxZoom.AddControlsHorizontally(1, btn_zoomin, btn_zoomreset, btn_zoomout);
            this.AddControlsVertically(1, this.BoxRotation, this.BoxZoom);

            this.Widget = new UICameraSettings(camera);
            this.BtnSettings = new IconButton(Icon.ArrowDown)
            {
                LocationFunc = () => this.BoxZoom.BottomRight,
                Anchor = Vector2.UnitX,
                BackgroundTexture = UIManager.Icon16Background,
                LeftClickAction = OpenCameraSettings
            };
            this.AddControls(this.BtnSettings);
        }

        public void OpenCameraSettings()
        {
            this.Widget.Location = this.BtnSettings.ScreenLocation + this.BtnSettings.Dimensions;
            this.Widget.Toggle();
        }
        public override void Draw(SpriteBatch sb, Rectangle viewport)
        {
            base.Draw(sb, viewport);
        }
    }
}
