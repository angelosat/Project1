using Microsoft.Xna.Framework;
using Start_a_Town_.Net;
using Start_a_Town_.UI;

namespace Start_a_Town_
{
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
                this.Target.Precise = InputState.IsKeyDown(System.Windows.Forms.Keys.ShiftKey) ? this.Target.Precise : Vector3.Zero;
        }
        public override ControlTool.Messages MouseLeftPressed(System.Windows.Forms.HandledMouseEventArgs e)
        {
            return Messages.Default;
        }
        public override ControlTool.Messages MouseLeftUp(System.Windows.Forms.HandledMouseEventArgs e)
        {
            if (this.Target == null)
                return Messages.Default;

            var position = this.Target.Global + this.Target.Face + GetPrecise();
            switch (this.Target.Type)
            {
                case TargetType.Position:
                    SpawnEntity();
                    break;

                case TargetType.Entity:
                    if (InputState.IsKeyDown(System.Windows.Forms.Keys.ControlKey))
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
            var map = Client.Instance.Map;
            var blockHeight = Block.GetBlockHeight(map, this.Target.Global);
            var position = this.Target.Global + this.Target.Face * new Vector3(1,1,blockHeight) + GetPrecise();
            PacketEntityRequestSpawn.SendTemplate(Client.Instance, this.TemplateID, position.At(map));
        }

        private void IncreaseQuantity()
        {
            var obj = this.Target.Object;
            obj.SyncSetStackSize(obj.StackSize + (InputState.IsKeyDown(System.Windows.Forms.Keys.ShiftKey) ? 5 : 1));
        }

        public override ControlTool.Messages MouseRightUp(System.Windows.Forms.HandledMouseEventArgs e)
        {
            return Messages.Remove;
        }
        public override ControlTool.Messages MouseRightDown(System.Windows.Forms.HandledMouseEventArgs e)
        {
            return Messages.Default;
        }
        internal override void DrawAfterWorld(MySpriteBatch sb, MapBase map)
        {
            var cam = map.Camera;
            if (InputState.IsKeyDown(System.Windows.Forms.Keys.ControlKey))
                return;
            if (this.Target is null || this.Target.Type == TargetType.Null)
                return;
            this.Entity.DrawPreview(sb, cam, this.Target, InputState.IsKeyDown(System.Windows.Forms.Keys.ShiftKey));
        }

        private Vector3 GetPrecise()
        {
            return InputState.IsKeyDown(System.Windows.Forms.Keys.ShiftKey) ? this.Target.Precise : Vector3.Zero;
        }
        protected override void WriteData(System.IO.BinaryWriter w)
        {
            w.Write(this.TemplateID);
        }
        protected override void ReadData(IDataReader r)
        {
            this.TemplateID = r.ReadInt32();
            this.Entity = GameObject.Templates[this.TemplateID];
        }

        internal override void DrawAfterWorldRemote(MySpriteBatch sb, MapBase map, Camera camera, Net.PlayerData player)
        {
            this.Entity.DrawPreview(sb, camera, player.Target, true);
        }
    }
}
