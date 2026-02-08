using Microsoft.Xna.Framework;
using Project1.Core.Blocks;
using Project1.Core.Base;
using Project1.Core.Input.Tools;
using Project1.Core.Net;
using System;
using System.Windows.Forms;
using Project1.Core.Rendering;
using Project1.Core.Materials;
using Project1.Core.Screens;
using Project1.Core.Helpers;
using Project1.Core.Simulation;
using Project1.Framework.UI;
using Project1.Framework.IO;
using Project1.Framework.Math;

namespace Project1.Core.Input
{
    class ToolBlockEdit : ToolManagement
    {
        Block Block;
        byte State;
        int Variation;
        bool Painting;
        readonly Random Random;
        int Orientation;
        readonly MaterialDef Material;
        Vector3 LastPainted = new(float.MinValue);
        readonly Keys KeyReplace = Keys.ShiftKey;
        readonly Keys KeyRemove = Keys.ControlKey;
        public override bool TargetOnlyBlocks => true;

        public ToolBlockEdit()
        {

        }
        public ToolBlockEdit(Block block, MaterialDef mat, byte state)
        {
            this.Material = mat;
            this.State = state;
            this.Random = new Random();
            this.Block = block;
            this.Variation = new Random().Next(this.Block.Variations.Count);
        }

        // TODO: move this to mouse left up
        public override ControlTool.Messages MouseLeftPressed(System.Windows.Forms.HandledMouseEventArgs e)
        {
            if (e.Handled)
                return Messages.Default;
            if (this.Target == null)
                return Messages.Default;
            this.Painting = true;
            this.Paint();
            return ControlTool.Messages.Default;
        }
        public override ControlTool.Messages MouseLeftUp(System.Windows.Forms.HandledMouseEventArgs e)
        {
            if (e.Handled)
                return Messages.Default;
            this.LastPainted = new Vector3(float.MinValue);
            this.Painting = false;
            return base.MouseLeftUp(e);
        }
        public override ControlTool.Messages MouseRightDown(System.Windows.Forms.HandledMouseEventArgs e)
        {
            return ControlTool.Messages.Remove;
        }
        public override ControlTool.Messages MouseRightUp(HandledMouseEventArgs e)
        {
            this.LastPainted = new Vector3(float.MinValue); // HACK
            return base.MouseRightUp(e);
        }

        void Paint()
        {
            bool isDelete = InputState.IsKeyDown(this.KeyRemove);
            bool isReplace = InputState.IsKeyDown(this.KeyReplace);
            var global = this.Target.Global + ((isDelete || isReplace) ? Vector3.Zero : this.Target.Face);
            var block = isDelete ? BlockDefOf.Air.Worker : this.Block;
            byte state = isDelete ? (byte)0 : this.State;

            if (global != this.LastPainted)
                Ingame.Instance.Events.Post(new PlayerPaintedBlockEvent(global, block, this.Material, state, this.Variation, this.Orientation));
                //PacketPlayerSetBlock.Send(Client.Instance, Client.Instance.GetPlayer(), global, block, this.Material, state, this.Variation, this.Orientation);
            this.LastPainted = global;

            this.Variation = this.Random.Next(block.Variations.Count);
        }

        internal override void RotateAntiClockwise()
        {
            this.Orientation -= 1;
            if (this.Orientation < 0)
                this.Orientation = 3;
        }

        internal override void RotateClockwise()
        {
            this.Orientation = (this.Orientation + 1) % 4;
        }
        internal override void DrawAfterWorld(MySpriteBatch sb, MapBase map)
        {
            var cam = map.Camera;
            base.DrawAfterWorld(sb, map);
            if (this.Painting)
                return;
            if (InputState.IsKeyDown(Keys.ControlKey))
                return;
            if (this.Target is null)
                return;

            var atlastoken = this.Block.GetDefault();
            var global = (IntVec3)this.Target.FaceGlobal;
            atlastoken.Atlas.Begin(cam.Effect);
            this.Block.DrawPreview(sb, map, global, cam, this.State, this.Material, this.Variation, this.Orientation);
            sb.Flush();
            this.Block.DrawInteractionCells(sb, cam, map, global, this.Orientation);
        }
        public override Icon GetIcon()
        {
            if (InputState.IsKeyDown(this.KeyReplace))
                return Icon.Replace;
            if (InputState.IsKeyDown(this.KeyRemove))
                return Icon.Cross;
            return base.GetIcon();
        }

        internal override void DrawAfterWorldRemote(MySpriteBatch sb, MapBase map, Camera camera, PlayerData player)
        {
            var targetArgs = player.Target;
            if (targetArgs.Type != TargetType.Position)
                return;
            var atlastoken = this.Block.GetDefault();
            var global = targetArgs.FaceGlobal;
            atlastoken.Atlas.Begin(camera.Effect);
            this.Block.DrawPreview(sb, map, global, camera, this.State, this.Material, this.Variation, this.Orientation);
            sb.Flush();
        }
        protected override void WriteData(System.IO.BinaryWriter w)
        {
            w.Write(this.Block.BlockDef);
            w.Write(this.State);
        }
        protected override void ReadData(IDataReader r)
        {
            this.Block = r.ReadDef<BlockDef>().Worker;
            this.State = r.ReadByte();
        }
    }
}