using Microsoft.Xna.Framework;
using Project1.Core.AI.Packets;
using Project1.Core.Networking;
using Project1.Core.Screens;
using Project1.Core.Simulation;
using Project1.Framework;
using Project1.Framework.Input;
using System;

namespace Project1.Core.Input;

[EnsureStaticCtorCall]
class ToolControlActor : ControlTool
{
    static readonly HotkeyCategory HotkeyCategoryMovement = new("Movement");
    static ToolControlActor()
    {
        HotkeyManager.RegisterHotkey(HotkeyCategoryMovement, "Move: Left", () => Left = true, () => Left = false, System.Windows.Forms.Keys.A, System.Windows.Forms.Keys.Left);
        HotkeyManager.RegisterHotkey(HotkeyCategoryMovement, "Move: Right", () => Right = true, () => Right = false, System.Windows.Forms.Keys.D, System.Windows.Forms.Keys.Right);
        HotkeyManager.RegisterHotkey(HotkeyCategoryMovement, "Move: Up", () => Up = true, () => Up = false, System.Windows.Forms.Keys.W, System.Windows.Forms.Keys.Up);
        HotkeyManager.RegisterHotkey(HotkeyCategoryMovement, "Move: Down", () => Down = true, () => Down = false, System.Windows.Forms.Keys.S, System.Windows.Forms.Keys.Down);
        HotkeyManager.RegisterHotkey(HotkeyCategoryMovement, "Jump", JumpNew, System.Windows.Forms.Keys.Space);
        HotkeyManager.RegisterHotkey(HotkeyCategoryMovement, "Toggle Mouse Move", ToggleMouseMove, System.Windows.Forms.Keys.M);
        HotkeyManager.RegisterHotkey(HotkeyCategoryMovement, "Walk", () => StartWalk(true), () => StartWalk(false), System.Windows.Forms.Keys.ControlKey);
        HotkeyManager.RegisterHotkey(HotkeyCategoryMovement, "Sprint", () => StartSprint(true), () => StartSprint(false), System.Windows.Forms.Keys.ShiftKey);
    }

    static bool Up, Down, Left, Right, Moving, WalkKeyDown, SprintKeyDown;
    static bool MovingByMouse, Attacking;
    static bool MouseMovementEnabled = true;

    public ToolControlActor()
    {

    }
    public override void Update()
    {
        base.Update();
        if (MovingByMouse)
            MoveMouse();
        else
            MoveKeys();
    }
    public void MoveMouse()
    {
        Moving = true;
        var final = GetDirection3();
        this.ChangeDirection(final);
    }
    public void ChangeDirection(Vector3 direction)
    {
        PacketPlayerInputDirection.Send(Client.Instance, direction.XY());
    }
    public static Vector3 GetDirection3()
    {
        var cam = Client.Instance.GetPlayer().ControllingEntity.Map.Camera;
        var playerScreenPosition = cam.GetScreenPosition(Client.Instance.GetPlayer().ControllingEntity.Global);
        int x = Controller.Instance.msCurrent.X - (int)playerScreenPosition.X;
        int y = Controller.Instance.msCurrent.Y - (int)playerScreenPosition.Y;
        float xx, yy;
        int xxx, yyy;
        Coords.Ortho(x, y, out xx, out yy);
        Coords.Rotate((int)cam.Rotation, xx, yy, out xxx, out yyy);
        var normal = new Vector2(xxx, yyy);
        normal.Normalize();
        var final = new Vector3(normal.X, normal.Y, 0);
        return final;
    }
    public override void HandleKeyDown(System.Windows.Forms.KeyEventArgs e)
    {
        if (e.Handled)
            return;
        if (HotkeyManager.Press(e.KeyCode, HotkeyCategoryMovement))
            e.Handled = true;
        base.HandleKeyDown(e);
    }
    public override void HandleKeyUp(System.Windows.Forms.KeyEventArgs e)
    {
        if (e.Handled)
            return;
        if (HotkeyManager.Release(e.KeyCode, HotkeyCategoryMovement))
            e.Handled = true;
        base.HandleKeyUp(e);
    }

    private static void StartSprint(bool enable)
    {
        if (SprintKeyDown && enable)
            return;
        SprintKeyDown = enable;
        PacketPlayerToggleSprint.Send(Client.Instance, enable);
    }

    private static void StartWalk(bool enable)
    {
        if (WalkKeyDown && enable)
            return;
        WalkKeyDown = enable;
        PacketPlayerToggleWalk.Send(Client.Instance, enable);
    }

    static void ToggleMouseMove()
    {
        MouseMovementEnabled = !MouseMovementEnabled;
        MovingByMouse = false;
        //Ingame.Instance.Hud.Chat.Write($"Mouse move {(MouseMovementEnabled ? "Enabled" : "Disabled")}");
        Log.System($"Mouse move {(MouseMovementEnabled ? "Enabled" : "Disabled")}");
    }
    public virtual void MoveKeys()
    {
        int xx = 0, yy = 0;

        if (Up)
        {
            xx -= 1;
            yy -= 1;
        }
        else if (Down)
        {
            yy += 1;
            xx += 1;
        }
        if (Left)
        {
            yy += 1;
            xx -= 1;
        }
        else if (Right)
        {
            yy -= 1;
            xx += 1;
        }
        else if (!(Up || Down || Left || Right))
        {
            StopMoving();
            return;
        }
        if (xx != 0 || yy != 0)
        {
            var cam = Ingame.MainViewportMap.Camera;
            double rx, ry;
            double cos = Math.Cos((-cam.Rotation) * Math.PI / 2f);
            double sin = Math.Sin((-cam.Rotation) * Math.PI / 2f);
            rx = (xx * cos - yy * sin);
            ry = (xx * sin + yy * cos);
            int roundx, roundy;
            roundx = (int)Math.Round(rx);
            roundy = (int)Math.Round(ry);

            var nextStep = new Vector2(roundx, roundy);
            nextStep.Normalize();
            PacketPlayerInputDirection.Send(Client.Instance, nextStep);
            if (!Moving)
                StartMoving();
            Moving = true;
        }
        else
            StopMoving();
    }
    public void StartMoving()
    {
        PacketPlayerToggleMove.Send(Client.Instance, true);
    }
    protected void StopMoving()
    {
        if (!Moving)
            return;
        if (MovingByMouse)
            return;

        PacketPlayerToggleMove.Send(Client.Instance, false);

        Moving = false;
    }
    public override ControlTool.Messages MouseLeftPressed(System.Windows.Forms.HandledMouseEventArgs e)
    {
        if (e.Handled)
            return Messages.Default;

        if (MouseMovementEnabled)
        {
            MovingByMouse = true;
            StartMoving();
        }
        else
            this.StartAttack();
        return Messages.Default;
    }

    static void JumpNew()
    {
        PacketPlayerJump.Send(Client.Instance);
    }
    public override Messages MouseRightDown(System.Windows.Forms.HandledMouseEventArgs e)
    {
        return Messages.Remove;
    }
    public override void HandleMouseWheel(System.Windows.Forms.HandledMouseEventArgs e)
    {
        base.HandleMouseWheel(e);
        if (InputState.IsKeyDown(System.Windows.Forms.Keys.LControlKey))
        {
            var delta = InputState.IsKeyDown(System.Windows.Forms.Keys.LShiftKey) ? e.Delta * 16 : e.Delta;
            var camera = this.Map.Camera;
            camera.AdjustDrawLevel(delta);
            e.Handled = true;
            return;
        }
    }
   
    public override ControlTool.Messages MouseLeftUp(System.Windows.Forms.HandledMouseEventArgs e)
    {
        if (MouseMovementEnabled)
        {
            MovingByMouse = false;
            StopMoving();
        }
        else
            this.FinishAttack();
        return base.MouseLeftUp(e);
    }
    private void StartAttack()
    {
        Attacking = true;
    }
    private void FinishAttack()
    {
        if (!Attacking)
            return;
    }

    internal override void CleanUp()
    {
        StopMoving();
        StartWalk(false);
        StartSprint(false);
        PacketControlActor.Send(Client.Instance, Client.Instance.GetPlayer().ID, -1);
    }
}
