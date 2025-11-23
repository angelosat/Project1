using System.IO;
using Microsoft.Xna.Framework.Input;

namespace Start_a_Town_
{
    public class GlobalVars
    {
        static string _SaveDir = Directory.GetCurrentDirectory() + @"\Saves\";
        static public string SaveDir
        {
            get
            {
                if (!Directory.Exists(_SaveDir))
                    Directory.CreateDirectory(_SaveDir);
                return _SaveDir;
            }
        }

        public enum Actions { ACTION_CONSTRUCT = 3 }
        public enum Skills { SKILL_FORAGING = 6 }

        public static int Fps;
        public static float DeltaTime;
        public static bool DebugMode = false;
        public static string Undefined = "<undefined>";

        public const string Version = "0.3.0 alpha";

        public class Settings
        {
            public static bool OutlineTileEdges = true;
            public static int TileEdgeOutlines = 0;
        }

        public class Physics
        {
            public static float WaterSpeed = 30;
        }

        public class KeyBindings
        {
            public static System.Windows.Forms.Keys North = System.Windows.Forms.Keys.W;
            public static System.Windows.Forms.Keys South = System.Windows.Forms.Keys.S;
            public static System.Windows.Forms.Keys East = System.Windows.Forms.Keys.D;
            public static System.Windows.Forms.Keys West = System.Windows.Forms.Keys.A;
            public static System.Windows.Forms.Keys RunWalk = System.Windows.Forms.Keys.ControlKey;
            public static System.Windows.Forms.Keys Sprint = System.Windows.Forms.Keys.ShiftKey;
            public static System.Windows.Forms.Keys Attack = System.Windows.Forms.Keys.Z;
            public static System.Windows.Forms.Keys Block = System.Windows.Forms.Keys.X;
            public static System.Windows.Forms.Keys Interact = System.Windows.Forms.Keys.RButton;
            public static Keys DebugWindow = Keys.F1;
            public static Keys DebugMode = Keys.F2;
            public static Keys Skills = Keys.K;
            public static Keys Character = Keys.C;
            public static Keys Browser = Keys.B;
            public static System.Windows.Forms.Keys ObjectBrowser = System.Windows.Forms.Keys.O;
            public static System.Windows.Forms.Keys Jobs = System.Windows.Forms.Keys.J;
            public static System.Windows.Forms.Keys Npcs = System.Windows.Forms.Keys.N;
            public static System.Windows.Forms.Keys Needs = System.Windows.Forms.Keys.J;
            public static System.Windows.Forms.Keys Menu = System.Windows.Forms.Keys.Escape;
            public static System.Windows.Forms.Keys Inventory = System.Windows.Forms.Keys.I;
            public static System.Windows.Forms.Keys Build = System.Windows.Forms.Keys.B;
            public static System.Windows.Forms.Keys HideInterface = System.Windows.Forms.Keys.OemPipe;
            public static Keys Options = Keys.Escape;
            public static Keys HideWalls = Keys.F5;
            public static System.Windows.Forms.Keys Activate = System.Windows.Forms.Keys.E;
            public static System.Windows.Forms.Keys PickUp = System.Windows.Forms.Keys.F;
            public static System.Windows.Forms.Keys Drop = System.Windows.Forms.Keys.Q;
            public static Keys ManageEquipment = Keys.Q;
            public static System.Windows.Forms.Keys Jump = System.Windows.Forms.Keys.Space;
            public static System.Windows.Forms.Keys Throw = System.Windows.Forms.Keys.G;
            public static System.Windows.Forms.Keys RotateMapLeft = System.Windows.Forms.Keys.Oemcomma;
            public static System.Windows.Forms.Keys RotateMapRight = System.Windows.Forms.Keys.OemPeriod;
            public static Keys Console = Keys.OemTilde;
            public static Keys Screenshot = Keys.F9;
            public static System.Windows.Forms.Keys DebugQuery = System.Windows.Forms.Keys.H;
            public static Keys QuickSlot1 = Keys.D1;
            public static Keys QuickSlot2 = Keys.D2;
            public static Keys QuickSlot3 = Keys.D3;
            public static Keys QuickSlot4 = Keys.D4;
            public static Keys QuickSlot5 = Keys.D5;
            public static Keys QuickSlot6 = Keys.D6;
            public static Keys QuickSlot7 = Keys.D7;
            public static Keys QuickSlot8 = Keys.D8;
            public static Keys QuickSlot9 = Keys.D9;
            public static Keys QuickSlot10 = Keys.D0;
        }
    }
}
