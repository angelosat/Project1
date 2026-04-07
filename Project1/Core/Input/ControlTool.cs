using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Project1.Core.Entities;
using Project1.Core.Graphics;
using Project1.Core.Networking;
using Project1.Core.Screens;
using Project1.Core.Simulation;
using Project1.Framework.Events;
using Project1.Framework.Input;
using Project1.Framework.Serialization;
using Project1.Framework.UI;
using System;
using System.Windows.Forms;

namespace Project1.Core.Input
{
    public class ControlTool : IInputEventHandler
    {
        public enum Messages { Default, Remove }
        protected MapBase Map => Ingame.Net.MainViewport.Map;


        protected void Sync()
        {
            //PacketPlayerToolSwitch.Send(Client.Instance, Client.Instance.PlayerData.ID, this);
            Ingame.Instance.Events.Post(new PlayerChangedActiveToolEvent(this));
        }
        public void DrawIcon(SpriteBatch sb, Vector2 pos)
        {
            if (this.GetIcon() is Icon icon)
                icon.Draw(sb, pos);
        }
        public TargetArgs Target;
        public TargetArgs TargetLast;
        public virtual Icon Icon => Icon.Cursor;
        
        public virtual Icon GetIcon()
        {
            return this.Icon;
        }
        public ToolManager Manager;

        public virtual GameObject GetTarget()
        {
            return Controller.Instance.Mouseover.Object as GameObject;
        }

        public virtual bool TryGetTarget(out GameObject target)
        {
            return Controller.Instance.Mouseover.TryGet<GameObject>(out target);
        }

        internal virtual void CleanUp()
        {
            
        }

        public virtual bool TryGetTarget(out GameObject target, out Vector3 face)
        {
            face = Controller.Instance.Mouseover.Face;
            return Controller.Instance.Mouseover.TryGet<GameObject>(out target);
        }
        public virtual void UpdateRemote(TargetArgs target) { this.Target = target; }
        public virtual void Update()
        {
            var cam = this.Map.Camera;
            //cam.MousePicking(Ingame.DrawServer ? Server.Instance.Map : Client.Instance.Map);
            cam.MousePicking(this.Map);

            UpdateTarget();

            if (this.Target != null)
                if (this.TargetLast != this.Target)
                    OnTargetChanged();

            if (InputState.IsKeyDown(Keys.LButton))
                return;
            if (InputState.IsKeyDown(Keys.RButton))
                return;
        }
        protected void UpdateTargetNew()
        {
            if ((Controller.Instance.Mouseover.Object is Element))
            {
                this.Target = TargetArgs.Null;
            }
            else
            {
                var mouseover = Controller.Instance.Mouseover;
                if (mouseover.Target != null)
                {
                    this.Target = mouseover.Target;
                    this.TargetLast = this.Target.Type != TargetType.Null ? this.Target : this.TargetLast;
                }
                else
                    this.Target = TargetArgs.Null;
            }
        }
        protected void UpdateTarget()
        {
            if (Controller.Instance.Mouseover.Object is Element)
                this.Target = TargetArgs.Null;
            else
            {
                if (Controller.Instance.Mouseover.Target != null)
                {
                    this.Target = Controller.Instance.Mouseover.Target;
                    this.TargetLast = this.Target.Type != TargetType.Null ? this.Target : this.TargetLast;
                }
                else
                    this.Target = TargetArgs.Null;
            }
        }
        public virtual void Update(SceneState scene)
        {
            this.Update();
        }

        public virtual Messages MouseRightUp(HandledMouseEventArgs e) { return Messages.Default; }
        public virtual Messages MouseRightDown(HandledMouseEventArgs e) { return Messages.Default; }
        public virtual Messages MouseLeftUp(HandledMouseEventArgs e) { return Messages.Default; }
        public virtual Messages MouseLeftPressed(HandledMouseEventArgs e) { return Messages.Default; }
        public virtual Messages MouseMiddle() { return Messages.Default; }
        public virtual Messages MouseMiddleUp(HandledMouseEventArgs e) { return Messages.Default; }
        public virtual Messages MouseMiddleDown(HandledMouseEventArgs e) { return Messages.Default; }
        public virtual Messages MouseWheel(InputState e, int value) { return Messages.Default; }

        public virtual Messages OnKey(KeyEventArgs e) { return Messages.Default; }

        protected virtual void OnTargetChanged()
        {
            
        }
        internal virtual void KeyDown(InputState input) { }
        internal virtual void PickUp() { }
        internal virtual void Drop() { }
        internal virtual void ManageEquipment() { }

        public virtual void HandleKeyPress(KeyPressEventArgs e) { }
        public virtual void HandleKeyDown(KeyEventArgs e) { }
        public virtual void HandleKeyUp(KeyEventArgs e) { }
        public virtual void HandleMouseMove(HandledMouseEventArgs e) { }
        public virtual void HandleInput(InputState e) { }
        public virtual void HandleLButtonDown(HandledMouseEventArgs e) { }
        public virtual void HandleLButtonUp(HandledMouseEventArgs e) { }
        public virtual void HandleRButtonDown(HandledMouseEventArgs e) { }
        public virtual void HandleRButtonUp(HandledMouseEventArgs e) { }
        public virtual void HandleMiddleUp(HandledMouseEventArgs e) { }
        public virtual void HandleMiddleDown(HandledMouseEventArgs e) { }
        public virtual void HandleMouseWheel(HandledMouseEventArgs e) { }
        public virtual void HandleLButtonDoubleClick(HandledMouseEventArgs e) { }

        internal virtual void Jump() { }
        internal virtual void Use() { }
        internal virtual void Throw() { }

        internal virtual void DrawUI(SpriteBatch sb, Camera camera)
        {
            var icon = this.GetIcon();
            icon?.Draw(sb, UIManager.Mouse + new Vector2(icon.SourceRect.Width / 2, 0));
        }
     
        internal virtual void DrawBeforeWorld(MySpriteBatch sb, MapBase map, Camera camera)
        {
        }

        public static bool IsCtrlKeyDown()
        {
            return InputState.Instance.GetKeyDown(System.Windows.Forms.Keys.ControlKey);
        }
        public static bool IsAltKeyDown()
        {
            return InputState.Instance.GetKeyDown(System.Windows.Forms.Keys.LMenu);
        }
        public static bool IsShiftKeyDown()
        {
            return InputState.Instance.GetKeyDown(System.Windows.Forms.Keys.LShiftKey);
        }

        internal virtual void OnGameEvent(GameEvent e)
        {
          
        }

        internal virtual void DrawAfterWorld(MySpriteBatch sb, MapBase map)
        {
            var camera = map.Camera;
            if (this.Target is not null)
                //ToolManager.DrawBlockHighlight(sb, map, camera, this.Target);
                camera.DrawBlockMouseover(sb, map, this.Target.Global, Color.White);
        }
        internal virtual void GetContextActions(ContextArgs args) { }
        internal virtual void OnActiveToolSet() { }
        internal virtual void SlotRightClick(GameObjectSlot slot) { }
        internal virtual void SlotLeftClick(GameObjectSlot gameObjectSlot) { }


        internal void Write(IDataWriter w)
        {
            w.Write(this.GetType().FullName);
            this.WriteData(w);
        }
        ControlTool Read(IDataReader r)
        {
            this.ReadData(r);
            return this;
        }
        protected virtual void WriteData(IDataWriter w) { }
        protected virtual void ReadData(IDataReader r) { }
        
        internal static ControlTool Create(IDataReader r)
        {
            var type = Type.GetType(r.ReadString());
            return (Activator.CreateInstance(type) as ControlTool).Read(r);
        }
        internal static ControlTool CreateOrSync(IDataReader r, PlayerData player)
        {
            var type = Type.GetType(r.ReadString());
            var tool = player.CurrentTool;
            if(tool.GetType() == type)
                return tool.Read(r).Read(player);
            return (Activator.CreateInstance(type) as ControlTool).Read(r).Read(player);
        }

        internal virtual ControlTool Read(PlayerData player)
        {
            return this;
        }
        internal virtual void DrawUIRemote(SpriteBatch sb, Camera camera, Vector2 vector2, TargetArgs targetArgs, PlayerData player)
        {
            UIManager.DrawStringOutlined(sb, this.GetType().Name, vector2 + new Vector2(0, UIManager.Cursor.Height));
        }
        internal virtual void DrawAfterWorldRemote(MySpriteBatch sb, MapBase map, Camera camera, PlayerData player)
        {
        }

        internal virtual void DrawUIRemote(SpriteBatch sb, Camera camera, PlayerData pl)
        {
        }

        public virtual string HelpText { get; } = "";
        public virtual bool TargetOnlyBlocks { get; } = false;


        internal virtual void RotateAntiClockwise() { }

        internal virtual void RotateClockwise() { }
    }
}
