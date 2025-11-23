using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System.Runtime.InteropServices;

namespace Start_a_Town_
{
    public class InputState : EventArgs
    {
        bool _Handled;
        public bool Handled
        {
            get { return _Handled; }
            set { _Handled = value; }
        }
        public bool KeyHandled;

        [DllImport("user32.dll")]
        static extern short GetKeyState(int key);
        [DllImport("user32.dll")]
        static public extern bool GetKeyboardState(byte[] lpKeyState);

        static public bool IsKeyDown(System.Windows.Forms.Keys key)
        {
            return GetKeyState((int)key) < 0;
        }

        public byte[] KeyState = new byte[256], LastKeyState;
        //public Vector2 LastMouse;
        //public Vector2 CurrentMouse;
        public KeyboardState CurrentKeyboardState, LastKeyboardState;
        public MouseState CurrentMouseState, LastMouseState;

        public bool GetKeyDown(System.Windows.Forms.Keys key)
        {
            int i = (int)key;
            byte b = this.KeyState[i];
            return ((b & 0x80) != 0);
        }
        public bool IsKeyDown(Keys key)
        {
            byte b = this.KeyState[(int)key];
            return ((b & 0x80) != 0);
        }
        public bool IsKeyPressed(Keys key)
        {
            byte b = this.KeyState[(int)key];
            if ((b & 0x80) != 0)
                return ((this.LastKeyState[(int)key] & 0x80) == 0);

            return false;
        }
        public bool IsKeyReleased(Keys key)
        {
            if ((this.KeyState[(int)key] & 0x80) == 0)
                return ((this.LastKeyState[(int)key] & 0x80) != 0);
            return false;
        }
        public bool LeftButtonPressed
        {
            get { return CurrentMouseState.LeftButton == ButtonState.Pressed && LastMouseState.LeftButton != ButtonState.Pressed; }
        }
        public bool LeftButtonReleased
        {
            get { return CurrentMouseState.LeftButton == ButtonState.Released && LastMouseState.LeftButton != ButtonState.Released; }
        }
        public bool RightButtonPressed
        {
            get { return CurrentMouseState.RightButton == ButtonState.Pressed && LastMouseState.RightButton != ButtonState.Pressed; }
        }
        public bool RightButtonReleased
        {
            get { return CurrentMouseState.RightButton == ButtonState.Released && LastMouseState.RightButton != ButtonState.Released; }
        }
        public List<System.Windows.Forms.Keys> GetPressedKeys()
        {
            List<System.Windows.Forms.Keys> keys = new List<System.Windows.Forms.Keys>();
            for (int i = 0; i < 256; i++)
            {
                byte current = this.KeyState[i];
                byte last = this.LastKeyState[i];
                if ((current & 0x80) != 0)
                    if ((last & 0x80) == 0)
                        keys.Add((System.Windows.Forms.Keys)i);
            }
            return keys;
        }
        public List<Keys> GetReleasedKeys()
        {
            List<Keys> keys = new List<Keys>();
            for (int i = 0; i < 256; i++)
            {
                byte current = this.KeyState[i];
                byte last = this.LastKeyState[i];
                if ((current & 0x80) == 0)
                    if ((last & 0x80) != 0)
                        keys.Add((Keys)i);
            }
            return keys;
        }
        public void UpdateKeyStates()
        {
            this.LastKeyState = this.KeyState;
            GetKeyState(0);
            this.KeyState = new byte[256];
            GetKeyboardState(this.KeyState);
        }
        public void Update()
        {
            KeyHandled = false;
            Handled = false;
            LastKeyboardState = CurrentKeyboardState;
            LastMouseState = CurrentMouseState;
            CurrentKeyboardState = Keyboard.GetState();
            CurrentMouseState = Mouse.GetState();
            //this.LastMouse = this.CurrentMouse;
            //this.CurrentMouse = new Vector2(System.Windows.Forms.Control.MousePosition.X - Game1.Instance.Window.ClientBounds.X, System.Windows.Forms.Control.MousePosition.Y - Game1.Instance.Window.ClientBounds.Y);
        }
    }
}
