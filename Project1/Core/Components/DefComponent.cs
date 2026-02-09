using Microsoft.Xna.Framework;
using Project1.Framework.UI;
using Project1.Framework.Serialization;
using Project1.Framework;
using Project1.Core.UI;
using Project1.Core.Entities;
using Project1.Core.Helpers;

namespace Project1.Core.Components
{
    public class DefComponent : EntityComp
    {
        public override string Name { get; } = "Info";
       
        public bool InCatalogue = true;
        public QualityDef Quality = QualityDefOf.Common;

        public string CustomName = "";
        public string ParentName
        {
            get => string.IsNullOrEmpty(this.CustomName) ? this.Owner.Def.LabelReadable : this.CustomName; 
            set => this.CustomName = value;
        }

        internal override void ApplyQuality(Entity parent, QualityDef quality)
        {
            this.Quality = quality;
        }
       
        public DefComponent()
            : base()
        {
            Quality = QualityDefOf.Common;
        }

        public override void OnTooltipCreated(GameObject parent, Control tooltip)
        {
            tooltip.Color = GetQualityColor();
            var namelabel = new Label(Vector2.Zero, parent.Name, tooltip.Color, Color.Black, UIManager.FontBold) { TextColorFunc = () => tooltip.Color, TextFunc = () => parent.Name };
            tooltip.Controls.Add(namelabel);
            tooltip.Controls.Add(new Label(this.Quality.LabelReadable) { Fill = Color.Gold, Location = tooltip.Controls.BottomLeft, TextColorFunc = () => Color.Gold });
            tooltip.Controls.Add(new Label(parent.Def.Description) { Location = tooltip.Controls.BottomLeft });
        }
      
        public Color GetQualityColor()
        {
            return Quality.Color;
        }

        public override void Write(IDataWriter w)
        {
            w.Write(this.CustomName);
            w.Write(this.Quality.Name);
        }

        public override void Read(IDataReader r)
        {
            this.CustomName = r.ReadString();
            this.Quality = r.ReadDef<QualityDef>();
        }
        internal override void SaveExtra(SaveTag tag)
        {
            tag.Save("CustomName", this.CustomName);
            tag.Save("Quality", this.Quality);
        }
        internal override void LoadExtra(SaveTag tag)
        {
            this.CustomName = tag.LoadString("CustomName");
            this.Quality = tag.LoadDef<QualityDef>("Quality");
        }
       
        public override void OnNameplateCreated(GameObject parent, Nameplate plate)
        {
            plate.Controls.Add(new Label()
            {
                Font = UIManager.FontBold,
                TextFunc = () => parent.Name,
                //TextColorFunc = parent.GetNameplateColor,
                //TintFunc = parent.GetNameplateColor, // we dont want tintfunc, we want to change textcolorfunc directly because the default textcolor is UIManager.DefaultTextColor = Color.LightGray
                TextColor = Color.White, // so i'll just set the text color to white, to get the full tint color
                TintFunc = parent.GetNameplateColor, // but tintfunc is applied on every draw call for ui controls, while textcolorfunc is applied only on validation for labels
                MouseThrough = true,
                TextBackgroundFunc = () => parent.HasFocus() ? this.Quality.Color * .5f : Color.Black * .5f
            });
        }
    }
}
