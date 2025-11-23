using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using Microsoft.Xna.Framework;

namespace Start_a_Town_
{
    static class ColorHelper
    {
        static readonly Random ColorRand = new Random();

        static public Color GetRandomColor()
        {
            var array = new byte[3];
            ColorRand.NextBytes(array);
            return new Color(array[0], array[1], array[2]);
        }
        public static Color Add(this Color c1, Vector4 c2)
        {
            return new Color(c1.R + c2.X * 255, c1.G + c2.Y * 255, c1.B + c2.Z * 255, c1.A + c2.W * 255);
        }
        public static Color Add(this Color c1, Color c2)
        {
            return new Color(c1.R + c2.R, c1.G + c2.G, c1.B + c2.B, c1.A + c2.A);
        }
        public static Color Multiply(this Color c1, Color c2)
        {
            float r = (c1.R / 255f) * (c2.R / 255f);
            float g = (c1.G / 255f) * (c2.G / 255f);
            float b = (c1.B / 255f) * (c2.B / 255f);
            float a = (c1.A / 255f) * (c2.A / 255f);
            return new Color(r, g, b, a);
        }

        public static List<SaveTag> Save(this Color c)
        {
            var tag = new List<SaveTag>
            {
                new SaveTag(SaveTag.Types.Byte, "R", c.R),
                new SaveTag(SaveTag.Types.Byte, "G", c.G),
                new SaveTag(SaveTag.Types.Byte, "B", c.B),
                new SaveTag(SaveTag.Types.Byte, "A", c.A)
            };
            return tag;
        }

        static public Color GetColor(this Random rand)
        {
            var array = new byte[3];
            rand.NextBytes(array);
            return new Color(array[0], array[1], array[2]);
        }
        public static void Write(this Color c, BinaryWriter w)
        {
            w.Write(c.R);
            w.Write(c.G);
            w.Write(c.B);
            w.Write(c.A);
        }
        public static void Write(this BinaryWriter w, Color c)
        {
            w.Write(c.R);
            w.Write(c.G);
            w.Write(c.B);
            w.Write(c.A);
        }
        public static Color ReadColor(this BinaryReader r)
        {
            var c = new Color
            {
                R = r.ReadByte(),
                G = r.ReadByte(),
                B = r.ReadByte(),
                A = r.ReadByte()
            };
            return c;
        }
        static public bool TryParseColor(this string text, out Color color)
        {
            var posFrom = text.IndexOf('{');
            if (posFrom != -1)
            {
                var posTo = text.IndexOf('}', posFrom + 1);
                if (posFrom != -1)
                {
                    var sub = text.Substring(posFrom + 1, posTo - posFrom - 1);
                    var elements = sub.Split(' ');
                    var values = elements.Select(e => int.Parse(e.Split(':')[1])).ToArray();
                    color = new Color(values[0], values[1], values[2], values[3]);
                    return true;
                }
            }
            color = Color.White;
            return false;
        }
        public static XElement ToXml(this Color col, string name)
        {
            var x = new XElement(name, col.PackedValue.ToString("x"));
            return x;
        }
        public static Color ReadColor(this XElement x)
        {
            return new Color() { PackedValue = uint.Parse(x.Value, System.Globalization.NumberStyles.HexNumber) };
        }
        public static bool TryReadColor(this XElement x, ref Color col)
        {
            if (!uint.TryParse(x.Value, System.Globalization.NumberStyles.HexNumber, null, out var packed))
                return false;
            col.PackedValue = packed;
            return true;
        }
        public static Color ParseColor(this XElement x)
        {
            var r = byte.Parse(x.Attribute("R").Value);
            var g = byte.Parse(x.Attribute("G").Value);
            var b = byte.Parse(x.Attribute("B").Value);
            var a = byte.Parse(x.Attribute("A").Value);
            return new Color(r, g, b, a);
        }
        public static bool TryParseColor(this XElement x, ref Color col)
        {
            if (!byte.TryParse(x.Attribute("R").Value, out var r))
                return false;
            if (!byte.TryParse(x.Attribute("G").Value, out var g))
                return false;
            if (!byte.TryParse(x.Attribute("B").Value, out var b))
                return false;
            if (!byte.TryParse(x.Attribute("A").Value, out var a))
                return false;
            col.R = r;
            col.G = g;
            col.B = b;
            col.A = a;
            return true;
        }
    }
}
