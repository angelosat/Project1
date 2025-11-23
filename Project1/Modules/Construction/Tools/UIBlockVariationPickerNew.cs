using Microsoft.Xna.Framework;
using Start_a_Town_.Components.Crafting;
using Start_a_Town_.UI;
using System;
using System.Linq;

namespace Start_a_Town_.Towns.Constructions
{
    class UIBlockVariationPickerNew : GroupBox
    {
        static UIBlockVariationPickerNew _instance;
        static UIBlockVariationPickerNew Instance => _instance ??= new UIBlockVariationPickerNew();

        readonly Panel Panel = new();
        public UIBlockVariationPickerNew()
            //: base(200, 300)
        {

        }
        public static void Refresh(Block block, Action<ProductMaterialPair> callback)
        {
            var variants = block.GetAllValidConstructionMaterialsNew().Select(m => new ProductMaterialPair(block, m)).GroupBy(p => p.Requirement.Material).ToList();
            if (!variants.Any())
                return;
            var count = variants.Sum(v => v.Count());
            Instance.Panel.Controls.Clear();
            Instance.Panel.AutoSize = true;
            Instance.ClearControls();

            var container = count <= 8 ? new GroupBox() : ScrollableBoxNewNew.FromClientSize(160, UIManager.LargeButton.Height * 8 + UIManager.LargeButton.Height / 2, ScrollModes.Vertical);

            var list = new ListBoxNoScroll<ProductMaterialPair, ButtonNew>(variant =>
            {
                var btn = new ButtonNew(160)// variant.GetName())
                {
                    BackgroundStyle = BackgroundStyle.LargeButton
                };
                var padding = btn.BackgroundStyle.Left.Width;
                var picbox = new PictureBox(variant.Block.PaintIcon(variant.Data, variant.Material)) { MouseThrough = true, Location = new Vector2(padding, btn.Height / 2), Anchor = new Vector2(0, .5f) };
                var label = new Label(variant.Requirement.ToString()) { Location = picbox.TopRight + Vector2.UnitX * padding, MouseThrough = true };
                btn.AddControls(picbox
                    , label
                    );
                btn.LeftClickAction = () =>
                {
                    callback(variant);
                    Instance.Hide();
                };
                return btn;
            })
            {
                Spacing = 0
            };
            foreach (var group in variants)
                list.AddItems(group);
            container.AddControls(list);
            Instance.Panel.AddControls(container);

            Instance.AddControls(Instance.Panel);
            if (Instance.Show())
                Instance.Location = UIManager.Mouse;
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
