using Microsoft.Xna.Framework;
using Project1.Core.AI.Packets;
using Project1.Core.Networking;
using Project1.Core.Screens;
using Project1.Core.Simulation;
using Project1.Framework;
using Project1.Framework.Input;
using System;
using System.Windows.Forms;

namespace Project1.Core.Input;

[EnsureStaticCtorCall]
class ToolControlActor : ControlTool
{
    static readonly HotkeyCategory HotkeyCategoryMovement = new("Movement");
    static ToolControlActor()
    {
        HotkeyManager.RegisterHotkey(HotkeyCategoryMovement, "Move: Left", () => Left = true, () => Left = false, Keys.A, Keys.Left);
        HotkeyManager.RegisterHotkey(HotkeyCategoryMovement, "Move: Right", () => Right = true, () => Right = false, Keys.D, Keys.Right);
        HotkeyManager.RegisterHotkey(HotkeyCategoryMovement, "Move: Up", () => Up = true, () => Up = false, Keys.W, Keys.Up);
        HotkeyManager.RegisterHotkey(HotkeyCategoryMovement, "Move: Down", () => Down = true, () => Down = false, Keys.S, Keys.Down);
        HotkeyManager.RegisterHotkey(HotkeyCategoryMovement, "Jump", JumpNew, Keys.Space);
        HotkeyManager.RegisterHotkey(HotkeyCategoryMovement, "Toggle Mouse Move", ToggleMouseMove, Keys.M);
        HotkeyManager.RegisterHotkey(HotkeyCategoryMovement, "Walk", () => StartWalk(true), () => StartWalk(false), Keys.ControlKey);
        HotkeyManager.RegisterHotkey(HotkeyCategoryMovement, "Sprint", () => StartSprint(true), () => StartSprint(false), Keys.ShiftKey);
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
        var cam = Ingame.MainViewport.Camera;
        //var cam = Client.Instance.GetPlayer().ControllingEntity.Map.Camera;
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
    public override void HandleKeyDown(KeyEventArgs e)
    {
        if (e.Handled)
            return;
        if (HotkeyManager.Press(e.KeyCode, HotkeyCategoryMovement))
            e.Handled = true;
        base.HandleKeyDown(e);
    }
    public override void HandleKeyUp(KeyEventArgs e)
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
            var cam = Ingame.MainViewport.Camera;
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
    public override ControlTool.Messages MouseLeftPressed(HandledMouseEventArgs e)
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
    public override Messages MouseRightDown(HandledMouseEventArgs e)
    {
        return Messages.Remove;
    }
    public override void HandleMouseWheel(HandledMouseEventArgs e)
    {
        base.HandleMouseWheel(e);
        if (InputState.IsKeyDown(Keys.LControlKey))
        {
            var delta = InputState.IsKeyDown(Keys.LShiftKey) ? e.Delta * 16 : e.Delta;
            //var camera = Ingame.MainViewport.;
            Ingame.MainViewport.AdjustDrawLevel(delta);
            e.Handled = true;
            return;
        }
    }
   
    public override ControlTool.Messages MouseLeftUp(HandledMouseEventArgs e)
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
