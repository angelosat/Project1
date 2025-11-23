using System;
using System.Xml.Linq;

namespace Start_a_Town_
{
    partial class HotkeyManager
    {
        public class Hotkey : IHotkey
        {
            public bool Pressed;

            public readonly Action ActionPress, ActionRelease;

            public readonly string Label;

            public readonly HotkeyContext Context;

            public readonly System.Windows.Forms.Keys[] _keys = new System.Windows.Forms.Keys[2];
            public readonly System.Windows.Forms.Keys[] _keysPrevious = new System.Windows.Forms.Keys[2];

            public System.Windows.Forms.Keys[] ShortcutKeys => this._keys;

            public System.Windows.Forms.Keys Key1
            {
                get => this._keys[0];
                set => this._keys[0] = value;
            }

            public System.Windows.Forms.Keys Key2
            {
                get => this._keys[1];
                set => this._keys[1] = value;
            }

            public Hotkey()
            {

            }
            public Hotkey(HotkeyContext context, string label, Action action, System.Windows.Forms.Keys key1 = System.Windows.Forms.Keys.None, System.Windows.Forms.Keys key2 = System.Windows.Forms.Keys.None)
                : this(context, label, action, delegate { }, key1, key2)
            {
            }
            public Hotkey(HotkeyContext context, string label, Action actionPress, Action actionRelease, System.Windows.Forms.Keys key1 = System.Windows.Forms.Keys.None, System.Windows.Forms.Keys key2 = System.Windows.Forms.Keys.None)
            {
                this.Context = context;
                this.Label = label;
                this.ActionPress = actionPress;
                this.ActionRelease = actionRelease;
                this.Key1 = key1;
                this.Key2 = key2;
                this.Apply();
            }
            public string GetLabel()
            {
                if (this.Key2 != 0)
                {
                    if (this.Key1 != 0)
                        return $"{this.Key1}, {this.Key2}";
                    else
                        return this.Key2.ToString();
                }
                else if (this.Key1 != 0)
                    return this.Key1.ToString();
                return "";
            }
            public bool Contains(System.Windows.Forms.Keys key)
            {
                return this._keys[0] == key || this._keys[1] == key;
            }
            public override string ToString()
            {
                return $"Hotkey:{this.Label}";
            }
            internal void Apply()
            {
                this._keysPrevious[0] = this._keys[0];
                this._keysPrevious[1] = this._keys[1];
            }
            internal void Undo()
            {
                this._keys[0] = this._keysPrevious[0];
                this._keys[1] = this._keysPrevious[1];
            }
            internal void XWrite(XElement node)
            {
                node.SetAttributeValue("Key1", this.Key1);
                node.SetAttributeValue("Key2", this.Key2);
            }
            internal void XRead(XElement node)
            {
                if (Enum.TryParse(node.Attribute("Key1").Value, out System.Windows.Forms.Keys v1))
                    this.Key1 = v1;
                if (Enum.TryParse(node.Attribute("Key2").Value, out System.Windows.Forms.Keys v2))
                    this.Key2 = v2;
            }
        }
    }
}
