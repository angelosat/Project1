using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System;

namespace Start_a_Town_
{
    public class Controller
    {
        public static bool GetMouseover<T>(out T obj) where T : GameObject
        {
            return Instance.Mouseover.TryGet(out obj);
        }

        public static event EventHandler<MouseoverEventArgs> MouseoverObjectChanged;
        public void OnMouseoverObjectChanged(MouseoverEventArgs e)
        {
            MouseoverObjectChanged?.Invoke(this, e);
        }

        public Mouseover Mouseover;
        public Mouseover MouseoverNext;

        public Rectangle MouseRect;

        public KeyboardState ksCurrent, ksPrevious;
        public MouseState msCurrent, msPrevious;

        static Controller _instance;
        public static Controller Instance => _instance ??= new();

        public static InputState Input = new();
        public Controller()
        {
            this.Mouseover = new();
            this.MouseoverNext = new();
        }

        void GetInputStates()
        {
            this.ksCurrent = Keyboard.GetState();
            this.msCurrent = Mouse.GetState();
        }

        public Vector2 MouseLocation => new(this.msCurrent.X, this.msCurrent.Y);
        public Mouseover GetMouseover()
        {
            return this.Mouseover;
        }
        public void Update()
        {
            AltDown = InputState.IsKeyDown(System.Windows.Forms.Keys.LMenu);
            this.UpdateMousovers();

            this.GetInputStates();
            this.MouseRect = new Rectangle(this.msCurrent.X, this.msCurrent.Y, 1, 1);

            if (this.ksCurrent.IsKeyDown(Keys.LeftAlt) || this.ksCurrent.IsKeyDown(Keys.RightAlt))
            {
                if (this.KeyPressCheck(Keys.Enter))
                {
                    Game1.Instance.graphics.ToggleFullScreen();
                }
            }

            this.ksPrevious = this.ksCurrent;
            this.msPrevious = this.msCurrent;
        }
        private void UpdateMousovers()
        {
            this.Mouseover.TryGet(out object mouseover);
            this.MouseoverNext.TryGet(out object mouseoverNext);

            if (mouseover != mouseoverNext)
                this.OnMouseoverObjectChanged(new MouseoverEventArgs(mouseoverNext, mouseover));
          
            this.Mouseover = this.MouseoverNext;
            this.MouseoverNext = new Mouseover();
        }

        public bool KeyPressCheck(Keys key)
        {
            return this.ksCurrent.IsKeyDown(key) && this.ksPrevious.IsKeyUp(key);
        }

        public Keys[] GetKeys()
        {
            return this.ksCurrent.GetPressedKeys();
        }

        public static void TrySetMouseoverEntity(Camera camera, GameObject entity, Vector3 face, float drawdepth)
        {
            var global = entity.Global;
            if (!camera.IsDrawable(entity.Map, global))
                return;

            float mouseoverDepth = drawdepth;
            if (mouseoverDepth >= Instance.MouseoverNext.Depth)
            {
                if (Instance.Mouseover.Object is TargetArgs target)
                {
                    if (target.Object != entity)
                        target = new TargetArgs(entity, face);// { Map = Engine.Map };
                }
                else
                    target = new TargetArgs(entity, face);// { Map = Engine.Map };

                Instance.MouseoverNext.Target = target;
                Instance.MouseoverNext.Object = target;
                Instance.MouseoverNext.Face = face;
                Instance.MouseoverNext.Depth = drawdepth;
            }
        }
        public static void SetMouseoverBlock(Camera camera, MapBase map, Vector3 global, Vector3 face, Vector3 precise)
        {
            var depth = global.GetDrawDepth(map, camera);
            if (Instance.MouseoverNext.Depth > depth)
                return;

            var target = new TargetArgs(map, global, face, precise);

            /// VERY HACKY
            /// it's to not create a new mouseover while hovering withing the same cell (the cell global remains the same but the precise and face vectors change while moving cursor around block)
            /// TODO tidy up
            if (global != _LastMouseoverBlockGlobal || (Instance.Mouseover.Object is TargetArgs t && t.Type != TargetType.Position)) // very hacky
                Instance.MouseoverNext.Object = target;
            else
                Instance.MouseoverNext.Object = Instance.Mouseover.Object;

            Instance.MouseoverNext.Face = face;
            Instance.MouseoverNext.Precise = precise;
            Instance.MouseoverNext.Target = target;
            Instance.MouseoverNext.Depth = depth;
            _LastMouseoverBlockGlobal = global;
        }
        static Vector3 _LastMouseoverBlockGlobal;
        public static bool BlockTargeting;

        static bool AltDown;
        public static bool IsBlockTargeting()
        {
            bool keydown = AltDown;
            return keydown ^ BlockTargeting;
        }

        public static TargetArgs TargetCell { get { return Instance.Mouseover.TargetCell; } }
        public static TargetArgs TargetEntity { get { return Instance.Mouseover.TargetEntity; } }
    }
}
