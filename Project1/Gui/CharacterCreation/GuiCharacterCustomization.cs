using Microsoft.Xna.Framework;
using Start_a_Town_.UI;

namespace Start_a_Town_
{
    class GuiCharacterCustomization : PanelLabeledNew
    {
        readonly PictureBox BodyFrame;
        Actor Actor => this.Tag as Actor;
        public CharacterColors Colors => this.Actor.Sprite.Customization;
        private readonly TableScrollableCompact<CharacterColor> Table;

        public GuiCharacterCustomization()
            : base("Customize")
        {
            this.AutoSize = true;
            var btnRandomize = IconButton.CreateRandomizeButton();
            btnRandomize.Location = this.Label.TopRight;
            btnRandomize.LeftClickAction = () => this.Colors.Randomize();
            this.Controls.Add(btnRandomize);

            this.BodyFrame = new PictureBox(BodyDef.NpcNew.GetMinimumRectangle(), 2);

            this.Client.AddControlsBottomLeft(this.BodyFrame.ToPanel());

            var picker = new ColorPickerBox();
            var pickerPanel = picker.ToPanelLabeled("");

            this.Table = new TableScrollableCompact<CharacterColor>() { ClientBoxColor = Color.Transparent }
                .AddColumn(null, "Name", 50, c => new Label(c.Name), 0)
                .AddColumn(null, "Color", 16, cc =>
                {
                    var btn = new ButtonColor
                    {
                        SelectedColorFunc = () => cc.Color,
                        Tag = cc
                    };
                    btn.LeftClickAction = () =>
                    {
                        pickerPanel.Label.Text = $"Choose {cc.Name} color";
                        var oldColor = cc.Color;
                        picker.CancelAction = () => cc.Color = oldColor;
                        picker.SelectColor(btn.SelectedColor);
                        picker.Callback = c => (btn.Tag as CharacterColor).Color = c;
                        picker.ColorChangedFunc = c => cc.Color = c;
                        if (!pickerPanel.IsOpen)
                            pickerPanel.SetLocation(btn.ScreenLocation + Vector2.UnitX * btn.Width).Show();
                    };
                    return btn;
                }, 0);
            
            this.Client.AddControlsTopRight(this.Table);
        }
        protected override void OnTagChanged()
        {
            this.Table.ClearItems();
            this.Table.AddItems(this.Colors.Colors.Values);
        }
        public override void Update()
        {
            base.Update();
            this.Actor.Body.RenderIcon(this.Actor, this.BodyFrame.Texture, this.Colors);
        }
    }
}
