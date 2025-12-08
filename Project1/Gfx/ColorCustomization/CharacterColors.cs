using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Xna.Framework;

namespace Start_a_Town_
{
    public class CharacterColors
    {
        static readonly Random Rand = new();

        public Dictionary<string, CharacterColor> Colors = new();

        public CharacterColors(params CharacterColor[] parts):this()
        {
            foreach (var item in parts)
                this.Colors.Add(item.Name, item);
        }
        public CharacterColors()
        {
            this.Colors = new Dictionary<string, CharacterColor>();
        }

        public void Apply(Bone body)
        {
            foreach (var item in this.Colors.Values)
                item.Apply(body);
        }

        public List<SaveTag> Save()
        {
            var tag = new List<SaveTag>();
            var list = new SaveTag(SaveTag.Types.List, "Colors", SaveTag.Types.Compound);
            foreach (var item in this.Colors.Values)
                list.Add(new SaveTag(SaveTag.Types.Compound, "", item.Save()));
            tag.Add(list);
            return tag;
        }
        public CharacterColors(SaveTag tag)
        {
            this.Colors = new Dictionary<string, CharacterColor>();
            if (!tag.TryGetTagValueOrDefault<List<SaveTag>>("Colors", out var list))
                return;
            foreach (var item in list)
            {
                if (item == null)
                    continue;
                var part = new CharacterColor(item);
                this.Colors.Add(part.Name, part);
            }
        }
       
        public void Write(IDataWriter w)
        {
            w.Write(this.Colors.Count);
            foreach (var item in this.Colors.Values)
                item.Write(w);
        }
        public CharacterColors(IDataReader r)
        {
            this.Colors = new Dictionary<string, CharacterColor>();
            int count = r.ReadInt32();
            for (int i = 0; i < count; i++)
            {
                var part = new CharacterColor(r);
                this.Colors.Add(part.Name, part);
            }
        }

        public CharacterColors(CharacterColors toClone)
        {
            this.Colors = new Dictionary<string, CharacterColor>();
            foreach (var item in toClone.Colors)
                this.Colors.Add(item.Key, new CharacterColor(item.Value));
        }
        public CharacterColors(Bone body)
        {
            var bones = body.GetAllBones();
            foreach(var bone in bones)
            {
                var spr = bone.Sprite;
                if (spr == null)
                    continue;
                this.TryAdd(spr.OverlayName);
                foreach (var ov in spr.Overlays.Keys)
                    this.TryAdd(ov);
            }
        }
      
        private void TryAdd(string overlayName)
        {
            if (string.IsNullOrEmpty(overlayName))
                return;
            if (this.Colors.ContainsKey(overlayName))
                return;
            this.Colors.Add(overlayName, new CharacterColor(overlayName));
        }
        public bool TryGetColor(string overlayName, ref Color color)
        {
            if (this.Colors.TryGetValue(overlayName, out var c))
            {
                color = c.Color;
                return true;
            }
            return false;
        }

        public CharacterColors Randomize()
        {
            foreach (var item in this.Colors.Values)
            {
                Color c = new()
                {
                    R = (byte)Rand.Next(256),
                    G = (byte)Rand.Next(256),
                    B = (byte)Rand.Next(256),
                    A = 255
                };
                item.Color = c;
            }
            return this;
        }
    }
}
