using Microsoft.Xna.Framework;
using Project1.Core.Entities;
using Project1.Core.Resources;
using Project1.Core.Graphics;
using Project1.Framework.UI;

namespace Project1.Core.UI.Hud
{
    public class UnitFrame : GroupBox
    {
        Panel Panel_Picture, Panel_Bars;
        Label Label_Name;
        PictureBox Picture;

        public UnitFrame()
        {
            AutoSize = true;
            Panel_Picture = new Panel
            {
                AutoSize = true
            };
            Panel_Bars = new Panel
            {
                AutoSize = true
            };
        }

        public override void Update()
        {
            this.Picture.Invalidate(); // repaint unitframes each frame for 2 reasons: 1) for some reason the player's texture isn't rendered at the beginning of game, 2) to have them animated
            base.Update();
        }

        public UnitFrame Track(GameObject obj)
        {
            this.Controls.Clear();
            this.Panel_Picture.Controls.Clear();
            Sprite npcSprite = obj.GetSprite();
            this.Picture = new PictureBox(new Vector2(32, 48), (r) => obj.Body.RenderNewer(obj, r));
            this.Panel_Picture.Controls.Add(Picture);
            this.Label_Name = new Label(Panel_Picture.TopRight, obj.Name);
            this.Panel_Bars.Controls.Clear();

            foreach (var r in obj.GetComponent<ResourcesComponent>().Resources.Values)
            {
                var ui = r.GetControlBar();
                if (ui == null)
                    continue;
                ui.Location = this.Panel_Bars.Controls.BottomLeft;
                this.Panel_Bars.Controls.Add(ui);
            }

            this.Panel_Bars.Location = Label_Name.BottomLeft;
            this.Controls.Add(Label_Name,
                Panel_Picture,
                Panel_Bars
                );

            this.Invalidate(true);
            return this;
        }
    }
}
