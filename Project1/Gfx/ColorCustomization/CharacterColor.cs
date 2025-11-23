using System.Collections.Generic;
using System.IO;
using Microsoft.Xna.Framework;

namespace Start_a_Town_
{
    public class CharacterColor
    {
        public string Name { get; set; }
        public string Overlay { get; set; }
        public Color Color { get; set; }
        public CharacterColor(string name) : this(name, name) { }
        public CharacterColor(string name, string overlay) : this(name, Color.White, overlay) { }
        public CharacterColor(string name, Color color, string overlay)
        {
            this.Name = name;
            this.Overlay = overlay;
            this.Color = color;
        }

        public void Apply(Bone body)
        {
            foreach (var bone in body.GetChildren())
                if (bone.Sprite != null)
                    foreach (var overlay in bone.Sprite.GetOverlays())
                        if (overlay.OverlayName == this.Overlay)
                            overlay.Material = MaterialDef.CreateColor(this.Color);
        }

        public List<SaveTag> Save()
        {
            List<SaveTag> tag = new List<SaveTag>();
            tag.Add(new SaveTag(SaveTag.Types.String, "Name", this.Name));
            tag.Add(new SaveTag(SaveTag.Types.String, "Overlay", this.Overlay));
            tag.Add(new SaveTag(SaveTag.Types.Compound, "Color", this.Color.Save()));
            return tag;
        }
        public CharacterColor(SaveTag tag)
        {
            this.Name = tag.GetValue<string>("Name");
            this.Overlay = tag.GetValue<string>("Overlay");
            tag.TryGetTag("Color", t => this.Color = t.LoadColor());
        }
        public void Write(BinaryWriter w)
        {
            w.WriteASCII(this.Name);
            w.Write(this.Overlay);
            this.Color.Write(w);
        }
        public CharacterColor(BinaryReader r)
        {
            this.Name = r.ReadASCII();
            this.Overlay = r.ReadString();
            this.Color = r.ReadColor();
        }
        public CharacterColor(CharacterColor toClone)
        {
            this.Name = toClone.Name;
            this.Overlay = toClone.Overlay;
            this.Color = toClone.Color;
        }
    }
}
