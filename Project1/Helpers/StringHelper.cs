using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace Start_a_Town_
{
    static public class StringHelper
    {
        static public bool IsNullEmptyOrWhiteSpace(this string text)
        {
            return string.IsNullOrEmpty(text) || string.IsNullOrWhiteSpace(text);
        }
        public static void ToConsole(this object obj)
        {
            Debug.WriteLine(obj.ToString());
        }
        public static int MaxWidth(this IEnumerable<string> strings, SpriteFont font)
        {
            var max = 0;
            foreach (var txt in strings)
                max = Math.Max(max, (int)font.MeasureString(txt).X);
            return max;
        }
        public static string Wrap(this string text, int maxWidthInPixels)
        {
            if (string.IsNullOrEmpty(text))
                return text;
            string[] words = text.Split(' ');
            var newtext = new StringBuilder();
            string line = "";
            foreach (var word in words)
            {
                if ((int)(UI.UIManager.Font.MeasureString(line + word)).X > maxWidthInPixels)
                {
                    newtext.AppendLine(line);
                    line = "";
                }
                line += $"{word} ";
            }
            if (line.Length > 0)
                newtext.Append(line);
            return newtext.ToString();
        }
        public static int GetMaxWidth(this IEnumerable<string> strings)
        {
            int max = 0;
            foreach (var item in strings)
                max = (int)Math.Max(max, Math.Ceiling(UI.UIManager.Font.MeasureString(item).X));
            return max;
        }
    }
}
