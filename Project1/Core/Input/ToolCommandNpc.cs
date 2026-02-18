using Project1.Core.AI.Packets;
using Project1.Core.Entities.Actors;
using Project1.Core.Networking;
using Project1.Framework.Input;
using Project1.Framework.UI;
using System.Collections.Generic;
using System.Linq;

namespace Project1.Core.Input
{
    class ToolCommandNpc : ToolManagement
    {
        private readonly List<Actor> Actors;
        public ToolCommandNpc()
        {

        }
        public ToolCommandNpc(Actor npc)
            : this(new List<Actor>() { npc })
        {
        }

        public ToolCommandNpc(List<Actor> actors)
        {
            this.Actors = actors.ToList();
        }
        public override Icon GetIcon()
        {
            return Icon.Replace;
        }
        public override ControlTool.Messages MouseLeftPressed(System.Windows.Forms.HandledMouseEventArgs e)
        {
            if (this.Target != null)
                if (this.Target.Type == TargetType.Cell)
                    PacketCommandNpc.Send(Client.Instance, this.Actors.Select(i => i.RefId).ToList(), this.Target, IsEnqueing);
            return base.MouseLeftPressed(e);
        }
        public override ControlTool.Messages MouseRightDown(System.Windows.Forms.HandledMouseEventArgs e)
        {
            return Messages.Remove;
        }

        public bool IsEnqueing
        {
            get
            {
                return InputState.IsKeyDown(System.Windows.Forms.Keys.LShiftKey);
            }
        }
    }
}
