using System;
using System.Windows.Forms;

namespace Start_a_Town_
{
    [Obsolete]
    sealed public class KeyBind : IHotkey
    {
        public string Name;
        public Keys Key;
        KeyBind(string name, Keys key)
        {
            this.Name = name;
            this.Key = key;
        }
        public static readonly KeyBind Cancel = new KeyBind("Cancel", Keys.C);
        public static readonly KeyBind Build = new KeyBind("Build", Keys.B);
        public static readonly KeyBind DigMine = new KeyBind("DigMine", Keys.M);
        public static readonly KeyBind Deconstruct = new KeyBind("Deconstruct", Keys.X);

        public Keys[] ShortcutKeys => new[] { this.Key };

        public string GetLabel()
        {
            return this.Key != 0 ? this.Key.ToString() : "";
        }

        public bool Contains(Keys key)
        {
            return this.Key == key;
        }
    }
}
