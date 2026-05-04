using Microsoft.Xna.Framework;
using Project1.Core.Blocks;
using Project1.Core.Entities;
using Project1.Core.Graphics;
using Project1.Core.Helpers;
using Project1.Core.Networking;
using Project1.Core.Networking.Entities;
using Project1.Core.Rendering;
using Project1.Framework.Input;
using Project1.Framework.Serialization;
using System.Windows.Forms;

namespace Project1.Core.Input;

class ObjectSpawnToolFromTemplate : ToolManagement
{
    GameObject Entity;
    int TemplateID;
    public ObjectSpawnToolFromTemplate()
    {

    }
   
    public ObjectSpawnToolFromTemplate(GameObject entity, int templateID)
    {
        this.Entity = entity;
        this.TemplateID = templateID;
    }
    public override void Update()
    {
        base.Update();
        if(this.Target != null)
            this.Target.Precise = InputState.IsKeyDown(Keys.ShiftKey) ? this.Target.Precise : Vector3.Zero;
    }
    public override ControlTool.Messages MouseLeftPressed(HandledMouseEventArgs e)
    {
        return Messages.Default;
    }
    public override ControlTool.Messages MouseLeftUp(HandledMouseEventArgs e)
    {
        if (this.Target == null)
            return Messages.Default;

        var position = this.Target.Global + this.Target.Face + GetPrecise();
        switch (this.Target.Type)
        {
            case TargetType.Cell:
                SpawnEntity();
                break;

            case TargetType.Entity:
                if (InputState.IsKeyDown(Keys.ControlKey))
                    PacketEntityDispose.Send(Client.Instance, Target.Object.RefId, Client.Instance.GetPlayer());
                else if (this.Target.Object.CanAbsorb(this.Entity))
                    IncreaseQuantity();
                break;

            case TargetType.Slot:
                // TODO: spawn entity as a child
                break;

            default:
                break;
        }
        return Messages.Default;
    }

    private void SpawnEntity()
    {
        //var map = Engine.Map;
        var map = this.Map;
        var blockHeight = Block.GetBlockHeight(map, this.Target.Global);
        var position = this.Target.Global + this.Target.Face * new Vector3(1,1,blockHeight) + GetPrecise();
        PacketEntityRequestSpawn.SendTemplate(Client.Instance, this.TemplateID, position.At(map));
    }

    private void IncreaseQuantity()
    {
        var obj = this.Target.Object;
        obj.SyncSetStackSize(obj.StackSize + (InputState.IsKeyDown(Keys.ShiftKey) ? 5 : 1));
    }

    public override ControlTool.Messages MouseRightUp(HandledMouseEventArgs e)
    {
        return Messages.Remove;
    }
    public override ControlTool.Messages MouseRightDown(HandledMouseEventArgs e)
    {
        return Messages.Default;
    }
    internal override void DrawAfterWorld(MySpriteBatch sb, RenderContext ctx)
    {
        if (InputState.IsKeyDown(Keys.ControlKey))
            return;
        if (this.Target is null || this.Target.Type == TargetType.Null)
            return;
        this.Entity.DrawPreview(sb, ctx, this.Target, InputState.IsKeyDown(Keys.ShiftKey));
    }

    private Vector3 GetPrecise()
    {
        return InputState.IsKeyDown(Keys.ShiftKey) ? this.Target.Precise : Vector3.Zero;
    }
    protected override void WriteData(IDataWriter w)
    {
        w.Write(this.TemplateID);
    }
    protected override void ReadData(IDataReader r)
    {
        this.TemplateID = r.ReadInt32();
        this.Entity = GameObject.Templates[this.TemplateID];
    }

    internal override void DrawAfterWorldRemote(MySpriteBatch sb, RenderContext ctx, PlayerData player)
    {
        this.Entity.DrawPreview(sb, ctx, player.Target, true);
    }
}
