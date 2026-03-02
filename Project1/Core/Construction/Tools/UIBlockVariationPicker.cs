using Microsoft.Xna.Framework;
using Project1.Core.Blocks;
using Project1.Framework.UI;
using System;
using System.Linq;

namespace Project1.Core.Construction.Tools
{
    class UIBlockVariationPicker : GroupBox
    {
        readonly Panel Panel = new();
        public UIBlockVariationPicker()
        {

        }
        public void Refresh(BlockDef block, Action<ConstructionDesignationArgs> callback)
        {
            var variants = block.Block.GetConstructionOptions().ToList();
            var count = variants.Count;//.Sum(v => v.Count);
            this.Panel.Controls.Clear();
            this.Panel.AutoSize = true;
            this.ClearControls();

            var list = new ListBoxNoScroll<ConstructionDesignationArgs, ButtonNew>(variant => CreateButton(block.Block, callback, variant))
            {
                Spacing = 0
            };
            foreach (var group in variants)
                list.AddItems(group);
            list.Layout(50, 60);

            var container = ScrollableBoxNewNewNew.FromContentsSize(160, UIManager.LargeButton.Height * 8, ScrollModes.Vertical);
            container.AddControls(list);
            this.Panel.AddControls(container);

            this.AddControls(this.Panel);
            if (this.Show())
                this.Location = UIManager.MouseScaled;
        }

        private ButtonNew CreateButton(Block block, Action<ConstructionDesignationArgs> callback, ConstructionDesignationArgs variant)
        {
            var btn = new ButtonNew(160) { BackgroundStyle = BackgroundStyle.LargeButton };
            var padding = btn.BackgroundStyle.Left.Width;
            var picbox = new PictureBox(block.PaintIcon(0, variant.Material)) { MouseThrough = true, Location = new Vector2(padding, btn.Height / 2), Anchor = new Vector2(0, .5f) };
            var label = new Label($"{variant.Material.LabelReadable} {variant.BlockDef.LabelReadable}") { Location = picbox.TopRight + Vector2.UnitX * padding, MouseThrough = true };
            btn.AddControls(picbox, label);
            btn.LeftClickAction = () =>
            {
                callback(variant);
                this.Hide();
            };
            return btn;
        }

        public override void HandleRButtonUp(System.Windows.Forms.HandledMouseEventArgs e)
        {
            this.Hide();
            base.HandleRButtonDown(e);
        }
        public override void HandleLButtonDown(System.Windows.Forms.HandledMouseEventArgs e)
        {
            if (!this.BoundsScreen.Contains(UIManager.MouseRect))
                this.Hide();
            else
                base.HandleLButtonDown(e);
        }
    }
}
