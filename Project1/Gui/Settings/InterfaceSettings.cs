using Microsoft.Xna.Framework;
using Start_a_Town_.UI;
using System.Xml.Linq;

namespace Start_a_Town_
{
    class InterfaceSettings : GameSettings
    {
        GroupBox _Gui;
        internal override GroupBox Gui => this._Gui ??= this.CreateGui();
        float _tmpGuiScale, _tmpTooltipDelay;
        bool _tmpMouseTooltip;
        Color _prevPrimary, _prevSecondary;

        //public static XElement XRoot => Engine.Config.GetOrCreateElement("Settings").GetOrCreateElement("Interface");
        //public static XElement XTintPrimary => XRoot.GetOrCreateElement("TintPrimary");
        //public static XElement XTintSecondary => XRoot.GetOrCreateElement("TintSecondary");
        //public static XElement XScale => XRoot.GetOrCreateElement("Scale");
        //public static XElement XMouseTooltip => XRoot.GetOrCreateElement("MouseTooltip"); 
        public static readonly XElement XRoot;// => Engine.Config.GetOrCreateElement("Settings").GetOrCreateElement("Interface");
        public static readonly XElement XTintPrimary;// => XRoot.GetOrCreateElement("TintPrimary");
        public static readonly XElement XTintSecondary;//=> XRoot.GetOrCreateElement("TintSecondary");
        public static readonly XElement XScale;// => XRoot.GetOrCreateElement("Scale");
        public static readonly XElement XMouseTooltip;// => XRoot.GetOrCreateElement("MouseTooltip");

        static InterfaceSettings()
        {
            XRoot = Engine.Config.GetOrCreateElement("Settings").GetOrCreateElement("Interface");
            XTintPrimary = XRoot.GetOrCreateElement("Colors").GetOrCreateElement("Primary");
            XTintSecondary = XRoot.GetOrCreateElement("Colors").GetOrCreateElement("Secondary");
            XScale = XRoot.GetOrCreateElement("Scale");
            XMouseTooltip = XRoot.GetOrCreateElement("MouseTooltip");
        }

        internal override string Name => "Interface";

        GroupBox CreateGui()
        {
            this._tmpGuiScale = UIManager.Scale;
            this._tmpTooltipDelay = TooltipManager.DelayInterval / Ticks.PerSecond;

            this._prevPrimary = UIManager.TintPrimary;
            this._prevSecondary = UIManager.TintSecondary;

            return new GroupBox() { Name = "Interface" }.AddControlsVertically(
                    SliderNew.CreateWithLabel("UI Scale", () => this._tmpGuiScale, v => this._tmpGuiScale = v, 100, 1, 2, 0.1f, "0%"),
                    SliderNew.CreateWithLabel("Tooltip Delay", () => this._tmpTooltipDelay, v => this._tmpTooltipDelay = v, 100, 0, 2, 0.1f, "0.0s"),
                    new CheckBoxNew("Mouse Tooltip", () => this._tmpMouseTooltip = !this._tmpMouseTooltip, () => this._tmpMouseTooltip) { HoverText = "Anchor tooltips to the mouse." },
                    new GroupBox().AddControlsHorizontally(new ButtonColorNew(() => UIManager.TintPrimary, c => UIManager.TintPrimary = c), new Label("UI tint primary")),
                    new GroupBox().AddControlsHorizontally(new ButtonColorNew(() => UIManager.TintSecondary, c => UIManager.TintSecondary = c), new Label("UI tint secondary"))
                ) as GroupBox;
        }

        internal override void Apply()
        {
            UIManager.Scale = this._tmpGuiScale;
            TooltipManager.MouseTooltips = this._tmpMouseTooltip;
            XScale.Value = this._tmpGuiScale.ToString();
            XMouseTooltip.Value = this._tmpMouseTooltip.ToString();

            XTintPrimary.Value = UIManager.TintPrimary.PackedValue.ToString("x");
            XTintSecondary.Value = UIManager.TintSecondary.PackedValue.ToString("x");
        }
        internal override void Cancel()
        {
            UIManager.TintPrimary = this._prevPrimary;
            UIManager.TintSecondary = this._prevSecondary;
        }
        internal override void Defaults()
        {
            UIManager.TintPrimary = UIManager.TintPrimaryDefault;
            UIManager.TintSecondary = UIManager.TintSecondaryDefault;
        }
    }
}
